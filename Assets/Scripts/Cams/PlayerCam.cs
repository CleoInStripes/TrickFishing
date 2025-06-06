using UnityEngine;

public class PlayerCam : SingletonMonoBehaviour<PlayerCam>
{
    public Transform cameraPosition;

    [HideInInspector] public Camera cam;

    private new void Awake()
    {
        base.Awake();
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        //connect camera to player view
        transform.position = cameraPosition.position;
    }
}
