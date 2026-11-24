using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameDataManager))]
public class GameDataManagerEditor : Editor
{
    private int _goldToAdd = 15;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameDataManager manager = (GameDataManager)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Test Gold", EditorStyles.boldLabel);

        bool canAddGold = EditorApplication.isPlaying && GameDataManager.Instance == manager;
        if (canAddGold)
            EditorGUILayout.LabelField("Current Gold", manager.GetGold().ToString());
        else
            EditorGUILayout.HelpBox("Enter Play Mode to use the test controls on the active save.", MessageType.Info);

        using (new EditorGUI.DisabledScope(!canAddGold))
        {
            _goldToAdd = Mathf.Max(1, EditorGUILayout.IntField("Gold to Add", _goldToAdd));
            if (GUILayout.Button("Add Gold"))
                AddTestGold(manager, _goldToAdd);
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Test Unlocks", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!canAddGold))
        {
            if (GUILayout.Button("Unlock Forest"))
                UnlockForest(manager, false);
            if (GUILayout.Button("Unlock Hard Mode Forest"))
                UnlockForest(manager, true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Reset Saved Data"))
            manager.ResetSavedData();
    }

    private static void UnlockForest(GameDataManager manager, bool hardMode)
    {
        if (!manager.HasFinishedTutorial())
            manager.SetFinishedTutorial(true);
        if (hardMode)
            manager.SetMapCompleted("Forest");

        FindFirstObjectByType<MenuManager>()?.RefreshMapButtons();
    }

    private static void AddTestGold(GameDataManager manager, int amount)
    {
        int balance = (int)System.Math.Min((long)manager.GetGold() + amount, int.MaxValue);
        manager.SetGold(balance);

        if (GoldUI.Instance != null)
            GoldUI.Instance.SetGold(balance);
    }
}
