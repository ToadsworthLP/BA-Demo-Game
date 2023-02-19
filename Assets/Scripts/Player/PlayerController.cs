using KinematicCharacterController;
using UnityEngine;

public class PlayerController : MonoBehaviour, ICharacterController
{
    public PlayerInput input { get; private set; }

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerInteraction interactionTrigger;

    [Header("Stable Movement")]
    [SerializeField] private float MaxStableMoveSpeed = 10f;
    [SerializeField] private float StableMovementSharpness = 15;
    [SerializeField] private float OrientationSharpness = 10;

    [Header("Air Movement")]
    [SerializeField] private float MaxAirMoveSpeed = 10f;
    [SerializeField] private float AirAccelerationSpeed = 5f;
    [SerializeField] private float Drag = 0.1f;

    [Header("Physics")]
    [SerializeField] private Vector3 Gravity = new Vector3(0, -30f, 0);

    private Vector3 movementInput;
    private Vector3 lookDirection;

    private KinematicCharacterMotor motor;

    private void Start()
    {
        motor = GetComponent<KinematicCharacterMotor>();
        motor.CharacterController = this;

        input = new PlayerInput();
        input.Enable();

        input.Main.Interact.performed += interactionTrigger.OnInteract;
    }

    private void FixedUpdate()
    {
        // Read input from input system
        Vector2 rawMovementInput = input.Main.Move.ReadValue<Vector2>();

        // Clamp input
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(rawMovementInput.x, 0, rawMovementInput.y), 1f);

        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.rotation * Vector3.forward, motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(cameraTransform.rotation * Vector3.up, motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, motor.CharacterUp);

        // Move and look inputs
        movementInput = cameraPlanarRotation * moveInputVector;
        lookDirection = movementInput;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (lookDirection != Vector3.zero && OrientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(motor.CharacterForward, lookDirection, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, motor.CharacterUp);
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 targetMovementVelocity = Vector3.zero;
        if (motor.GroundingStatus.IsStableOnGround)
        {
            // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(movementInput, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(motor.GroundingStatus.GroundNormal, inputRight).normalized * movementInput.magnitude;
            targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
        }
        else
        {
            // Add move input
            if (movementInput.sqrMagnitude > 0f)
            {
                targetMovementVelocity = movementInput * MaxAirMoveSpeed;

                // Prevent climbing on un-stable slopes with air movement
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal), motor.CharacterUp).normalized;
                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                }

                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
            }

            // Gravity
            currentVelocity += Gravity * deltaTime;

            // Drag
            currentVelocity *= 1f / (1f + (Drag * deltaTime));
        }
    }

    public void AfterCharacterUpdate(float deltaTime) { }

    public void BeforeCharacterUpdate(float deltaTime) { }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider) { }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    public void PostGroundingUpdate(float deltaTime) { }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

}
