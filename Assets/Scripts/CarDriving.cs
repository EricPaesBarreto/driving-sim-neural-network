using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CarDriving : MonoBehaviour
{
    // car's properties
    // private
    private Rigidbody2D rb;
    private bool accelerating = false;
    private bool braking = false;
    private bool reversing = false;
    private GameObject[] wheels = new GameObject[4];// GameObject references
    private Wheel[] frontWheels = new Wheel[2];     // Script reference
    private Wheel[] rearWheels = new Wheel[2];      // Script references
    private GameObject frontAxel;
    private GameObject rearAxel;

    // magic numbers for the wheel offsets from the center of the car, used to spawn the wheels in the correct position
    private Dictionary<string, float> wheelOffsets = new Dictionary<string, float>()
    {
        {"left_right", 1.2f},
        {"front_back", 1.82f}
    };

    // serialized fields
    [SerializeField] private float mass = 1;
    [SerializeField] private float engineForce = 1;
    [SerializeField] private float brakeStrength = 0.3f;
    [SerializeField] private float minSwitchSpeed = 1;
    [SerializeField] private float maxSteeringAngle = 40; // degrees, most cars are between 30 and 40

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

        // spawn the wheels and assign them to the front and back wheel arrays
        CreateWheels();
        CreateAxels();
    }

    private void Update()
    {
        // apply forces to the car based on the input
        if (accelerating)
        {
            Accelerate();
        }
        if (braking)
        {
            Brake();
        }
        if (reversing)
        {
            Reverse();
        }
    }

    private void FixedUpdate()
    {
        // apply friction to the car
        ApplyFriction();
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
                wheels[fb * 2 + lr] = Instantiate(
                    Wheel, 
                    transform.position + wheelOffset, 
                    transform.rotation, 
                    this.transform);

                wheels[fb * 2 + lr].name = "Wheel " + (fb % 2 == 0? "Front " : "Rear ") + "-" + (lr % 2 == 0? "Left" : "Right");
            }
        }

        // assign the front and rear wheels to their respective arrays for easier access later
        frontWheels[0] = wheels[0].GetComponent<Wheel>();
        frontWheels[1] = wheels[1].GetComponent<Wheel>();
        rearWheels[0] = wheels[2].GetComponent<Wheel>();
        rearWheels[1] = wheels[3].GetComponent<Wheel>();
    }

    private void CreateAxels()
    {
        // AFTER creating the wheels
        // spawned from top to bottom

        // create the front axel
        frontAxel = Instantiate(
            Axel, 
            transform.position, 
            transform.rotation, 
            this.transform);
        frontAxel.name = "Front Axel";

        // create the rear axel
        rearAxel = Instantiate(
            Axel, 
            transform.position, 
            transform.rotation, 
            this.transform);
        rearAxel.name = "Rear Axel";

        // change the position of the front axel's linerenderer points to match the position of the front wheels
        LineRenderer frontAxelLineRenderer = frontAxel.GetComponent<LineRenderer>();
        frontAxelLineRenderer.SetPosition(0, frontWheels[0].transform.position / 2);
        frontAxelLineRenderer.SetPosition(1, frontWheels[1].transform.position / 2);

        // change the position of the rear axel's linerenderer points to match the position of the rear wheels
        LineRenderer rearAxelLineRenderer = rearAxel.GetComponent<LineRenderer>();
        rearAxelLineRenderer.SetPosition(0, rearWheels[0].transform.position / 2);
        rearAxelLineRenderer.SetPosition(1, rearWheels[1].transform.position / 2);
    }
    // input methods
    public void AccelerateInput(InputAction.CallbackContext context)
    {
        // checks if the car is currently moving forwards or backwards, to prevent reversing while accelerating and vice versa
        float direction = Vector3.Dot(rb.linearVelocity, rb.transform.rotation * Vector2.up);

        if ((context.started || context.performed) // only start accelerating if the input is started or performed, not when it's canceled
            && !reversing   // cannot accelerate while reversing
            && !braking     // cannot accelerate while braking
            )
        {
            if(direction >= 0 || rb.linearVelocity.magnitude < minSwitchSpeed)
            {
                // checks if the car is currently moving forwards or backwards
                accelerating = true;
                Debug.Log("Started accelerating");
            }
            else
            {
                // cannot accelerate while moving backwards
                // apply brakes instead to decelerate the car before accelerating forwards
                // BrakeInput(context);
            }

        }
        else if (context.canceled)
        {
            accelerating = false;
            Debug.Log("Stopped accelerating");
        }
    }

    public void ReverseInput(InputAction.CallbackContext context)
    {
        // store the direction of the car's movement in relation to its forward direction, to prevent reversing while accelerating and vice versa
        int direction = (Vector3.Dot(rb.linearVelocity, rb.transform.rotation * Vector2.up) > 0)? 1 : -1;

        if ((context.started || context.performed) // only start reversing if the input is started or performed, not when it's canceled
            && !braking         // cannot reverse while braking
            && !accelerating)   // cannot reverse while accelerating
        {
            if (direction <= 0 || rb.linearVelocity.magnitude < minSwitchSpeed)
            {
                // checks if the car is currently moving forwards or backwards
                reversing = true;
                Debug.Log("Started reversing");
            }
            else
            {
                // cannot reverse while moving forwards
                // apply brakes instead to decelerate the car before reversing
                // BrakeInput(context);
            }
        }
        else if (context.canceled)
        {
            reversing = false;
            Debug.Log("Stopped reversing");
        }
    }

    public void BrakeInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            braking = true;
            Debug.Log("Started braking");
        }
        else if (context.canceled)
        {
            braking = false;
            Debug.Log("Stopped braking");
        }
    }

    public void TurnLeftInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            Debug.Log("Started turning left");
            // tell the wheels to do something
        }
        else if (context.canceled)
        {
            Debug.Log("Stopped turning left");
            // tell the wheels to do something
        }
    }
    
    public void TurnRightInput(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            Debug.Log("Started turning right");
            // tell the wheels to do something
        }
        else if (context.canceled)
        {
            Debug.Log("Stopped turning right");
            // tell the wheels to do something
        }
    }

    // physics methods --> to be changed to be called from the wheel scripts instead of the car script, since the wheels are the ones that apply forces to the car
    public void Accelerate()
    {
        // Accelerate the car "forwards"
        // go through each wheel and update its drive force based on the input and the maximum drive force
        for (int i = 0; i < 2; i++)
        {
            frontWheels[i].DriveForce = engineForce;
            rearWheels[i].DriveForce = engineForce;
        }
    }
    
    public void Reverse()
    {
        // Accelerate the car "backwards"
        for (int i = 0; i < 2; i++)
        {
            frontWheels[i].DriveForce = engineForce;
            rearWheels[i].DriveForce = engineForce;
        }
    }

    public void Brake()
    {
        // Descelerate the car
        ApplyFriction(brakeStrength);
    }

    private void ApplyForce(Vector3 force, ForceMode2D mode = ForceMode2D.Force)
    {
        rb.AddForce(force, mode);
    }

    private void ApplyFriction(float frictionCoefficient = 0.7f)
    {
        // apply friction to the car
        if (rb.linearVelocity.magnitude > 0)
        {
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            rb.AddForce(frontAxel.transform.rotation * Vector2.down * rb.linearVelocity * frictionCoefficient);
        }
    }
}
