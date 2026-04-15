using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CarDriving : MonoBehaviour
{
    // car's properties
    // private
    private Rigidbody2D rb;
    private float _steeringInput = 0f; // the current steering angle of the car, used to smoothly transition between steering angles when turning
    private float _steeringTarget = 0f; // the target steering angle of the car, set by the input methods, -1 --> full left, 0 --> straight, 1 --> full right
    private GameObject[] _wheels = new GameObject[4];// GameObject references
    private Wheel[] _frontWheels = new Wheel[2];     // Script reference
    private Wheel[] _rearWheels = new Wheel[2];      // Script references
    private GameObject _frontAxel;
    private GameObject _rearAxel;
    private float _throttleInput = 0f; // the current throttle input, used to smoothly transition between throttle values when accelerating and decelerating
    private float _throttleTarget = 0f; // the target throttle input, set by the

    // magic numbers for the wheel offsets from the center of the car, used to spawn the wheels in the correct position
    private Dictionary<string, float> wheelOffsets = new Dictionary<string, float>()
    {
        {"left_right", 1.2f},
        {"front_back", 1.82f}
    };

    // serialized fields
    [SerializeField] private float mass = 20f;
    [SerializeField] private float maxDriveForce = 500f;
    [SerializeField] private float maxReverseDriveForce = 250f;
    [SerializeField] private float maxSwitchSpeed = 1;
    // sudo serialized steering properties
    private float maxSteeringAngle = 35; // degrees, most cars are between 30 and 40
    public float MaxSteeringAngle { get { return maxSteeringAngle; } set { maxSteeringAngle = value; for(int i = 0; i < 2; i++) { _frontWheels[i].SetMaxSteeringAngle(value); } } }
    [SerializeField] private float throttleRiseSpeed = 2f; // how quickly the throttle input rises when accelerating
    [SerializeField] private float throttleFallSpeed = 2f; // how quickly the throttle input falls when decelerating
    [SerializeField] private float angularDampingFactor = 5f;

    // prefabs and reference objects
    [SerializeField] private GameObject Wheel;
    [SerializeField] private GameObject Axel;
    [SerializeField] private Camera CarCamera;


    // base methods
    private void Awake()
    {
        // get the rigidbody component and set its mass
        rb = this.GetComponent<Rigidbody2D>();
        rb.mass = mass;
        rb.angularDamping = angularDampingFactor;

        // spawn the wheels and assign them to the front and back wheel arrays
        CreateWheels();
        CreateAxels();
    }

    private void Update()
    {
        // smooth steering
        InterpolateSteering();

        // use for physics updates --> rigidbody forces, wheel rotation etc...
        
        // deals with updating the throttle input, applying it to the wheels
        // throttle target is controlled by input methods, 0 --> idle, 1 --> full throttle, -1 --> full reverse
        // throttle is interpolated to smoothly transition between throttle values when accelerating and decelerating
        Throttle();

        // updates the steeringInput variable in the wheel script
        UpdateWheelRotation();
    }

    private void FixedUpdate()
    {

    }

    private void CreateWheels()
    {
        // spawned from top left to bottom right
        for (int fb = 0; fb < 2; fb++)
        {
            // front and back wheels
            for (int lr = 0; lr < 2; lr++)
            {
                // left and right wheels offset from the center of the car
                Vector3 wheelOffset = new Vector3(
                    (lr % 2 == 0)? -wheelOffsets["left_right"] : wheelOffsets["left_right"], 
                    (fb % 2 == 0)? wheelOffsets["front_back"] : -wheelOffsets["front_back"], 0);
                
                // instantiate the wheel
                _wheels[fb * 2 + lr] = Instantiate(
                    Wheel, 
                    transform.position + wheelOffset, 
                    transform.rotation, 
                    this.transform);

                    // if the wheel is a front wheel, set it to be a drive wheel
                    if (fb % 2 == 0)
                    {
                        // for now the cars are FWD and the back wheels are idle
                        _wheels[fb * 2 + lr].GetComponent<Wheel>().SetDriveWheel(true);
                        _wheels[fb * 2 + lr].GetComponent<Wheel>().SetFrontWheel(true);
                    }

                _wheels[fb * 2 + lr].name = "Wheel " + (fb % 2 == 0? "Front " : "Rear ") + "- " + (lr % 2 == 0? "Left" : "Right");
            }
        }

        // assign the front and rear wheels to their respective arrays for easier access later
        _frontWheels[0] = _wheels[0].GetComponent<Wheel>();
        _frontWheels[1] = _wheels[1].GetComponent<Wheel>();
        _rearWheels[0] = _wheels[2].GetComponent<Wheel>();
        _rearWheels[1] = _wheels[3].GetComponent<Wheel>();
    }

    private void CreateAxels() // visual for the moment
    {
        // AFTER creating the wheels
        // spawned from top to bottom

        // create the front axel
        _frontAxel = Instantiate(
            Axel, 
            transform.position, 
            transform.rotation, 
            this.transform);
        _frontAxel.name = "Front Axel";

        // create the rear axel
        _rearAxel = Instantiate(
            Axel, 
            transform.position, 
            transform.rotation, 
            this.transform);
        _rearAxel.name = "Rear Axel";

        // change the position of the front axel's linerenderer points to match the position of the front wheels
        LineRenderer _frontAxelLineRenderer = _frontAxel.GetComponent<LineRenderer>();
        _frontAxelLineRenderer.SetPosition(0, _frontWheels[0].transform.position / 2);
        _frontAxelLineRenderer.SetPosition(1, _frontWheels[1].transform.position / 2);

        // change the position of the rear axel's linerenderer points to match the position of the rear wheels
        LineRenderer _rearAxelLineRenderer = _rearAxel.GetComponent<LineRenderer>();
        _rearAxelLineRenderer.SetPosition(0, _rearWheels[0].transform.position / 2);
        _rearAxelLineRenderer.SetPosition(1, _rearWheels[1].transform.position / 2);
    }
    // input methods
    public void AccelerateInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            // accelerate
            _throttleTarget = 1f;
            Debug.Log("Started accelerating");
        }
        else if (context.canceled)
        {
            // idle
            _throttleTarget = 0f;
            Debug.Log("Stopped accelerating");
        }
    }

    public void ReverseInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            // reverse
            _throttleTarget = -1f;
            Debug.Log("Started reversing");

        }
        else if (context.canceled)
        {
            // idle
            _throttleTarget = 0f;
            Debug.Log("Stopped revcersing");

        }
    }

    public void BrakeInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            // brake is applied by the wheel script
            for (int i = 0; i < 2; i++)
            {
                _frontWheels[i].Brake();
                _rearWheels[i].Brake();
            }
            
            Debug.Log("Started braking");
        }
        else if (context.canceled)
        {
            // brake is released by the wheel script
            for (int i = 0; i < 2; i++)
            {
                _frontWheels[i].ReleaseBrake();
                _rearWheels[i].ReleaseBrake();
            }

            Debug.Log("Stopped braking");
        }
    }

    public void TurnLeftInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            _steeringTarget = -1f;
            Debug.Log("Started turning left");
        }
        else if (context.canceled && _steeringTarget < 0f) // make sure to not override right turn input
        {
            _steeringTarget = 0f;
            Debug.Log("Stopped turning left");
        }
    }
    
    public void TurnRightInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            _steeringTarget = 1f;
            Debug.Log("Started turning right");
        }
        else if (context.canceled && _steeringTarget > 0f)  // make sure to not override right turn input
        {
            _steeringTarget = 0f;
            Debug.Log("Stopped turning right");
        }
    }

    // physics methods --> to be changed to be called from the wheel scripts instead of the car script, since the wheels are the ones that apply forces to the car

    public void Throttle()
    {
        // linearly interpolate the value of the throttle input (-1, 0, 1) --> reverse, idle, accelerate
        if (_throttleInput < _throttleTarget)
        {
            _throttleInput += throttleRiseSpeed * Time.deltaTime;
            _throttleInput = Mathf.Min(_throttleInput, _throttleTarget);
        }
        else if (_throttleInput > _throttleTarget)
        {
            _throttleInput -= throttleFallSpeed * Time.deltaTime;
            _throttleInput = Mathf.Max(_throttleInput, _throttleTarget);
        }

        // apply to the wheels
        for (int i = 0; i < 2; i++)
        {
            if (_throttleInput > 0)
            {
                // accelerate
                _frontWheels[i].SetDriveForce(_throttleInput * maxDriveForce); // --> front wheel drive
                // _rearWheels[i].SetDriveForce(_throttleInput * maxDriveForce); // --> rear wheel drive
            }
            else if (_throttleInput < 0)
            {
                // reverse
                _frontWheels[i].SetDriveForce(_throttleInput * maxReverseDriveForce); // --> front wheel drive
                // _rearWheels[i].SetDriveForce(_throttleInput * maxReverseDriveForce); // --> rear wheel drive
            }
            else
            {
                // idle
                _frontWheels[i].SetDriveForce(0); // --> front wheel drive
                // _rearWheels[i].SetDriveForce(0); // --> rear wheel drive
            }
        }
    }

    private void ApplyForce(Vector3 force, ForceMode2D mode = ForceMode2D.Force)
    {
        // currently not used, could be used for applying forces such as air resistance in the future
        rb.AddForce(force, mode);
    }

    private void UpdateWheelRotation()
    {
        if (_steeringInput == 0f) { return; }
        // update the position and rotation of the wheels to match the steering input
        for (int i = 0; i < 2; i++)
        {
            _frontWheels[i].SetSteeringTarget(_steeringInput);
        }
    }
    private void InterpolateSteering()
    {
        // interpolates the steering input value to move smoothly between values.
        _steeringInput = Mathf.Lerp(
            _steeringInput,
            _steeringTarget,
            Time.deltaTime * 5f
        );
    }
}
