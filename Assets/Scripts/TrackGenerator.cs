using JetBrains.Annotations;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrackGenerator : MonoBehaviour
{
    // first we need to create some randomly generated points, 
    // then what we can do is shoot them out in a random direction and magnitude, 
    // we repeat pushing them outwards in the same direction incrementaly 
    // until the track fits within a minimum and maximum size range

    // I call this algorithm, uhh, the "paes-distribution method"

    // now we need some parameters:
    // target length
    [SerializeField] private float targetLength;
    [SerializeField] private float lengthVariation; // ammount of variation allowed in length before acceptance.
    [SerializeField] private int numberOfPoints; // number of points generated
    [SerializeField] private int maximumWidth;
    [SerializeField] private int maximumHeight;
    public LineRenderer lineRenderer; // temporary, used to draw the connections between the points for development purposes
    private Vector3 center; // center of the world (from which the points will be pushed)
    private Vector3[] targetPoints;

    // debugging / development
    public GameObject PointRepresentation;
    private GameObject[] visualPoints; // used for debuggin / development
    private bool visualisePoints;

    public TrackGenerator()
    {
        center = new Vector3(0,0,0);
        // development
        visualisePoints = false;
    }

    private void GenerateRandomPoints()
    {
        targetPoints = new Vector3[numberOfPoints];
        visualPoints = new GameObject[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            // generate random point coordinates around center point
            float xPosition = Random.Range(center.x - (maximumWidth / 2), center.x + (maximumWidth / 2));
            Debug.Log(xPosition);
            float yPosition = Random.Range(center.y - (maximumHeight/2), center.x + (maximumHeight/2));
            Debug.Log(yPosition);

            // assign new point to array
            targetPoints[i] = new Vector3(xPosition, yPosition, 0);
        }
    }

    private void VisualisePoints()
    {
        DeleteVisualPoints();

        for (int i = 0; i < numberOfPoints; i++)
        {
            visualPoints[i] = GameObject.Instantiate(PointRepresentation);
        }
    }

    private void DeleteVisualPoints()
    {
        for ( int i = 0; i < numberOfPoints; i++ )
        {
            // deletes existing points
            if (visualPoints[i])
            {
                GameObject.Destroy(visualPoints[i]);       
            }
        }
    }

    public void VisualisePointsInput(InputAction.CallbackContext context)
    {
        Debug.Log("VisualisePointsInput");
        ToggleVisualisation();
        if (visualisePoints)
        {
            VisualisePoints();
        }
        else
        {
            DeleteVisualPoints();
        }
    }

    public void GenerateNewPointsInput(InputAction.CallbackContext context)
    {
        Debug.Log("GenerateNewPointsInput");
        GenerateRandomPoints();
        if (visualisePoints)
        {
            GenerateRandomPoints();
        }
    }

    private void ToggleVisualisation()
    {
        visualisePoints = !visualisePoints;
    }
}
