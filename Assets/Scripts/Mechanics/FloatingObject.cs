using KinematicCharacterController;
using UnityEngine;

public class FloatingObject : MonoBehaviour, IMoverController, IInteractable
{
    [Header("Ground")]
    [SerializeField] private Vector3 raycastOffset;
    [SerializeField] private LayerMask groundCheckLayerMask;
    [SerializeField] private float groundRaycastDistance;
    [SerializeField] private float ceilingRaycastDistance;

    [Header("Floating")]
    [SerializeField] private FloatContainer currentWaterLevel;
    [SerializeField] private LayerMask floatingCheckLayerMask;

    [Header("Physics")]
    [SerializeField] private Vector3 gravity;

    [Header("Interaction")]
    [SerializeField] private GameObject rendererObject;
    [SerializeField] private LayerMask wallCheckLayerMask;
    [SerializeField] private float pushDistance;
    [SerializeField] private float pushSpeed;
    [SerializeField] private Vector3[] pushDirections;
    [SerializeField] private string stickyFloorTag;

    private BoxCollider collider;
    private PhysicsMover physicsMover;
    private bool isSubmerged;
    private Vector3 fallVelocity;
    private bool isGrounded;
    private float groundDistance;
    private float ceilingDistance;
    private bool isAgainstCeiling;

    private bool interactionMovementInProgress;
    private Vector3 interactionTargetPosition;

    private RaycastHit[] groundCheckBuffer;
    private RaycastHit[] wallCheckBuffer;

    private FloatingObject floatingObjectBelow;
    private FloatingObject floatingObjectAbove;

    private bool hasPanickedThisFrame;

