using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.SDF;
using Debug = UnityEngine.Debug;

public class SDFUpdater : MonoBehaviour
{
    public Vector3 sizeBox;
    public int maxResolution;


    public VisualEffect vfx;

    public Vector3 overlapBoxExtents;
    public LayerMask overlapBoxLayerMask;

    public MeshToSDFBaker Baker;

    private List<Mesh> _meshes;
    private List<Matrix4x4> _mat;

    private List<MeshFilter> _meshFilters = new List<MeshFilter>();

    // slicing and 3d texture rendering
    public ComputeShader sampler;
    private Vector3 _sizeBox2;
    private Vector3 _halfBox = new Vector3(.5f, .5f, .5f);

    private int _samplerKernelIndex;
    private ComputeBuffer _samplerPositionsBuffer, _samplerResultsBuffer;
    private uint _samplerThreadGroupSize;

    // private Stopwatch _watch = new Stopwatch();
    
    // Start is called before the first frame update
    private void Start()
    {
        _sizeBox2 = new Vector3(1f / sizeBox.x, 1f / sizeBox.y, 1f / sizeBox.z);
        SetupBaker();
    }

    private void UpdateOverlappingMeshes()
    {
        var cols = Physics.OverlapBox(transform.position, overlapBoxExtents, transform.rotation,
            overlapBoxLayerMask);
        _meshFilters.Clear();
        _meshes.Clear();
        _mat.Clear();
        foreach (var col in cols)
        {
            if (col.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                _meshFilters.Add(meshFilter);
                _meshes.Add(meshFilter.mesh);
                _mat.Add(meshFilter.transform.localToWorldMatrix);
            }
        }
    }

    // private void WatchStart()
    // {
    //     _watch.Reset();
    //     _watch.Start();
    // }
    //
    // private void WatchPrint(string taskName)
    // {
    //     Debug.Log($"SDFUpdate watch timer for {taskName}: {_watch.Elapsed}");
    //     _watch.Restart();
    // }

    // Update is called once per frame
    public void UpdateSDF()
    {
        //WatchStart();
        UpdateOverlappingMeshes();  //WatchPrint("UpdateOverlappingMeshes");
        BakeSDFTexture();  //WatchPrint("BakeSDFTexture");
        // debug VFX
        if (vfx)
            vfx.SetTexture("SDF", Baker.SdfTexture);

        //Make3DTextureFromSlices();
    }

    private void OnDestroy()
    {
        Baker.Dispose();
    }

    private void SetupBaker()
    {
        _mat = new List<Matrix4x4>();
        _meshes = new List<Mesh>();
        Baker = new MeshToSDFBaker(sizeBox, Vector3.zero, maxResolution,
            _meshes, _mat);
    }

    private void BakeSDFTexture()
    {
        for (var i = 0; i < _meshFilters.Count; ++i)
        {
            _mat[i] = transform.localToWorldMatrix.inverse * _meshFilters[i].transform.localToWorldMatrix;
        }

        Baker.Reinit(sizeBox, Vector3.zero, maxResolution, _meshes, _mat);

        Baker.BakeSDF();
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
        //WatchStart();
        _samplerKernelIndex = sampler.FindKernel("CSMain");
        sampler.GetKernelThreadGroupSizes(_samplerKernelIndex, out _samplerThreadGroupSize, out _, out _);
        Debug.Log($"SetupSampler: samplerThreadGroupSize={_samplerThreadGroupSize}");

        _samplerPositionsBuffer = new ComputeBuffer(pointsCount, sizeof(float) * 3);
        _samplerResultsBuffer = new ComputeBuffer(pointsCount, sizeof(float));
        sampler.SetBuffer(_samplerKernelIndex, "Positions", _samplerPositionsBuffer);
        sampler.SetBuffer(_samplerKernelIndex, "Results", _samplerResultsBuffer);
        sampler.SetTexture(_samplerKernelIndex, "Voxels", Baker.SdfTexture);
    }

    public void SetSamplerData(List<Vector3> inputPositions)
    {
        _samplerPositionsBuffer.SetData(inputPositions);
    }

    public void RunSampler(float[] results)
    {
        //WatchStart();
        sampler.Dispatch(_samplerKernelIndex, 1, 1, 1);
        _samplerResultsBuffer.GetData(results);
        //WatchPrint("RunSampler");
    }

    public void ReleaseSampler()
    {
        _samplerPositionsBuffer.Release();
        _samplerResultsBuffer.Release();
    }
}