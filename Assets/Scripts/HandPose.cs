using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HandPose", menuName = "Hand pose", order = 0)]
public class HandPose : ScriptableObject
{
    class VV3 : Vector3[]
    {
        
    }
    public Vector3[][] positions;
    public Quaternion[][] rotations;
}