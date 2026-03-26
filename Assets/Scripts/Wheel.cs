using UnityEngine;

public class Wheel : MonoBehaviour
{
    public bool IsFrontWheel; // connected to the steering
    public bool IsDriveWheel; // connected to the engine
    public float DriveForce; // the amount of rotational force applied to the wheel 

    [SerializeField] private float maxDriveForce = 1500f; // the maximum drive force that can be applied to the wheel
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float forwardGrip = 1.0f;
    [SerializeField] private float sidewaysGrip = 4.0f;
    [SerializeField] private float maxSteeringAngle = 35.0f; // degrees, most cars are between 30 and 40
    private float steeringInput = 0.0f; // a continuous value between -1 and 1, representing ( full-left: -1, center: 0, full-right: 1 )
    private float currentSteeringAngle = 0.0f; // the current angle of the wheel, used for smooth steering transitions
    private float surfaceGrip = 1f;
    private Rigidbody2D rb;

    void Awake()
    {
        // gets the rigidbody of the car
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // apply the drive force to the wheel if it's a drive wheel
        if (IsDriveWheel)
        {
            ApplyDriveForce();
        }

        // apply the brake force to the wheel if it's braking
        if (brakeForce > 0)
        {
            ApplyBrakeForce();
        }

        // apply grip forces to the wheel to simulate friction and prevent slipping
        ApplyGripForces();

        // update the steering angle of the wheel if it's a front wheel
        if (IsFrontWheel)
        {
            UpdateSteeringAngle();
        }
    }

    void ApplyDriveForce()
    {
        // calculate the drive force based on the input and the maximum drive force
        DriveForce = steeringInput * maxDriveForce;

        // apply the drive force to the wheel in the direction it's facing
        Vector2 forceDirection = transform.up; // assuming the wheel's forward direction is along its local up axis
        rb.AddForceAtPosition(forceDirection * DriveForce, transform.position);
    }

    void ApplyBrakeForce()
    {
        // apply a braking force opposite to the wheel's current velocity
        Vector2 brakeDirection = -rb.GetPointVelocity(transform.position).normalized; // opposite to the wheel's velocity
        rb.AddForceAtPosition(brakeDirection * brakeForce, transform.position);
    }

    void UpdateSteeringAngle()
    {
        // calculate the target steering angle based on the input and the maximum steering angle
        float targetSteeringAngle = steeringInput * maxSteeringAngle;

        // smoothly transition to the target steering angle for more realistic steering behavior
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle, Time.fixedDeltaTime * 5f); // 5f is a smoothing factor

        // apply the steering angle to the wheel's rotation
        transform.localRotation = Quaternion.Euler(0, 0, currentSteeringAngle);
    }
}
