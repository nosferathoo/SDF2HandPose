using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

public class HandPoserBase : MonoBehaviour
{
    [SerializeField] protected FingerBase[] fingers;
    [SerializeField] private HandPose openPose, closedPose;
    [OnValueChanged("OnBaseSquishChangeCallback")] [Range(0,1)]
    private Dictionary<FingerBase, int> _fingerLookUp = new();

    private float _squish = 0f;
    public float Squish
    {
        get => _squish;
        set
        {
            _squish = value;
            foreach(var finger in Fingers) SquishFinger(finger, value);
        }
    }

    public FingerBase[] Fingers => fingers;

    protected virtual void Start()
    {
        _fingerLookUp.Clear();
        for (var i = 0; i < Fingers.Length; ++i)
        {
            _fingerLookUp.Add(Fingers[i],i);
            Fingers[i].Poser = this;
        }
    }

    #if UNITY_EDITOR
    public void RecordPose(HandPose pose)
    {
        EditorUtility.SetDirty(pose);
        pose.fingerStates = new HandPose.Record[Fingers.Length];
        for (var i = 0; i < Fingers.Length; ++i)
        {
            var finger = Fingers[i];
            var fingerState = pose.fingerStates[i] = new HandPose.Record();
            fingerState.positions = new Vector3[finger.ChainLength];
            fingerState.rotations = new Quaternion[finger.ChainLength];
            var tr = finger.transform;
            for (var j = 0; j < finger.ChainLength; ++j)
            {
                fingerState.positions[j] = tr.localPosition;
                fingerState.rotations[j] = tr.localRotation;
                if (tr.childCount > 0)
                {
                    tr = tr.GetChild(0);
                }
            }
        }
    }
    #endif

    public void SetPose(HandPose pose)
    {
        for (var i = 0; i < Fingers.Length; ++i)
        {
            var finger = Fingers[i];
            var fingerState = pose.fingerStates[i];
            var tr = finger.transform;
            for (var j = 0; j < finger.ChainLength; ++j)
            {
                tr.localPosition = fingerState.positions[j];
                tr.localRotation = fingerState.rotations[j];
                if (tr.childCount > 0)
                {
                    tr = tr.GetChild(0);
                }
            }
        }
    }
    
    private void InternalBlend(FingerBase finger, HandPose.Record fingerState1, HandPose.Record fingerState2, float t)
    {
        var tr = finger.transform;
        for (var j = 0; j < finger.ChainLength; ++j)
        {
            tr.localPosition = Vector3.Lerp(fingerState1.positions[j], fingerState2.positions[j], t);
            tr.localRotation = Quaternion.Lerp(fingerState1.rotations[j], fingerState2.rotations[j], t);
            if (tr.childCount > 0)
            {
                tr = tr.GetChild(0);
            }
        }
    }
    
    public void SquishFinger(FingerBase finger, float t)
    {
        var i = _fingerLookUp[finger];
        var fingerState1 = openPose.fingerStates[i];
        var fingerState2 = closedPose.fingerStates[i];
        InternalBlend(finger,fingerState1,fingerState2,t);
    }

    private void OnBaseSquishChangeCallback()
    {
        if (!openPose || !closedPose) return;
        Squish = _squish;
    }

    public void SaveOpenPose()
    {
        RecordPose(openPose);
    }

    public void SaveClosedPose()
    {
        RecordPose(closedPose);
    }
}
