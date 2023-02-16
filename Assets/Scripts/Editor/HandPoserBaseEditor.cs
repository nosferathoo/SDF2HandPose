using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HandPoserBase))]
public class HandPoserBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var poser = target as HandPoserBase;

        if(GUILayout.Button("Save open pose"))
        {
            poser.SaveOpenPose();
        }
        if(GUILayout.Button("Save closed pose"))
        {
            poser.SaveClosedPose();
        }
        if(GUILayout.Button("Show open pose"))
        {
            poser.OpenPose();
        }
        if(GUILayout.Button("Show closed pose"))
        {
            poser.ClosedPose();
        }
    }
}