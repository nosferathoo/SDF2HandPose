using System.Linq;
using UnityEngine;
using UnityEditor;

public class RotateSelectedObjects : ScriptableObject
{
    [MenuItem("Tools/Rotate Selected Objects")]
    static void Rotate()
    {
        float angle = 10.0f;
        Vector3 axis = Vector3.up;
        RotatePopupWindow window = (RotatePopupWindow)EditorWindow.GetWindow(typeof(RotatePopupWindow));
        window.angle = angle;
        window.axis = axis;
        window.Show();
    }
}

public class RotatePopupWindow : EditorWindow
{
    public float angle;
    public Vector3 axis;

    void OnGUI()
    {
        angle = EditorGUILayout.FloatField("Enter angle in degrees:", angle);
        axis = EditorGUILayout.Vector3Field("Enter axis:", axis);

        if (GUILayout.Button("OK"))
        {
            Undo.RecordObjects(Selection.gameObjects.Select(o=>(Object)o).ToArray(), "Rotate Selected Objects");
            foreach (var obj in Selection.gameObjects)
            {
                obj.transform.Rotate(axis, angle);
                Undo.FlushUndoRecordObjects();
            }
            Close();
        }
    }
}