    private void Start()
    {
        collider = GetComponent<BoxCollider>();

        physicsMover = GetComponent<PhysicsMover>();
        physicsMover.MoverController = this;

        groundCheckBuffer = new RaycastHit[16];
        wallCheckBuffer = new RaycastHit[16];
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        isSubmerged = Physics.CheckSphere(transform.position, 0.01f, floatingCheckLayerMask);

        // Ground check
        int raycastHitCount = Physics.BoxCastNonAlloc(transform.position + raycastOffset, new Vector3(collider.size.x, 0.01f, collider.size.z) / 2 * 0.99f, Vector3.down, groundCheckBuffer, transform.rotation, Mathf.Max(Mathf.Abs(fallVelocity.y), groundRaycastDistance), groundCheckLayerMask);
        if (raycastHitCount > 0)
        {
            // Find closest hit that isn't a self-hit
            float currentClosestDistance = float.PositiveInfinity;
            int currentClosestIndex = -1;

            float currentClosestToFloatingObjBelowDistance = float.PositiveInfinity;
            int currentClosestFloatingObjBelowIndex = -1;

            for (int i = 0; i < raycastHitCount; i++)
            {
                RaycastHit hit = groundCheckBuffer[i];

                if (hit.collider.gameObject == gameObject) continue;

                if (hit.distance < currentClosestDistance)
                {
                    currentClosestDistance = hit.distance - raycastOffset.y - (collider.size.y / 2);
                    currentClosestIndex = i;
                }

                if (hit.distance < currentClosestToFloatingObjBelowDistance && groundCheckBuffer[currentClosestIndex].collider.CompareTag(stickyFloorTag))
                {
                    currentClosestToFloatingObjBelowDistance = hit.distance + raycastOffset.y - (collider.size.y / 2);
                    currentClosestFloatingObjBelowIndex = i;
                }
            }

            if (currentClosestIndex >= 0)
            {
                isGrounded = currentClosestDistance <= 0.1f;
                groundDistance = currentClosestDistance;

                if (currentClosestFloatingObjBelowIndex >= 0)
                {
                    if (floatingObjectBelow == null || groundCheckBuffer[currentClosestFloatingObjBelowIndex].transform != floatingObjectBelow.transform)
                        floatingObjectBelow = groundCheckBuffer[currentClosestFloatingObjBelowIndex].transform.GetComponent<FloatingObject>();
                }
                else
                {
                    floatingObjectBelow = null;
                }
            }
            else
            {
                isGrounded = false;
                groundDistance = float.PositiveInfinity;
                floatingObjectBelow = null;
            }
        }
        else
        {
            isGrounded = false;
            groundDistance = float.PositiveInfinity;
            floatingObjectBelow = null;
        }

        // Ceiling check
        raycastHitCount = Physics.BoxCastNonAlloc(transform.position + raycastOffset, new Vector3(collider.size.x, 0.01f, collider.size.z) / 2 * 0.99f, Vector3.up, groundCheckBuffer, transform.rotation, ceilingRaycastDistance, groundCheckLayerMask);
        if (raycastHitCount > 0)
        {
            // Find closest hit that isn't a self-hit
            float currentClosestDistance = float.PositiveInfinity;
            int currentClosestIndex = -1;

            float currentClosestToFloatingObjAboveDistance = float.PositiveInfinity;
            int currentClosestFloatingObjAboveIndex = -1;

            for (int i = 0; i < raycastHitCount; i++)
            {
                RaycastHit hit = groundCheckBuffer[i];

                if (hit.collider.gameObject == gameObject) continue;

                //if (floatingObjectAbove != null && hit.collider.gameObject == floatingObjectAbove.gameObject) continue;

                if (hit.distance < currentClosestDistance)
                {
                    currentClosestDistance = hit.distance + raycastOffset.y - (collider.size.y / 2);
                    currentClosestIndex = i;
                }

                if (hit.distance < currentClosestToFloatingObjAboveDistance && groundCheckBuffer[currentClosestIndex].collider.CompareTag(stickyFloorTag))
                {
                    currentClosestToFloatingObjAboveDistance = hit.distance + raycastOffset.y - (collider.size.y / 2);
                    currentClosestFloatingObjAboveIndex = i;
                }
            }

            if (currentClosestIndex >= 0)
            {
                isAgainstCeiling = currentClosestDistance <= 0.5f;
                ceilingDistance = currentClosestDistance;

                if (currentClosestFloatingObjAboveIndex >= 0)
                {
                    if (floatingObjectAbove == null || groundCheckBuffer[currentClosestFloatingObjAboveIndex].transform != floatingObjectAbove.transform)
                        floatingObjectAbove = groundCheckBuffer[currentClosestFloatingObjAboveIndex].transform.GetComponent<FloatingObject>();
                }
                else
                {
                    floatingObjectAbove = null;
                }
            }
            else
            {
                isAgainstCeiling = false;
                ceilingDistance = float.PositiveInfinity;
                floatingObjectAbove = null;
            }
        }
        else
        {
            isAgainstCeiling = false;
            ceilingDistance = float.PositiveInfinity;
            floatingObjectAbove = null;
        }

        //Debug.Log($"{name}: G {isGrounded} {groundDistance} Fl. below {floatingObjectBelow != null} C {isAgainstCeiling} {ceilingDistance} Fl. above {floatingObjectAbove != null} Sub {isSubmerged}", this);

        Vector3 position = transform.position;

        if (interactionMovementInProgress) // Pushing motion takes precedence over everything else
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

            // Still float up  and down if submerged
            if (!isGrounded && isSubmerged)
            {
                if (isAgainstCeiling && floatingObjectAbove == null)
                {
                    position = new Vector3(position.x, Mathf.Min(currentWaterLevel, position.y + ceilingDistance), position.z);
                }
                else
                {
                    position = new Vector3(position.x, currentWaterLevel, position.z);
                }
            }

            if (isGrounded && isSubmerged)
            {
                if (isAgainstCeiling && floatingObjectAbove == null)
                {
                    position = new Vector3(position.x, Mathf.Min(currentWaterLevel, position.y + ceilingDistance), position.z);
                }
                else
                {
                    position = new Vector3(position.x, Mathf.Max(currentWaterLevel, position.y - groundDistance), position.z);
                }
            }
        }
        else // Do normal physics
        {
            if (floatingObjectBelow != null && floatingObjectBelow == floatingObjectAbove) // Panic if stuck inside another crate and try to get out
            {
                Vector3 abovePos = floatingObjectAbove.transform.position;
                floatingObjectAbove.transform.position = new Vector3(abovePos.x, abovePos.y + floatingObjectBelow.collider.size.y + 0.01f, abovePos.z);

                hasPanickedThisFrame = true;
            }
            else
            {
                hasPanickedThisFrame = false;
            }

            if (floatingObjectBelow == null) // Do normal physics if not glued to another crate below
            {
                if (isGrounded || isSubmerged)
                {
                    fallVelocity = Vector3.zero;
                }

                if (isGrounded && isSubmerged)
                {
                    if (isAgainstCeiling && floatingObjectAbove == null)
                    {
                        position = new Vector3(position.x, Mathf.Min(currentWaterLevel, position.y + ceilingDistance), position.z);
                    }
                    else
                    {
                        position = new Vector3(position.x, Mathf.Max(currentWaterLevel, position.y - groundDistance), position.z);
                    }
                }

                if (isGrounded && !isSubmerged)
                {
                    if (isAgainstCeiling && floatingObjectAbove == null)
                    {
                        position = new Vector3(position.x, Mathf.Min(position.y - groundDistance, position.y + ceilingDistance), position.z);
                    }
                    else
                    {
                        position = new Vector3(position.x, position.y - groundDistance, position.z);
                    }
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
                    if (isAgainstCeiling)
                    {
                        if (floatingObjectAbove == null)
                        {
                            position = new Vector3(position.x, Mathf.Min(currentWaterLevel, position.y + ceilingDistance), position.z);
                        }
                        else
                        {
                            position = new Vector3(position.x, currentWaterLevel, position.z);
                        }
                    }
                    else
                    {
                        position = new Vector3(position.x, currentWaterLevel, position.z);
                    }
                }
            }
            else // Stick to the crate below if there is one, destroy this crate if squished
            {
                position.y = floatingObjectBelow.transform.position.y + floatingObjectBelow.collider.size.y + 0.01f;

                if (isAgainstCeiling && !hasPanickedThisFrame)
                {
                    Destroy(gameObject);
                }
            }

            position = new Vector3(Mathf.RoundToInt(position.x), position.y, Mathf.RoundToInt(position.z));
        }

