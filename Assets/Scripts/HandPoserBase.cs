using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/**
 * HandPoserBase class holds open and close poses, can also record poses
 * holds info about Finger class that bends chain of bones
 */
[ExecuteInEditMode]
public class HandPoserBase : MonoBehaviour
{
    [SerializeField] private Finger[] fingers;
    [SerializeField] private HandPose openPose, closedPose;
    [OnValueChanged("OnSquishChangeCallback")]
    [SerializeField] private float squish = 0f;
    private Dictionary<Finger, int> _fingerLookUp = new();

    private void Start()
    {
        for (var i = 0; i < fingers.Length; ++i)
        {
            _fingerLookUp.Add(fingers[i],i);
        }
    }

    private void OnValidate()
    {
        fingers = GetComponentsInChildren<Finger>();
    }

    public void RecordPose(HandPose pose)
    {
        pose.positions = new Vector3[fingers.Length][];
        pose.rotations = new Quaternion[fingers.Length][];
        for (var i = 0; i < fingers.Length; ++i)
        {
            var finger = fingers[i];
            pose.positions[i] = new Vector3[finger.ChainLength];
            pose.rotations[i] = new Quaternion[finger.ChainLength];
            var tr = finger.transform;
            for (var j = 0; j < finger.ChainLength; ++j)
            {
                pose.positions[i][j] = tr.localPosition;
                pose.rotations[i][j] = tr.localRotation;
                if (tr.childCount > 0)
                {
                    tr = tr.GetChild(0);
                }
            }
        }
    }

    public void SetPose(HandPose pose)
    {
        for (var i = 0; i < fingers.Length; ++i)
        {
            var finger = fingers[i];
            var tr = finger.transform;
            for (var j = 0; j < finger.ChainLength; ++j)
            {
                tr.localPosition = pose.positions[i][j];
                tr.localRotation = pose.rotations[i][j];
                if (tr.childCount > 0)
                {
                    tr = tr.GetChild(0);
                }
            }
        }
    }

    public void BlendPose(HandPose pose1, HandPose pose2, float t)
    {
        for (var i = 0; i < fingers.Length; ++i)
        {
            var finger = fingers[i];
            var tr = finger.transform;
            for (var j = 0; j < finger.ChainLength; ++j)
            {
                tr.localPosition = Vector3.Lerp(pose1.positions[i][j], pose2.positions[i][j], t);
                tr.localRotation = Quaternion.Lerp(pose1.rotations[i][j], pose2.rotations[i][j], t);
                if (tr.childCount > 0)
                {
                    tr = tr.GetChild(0);
                }
            }
        }
    }
    
    private void OnSquishChangeCallback()
    {
        if (!openPose || !closedPose) return;
        BlendPose(openPose, closedPose, squish);
    }

    public void SaveOpenPose()
    {
        RecordPose(openPose);
    }

    public void SaveClosedPose()
    {
        RecordPose(closedPose);
    }

    public void OpenPose()
    {
        SetPose(openPose);
    }

    public void ClosedPose()
    {
        SetPose(closedPose);
    }
}
