using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Oculus.Interaction.PoseDetection;
using UnityEngine;
using UnityEngine.Rendering;

public class SDFHandMultiPhase : BaseHand
{
    [Serializable]
    public struct MultiPhaseFinger
    {
        public FingerPart[] parts;
    }

    [SerializeField] private int phaseCount;
    [SerializeField] private MultiPhaseFinger[] fingers;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    [SerializeField] private SDFSampler sdfSampler;
    
    [SerializeField] private GameObject fingerTipPositionIndicatorPrefab;

    [SerializeField] private bool fineTune = false;
    [SerializeField] private float mull = 1f;
    
    private class MultiPhaseFingerTipPositionCache
    {
        public List<Vector3> positions = new List<Vector3>();
        public List<float> deltas = new List<float>();
    }
    
    private MultiPhaseFingerTipPositionCache _fingerTipPositionCache;
    //private float[] _fingerTipResult;

    // private void OnValidate()
    // {
    //     fingers = GetComponentsInChildren<Finger>();
    // }

    public override void OpenHand()
    {
        for (var level = 0; level < phaseCount; ++level)
        {
            OpenHand2(level);
        }
    }

    private void OpenHand2(int level = -1)
    {
        foreach (var finger in fingers)
        {
            finger.parts[level].Squish = 0;
        }
    }

    private void Start()
    {
        _fingerTipPositionCache = new MultiPhaseFingerTipPositionCache();

        PrepareFingerTipPositionCache().ContinueWith(() =>
        {
            sdfSampler.SetupSampler(sdfUpdater.renderTexture, _fingerTipPositionCache.positions.Count);
            sdfSampler.SetSamplerData(_fingerTipPositionCache.positions);
        });
    }
    
    private async UniTask PrepareFingerTipPositionCache()
    {
        foreach (var finger in fingers)
        {
            async UniTask<Vector3> BendStep(int level, float alpha, Vector3 prevPos)
            {
                finger.parts[level].Squish = alpha;
                await UniTask.NextFrame();
                
                var texPos = sdfUpdater.WorldToTexPos(finger.parts[level].Tip.position);
                _fingerTipPositionCache.positions.Add(texPos);
                if (prevPos!=Vector3.positiveInfinity)
                    _fingerTipPositionCache.deltas.Add((texPos - prevPos).magnitude);
                else
                    _fingerTipPositionCache.deltas.Add(0);

                if (level < phaseCount-1)
                    await BendLoop(level+1);
                
                if (fingerTipPositionIndicatorPrefab)
                {
                    Instantiate(fingerTipPositionIndicatorPrefab, finger.parts[level].Tip.position, Quaternion.identity, transform);
                }

                return texPos;
            }

            async UniTask BendLoop(int level)
            {
                var prevPos = Vector3.positiveInfinity;
                if (level==0)
                    for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
                        prevPos = await BendStep(level, alpha, prevPos);
                else
                    for (var alpha = 1f; alpha > 0f; alpha -= alphaStep)
                        prevPos = await BendStep(level, alpha, prevPos);
                _fingerTipPositionCache.deltas.Add(0); // finger bent at 100% doesnt move
            }
            
            await BendLoop(0);
        }
        
        OpenHand();
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
            OpenHand();
            var resultArr = request.GetData<float>();

            float alpha, alphaStep2;
            var level = 0;
            var alphaStack = new Stack<float>(); // for recurrency
            var stoppedStack = new Stack<bool>();
            void SetAlphaAndStep()
            {
                (alpha, alphaStep2) = level switch
                {
                    0 => (0, alphaStep),
                    _ => (1, -alphaStep)
                };
            }
            
            SetAlphaAndStep();

            var stopped = false;
            var stoppedFinal = false;
            var eFinger = fingers.GetEnumerator();
            eFinger.MoveNext();
            
            var finger = (MultiPhaseFinger)eFinger.Current;
            var eDelta = _fingerTipPositionCache.deltas.GetEnumerator();

            
            
            foreach (var result in resultArr)
            {
                eDelta.MoveNext();

                if (
                        !stoppedFinal &&
                            (
                                    (level==0 && !stopped && result < minTipDistance)
                                 || (level>0 && !stopped && stoppedStack.Peek() && result > minTipDistance)
                            )// current part not stopped but previous stopped
                        //&& (!fineTune || alpha > alphaStep) // this is needed for finger to move even a bit into the surface for it to comeback
                    ) 
                {
                    stopped = true;
                    if (level == phaseCount - 1)
                        stoppedFinal = true;
                    Debug.Log($"stopping finger {finger} at level {level} at alpha={alpha}");
                    finger.parts[level].Squish = alpha + (fineTune ? FineTuneFinger(eDelta.Current, result) : 0);
                }

                if (level < phaseCount - 1)
                {
                    alphaStack.Push(alpha);
                    stoppedStack.Push(stopped);
                    stopped = false;
                    level++;
                    SetAlphaAndStep();
                }
                else
                {
                    while (true)
                    {
                        alpha += alphaStep2;
                        if (
                            (alpha > 1.0f && level == 0) ||
                            (alpha < 0.0f && level > 0)
                        )
                        {
                            if (!stoppedFinal &&
                                ((level == 0 && !stopped) || (level>0 && !stopped && stoppedStack.Peek()))
                                )
                                  finger.parts[level].Squish = level == 0 ? 1f : 0;

                            if (level == 0)
                            {
                                stopped = false;
                                stoppedFinal = false;
                                SetAlphaAndStep();
                                if (eFinger.MoveNext())
                                {
                                    finger = (MultiPhaseFinger) eFinger.Current;
                                }

                                break;
                            }
                            else
                            {
                                level--;
                                SetAlphaAndStep();
                                alpha = alphaStack.Pop();
                                stopped = stoppedStack.Pop();
                            }
                        }
                        else break;
                    }
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
