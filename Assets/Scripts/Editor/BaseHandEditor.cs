﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseHand), true)]
public class BaseHandEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = Application.isPlaying;
        
        EditorGUILayout.HelpBox("Below buttons will currently work only in play mode",MessageType.Info);
        
        if(GUILayout.Button("Open Hand"))
        {
            (target as BaseHand)?.OpenHand();
        }
        if(GUILayout.Button("Close Hand"))
        {
            (target as BaseHand)?.CloseHand();
        }
    }
}