﻿using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MultiPhaseSDFHand : BaseHand
{
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    [SerializeField] private SDFSampler sdfSampler;
    
    [SerializeField] private bool fineTune = false;
    [SerializeField] private float mull = 1f;
    
    private List<Vector3> _fingerTipPositionCache = new List<Vector3>();

    private List<float> _fingerTipDeltaPositionCache = new List<float>();

    private MultiPhaseHandPoser MultiPhaseHandPoser => (MultiPhaseHandPoser) Poser;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        PrepareFingerTipPositionCache(0);
        sdfSampler.SetupSampler(sdfUpdater.renderTexture, _fingerTipPositionCache.Count);
        SetSamplerData();
    }
    
    private void PrepareFingerTipPositionCache(int phase)
    {
        var prevPos = Vector3.zero;
        _fingerTipPositionCache.Clear();
        _fingerTipDeltaPositionCache.Clear();
        
        foreach (var fingerPart in MultiPhaseHandPoser.FingerPartsPerPhase[phase])
        {
            for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
            {
                fingerPart.Squish = alpha;
                // yield return new WaitForEndOfFrame();
                var texPos = sdfUpdater.WorldToTexPos(fingerPart.Tip.position);
                _fingerTipPositionCache.Add(texPos);
                if (alpha>0f)
                    _fingerTipDeltaPositionCache.Add((texPos - prevPos).magnitude);
                prevPos = texPos;
            }
            _fingerTipDeltaPositionCache.Add(0); // finger bent at 100% doesnt move
        }
        Debug.Log($"PrepareFingerTipPositionCache: final positions count {_fingerTipPositionCache.Count}");
    }

    private void SetSamplerData()
    {
        sdfSampler.SetSamplerData(_fingerTipPositionCache);
    }

    private float FineTuneFinger(float delta, float voxelValue)
    {
        return (voxelValue-minTipDistance) * mull / delta * alphaStep;
    }

    protected override void CloseHand2()
    {
        StartWatch();
        CloseHand3().Forget();
    }

    private async UniTaskVoid CloseHand3()
    {
        for(var phase=0;phase<MultiPhaseHandPoser.PhaseCount;++phase)
        {
            PrepareFingerTipPositionCache(phase);
            SetSamplerData();
            var output = sdfSampler.RunSampler();

            var request = await AsyncGPUReadback.Request(output);

            if (!Application.isPlaying) // FIX for GPU callback running after stopping app in unity editor
                return;

            var resultArr = request.GetData<float>();
            Debug.Log($"Sampler result array length {resultArr.Length}");
            var alpha = 0f;
            var stopped = false;
            var eFinger = MultiPhaseHandPoser.FingerPartsPerPhase[phase].GetEnumerator();
            eFinger.MoveNext();
            var currentFingerPart = (FingerPart) eFinger.Current;

            var eDelta = _fingerTipDeltaPositionCache.GetEnumerator();
            foreach (var result in resultArr)
            {
                eDelta.MoveNext();
                if (!stopped && result < minTipDistance
                             && (!fineTune ||
                                 alpha >
                                 alphaStep) // this is needed for finger to move even a bit into the surface for it to comeback
                   )
                {
                    stopped = true;
                    currentFingerPart.Squish = alpha + (fineTune ? FineTuneFinger(eDelta.Current, result) : 0);
                }

                alpha += alphaStep;
                if (alpha > 1.0f)
                {
                    if (!stopped)
                        currentFingerPart.Squish = 1f;
                    alpha = 0;
                    stopped = false;
                    if (eFinger.MoveNext())
                        currentFingerPart = (FingerPart) eFinger.Current;
                }
            }

            await UniTask.NextFrame();
        }
        StopWatch("Multiphase bending");
    }
}