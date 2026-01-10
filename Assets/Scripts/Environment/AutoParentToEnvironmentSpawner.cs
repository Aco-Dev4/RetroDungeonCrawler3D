#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteAlways]
public class AutoParentToEnvironmentSpawner : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnEnable()
    {
        if (Application.isPlaying) return;
        if (transform.parent != null) return;

        EditorApplication.delayCall += TryAutoParent;
    }

    private void TryAutoParent()
    {
        if (this == null) return; // object deleted
        if (transform.parent != null) return;

        EnvironmentSpawner spawner = Object.FindAnyObjectByType<EnvironmentSpawner>();
        if (spawner == null) return;

        Undo.SetTransformParent(transform, spawner.transform, "Auto-parent Spawn Point");
    }
#endif
}


