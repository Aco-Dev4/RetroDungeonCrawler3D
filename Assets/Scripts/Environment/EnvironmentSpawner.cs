using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Settings")]
    public float yOffset = -0.5f;
    public NavMeshSurface navMeshSurface;

    [HideInInspector]
    public List<GameObject> spawnedObjects = new();

#if UNITY_EDITOR
    public void SpawnAll()
    {
        if (Application.isPlaying) return;

        ClearAll();

        EnvironmentSpawnPoint[] points = GetComponentsInChildren<EnvironmentSpawnPoint>();

        foreach (var point in points)
        {
            if (point.prefab == null)
                continue;

            #if UNITY_EDITOR
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(point.prefab, transform);
            #else
            GameObject obj = Instantiate(point.prefab, transform);
            #endif

            obj.transform.position = point.transform.position + Vector3.up * yOffset;

            obj.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            spawnedObjects.Add(obj);

            point.gameObject.SetActive(false);
        }

        RebuildNavMesh();
    }

    public void ClearAll()
    {
        if (Application.isPlaying) return;

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
                DestroyImmediate(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
        EnvironmentSpawnPoint[] points = GetComponentsInChildren<EnvironmentSpawnPoint>(true);

        foreach (var point in points)
        {
            point.gameObject.SetActive(true);
        }
    }

    void RebuildNavMesh()
    {
        if (navMeshSurface == null)
        {
            Debug.LogWarning("NavMeshSurface not assigned.");
            return;
        }

        navMeshSurface.RemoveData();
        navMeshSurface.BuildNavMesh();
    }
    #endif
}

