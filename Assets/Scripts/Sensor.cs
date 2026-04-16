using UnityEngine;
using System;

public class Sensor : MonoBehaviour
{
    [SerializeField] private int NoRays = 7;
    [SerializeField] private float MaxRayLength = Mathf.Infinity;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private bool debugMode = false; // toggle for ray visualization and logging
    private LayerMask roadSideLayerMask;
    private float[] rayDistances;
    private float angleBetweenRays;
    private float StartAngle;
    private Rigidbody2D rb; // assigned by car script

    void Awake()
    {
        InstantiateValues();
    }

    void Update()
    {
        if (rb.linearVelocity.magnitude > 0.05f) // only draw rays when the car is moving, otherwise the rays will be inaccurate and distracting
        {
            DrawRays(ShowRays: debugMode, logRays: debugMode);   
        }
        else if (debugMode)
        {
            DebugRays();
        }
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        InstantiateValues();
    }
    #endif

    private void InstantiateValues()
    {
        // constraints
        fieldOfView = Mathf.Clamp(fieldOfView, 0, 360); // weird stuff happens after 360
        NoRays = Mathf.Clamp(NoRays, 2, 360); // performance
        MaxRayLength = Mathf.Clamp(MaxRayLength, 0.1f, Mathf.Infinity); // no negative ray lengths

        // layer mask for side of road
        roadSideLayerMask = LayerMask.GetMask("RoadSide");

        // distance array
        rayDistances = new float[NoRays];

        // angle between rays
        angleBetweenRays = fieldOfView / (NoRays - 1);

        // angle offset from front of sensor
        StartAngle = -(fieldOfView / 2);

        // calculate initial ray distances
        DrawRays();
        if (debugMode)
        {
            DebugRays();
        }
    }

    public void SetRigidbody(Rigidbody2D rigidbody)
    {
        rb = rigidbody;
    }

    private void DrawRays(bool ShowRays = false, bool logRays = false)
    {
        for(int i = 0; i < NoRays; i++)
        {
            // calculate the direction vector
            Vector2 rayDirection = Quaternion.Euler(0, 0, StartAngle + (i * angleBetweenRays)) * transform.up;
            // define a ray per angle, check for hits against side of road
            RaycastHit2D ray = Physics2D.Raycast(
                transform.position,
                rayDirection,
                MaxRayLength,
                roadSideLayerMask
            );

            // append the distance to the rayDistances array depending on hit
            rayDistances[i] = ray.collider != null ? ray.distance : MaxRayLength;

            // debug (visualization)
            if (ShowRays)
            {
                Debug.DrawRay(transform.position, rayDirection * rayDistances[i], Color.red);
            }
        }
        
        if (logRays)
        {
            string raysString = "Ray Distances";
            for(int i = 0; i < NoRays; i++)
            {
                raysString += $"; {rayDistances[i]}";
            }
            UnityEngine.Debug.Log(raysString);
        }
    }

    void DebugRays() // can be costly
    {
        // store string to log ray distances to console
        string raysString = "Ray Distances";

        // iterate through distance values
        for (int i = 0; i < NoRays; i++)
        {
            // visualise
            Vector2 rayDirection = Quaternion.Euler(0, 0, StartAngle + (i * angleBetweenRays)) * transform.up;
            Debug.DrawRay(transform.position, rayDirection * rayDistances[i], Color.red);

            // append to string
            raysString += $"; {rayDistances[i]}";
        }

        // debug log values
        UnityEngine.Debug.Log(raysString);
    }
}
