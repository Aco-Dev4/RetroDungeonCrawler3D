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
            EditorGUILayout.HelpBox("Enter Play Mode to add Gold to the active save.", MessageType.Info);

        using (new EditorGUI.DisabledScope(!canAddGold))
        {
            _goldToAdd = Mathf.Max(1, EditorGUILayout.IntField("Gold to Add", _goldToAdd));
            if (GUILayout.Button("Add Gold"))
                AddTestGold(manager, _goldToAdd);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Reset Saved Data"))
            manager.ResetSavedData();
    }

    private static void AddTestGold(GameDataManager manager, int amount)
    {
        int balance = (int)System.Math.Min((long)manager.GetGold() + amount, int.MaxValue);
        manager.SetGold(balance);

        if (GoldUI.Instance != null)
            GoldUI.Instance.SetGold(balance);
    }
}
