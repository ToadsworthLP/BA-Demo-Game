using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementAcceleration;
    [SerializeField] private float stopAcceleration;
    [SerializeField] private float movementSpeedLimit;
    [SerializeField] private Transform cameraTransform;

    [Header("Ground Checking")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float sphereCastDistance;
    [SerializeField] private Vector3 sphereCastOffset;

    private Rigidbody rb;

    private PlayerInput input;

    private RaycastHit[] groundCheckHitBuffer;

    private bool isOnGround;
    private Vector3 groundNormal;

    private void Start()
    {
        input = new PlayerInput();
        input.Enable();
        groundCheckHitBuffer = new RaycastHit[16];

        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        UpdateGroundState();

        // Move player along ground
        Vector2 movementInput = input.Main.Move.ReadValue<Vector2>();
        if (movementInput.SqrMagnitude() > 0f)
        {
            Vector3 cameraRelativeForward = Vector3.ProjectOnPlane(transform.position - cameraTransform.position, Vector3.up);
            Quaternion cameraRotationCorrection = Quaternion.FromToRotation(Vector3.forward, cameraRelativeForward);

            Vector3 raw3dInput = cameraRotationCorrection * new Vector3(movementInput.x, 0, movementInput.y);
            Vector3 projectedInput = Vector3.ProjectOnPlane(raw3dInput, groundNormal);

            Vector3 finalMovement = projectedInput.normalized * movementAcceleration * movementInput.magnitude * Time.fixedDeltaTime;

            rb.velocity += finalMovement;
        }
        else
        {
            if (rb.velocity.sqrMagnitude > stopAcceleration * Time.fixedDeltaTime)
            {
                rb.velocity -= rb.velocity.normalized * stopAcceleration * Time.fixedDeltaTime;
            }
            else
            {
                rb.velocity = Vector3.zero;
            }
        }

        // Enforce speed limits
        Vector3 projectedSpeed = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
        if (projectedSpeed.magnitude > movementSpeedLimit)
        {
            Vector3 fixedSpeed = projectedSpeed.normalized * movementSpeedLimit;
            rb.velocity = new Vector3(fixedSpeed.x, rb.velocity.y, fixedSpeed.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position + sphereCastOffset, sphereCastRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + sphereCastOffset + (-transform.up * sphereCastDistance), sphereCastRadius);
    }

    private void UpdateGroundState()
    {
        // Ground check
        int hitCount = Physics.SphereCastNonAlloc(transform.position + sphereCastOffset, sphereCastRadius, Vector3.down, groundCheckHitBuffer, sphereCastDistance, groundLayerMask);
        isOnGround = hitCount > 0;

        // Get normal vector of closest hit
        if (hitCount > 0)
        {
            float currentShortestDistance = 0f;
            int currentShortestIndex = 0;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit currentHit = groundCheckHitBuffer[i];
                if (currentShortestDistance > currentHit.distance)
                {
                    currentShortestDistance = currentHit.distance;
                    currentShortestIndex = i;
                }
            }

            groundNormal = groundCheckHitBuffer[currentShortestIndex].normal;
        }
        else
        {
            groundNormal = Vector3.up;
        }
    }
}
