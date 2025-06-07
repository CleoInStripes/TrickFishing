using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform playerObj;
    private Rigidbody rb;
    private playerMovement pm;

    [Header("Sliding")]
    [Tooltip("The amount of time that you can slide for")]
    public float maxSlideTime;
    [Tooltip("omph of the slide. Feels more forceful than speed")]
    public float slideForce;
    private float slideTimer;

    [Tooltip("The size that you shrink to upon sliding")]
    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<playerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A + D keys
        verticalInput = Input.GetAxisRaw("Vertical"); //W + S keys

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && pm.isSliding)
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if (pm.isSliding)
        {
            SlidingMovement();
        }
    }

    private void StartSlide()
    {
        pm.isSliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        var cam = PlayerCam.Instance.cam;
        Vector3 inputDirection = cam.transform.forward * verticalInput + cam.transform.right * horizontalInput;

        //sliding normal style
        if(!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        //sliding down a slope
        else
        {
            Debug.Log("Slope detected");
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        pm.isSliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
