public interface IInteractable
{
    void Focus(InteractionContext context);
    void Unfocus(InteractionContext context);
    void Interact(InteractionContext context);
}

public struct InteractionContext
{
    public PlayerController player;
}