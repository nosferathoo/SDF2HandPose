using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class SDFHand : BaseHand
{
    private const float MaxSquishAngle = 90f;

    [SerializeField] private Transform[] fingerPartsPads;
    [SerializeField] private Transform[] fingerParts;
    [SerializeField] private float alphaStep = 0.01f;
    [SerializeField] private float minTipDistance = 0.01f;
    [SerializeField] private SDFUpdater sdfUpdater;
    
    private Quaternion[] _originalRotations;

    private void OnValidate()
    {
        fingerPartsPads = GetComponentsInChildren<Transform>().Where(t => t.name.EndsWith("pad_marker")).ToArray();
        fingerParts = fingerPartsPads.Select(t => t.parent).ToArray();
        
        sdfUpdater = GetComponentInChildren<SDFUpdater>();
    }

    private void Start()
    {
        _originalRotations = fingerParts.Select(t => t.localRotation).ToArray();
    }

    public override void OpenHand()
    {
        for (var i = 0; i < fingerParts.Length; ++i)
        {
            fingerParts[i].localRotation = _originalRotations[i];
        }
    }

    protected override void CloseHand2()
    {
        sdfUpdater.UpdateSDF();
        var stopped = new bool[fingerParts.Length];
        Vector3[] fingerPartsPositions = new Vector3[fingerParts.Length];
        float[] fingerPartsResult = new float[fingerParts.Length];
        
        sdfUpdater.SetupSampler(sdfUpdater.Baker.SdfTexture, fingerParts.Length);

        for (var alpha = 0f; alpha < 1f; alpha += alphaStep)
        {
            var q = Quaternion.Euler(0,0,-MaxSquishAngle * alpha);
            for (var  i = 0; i < fingerParts.Length; ++i)
            {
                if (!stopped[i])
                {
                    fingerParts[i].localRotation = _originalRotations[i] * q;
                }
            }
            
            // probing
            for (var i = 0; i < fingerParts.Length; ++i)
            {
                fingerPartsPositions[i] = sdfUpdater.WorldToTexPos(fingerParts[i].position);
            }
            
            sdfUpdater.RunSampler(fingerPartsPositions, fingerPartsResult);
            
            
            for (var  i = 0; i < fingerParts.Length; ++i)
            {
                if (stopped[i]) continue;
                if (fingerPartsResult[i] < minTipDistance)
                {
                    stopped[i] = true;
                }
            }
        }
        
        sdfUpdater.ReleaseSampler();
    }
}