        goalPosition = position;
        goalRotation = transform.rotation;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position, new Vector3(2, 0.01f, 2));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + raycastOffset, transform.position + raycastOffset + (Vector3.down * groundRaycastDistance));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + raycastOffset, transform.position + raycastOffset + (Vector3.up * ceilingRaycastDistance));
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
        if (interactionMovementInProgress || (!isGrounded && !isSubmerged)) return;

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

        MoveObject(targetDirection, pushDistance);
    }

    private void MoveObject(Vector3 targetDirection, float pushDistance)
    {
        if (CheckMovement(targetDirection, pushDistance))
        {
            interactionTargetPosition = transform.position + (targetDirection * pushDistance);
            interactionMovementInProgress = true;

            if (floatingObjectAbove != null) floatingObjectAbove.MoveObject(targetDirection, pushDistance);
        }
    }

    private bool CheckMovement(Vector3 targetDirection, float pushDistance)
    {
        int hitCount = Physics.BoxCastNonAlloc(transform.position, collider.size / 2 * 0.98f, targetDirection, wallCheckBuffer, transform.rotation, pushDistance, wallCheckLayerMask);
        bool hitSomething = false;

        for (int i = 0; i < hitCount; i++)
        {
            if (wallCheckBuffer[i].transform.gameObject != gameObject)
            {
                hitSomething = true;
                break;
            }
        }

        return !hitSomething;
    }
}
