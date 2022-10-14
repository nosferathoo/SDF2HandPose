using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.SDF;

public class SDFUpdater : MonoBehaviour
{
    public Vector3 sizeBox;
    public int maxResolution;


    public VisualEffect vfx;

    public Vector3 overlapBoxExtents;
    public LayerMask overlapBoxLayerMask;

    private MeshToSDFBaker _baker;

    private List<Mesh> _meshes;
    private List<Matrix4x4> _mat;

    private List<MeshFilter> _meshFilters = new List<MeshFilter>();

    // slicing and 3d texture rendering
    public ComputeShader slicer;
    private RenderTexture _sliceRenderTexture; // for 2D slice rendering
    private int _kernelIndex;
    private Texture2D _slice2Dtexture;
    private Color[] _slicePixels;
    private Texture3D _final3Dtexture;
    private Color[] _final3DtexturePixels;
    private Vector3 _sizeBox2;
    private Vector3 _halfBox = new Vector3(.5f, .5f, .5f);
    
    // Start is called before the first frame update
    private void Start()
    {
        _sizeBox2 = new Vector3(1f / sizeBox.x, 1f / sizeBox.y, 1f / sizeBox.z);
        SetupBaker();
        SetupSlicer();
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

    // Update is called once per frame
    public void UpdateSDF()
    {
        UpdateOverlappingMeshes();
        BakeSDFTexture();
        // debug VFX
        if (vfx)
            vfx.SetTexture("SDF", _baker.SdfTexture);

        Make3DTextureFromSlices();
    }

    private void OnDestroy()
    {
        _baker.Dispose();
    }

    private void SetupBaker()
    {
        _mat = new List<Matrix4x4>();
        _meshes = new List<Mesh>();
        _baker = new MeshToSDFBaker(sizeBox, Vector3.zero, maxResolution,
            _meshes, _mat);
    }

    private void BakeSDFTexture()
    {
        for (var i = 0; i < _meshFilters.Count; ++i)
        {
            _mat[i] = transform.localToWorldMatrix.inverse * _meshFilters[i].transform.localToWorldMatrix;
        }

        _baker.Reinit(sizeBox, Vector3.zero, maxResolution, _meshes, _mat);

        _baker.BakeSDF();
    }

    private void SetupSlicer()
    {
        _sliceRenderTexture = new RenderTexture(maxResolution, maxResolution, 0, RenderTextureFormat.RFloat)
        {
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            enableRandomWrite = true,
            wrapMode = TextureWrapMode.Clamp
        };
        _sliceRenderTexture.Create();

        _slice2Dtexture = new Texture2D(maxResolution, maxResolution);

        _final3Dtexture = new Texture3D(maxResolution, maxResolution, maxResolution, TextureFormat.RFloat, false);
        _final3Dtexture.filterMode = FilterMode.Trilinear;
        _final3DtexturePixels = _final3Dtexture.GetPixels();
    }

    private void Copy3DSliceToRenderTexture(RenderTexture source, int layer)
    {
        int kernelIndex = slicer.FindKernel("CSMain");
        slicer.SetTexture(kernelIndex, "Result", _sliceRenderTexture);
        slicer.SetTexture(_kernelIndex, "voxels", source);
        slicer.SetInt("layer", layer);
        slicer.Dispatch(_kernelIndex, maxResolution, maxResolution, 1);
    }

    private void MakeSlice2DTextureFromRenderTexture()
    {
        RenderTexture.active = _sliceRenderTexture;
        _slice2Dtexture.ReadPixels(new Rect(0, 0, maxResolution, maxResolution), 0, 0);
        _slice2Dtexture.Apply();
    }

    private void Make3DTextureFromSlices()
    {
        var layerSize = maxResolution * maxResolution;
        for (int k = 0; k < maxResolution; k++)
        {
            Copy3DSliceToRenderTexture(_baker.SdfTexture, k);
            MakeSlice2DTextureFromRenderTexture();
            _slicePixels = _slice2Dtexture.GetPixels();
            Array.Copy(_slicePixels, 0, _final3DtexturePixels, k * layerSize, layerSize);
        }

        _final3Dtexture.SetPixels(_final3DtexturePixels);
        _final3Dtexture.Apply();
    }

    public float ProbeSDFTexture(Vector3 worldPos)
    {
        var localPos = transform.InverseTransformPoint(worldPos);
        localPos.Scale(_sizeBox2);
        localPos+= _halfBox;

        return _final3Dtexture.GetPixelBilinear(localPos.x, localPos.y, localPos.z).r;

        //localPos *= maxResolution;
        //return _final3Dtexture.GetPixel((int) localPos.x, (int) localPos.y, (int) localPos.z);
    }
}