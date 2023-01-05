using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SDFHand : BaseHand
{
    [SerializeField] private Finger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    
    [SerializeField] private GameObject fingerTipPositionIndicatorPrefab;
    
    private List<Vector3> _fingerTipPositionCache = new List<Vector3>();
    //private float[] _fingerTipResult;

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
                if (fingerTipPositionIndicatorPrefab)
                {
                    Instantiate(fingerTipPositionIndicatorPrefab, finger.Tip.position, Quaternion.identity, transform);
                }
            }
        }
        
        Debug.Log($"PrepareFingerTipPositionCache: final positions count {_fingerTipPositionCache.Count}");
        OpenHand();
        sdfUpdater.SetupSampler(_fingerTipPositionCache.Count);
        sdfUpdater.SetSamplerData(_fingerTipPositionCache);
        //_fingerTipResult = new float[_fingerTipPositionCache.Count];
    }

    protected override void CloseHand2()
    {
        StartWatch();

        var output = sdfUpdater.RunSampler();
        StopWatch(); StartWatch();
        AsyncGPUReadback.Request(output, request =>
        {
            StopWatch(); StartWatch();
            // OpenHand();
            var resultArr = request.GetData<float>();
            var alpha = 0f;
            var stopped = false;
            var eFinger = fingers.GetEnumerator();
            eFinger.MoveNext();
            foreach (var result in resultArr)
            {
                if (!stopped && result < minTipDistance)
                {
                    stopped = true;
                    ((Finger)eFinger.Current).Squish = alpha;
                }
            
                alpha += alphaStep;
                if (alpha > 1.0f)
                {
                    if (!stopped)
                        ((Finger)eFinger.Current).Squish = 1f;
                    alpha = 0;
                    stopped = false;
                    eFinger.MoveNext();
                }
            }
            StopWatch();
        });

        // var e = _fingerTipResult.GetEnumerator();
        // foreach (var finger in fingers)
        // {
        //     var stopped = false;
        //     for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
        //     {
        //         e.MoveNext();
        //         if (stopped) continue;
        //
        //         if ((float)e.Current < minTipDistance)
        //         {
        //             finger.Squish = alpha;
        //             stopped = true;
        //         }
        //     }
        // }
        
        
    }
}
