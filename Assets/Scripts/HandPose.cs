using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HandPose", menuName = "Hand pose", order = 0)]
public class HandPose : ScriptableObject
{
    // Record of one finger's each part
    [Serializable] public class Record
    {
        public Vector3[] positions;
        public Quaternion[] rotations;
    }

    public Record[] fingerStates; // state of each finger
}