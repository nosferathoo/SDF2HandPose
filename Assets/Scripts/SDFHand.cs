using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SDFHand : MonoBehaviour
{
    [SerializeField] private SDFFinger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;

    private WaitForEndOfFrame _wfeof = new WaitForEndOfFrame();
    private void OnValidate()
    {
        fingers = GetComponentsInChildren<SDFFinger>();
        sdfUpdater = GetComponentInChildren<SDFUpdater>();
    }

    public void OpenHand()
    {
        foreach (var finger in fingers)
        {  
            finger.Squish = 0;
        }
    }

    public void CloseHand()
    {
        StopAllCoroutines();
        OpenHand();
        StartCoroutine(CloseHand2());
    }
    
    private IEnumerator CloseHand2()
    {
        for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
        {
            foreach (var finger in fingers)
            {
                if (sdfUpdater.ProbeSDFTexture(finger.Pad.position) > minTipDistance)
                {
                    finger.Squish = alpha;
                }
            }

            yield return _wfeof;
        }
    }
}
