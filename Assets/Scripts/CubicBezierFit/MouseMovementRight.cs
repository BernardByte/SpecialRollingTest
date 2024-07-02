/**
 * @author Muhammad Imran
 * 06/24/2024
 **/

// This class capture mouse movements. mouse movements (posisitons) are stored in a circular queue.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovementRight : MonoBehaviour
{
    // mouse position holder
    public Vector3 currentMousePosition;
    public Vector3 PreviousMousePosition;
    // circular control points array
    [SerializeField] public const int maxMouseArraySize = 150;
    [SerializeField] public Vector2[] rightCursorMovements = new Vector2[maxMouseArraySize];
    [SerializeField] public Vector2[] rightCursorPositionsInOrder;
    [SerializeField] public int rRearIndex = -1;
    [SerializeField] public int rFrontIndex = -1;


    // for tracking control points. if they exceeds from maxArraySize then delete oldest control point.
    public GameObject[] controlPointSpheres = new GameObject[maxMouseArraySize]; // Array to track control points
    private int currentPointIndex = 0;
    public float sphereRadius = 0.1f; // Radius for the spheres

    /// <summary>
    /// This function behave like Enqueue.
    /// Below funciton uses the features of circular queue.
    /// It updates the 1st parameter behalf on the 2nd parameter.
    /// </summary>
    /// <param name="rightMouseMovements"> Vector2 array</param>
    /// <param name="cursorPosition">Cursor position type of Vector3</param>

    public void AddMousePositionRight(ref Vector2[] rightMouseMovements, Vector3 cursorPosition)
    {

        if ((rRearIndex + 1) % maxMouseArraySize == rFrontIndex)
        {
            DeleteMousePositionRight();
            //Debug.Log("Circular array is full");
            //return;
        }

        if (rFrontIndex == -1)
        {
            rFrontIndex = 0;
        }

        rRearIndex = (rRearIndex + 1) % maxMouseArraySize;
        rightMouseMovements[rRearIndex] = new Vector2(cursorPosition.x, cursorPosition.y);
    }


    /// <summary>
    /// This function behave like dequeue
    /// </summary>
    public void DeleteMousePositionRight()
    {
        if (IsEmptyRight())
        {
            Debug.Log("Cirtualr array is empty. Unable to dequeue.");
        }


        if (rFrontIndex == rRearIndex)
        {
            rFrontIndex = -1;
            rRearIndex = -1;
        }
        else
        {
            rFrontIndex = (rFrontIndex + 1) % maxMouseArraySize;
        }
    }


    /// <summary>
    /// Check if circular queue is empty or not. Return bool value
    /// </summary>
    /// <returns>if index(s) value is -1 return true.</returns>
    public bool IsEmptyRight()
    {
        return rFrontIndex == -1 && rRearIndex == -1;
    }

    // Get all mouse positions

    /// <summary>
    /// This function return Vector2 order array. order is culculated from front index to rear index position.
    /// </summary>
    /// <returns>Vector2 FIFO ordered array.</returns>
    public Vector2[] RightMousePositionInOrder()
    {
        if (IsEmptyRight())
        {
            Debug.Log("Cirtualr array is empty. Unable to dequeue.");
            return new Vector2[0];
        }

        Vector2[] mousePositions = new Vector2[maxMouseArraySize];

        int index = 0;

        for (int i = rFrontIndex; i != rRearIndex; i = (i + 1) % maxMouseArraySize)
        {
            mousePositions[index] = rightCursorMovements[i];
            index++;
        }

        mousePositions[index] = rightCursorMovements[rRearIndex];

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
    //public Vector3 GetMousePosition()
    //{
    //    float mouseXDelta = Input.GetAxis("Mouse X");
    //    float mouseYDelta = Input.GetAxis("Mouse Y");
    //    Vector3 mousePosition = new Vector3(mouseXDelta,mouseYDelta,-10.0f);
    //    mousePosition.z = -10.0f;
    //    Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition);
    //    return newPosition;
    //}

    ///<summary>
    /// Visualize mouse poniter position.
    /// <para name="GameObject">A sprite that will be spawned.</para>
    /// </summary>
    ///
    public void VisualizeMousePosition(GameObject point)
    {
        // Remove old control points if necessary
        while (rightCursorMovements.Length > maxMouseArraySize)
        {
            // Remove the oldest control point
            Destroy(controlPointSpheres[currentPointIndex]);
            controlPointSpheres[currentPointIndex] = null;
            currentPointIndex = (currentPointIndex + 1) % maxMouseArraySize;
          
        }

        // Create spheres at control points
        for (int i = 0; i < rightCursorMovements.Length; i++)
        {
            int sphereIndex = (currentPointIndex + i) % maxMouseArraySize;
            if (controlPointSpheres[sphereIndex] != null)
            {
                Destroy(controlPointSpheres[sphereIndex]);
            }

            
            GameObject sphere = Instantiate(point, rightCursorMovements[i], Quaternion.identity);
            sphere.transform.localScale = Vector2.one * sphereRadius;
            controlPointSpheres[sphereIndex] = sphere;
        }
    }

}
