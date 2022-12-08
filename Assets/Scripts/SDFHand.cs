using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFHand : BaseHand
{
    [SerializeField] private Finger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    
    private List<Vector3> _fingerTipPositionCache = new List<Vector3>();
    private float[] _fingerTipResult;

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
                yield return new WaitForEndOfFrame();
                var texPos = sdfUpdater.WorldToTexPos(finger.Tip.position);
                _fingerTipPositionCache.Add(texPos);
            }
        }
        
        Debug.Log($"PrepareFingerTipPositionCache: final positions count {_fingerTipPositionCache.Count}");
        OpenHand();
        sdfUpdater.SetupSampler(_fingerTipPositionCache.Count);
        sdfUpdater.SetSamplerData(_fingerTipPositionCache);
        _fingerTipResult = new float[_fingerTipPositionCache.Count];
    }

    protected override void CloseHand2()
    {
        StartWatch();
        OpenHand();
        
        sdfUpdater.RunSampler(_fingerTipResult);
        
        var e = _fingerTipResult.GetEnumerator();

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
        
        StopWatch();
    }
}
