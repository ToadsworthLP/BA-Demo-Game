using UnityEngine;

public class WaterLevelChanger : MonoBehaviour, IInteractable
{
    [SerializeField] private float targetWaterLevel;
    [SerializeField] private ChangeWaterLevelEvent changeWaterLevelEvent;
    [SerializeField] private GameObject rendererObject;

    public void Focus(InteractionContext context)
    {
        rendererObject.layer = Constants.FOCUSED_INTERACTABLE_LAYER;
    }

    public void Interact(InteractionContext context)
    {
        Debug.Log($"Change water level to {targetWaterLevel}");

        changeWaterLevelEvent.Invoke(new ChangeWaterLevelEventArgs()
        {
            targetLevel = targetWaterLevel
        });
    }

    public void Unfocus(InteractionContext context)
    {
        rendererObject.layer = Constants.INTERACTABLE_LAYER;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawCube(new Vector3(transform.position.x, targetWaterLevel, transform.position.z), new Vector3(3, 0.1f, 3));
    }
}
