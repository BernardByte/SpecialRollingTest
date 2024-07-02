/**
 * @author Muhammad Imran
 * 06/24/2024
 **/

// This class capture mouse movements. mouse movements (posisitons) are stored in a circular queue.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovements : MonoBehaviour
{
    // mouse position holder
    public Vector3 currentMousePosition;
    public Vector3 PreviousMousePosition;
    // circular control points array
    [SerializeField] public const int maxMouseArraySize = 20;
    [SerializeField] public Vector2[] CursorMovements = new Vector2[maxMouseArraySize];
    [SerializeField] public Vector2[] CursorPositionsInOrder;
    [SerializeField] public int RearIndex = -1;
    [SerializeField] public int FrontIndex = -1;


    // for tracking control points. if they exceeds from maxArraySize then delete oldest control point.
    public GameObject[] controlPointSpheres = new GameObject[maxMouseArraySize]; // Array to track control points
    private int currentPointIndex = 0;
    public float sphereRadius = 1.5f; // Radius for the spheres
    public Transform cursorTransformAsAParent;
    public Transform cursorTransformAsAParentRight;

    /// <summary>
    /// This function behave like Enqueue.
    /// Below funciton uses the features of circular queue.
    /// It updates the 1st parameter behalf on the 2nd parameter.
    /// </summary>
    /// <param name="MouseMovements"> Vector2 array</param>
    /// <param name="cursorPosition">Cursor position type of Vector3</param>

    public void AddMousePosition(ref Vector2[] MouseMovements, Vector3 cursorPosition)
    {

        if ((RearIndex + 1) % maxMouseArraySize == FrontIndex)
        {
            DeleteMousePosition();
            //Debug.Log("Circular array is full");
            //return;
        }

        if (FrontIndex == -1)
        {
            FrontIndex = 0;
        }

        RearIndex = (RearIndex + 1) % maxMouseArraySize;
        MouseMovements[RearIndex] = new Vector2(cursorPosition.x, cursorPosition.y);
    }


    /// <summary>
    /// This function behave like dequeue
    /// </summary>
    public void DeleteMousePosition()
    {
        if (IsEmpty())
        {
            Debug.Log("Cirtualr array is empty. Unable to dequeue.");
        }


        if (FrontIndex == RearIndex)
        {
            FrontIndex = -1;
            RearIndex = -1;
        }
        else
        {
            FrontIndex = (FrontIndex + 1) % maxMouseArraySize;
        }
    }


    /// <summary>
    /// Check if circular queue is empty or not. Return bool value
    /// </summary>
    /// <returns>if index(s) value is -1 return true.</returns>
    public bool IsEmpty()
    {
        return FrontIndex == -1 && RearIndex == -1;
    }

    // Get all mouse positions

    /// <summary>
    /// This function return Vector2 order array. order is culculated from front index to rear index position.
    /// </summary>
    /// <returns>Vector2 FIFO ordered array.</returns>
    public Vector2[] MousePositionInOrder()
    {
        if (IsEmpty())
        {
            //Debug.Log("Cirtualr array is empty. Unable to dequeue.");
            return new Vector2[0];
        }

        Vector2[] mousePositions = new Vector2[maxMouseArraySize];

        int index = 0;

        for (int i = FrontIndex; i != RearIndex; i = (i + 1) % maxMouseArraySize)
        {
            mousePositions[index] = CursorMovements[i];
            index++;
        }

        mousePositions[index] = CursorMovements[RearIndex];

        return mousePositions;
    }


    /// <summary>
    /// Return the mouse position type of Vector3.
    /// </summary>
    /// <returns>Vector3 array.</returns>
    public Vector3 GetMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -10.0f;
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return newPosition;
    }
    

    ///<summary>
    /// Visualize mouse poniter position.
    /// <para name="GameObject">A sprite that will be spawned.</para>
    /// </summary>
    ///
    public void VisualizeMousePosition(GameObject point)
    {
        // Remove old control points if necessary
        while (CursorMovements.Length > maxMouseArraySize)
        {
            // Remove the oldest control point
            Destroy(controlPointSpheres[currentPointIndex]);
            controlPointSpheres[currentPointIndex] = null;
            currentPointIndex = (currentPointIndex + 1) % maxMouseArraySize;
          
        }

        // Create spheres at control points
        for (int i = 0; i < CursorMovements.Length; i++)
        {
            int sphereIndex = (currentPointIndex + i) % maxMouseArraySize;
            if (controlPointSpheres[sphereIndex] != null)
            {
                Destroy(controlPointSpheres[sphereIndex]);
            }

            //Vector3 vectorForMouseTrail = new Vector3(CursorMovements[i].x, CursorMovements[i].y, cursorTransformAsAParent.position.z);
            Vector2 vectorForMouseTrail = new Vector2(CursorMovements[i].x, CursorMovements[i].y);
            //Debug.Log("Mouse Trail Position: " + vectorForMouseTrail);
            GameObject sphere = Instantiate(point, vectorForMouseTrail, Quaternion.identity);
            sphere.transform.SetParent(cursorTransformAsAParent, false);
            sphere.transform.localScale = Vector3.one * sphereRadius;
            controlPointSpheres[sphereIndex] = sphere;
        }
    }

}
