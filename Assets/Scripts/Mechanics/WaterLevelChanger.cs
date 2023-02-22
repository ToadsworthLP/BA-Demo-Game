using UnityEngine;

public class WaterLevelChanger : MonoBehaviour, IInteractable
{
    [SerializeField] private float targetWaterLevel;
    [SerializeField] private float changeRate;
    [SerializeField] private ChangeWaterLevelEvent changeTargetWaterLevelEvent;
    [SerializeField] private GameObject rendererObject;

    public void Focus(InteractionContext context)
    {
        rendererObject.layer = Constants.FOCUSED_INTERACTABLE_LAYER;
    }

    public void Interact(InteractionContext context)
    {
        changeTargetWaterLevelEvent.Invoke(new ChangeWaterLevelEventArgs()
        {
            targetLevel = targetWaterLevel,
            changeRate = changeRate
        });
    }

    public void Unfocus(InteractionContext context)
    {
        rendererObject.layer = Constants.INTERACTABLE_LAYER;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(new Vector3(transform.position.x, targetWaterLevel, transform.position.z), new Vector3(3, 0.01f, 3));
    }
}
