using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseHand), true)]
public class BaseHandEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUI.enabled = Application.isPlaying;
        
        EditorGUILayout.HelpBox("Below buttons will currently work only in play mode",MessageType.Info);
        
        var bh = target as BaseHand;

        if (bh)
        {
            if(GUILayout.Button("Open Hand"))
            {
                bh.OpenHand();
            }
            if(GUILayout.Button("Close Hand"))
            {
                bh.CloseHand();
            }

            if (GUILayout.Button("Toggle interactive update"))
            {
                if (bh.interactiveUpdate != null)
                {
                    bh.StopCoroutine(bh.interactiveUpdate);
                    bh.interactiveUpdate = null;
                }
                else
                {
                    bh.interactiveUpdate = bh.StartCoroutine(bh.InteractiveUpdate());
                }
            }
            
            GUILayout.Label(bh.interactiveUpdate==null?"Update off":"Update on");
        }
    }
}