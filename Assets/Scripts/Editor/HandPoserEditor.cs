using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HandPoser))]
public class HandPoserEditor : Editor
{
    private float _squish = 0;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var poser = target as HandPoser;

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
            poser.Squish = 0;
        }
        if(GUILayout.Button("Show closed pose"))
        {
            poser.Squish = 1;
        }
        
        GUILayout.Label("Squish test:");
        var newSquish = GUILayout.HorizontalSlider(_squish, 0f, 1f); 
        if (Mathf.Abs(newSquish - _squish)>0.001f)
        {
            poser.Squish = _squish = newSquish;
        }

        GUILayout.Space(20);
    }
}