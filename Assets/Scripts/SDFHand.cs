using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class SDFHand : BaseHand
{
    [SerializeField] private Finger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    [SerializeField] private SDFSampler sdfSampler;
    
    [SerializeField] private GameObject fingerTipPositionIndicatorPrefab;

    [SerializeField] private bool fineTune = false;
    [SerializeField] private float mull = 1f;
    
    private List<Vector3> _fingerTipPositionCache = new List<Vector3>();

    private List<float> _fingerTipDeltaPositionCache = new List<float>();
    //private float[] _fingerTipResult;

    private void OnValidate()
    {
        fingers = GetComponentsInChildren<Finger>().Where(finger => finger is not FingerPart).ToArray();
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
        
        var prevPos = Vector3.zero;
        foreach (var finger in fingers)
        {
            
            for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
            {
                finger.Squish = alpha;
                yield return new WaitForEndOfFrame();
                var texPos = sdfUpdater.WorldToTexPos(finger.Tip.position);
                _fingerTipPositionCache.Add(texPos);
                if (alpha>0f)
                    _fingerTipDeltaPositionCache.Add((texPos - prevPos).magnitude);
                prevPos = texPos;
                if (fingerTipPositionIndicatorPrefab)
                {
                    Instantiate(fingerTipPositionIndicatorPrefab, finger.Tip.position, Quaternion.identity, transform);
                }
            }
            _fingerTipDeltaPositionCache.Add(0); // finger bent at 100% doesnt move
        }
        
        Debug.Log($"PrepareFingerTipPositionCache: final positions count {_fingerTipPositionCache.Count}");
        OpenHand();
        sdfSampler.SetupSampler(sdfUpdater.renderTexture, _fingerTipPositionCache.Count);
        sdfSampler.SetSamplerData(_fingerTipPositionCache);
        //_fingerTipResult = new float[_fingerTipPositionCache.Count];
    }

    private float FineTuneFinger(float delta, float voxelValue)
    {
        return (voxelValue-minTipDistance) * mull / delta * alphaStep;
    }
    
    protected override void CloseHand2()
    {
        StartWatch();

        var output = sdfSampler.RunSampler();
        StopWatch("running sampler"); StartWatch();
        AsyncGPUReadback.Request(output, request =>
        {
            if (!Application.isPlaying) // FIX for GPU callback running after stopping app in unity editor
                return;
            
            StopWatch("GPU readback"); StartWatch();
            // OpenHand();
            var resultArr = request.GetData<float>();
            var alpha = 0f;
            var stopped = false;
            var eFinger = fingers.GetEnumerator();
            eFinger.MoveNext();

            var eDelta = _fingerTipDeltaPositionCache.GetEnumerator();
            var prevDelta = 0f;
            foreach (var result in resultArr)
            {
                eDelta.MoveNext();
                if (!stopped && result < minTipDistance
                             && (!fineTune || alpha > alphaStep) // this is needed for finger to move even a bit into the surface for it to comeback
                             ) 
                {
                    stopped = true;
                    ((Finger)eFinger.Current).Squish = alpha + (fineTune ? FineTuneFinger(eDelta.Current, result) : 0);
                }
            
                prevDelta = eDelta.Current;
                alpha += alphaStep;
                if (alpha > 1.0f)
                {
                    if (!stopped)
                        ((Finger)eFinger.Current).Squish = 1f;
                    alpha = 0;
                    prevDelta = 0;
                    stopped = false;
                    eFinger.MoveNext();
                }
            }

            StopWatch("parsing results");
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
