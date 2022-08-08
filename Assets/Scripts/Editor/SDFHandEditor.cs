using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SDFHand))]
[CanEditMultipleObjects]
public class SDFHandEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = Application.isPlaying;
        
        EditorGUILayout.HelpBox("Below buttons will currently work only in play mode",MessageType.Info);
        
        if(GUILayout.Button("Open Hand"))
        {
            (target as SDFHand)?.OpenHand();
        }
        if(GUILayout.Button("Close Hand"))
        {
            (target as SDFHand)?.CloseHand();
        }
    }
}