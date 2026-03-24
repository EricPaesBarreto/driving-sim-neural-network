using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarDriving : MonoBehaviour
{
    // car's properties
    //private
    private Rigidbody2D rb;

    // public
    public float Mass = 1;
    public float Horsepower = 1;
    public GameObject Wheel;


    //methods
    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.mass = Mass;
    }

    public void Accelerate(InputAction.CallbackContext context)
    {
        // Accelerate the car "forwards"
        Debug.Log($"Accelerate: {context}");
        if (context.performed)
        {
            rb.AddForce(this.transform.rotation * Vector2.up * Horsepower);
        }
    }

    public void Brake(InputAction.CallbackContext context)
    {
        // Descelerate the car
        if (context.performed)
        {
            // do stuff
        }
    }
}
