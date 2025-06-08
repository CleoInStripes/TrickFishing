using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class mouseLook : MonoBehaviour
{
    public float xRotation = 0f;
    public float yRotation = 0f;

    void Start()
    {
        //lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public Transform playerOrientation;


    void Update()
    {
        if (!PlayerModel.Instance.allowInput)
        {
            return;
        }

        //get mouse input
        float mouseX = Input.GetAxis("Mouse X") * GameManager.Instance.mouseSensitivity.x * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * GameManager.Instance.mouseSensitivity.y * Time.deltaTime;

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
