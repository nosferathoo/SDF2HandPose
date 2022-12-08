using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(SDFTexture))]
public class SDFUpdater : MonoBehaviour
{
    [SerializeField] private SDFTexture sdfTexture;
    public Vector3 sizeBox;
    public RenderTexture renderTexture;
    
    // slicing and 3d texture rendering
    public ComputeShader sampler;
    private Vector3 _sizeBox2;
    private Vector3 _halfBox = new Vector3(.5f, .5f, .5f);

    private int _samplerKernelIndex;
    private ComputeBuffer _samplerPositionsBuffer, _samplerResultsBuffer;
    private uint _samplerThreadGroupSize;
    
    // Start is called before the first frame update
    private void Start()
    {
        _sizeBox2 = new Vector3(1f / sizeBox.x, 1f / sizeBox.y, 1f / sizeBox.z);
    }

    private MeshToSDF _meshToSDF;
    private int Res => sdfTexture.resolution;

    private void OnValidate()
    {
        if (sdfTexture) sdfTexture = GetComponent<SDFTexture>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<MeshToSDF>(out var mesh))
        {
            if (_meshToSDF) _meshToSDF.enabled = false;
            _meshToSDF = mesh;
            mesh.sdfTexture = sdfTexture;
            mesh.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<MeshToSDF>(out var mesh) && mesh.Equals(_meshToSDF))
        {
            _meshToSDF.enabled = false;
            _meshToSDF = null;
        }
    }
    
    public Vector3 WorldToTexPos(Vector3 worldPos)
    {
        var localPos = transform.InverseTransformPoint(worldPos);
        localPos.Scale(_sizeBox2);
        localPos += _halfBox;
        return localPos;
    }

    public void SetupSampler(int pointsCount)
    {

        _samplerKernelIndex = sampler.FindKernel("CSMain");
        sampler.GetKernelThreadGroupSizes(_samplerKernelIndex, out _samplerThreadGroupSize, out _, out _);
        Debug.Log($"SetupSampler: samplerThreadGroupSize={_samplerThreadGroupSize}");

        _samplerPositionsBuffer = new ComputeBuffer(pointsCount, sizeof(float) * 3);
        _samplerResultsBuffer = new ComputeBuffer(pointsCount, sizeof(float));
        sampler.SetBuffer(_samplerKernelIndex, "Positions", _samplerPositionsBuffer);
        sampler.SetBuffer(_samplerKernelIndex, "Results", _samplerResultsBuffer);
        sampler.SetTexture(_samplerKernelIndex, "Voxels", renderTexture);
    }


    public void SetSamplerData(List<Vector3> inputPositions)
    {
        _samplerPositionsBuffer.SetData(inputPositions);
    }

    public void RunSampler(float[] results)
    {
        sampler.Dispatch(_samplerKernelIndex, 1, 1, 1);
        _samplerResultsBuffer.GetData(results);
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