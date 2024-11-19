using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    //Speed rate of the mouse sensitivity
    public float sensX;
    public float sensY;

    //Location of the object ingame (invisible)
    public Transform orientation;

    //Camera Movement of the X axis and Y axis
    float xRotation;
    float yRotation;

    private void Start()
    {
        //Ensure that the cursor is locked in the middle of the screen
        Cursor.lockState = CursorLockMode.Locked;
        //The cursor is also invisible
        Cursor.visible = false;
    }

    private void Update()
    {
        //Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        //Code makes it so the camera is restricted to continously do a 360 degree while looking up or down
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate Camera and Orientation
        //Rotate the Camera along the X and Y axis
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        //Rotate the player along the Y axis
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }



}
