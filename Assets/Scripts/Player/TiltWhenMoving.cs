using KinematicCharacterController;
using UnityEngine;

public class TiltWhenMoving : MonoBehaviour
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private float tiltIntensity;

    private void Update()
    {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            transform.localEulerAngles = new Vector3(motor.BaseVelocity.magnitude * tiltIntensity, 0, 0);
        }
    }
}
