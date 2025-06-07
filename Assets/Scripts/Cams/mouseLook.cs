using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class mouseLook : MonoBehaviour
{
    
    public float mouseSenseX;
    public float mouseSenseY;

    float xRotation = 0f;
    float yRotation = 0f;

    void Start()
    {
        //lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public Transform playerOrientation;


    void Update()
    {
        //get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSenseX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSenseY * Time.deltaTime;

        //mouse rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        //xRotation -= mouseY;
        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate Camera and player oreintation
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);


    } 
}
