using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public void Focus(InteractionContext context)
    {
        gameObject.layer = Constants.FOCUSED_INTERACTABLE_LAYER;
    }

    public void Interact(InteractionContext context)
    {
        Debug.Log("Interaction");
    }

    public void Unfocus(InteractionContext context)
    {
        gameObject.layer = Constants.INTERACTABLE_LAYER;
    }
}
