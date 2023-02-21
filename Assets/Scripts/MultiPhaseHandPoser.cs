using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class MultiPhaseHandPoser : HandPoserBase
{
    [SerializeField] private int phaseCount;
    
    // per phase lookup
    private FingerPart[][] _fingerPartsPerPhase;

    public FingerPart[][] FingerPartsPerPhase => _fingerPartsPerPhase;

    public int PhaseCount => phaseCount;

    private void OnValidate()
    {
        fingers = GetComponentsInChildren<FingerBase>().Where(f=>f is FingerPart).ToArray();
        phaseCount = fingers.Max(part => ((FingerPart)part).Phase) + 1;
    }

    private void Awake()
    {
        _fingerPartsPerPhase = new FingerPart[PhaseCount][];
        for (var i = 0; i < PhaseCount; ++i)
        {
            _fingerPartsPerPhase[i] = fingers.Select(fb => fb as FingerPart).Where(part => part.Phase == i).ToArray();
            Debug.Log($"Finger parts in phase {i} = {FingerPartsPerPhase[i].Length}");
        }
    }
}