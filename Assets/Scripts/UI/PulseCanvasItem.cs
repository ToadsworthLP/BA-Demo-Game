using UnityEngine;

public class PulseCanvasItem : MonoBehaviour
{
    [SerializeField] private float speed;

    private CanvasRenderer renderer;

    private void Start()
    {
        renderer = GetComponent<CanvasRenderer>();
    }

    private void Update()
    {
        renderer.SetAlpha((Mathf.Sin(Time.time * speed) + 1) / 2);
    }
}
