using KinematicCharacterController;
using UnityEngine;

public class FloatingObject : MonoBehaviour, IMoverController, IInteractable
{
    [Header("Ground")]
    [SerializeField] private Vector3 raycastOffset;
    [SerializeField] private LayerMask groundCheckLayerMask;

    [Header("Floating")]
    [SerializeField] private FloatContainer currentWaterLevel;
    [SerializeField] private Vector3 floatingCheckPlaneOffset;
    [SerializeField] private LayerMask floatingCheckLayerMask;

    [Header("Physics")]
    [SerializeField] private Vector3 gravity;

    [Header("Interaction")]
    [SerializeField] private GameObject rendererObject;
    [SerializeField] private LayerMask wallCheckLayerMask;
    [SerializeField] private float pushDistance;
    [SerializeField] private float pushSpeed;
    [SerializeField] private Vector3[] pushDirections;

    private BoxCollider collider;
    private PhysicsMover physicsMover;
    private bool isSubmerged;
    private Vector3 fallVelocity;
    private bool isGrounded;
    private float groundDistance;

    private bool interactionMovementInProgress;
    private Vector3 interactionTargetPosition;

    private RaycastHit[] groundCheckBuffer;

    private void Start()
    {
        collider = GetComponent<BoxCollider>();

        physicsMover = GetComponent<PhysicsMover>();
        physicsMover.MoverController = this;

        groundCheckBuffer = new RaycastHit[16];
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        isSubmerged = Physics.CheckSphere(transform.position + floatingCheckPlaneOffset, 0.01f, floatingCheckLayerMask);

        int raycastHitCount = Physics.RaycastNonAlloc(transform.position + raycastOffset, Vector3.down, groundCheckBuffer, Mathf.Max(Mathf.Abs(fallVelocity.y), 0.1f), groundCheckLayerMask);
        if (raycastHitCount > 0)
        {
            // Find closest hit
            float currentClosestDistance = float.PositiveInfinity;
            int currentClosestIndex = 0;

            for (int i = 0; i < raycastHitCount; i++)
            {
                RaycastHit hit = groundCheckBuffer[i];
                if (hit.distance < currentClosestDistance)
                {
                    currentClosestDistance = hit.distance;
                    currentClosestIndex = i;
                }
            }

            isGrounded = currentClosestDistance <= 0.1f;
            groundDistance = currentClosestDistance;
        }
        else
        {
            isGrounded = false;
            groundDistance = float.PositiveInfinity;
        }

        Vector3 position = transform.position;

        if (interactionMovementInProgress)
        {
            float remainingDistance = (interactionTargetPosition - transform.position).magnitude;
            if (remainingDistance < pushSpeed * Time.fixedDeltaTime)
            {
                position = interactionTargetPosition;
                interactionMovementInProgress = false;
            }
            else
            {
                position += (interactionTargetPosition - transform.position).normalized * pushSpeed * Time.fixedDeltaTime;
            }
        }

        if (isGrounded || isSubmerged)
        {
            fallVelocity = Vector3.zero;
        }

        if (isGrounded && isSubmerged)
        {
            position = new Vector3(position.x, Mathf.Max(currentWaterLevel, position.y + floatingCheckPlaneOffset.y - groundDistance), position.z);
        }

        if (!isGrounded && !isSubmerged)
        {
            fallVelocity += gravity * Time.fixedDeltaTime;

            if (Mathf.Abs(fallVelocity.y) < groundDistance)
            {
                position += fallVelocity;
            }
            else
            {
                position = new Vector3(position.x, position.y - groundDistance, position.z);
            }
        }

        if (!isGrounded && isSubmerged)
        {
            position = new Vector3(position.x, currentWaterLevel, position.z);
        }

        goalPosition = position;
        goalRotation = transform.rotation;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position + floatingCheckPlaneOffset, new Vector3(2, 0.01f, 2));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + raycastOffset, transform.position + raycastOffset + gravity);
    }

    public void Focus(InteractionContext context)
    {
        rendererObject.layer = Constants.FOCUSED_INTERACTABLE_LAYER;
    }

    public void Unfocus(InteractionContext context)
    {
        rendererObject.layer = Constants.INTERACTABLE_LAYER;
    }

    public void Interact(InteractionContext context)
    {
        if (interactionMovementInProgress || !isGrounded || isSubmerged) return;

        Vector3 bestDirection;
        Vector3 playerPosition = context.player.transform.position;
        Vector3 checkVector = transform.position - playerPosition;

        float bestAngle = float.PositiveInfinity;
        int bestIndex = 0;
        for (int i = 0; i < pushDirections.Length; i++)
        {
            float angle = Vector3.Angle(checkVector, pushDirections[i]);

            if (angle < bestAngle)
            {
                bestAngle = angle;
                bestIndex = i;
            }
        }

        Vector3 targetDirection = pushDirections[bestIndex];

        RaycastHit raycastResult;
        bool hitSomething = Physics.Raycast(transform.position, targetDirection, (collider.size.x / 2) + (pushDistance * 0.99f), wallCheckLayerMask);

        if (!hitSomething)
        {
            interactionTargetPosition = transform.position + (targetDirection * pushDistance);
            interactionMovementInProgress = true;
        }
    }
}
