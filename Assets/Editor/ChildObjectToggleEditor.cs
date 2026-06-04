using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChildObjectToggle))]
public class ChildObjectToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ChildObjectToggle toggle = (ChildObjectToggle)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Show Children"))
            toggle.SetChildrenActive(true);

        if (GUILayout.Button("Hide Children"))
            toggle.SetChildrenActive(false);

        if (GUILayout.Button("Toggle Children"))
            toggle.ToggleChildren();
    }
}