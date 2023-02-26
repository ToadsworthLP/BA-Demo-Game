using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StayOverWorldspaceObject : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private Transform target;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.position = camera.WorldToScreenPoint(target.position, Camera.MonoOrStereoscopicEye.Mono);
    }
}
