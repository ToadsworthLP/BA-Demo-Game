using UnityEngine;

public class PulseCanvasItem : MonoBehaviour
{
    [SerializeField] private float speed;

    private CanvasRenderer renderer;
    private float enableTime;

    private void Start()
    {
        renderer = GetComponent<CanvasRenderer>();
    }

    private void OnEnable()
    {
        enableTime = Time.time;
    }

    private void Update()
    {
        renderer.SetAlpha((Mathf.Sin((Time.time - enableTime) * speed) + 1) / 2);
    }
}
