using UnityEngine;

[RequireComponent(typeof(SDFTexture))]
public class SDFUpdater : MonoBehaviour
{
    [SerializeField] private SDFTexture sdfTexture;
    public Vector3 sizeBox;
    public RenderTexture renderTexture;
    
    private Vector3 _sizeBox2;
    private Vector3 _halfBox = new Vector3(.5f, .5f, .5f);

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
}