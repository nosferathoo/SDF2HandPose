using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class Finger : MonoBehaviour
{
    private const float MaxSquishAngle = 90f;
    [SerializeField] private int chainLength = 3;
    [OnValueChanged("OnSquishChangeCallback")]
    [SerializeField] private float squish = 0f;
    [SerializeField] private Transform tip;
    
    private Dictionary<Transform,Quaternion> originalRotations = new Dictionary<Transform, Quaternion>();

    public Transform Tip => tip;

    public float Squish
    {
        get => squish;
        set
        {
            squish = value;
            var q = Quaternion.Euler(0,0,-MaxSquishAngle * Mathf.Clamp01(Squish));
            foreach (var pair in originalRotations)
            {
                pair.Key.localRotation = pair.Value * q;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var t = transform;
        for (var i = 0; i < chainLength; ++i)
        {
            originalRotations.Add(t, t.localRotation);
            if (t.childCount == 0)
                break;
            t = t.GetChild(0);
        }
    }

    private void OnSquishChangeCallback()
    {
        Squish = squish;
    }
}
