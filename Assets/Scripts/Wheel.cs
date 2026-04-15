using UnityEngine;

public class Wheel : MonoBehaviour
{
    public bool IsFrontWheel; // connected to the steering
    public bool IsDriveWheel; // connected to the engine
    private bool _isBraking = false; // whether the brake is currently applied, used to apply brake forces in the FixedUpdate method
    private float _driveForce; // the amount of rotational force applied to the wheel
    float steeringInput = 0f; // the current steering input, used to calculate the target steering angle for the wheel;

    [SerializeField] private float _brakeForce = 100f; // the amount of force applied when braking
    [SerializeField] private float _forwardGrip = 5f; // friction is applied using this
    [SerializeField] private float _sidewaysGrip = 15f; // sideways grip (the car shouldn't slide sideways)
    [SerializeField] private float _maxGripForce = 300f; // the maximum force that can be applied to the wheel before it loses grip and starts sliding
    [SerializeField] private float _maxSteeringAngle = 35.0f; // degrees, most cars are between 30 and 40
    [SerializeField] private float _turnSmoothingFactor = 10f; // a factor for smoothing steering transitions
    private float currentSteeringAngle = 0.0f; // the current angle of the wheel, used for smooth steering transitions
    private Rigidbody2D rb;

    void Awake()
    {
        // gets the rigidbody of the car
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // update the steering angle of the wheel if it's a front wheel
        if (IsFrontWheel)
        {
            UpdateSteeringAngle();
        }

        // apply grip forces to the wheel to simulate friction and prevent slipping
        ApplyFriction();

        // apply the drive force to the wheel if it's a drive wheel
        if(!_isBraking)
        {
            ApplyDriveForce();
        }
    }

    void ApplyDriveForce()
    {
        // apply the drive force to the wheel in the direction it's facing
        if( IsDriveWheel)
        {
            Vector2 forceDirection = transform.up; // assuming the wheel's forward direction is along its local up axis
            rb.AddForceAtPosition(forceDirection * _driveForce, transform.position);
        }
    }

    public void UpdateSteeringAngle()
    {
        // the steering angle as a percentage of the maximum steering angle
        float targetSteeringAngle = -steeringInput * _maxSteeringAngle; // negate steering angle to correct rotation direction

        // smooth transition
        currentSteeringAngle = Mathf.Lerp(
            currentSteeringAngle, 
            targetSteeringAngle, 
            Time.fixedDeltaTime * _turnSmoothingFactor);

        // apply the steering angle to the rotation of the wheel
        transform.localRotation = Quaternion.Euler(0, 0, currentSteeringAngle);
    }

    public void SetSteeringTarget(float input)
    {
        steeringInput = input;
    }
    private void ApplyFriction()
    {
        // get the velocity vector at the position of the wheel
        Vector2 velocity = rb.GetPointVelocity(transform.position);

        // get lateral and longitudinal components of the velocity vector
        float forwardSpeed = Vector2.Dot(velocity, transform.up);
        float sidewaysSpeed = Vector2.Dot(velocity, transform.right);

        // get the vector components of the friction force to apply next
        Vector2 forwardForce = -transform.up * forwardSpeed * _forwardGrip; // "forward" is the direcition the wheel is facing
        Vector2 sidewaysForce = -transform.right * sidewaysSpeed * _sidewaysGrip; // "sideways" is perpendicular to the direction the wheel is facing
        Vector2 brakeVector = Vector2.zero; // named so to avoid confusion with _brakeForce

        // braking force
        if (_isBraking)
        {
            brakeVector = -velocity * _brakeForce;
        }

        // sum of the forces
        Vector2 totalForce = forwardForce + sidewaysForce + brakeVector;

        // force of tires cannot exceed the maximum grip force
        totalForce = Vector2.ClampMagnitude(totalForce, _maxGripForce);

        // apply friction forces to the whel at th the position of the wheel !! NOT the CENTER of the CAR !!
        rb.AddForceAtPosition(totalForce, transform.position);
    }

    public void SetDriveForce(float force)
    {
        // the ammount of force applied to the wheel each update
        _driveForce = force;
    }

    public void Brake()
    {
        _isBraking = true;
    }

    public void ReleaseBrake()
    {
        _isBraking = false;
    }

    // utilities

    public void SetDriveWheel(bool isDriveWheel)
    {
        IsDriveWheel = isDriveWheel;
    }
    public void SetFrontWheel(bool isFrontWheel)
    {
        IsFrontWheel = isFrontWheel;
    }
    public void SetMaxSteeringAngle(float angle)
    {
        _maxSteeringAngle = angle;
    }
}
