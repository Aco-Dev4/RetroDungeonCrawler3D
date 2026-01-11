using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HealthBar))]
public class HealthBarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty slider = serializedObject.FindProperty("slider");
        SerializedProperty player = serializedObject.FindProperty("player");
        SerializedProperty gradient = serializedObject.FindProperty("gradient");
        SerializedProperty fill = serializedObject.FindProperty("fill");

        EditorGUILayout.PropertyField(slider);
        EditorGUILayout.PropertyField(player);

        if (player.boolValue)
        {
            EditorGUILayout.PropertyField(gradient);
            EditorGUILayout.PropertyField(fill);
        }

        serializedObject.ApplyModifiedProperties();
    }
}