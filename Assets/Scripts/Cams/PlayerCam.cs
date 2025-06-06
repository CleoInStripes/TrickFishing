using UnityEngine;

public class PlayerCam : SingletonMonoBehaviour<PlayerCam>
{
    public Transform cameraPosition;

    public float mouseSenseX = 80f;
    public float mouseSenseY;

    float xRotation = 0f;
    float yRotation = 0f;

    public Transform playerOrientation;

    public Camera cam;

    private new void Awake()
    {
        base.Awake();
        cam = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        //lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        //connect camera to player view
        transform.position = cameraPosition.position;
    }
}
