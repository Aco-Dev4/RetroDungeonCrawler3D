using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnvironmentSpawner))]
public class EnvironmentSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnvironmentSpawner spawner = (EnvironmentSpawner)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Spawn Environment"))
        {
            spawner.SpawnAll();
        }

        if (GUILayout.Button("Clear Spawned Objects"))
        {
            spawner.ClearAll();
        }
    }
}
