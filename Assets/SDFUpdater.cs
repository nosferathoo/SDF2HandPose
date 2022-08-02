using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.SDF;

public class SDFUpdater : MonoBehaviour
{
    public Vector3 sizeBox;
    public int maxResolution;
    public MeshFilter sampleMesh;

    public VisualEffect vfx;

    public Vector3 overlapBoxExtents;
    public LayerMask overlapBoxLayerMask;

    private MeshToSDFBaker _baker;

    private List<Mesh> _meshes;
    private List<Matrix4x4> _mat;

    private List<MeshFilter> _meshFilters = new List<MeshFilter>();
    


    // Start is called before the first frame update
    private void Start()
    {
        _mat = new List<Matrix4x4>();
        _meshes = new List<Mesh>();
        _baker = new MeshToSDFBaker(sizeBox, Vector3.zero, maxResolution,
            _meshes, _mat );

        StartCoroutine(UpdateOverlappingMeshes());
    }

    IEnumerator UpdateOverlappingMeshes()
    {
        while (true)
        {
            var cols = Physics.OverlapBox(transform.position, overlapBoxExtents, transform.rotation, overlapBoxLayerMask);
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

            yield return new WaitForSeconds(.1f);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        for (int i = 0; i < _meshFilters.Count; ++i)
        {
            _mat[i] = transform.localToWorldMatrix.inverse * _meshFilters[i].transform.localToWorldMatrix;
        }
        
        _baker.Reinit(sizeBox,Vector3.zero,maxResolution,_meshes,_mat);
        _baker.BakeSDF();
        vfx.SetTexture("SDF",_baker.SdfTexture);
    }

    private void OnDestroy()
    {
        _baker.Dispose();
    }
}
