using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// Keys:
///	wasd / arrows	- movement
///	q/e 			- up/down (local space)
///	r/f 			- up/down (world space)
///	pageup/pagedown	- up/down (world space)
///	hold shift		- enable fast movement mode
///	right mouse  	- enable free look
///	mouse			- free look / rotation

public class FreeLookCamera : MonoBehaviour
{
    /// Normal speed of camera movement.
    public float movementSpeed = 10f;

    /// Speed of camera movement when shift is held down,
    public float fastMovementSpeed = 100f;

    /// Sensitivity for free look.
    public float freeLookSensitivity = 3f;


    /// Amount to zoom the camera when using the mouse wheel.
    public float zoomSensitivity = 10f;


    /// Amount to zoom the camera when using the mouse wheel (fast mode).
    public float fastZoomSensitivity = 50f;


    /// Set to true when free looking (on right mouse button).
    private bool looking = false;

    void Update()
    {
        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

        //If A or left arrow key pressed then move camera left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
        }
        //If D or right arrow key pressed then move camera right
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime);
        }
        //If W or Up arrow key pressed then move camera forward
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
        }
        //If S or Down arrow key pressed then move camera backwards
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
        }
        //If Q key is pressed move the camera up (in local space)
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime);
        }

        //If E key is pressed move the camera down (in local space)
        if (Input.GetKey(KeyCode.E))
        {
            transform.position = transform.position + (-transform.up * movementSpeed * Time.deltaTime);
        }
        //If R key is pressed move the camera up (in world space)
        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
        {
            transform.position = transform.position + (Vector3.up * movementSpeed * Time.deltaTime);
        }
        //If F key is pressed move the camera down (in world space)
        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
        {
            transform.position = transform.position + (-Vector3.up * movementSpeed * Time.deltaTime);
        }

        //If user is looking with the mouse then change the camera rotation based on the mouse axis values
        if (looking)
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        //Based on the mouse scroll wheel change the camera position
        float axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
            transform.position = transform.position + transform.forward * axis * zoomSensitivity;
        }

        //If mouse button is pressed then start looking (start changing the camera rotation based of the mouse axis)
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartLooking();
        }
        //If the mouse button is released then stop looking (stop changing the camera rotation)
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            StopLooking();
        }
    }

    void OnDisable()
    {
        StopLooking();
    }

    /// Enable free looking.
    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    /// Disable free looking.
    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
