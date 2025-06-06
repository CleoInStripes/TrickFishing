using UnityEngine;

public class PlayerCam : SingletonMonoBehaviour<PlayerCam>
{
    public Transform cameraPosition;
    void Update()
    {
        //connect camera to player view
        transform.position = cameraPosition.position;
    }
}
