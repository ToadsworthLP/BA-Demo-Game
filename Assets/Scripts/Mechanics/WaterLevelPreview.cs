using UnityEngine;

public class WaterLevelPreview : MonoBehaviour
{
    [SerializeField] private ChangeWaterLevelEvent previewOnEvent;
    [SerializeField] private ChangeWaterLevelEvent previewOffEvent;
    [SerializeField] private GameObject model;

    private void OnEnable()
    {
        previewOnEvent.Subscribe(PreviewOn);
        previewOffEvent.Subscribe(PreviewOff);
    }

    private void OnDisable()
    {
        previewOnEvent.Unsubscribe(PreviewOn);
        previewOffEvent.Unsubscribe(PreviewOff);
    }

    private void PreviewOn(ChangeWaterLevelEventArgs args)
    {
        transform.position = new Vector3(0, args.targetLevel, 0);
        model.SetActive(true);
    }

    private void PreviewOff(ChangeWaterLevelEventArgs args)
    {
        model.SetActive(false);
    }
}
