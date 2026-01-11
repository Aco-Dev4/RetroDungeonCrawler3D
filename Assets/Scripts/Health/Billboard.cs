using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam != null) transform.LookAt(transform.position + cam.forward);
    }
}
