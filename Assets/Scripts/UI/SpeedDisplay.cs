using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeedDisplay : MonoBehaviour
{
    // This script exists for game testing purposes only
    public TMP_Text textDisplay;

    playerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<playerMovement>();
    }

    void Update()
    {
        textDisplay.text = ("Speed = " + (playerMovement.moveSpeed));
    }
}
