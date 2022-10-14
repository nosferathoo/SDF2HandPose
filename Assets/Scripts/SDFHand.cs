using System.Collections;
using UnityEngine;

public class SDFHand : BaseHand
{
    [SerializeField] private Finger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    private void OnValidate()
    {
        fingers = GetComponentsInChildren<Finger>();
        sdfUpdater = GetComponentInChildren<SDFUpdater>();
    }

    public override void OpenHand()
    {
        foreach (var finger in fingers)
        {  
            finger.Squish = 0;
        }
    }

    protected override void CloseHand2()
    {
        sdfUpdater.UpdateSDF();
        foreach (var finger in fingers)
        {
            for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
            {
                finger.Squish = alpha;
                
                if (sdfUpdater.ProbeSDFTexture(finger.Pad.position) < minTipDistance)
                {
                    break;
                }
            }
        }
    }
}
