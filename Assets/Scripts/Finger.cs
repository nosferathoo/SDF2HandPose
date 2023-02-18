using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
public class Finger : MonoBehaviour
{
    [SerializeField] private int chainLength = 3;
    [OnValueChanged("OnSquishChangeCallback")] [Range(0,1)]
    [SerializeField] private float squish = 0f;
    [SerializeField] private Transform tip;

    private HandPoserBase _poser;
    
    public Transform Tip => tip;

    public float Squish
    {
        get => squish;
        set
        {
            squish = value;
            _poser.SquishFinger(this, value);
        }
    }

    public int ChainLength => chainLength;

    public HandPoserBase Poser
    {
        get => _poser;
        set => _poser = value;
    }

    private void OnSquishChangeCallback()
    {
        Squish = squish;
    }
}
