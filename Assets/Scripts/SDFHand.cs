using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SDFHand : BaseHand
{
    private const float MaxSquishAngle = 90f;

    [SerializeField] private Finger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    
    private List<Vector3> _fingerTipPositionCache = new List<Vector3>();

    private void OnValidate()
    {
        fingers = GetComponentsInChildren<Finger>();
    }

    public override void OpenHand()
    {
        foreach (var finger in fingers)
        {  
            finger.Squish = 0;
        }
    }

    private void Start()
    {
        StartCoroutine(PrepareFingerTipPositionCache());
    }
    
    private IEnumerator PrepareFingerTipPositionCache()
    {
        yield return new WaitForEndOfFrame();
        OpenHand();
        
        foreach (var finger in fingers)
        {
            for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
            {
                finger.Squish = alpha;
                var texPos = sdfUpdater.WorldToTexPos(finger.Tip.position);
                _fingerTipPositionCache.Add(texPos);
            }
        }
        
        Debug.Log($"PrepareFingerTipPositionCache: final positions count {_fingerTipPositionCache.Count}");
        OpenHand();
        sdfUpdater.SetupSampler(_fingerTipPositionCache.Count);
    }

    protected override void CloseHand2()
    {
        OpenHand();
        sdfUpdater.UpdateSDF();
        sdfUpdater.SetSamplerData(_fingerTipPositionCache);
;
        var fingerTipResult = new float[_fingerTipPositionCache.Count];
        
        sdfUpdater.RunSampler(fingerTipResult);
        
        var e = fingerTipResult.GetEnumerator();

        foreach (var finger in fingers)
        {
            var stopped = false;
            for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
            {
                e.MoveNext();
                if (stopped) continue;
                
                if ((float)e.Current < minTipDistance)
                {
                    finger.Squish = alpha;
                    stopped = true;
                }
            }
        }
    }
}
