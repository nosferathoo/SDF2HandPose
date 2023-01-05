using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Reference : MonoBehaviour
{
    public Texture texture;
    public Transform referencePoint;
    public float stepScale = 1;
    public float surfaceOffset;
    public bool useCustomColorRamp;
    public bool active = true;

    // We should initialize this gradient before using it as a custom color ramp
    public Gradient customColorRampGradient;
}

[CanEditMultipleObjects]
[CustomEditor(typeof(Reference))]
public class Handle : Editor
{
    private void OnSceneViewGUI(SceneView sv)
    {
        Object[] objects = targets;
        foreach (var obj in objects)
        {
            Reference reference = obj as Reference;
            if (reference != null && reference.texture != null && reference.active)
            {
                Handles.matrix = reference.referencePoint.localToWorldMatrix;
                Handles.DrawTexture3DSDF(reference.texture, reference.stepScale, reference.surfaceOffset,
                    reference.useCustomColorRamp ? reference.customColorRampGradient : null);
            }
        }
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneViewGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneViewGUI;
    }
}