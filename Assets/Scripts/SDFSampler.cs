using System.Collections.Generic;
using UnityEngine;

public class SDFSampler : MonoBehaviour
{
    public ComputeShader sampler;
    
    private int _samplerKernelIndex;
    private ComputeBuffer _samplerPositionsBuffer, _samplerResultsBuffer;
    private uint _samplerThreadGroupSize;
    
    public void SetupSampler(RenderTexture renderTexture, int pointsCount, int resultSize = 1)
    {
        _samplerKernelIndex = sampler.FindKernel("CSMain");
        sampler.GetKernelThreadGroupSizes(_samplerKernelIndex, out _samplerThreadGroupSize, out _, out _);
        Debug.Log($"SetupSampler: samplerThreadGroupSize={_samplerThreadGroupSize}, pointsToSample={pointsCount}");

        _samplerPositionsBuffer = new ComputeBuffer(pointsCount, sizeof(float) * 3);
        _samplerResultsBuffer = new ComputeBuffer(pointsCount, sizeof(float) * resultSize);
        sampler.SetBuffer(_samplerKernelIndex, "Positions", _samplerPositionsBuffer);
        sampler.SetBuffer(_samplerKernelIndex, "Results", _samplerResultsBuffer);
        sampler.SetTexture(_samplerKernelIndex, "Voxels", renderTexture);
    }


    public void SetSamplerData(List<Vector3> inputPositions)
    {
        _samplerPositionsBuffer.SetData(inputPositions);
    }

    public ComputeBuffer RunSampler()
    {
        sampler.Dispatch(_samplerKernelIndex, 1, 1, 1);
        return _samplerResultsBuffer;
    }

    private void OnDestroy()
    {
        ReleaseSampler();
    }

    public void ReleaseSampler()
    {
        _samplerPositionsBuffer.Release();
        _samplerResultsBuffer.Release();
    }        
}