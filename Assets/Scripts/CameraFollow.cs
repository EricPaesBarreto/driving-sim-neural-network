using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float CameraFollowSpeed = 0.3f;
    [SerializeField] private Vector3 followOffset = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 cameraOffset = new Vector3(0, 0, -10);

    void FixedUpdate()   
    {
        // fixed update is used to ensure that the camera movement is smooth and consistent
        // as it updates with the physics movement of the car
        MoveCamera();
    }    

    private void MoveCamera()
    {
        // moves the camera to follow the target with a smooth dampening effect
        if(target)
        {
            // prevents exceptions when the target is destroyed

            // set desired position to interpolate towards
            Vector3 desiredPosition = target.position + followOffset + cameraOffset;

            // smoothly move the camera towards the desired position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, CameraFollowSpeed);
        }
    }
}
