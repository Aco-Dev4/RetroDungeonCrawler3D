using UnityEngine;

public class ChildObjectToggle : MonoBehaviour
{
    public void SetChildrenActive(bool active)
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(active);
    }

    public void ToggleChildren()
    {
        if (transform.childCount == 0) return;

        bool newState = !transform.GetChild(0).gameObject.activeSelf;
        SetChildrenActive(newState);
    }
}