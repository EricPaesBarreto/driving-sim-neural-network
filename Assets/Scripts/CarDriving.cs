using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarDriving : MonoBehaviour
{
    // car's properties
    // public
    public GameObject Tire;
    public float Acceleration_speed = 3; // metres / second^2
    public float Braking_speed = 5;
    public float mass = 5;

    //methods
    private void Accelerate()
    {
        GetComponent<Collider2D>().attachedRigidbody.AddForce(this.transform.rotation * Vector2.up * Acceleration_speed * Time.deltaTime);
    }

    private void Brake()
    {
        if (GetComponent<Collider2D>().attachedRigidbody.totalForce.magnitude > Braking_speed)
        {
            GetComponent<Collider2D>().attachedRigidbody.AddForce(this.transform.rotation * Vector2.down * Braking_speed * Time.deltaTime);
        }
        else
        {
            GetComponent<Collider2D>().attachedRigidbody.AddForce(this.transform.rotation * Vector2.down * Acceleration_speed * Time.deltaTime);
        }
        
    }

    void Update()
    {
        if (Input.GetKey("w"))
        {
            Accelerate();
        }

        if (Input.GetKey("s"))
        {
            Brake();
        }
    }
}
