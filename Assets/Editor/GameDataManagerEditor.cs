using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameDataManager))]
public class GameDataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        GameDataManager manager = (GameDataManager)target;

        if (GUILayout.Button("Reset Saved Data"))
            manager.ResetSavedData();
    }
}