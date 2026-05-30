using System.Collections;
using UnityEngine;

public class MenuShopCamera : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform normalView;
    [SerializeField] private Transform shopView;

    [Header("Settings")]
    [SerializeField] private float moveDuration = 0.5f;

    private Coroutine _moveRoutine;

    public void MoveToShopView()
    {
        MoveTo(shopView);
    }

    public void MoveToNormalView()
    {
        MoveTo(normalView);
    }

    private void MoveTo(Transform target)
    {
        if (target == null) return;

        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float lerp = t / moveDuration;

            transform.position = Vector3.Lerp(startPos, target.position, lerp);
            transform.rotation = Quaternion.Slerp(startRot, target.rotation, lerp);

            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
        
        MainMenuCameraParallax parallax = GetComponent<MainMenuCameraParallax>();
        if (parallax != null)
            parallax.SetBaseRotation(target.rotation);
    }
}