using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/**
 * HandPoserBase class holds open and close poses, can also record poses
 * and holds info about Finger class that bends chain of bones
 */
[ExecuteAlways]
public class HandPoser : HandPoserBase
{
    private void OnValidate()
    {
        fingers = GetComponentsInChildren<FingerBase>().Where(f => f is Finger).ToArray();
    }



    // public void BlendPose(HandPose pose1, HandPose pose2, float t)
    // {
    //     foreach (var finger in fingers)
    //     {
    //         SquishFinger(finger, t);
    //     }
    //     // for (var i = 0; i < fingers.Length; ++i)
    //     // {
    //     //     var finger = fingers[i];
    //     //     var fingerState1 = pose1.fingerStates[i];
    //     //     var fingerState2 = pose2.fingerStates[i];
    //     //     InternalBlend(finger,fingerState1,fingerState2,t);
    //     // }
    // }
    

}
