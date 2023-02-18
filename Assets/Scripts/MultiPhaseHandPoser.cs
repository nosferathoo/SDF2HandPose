using System.Linq;
using UnityEngine;

public class MultiPhaseHandPoser : HandPoserBase
{
    [SerializeField] private int phaseCount;
    
    // per phase lookup
    private FingerPart[][] _fingerPartsPerPhase;

    private void OnValidate()
    {
        fingers = GetComponentsInChildren<FingerBase>().Where(f=>f is FingerPart).ToArray();
        phaseCount = fingers.Max(part => ((FingerPart)part).Phase) + 1;
    }

    private void Awake()
    {
        _fingerPartsPerPhase = new FingerPart[phaseCount][];
        for (var i = 0; i <= phaseCount; ++i)
        {
            _fingerPartsPerPhase[i] = (FingerPart[]) fingers.Where(part => ((FingerPart) part).Phase == i).ToArray();
        }
    }
}