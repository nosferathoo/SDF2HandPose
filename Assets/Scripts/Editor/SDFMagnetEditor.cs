using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SDFMagnet), true)]
public class SDFMagnetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = Application.isPlaying;
        
        EditorGUILayout.HelpBox("Below buttons will currently work only in play mode",MessageType.Info);
        
        var magnet = target as SDFMagnet;

        if (magnet)
        {
            if(GUILayout.Button("Step"))
            {
                magnet.Step();
            }
            
            if(GUILayout.Button("Approach and autograsp"))
            {
                magnet.ApproachAndGrasp();
            }

            if(GUILayout.Button("Repeat approach and autograsp"))
            {
                magnet.ApproachAndGraspAndRepeat();
            }
        }
    }
}