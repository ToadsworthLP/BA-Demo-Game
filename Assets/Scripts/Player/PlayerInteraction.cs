using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private float triggerRadius;
    [SerializeField] private LayerMask interactionLayerMask;

    private IInteractable targetInteractable;
    private Collider[] overlapBuffer;
    private PlayerInput input;

    public void OnInteract(InputAction.CallbackContext context)
    {
        targetInteractable?.Interact(new InteractionContext()
        {
            player = player
        });
    }

    private void Start()
    {
        overlapBuffer = new Collider[16];
    }

    private void FixedUpdate()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, triggerRadius, overlapBuffer, interactionLayerMask);

        if (hitCount > 0)
        {
            float bestDistanceSquared = float.MaxValue;
            Collider bestCollider = null;

            for (int i = 0; i < hitCount; i++)
            {
                Collider current = overlapBuffer[i];

                float currentDistanceSquared = current.ClosestPoint(transform.position).sqrMagnitude;
                if (currentDistanceSquared < bestDistanceSquared)
                {
                    bestDistanceSquared = currentDistanceSquared;
                    bestCollider = current;
                }
            }

            InteractionContext context = new InteractionContext()
            {
                player = player
            };

            if (targetInteractable != null) targetInteractable.Unfocus(context);

            targetInteractable = bestCollider.GetComponent<IInteractable>();
            targetInteractable.Focus(context);
        }
        else
        {
            if (targetInteractable != null)
            {
                targetInteractable.Unfocus(new InteractionContext()
                {
                    player = player
                });

                targetInteractable = null;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
