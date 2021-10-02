using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CarCameraController : MonoBehaviour
{
    
    [SerializeField] private Camera cam; //The child camera of the car
    [SerializeField] private Transform target; //The car
    [SerializeField] private float distanceToTarget = 10;
    private Vector3 previousPosition;

    void Update()
    {
        //true only on the first frame during which the left mouse button is clicked 
        if (Input.GetMouseButtonDown(0))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        //true always if the left mouse button is being pressed
        else if (Input.GetMouseButton(0))
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;

            float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
            float rotationAroundXAxis = direction.y * 180; // camera moves vertically

            //Move camera along with car position
            cam.transform.position = target.position;

            //Change the rotation of the camera around of the car based of the mouse input
            cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);

            cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

            previousPosition = newPosition;
        }

        //If mouse wheel is moving upwards then zoom the camera by decreasing the field of view
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (cam.fieldOfView > 1)
            {
                cam.fieldOfView--;
            }
        }
        //If mouse wheel is moving downwards then unzoom the camera by increasing the field of view
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (cam.fieldOfView < 100)
            {
                cam.fieldOfView++;
            }
        }
    }
}