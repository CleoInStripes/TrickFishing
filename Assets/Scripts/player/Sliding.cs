using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform playerObj;
    private Rigidbody rb;
    private playerMovement pm;
    public Camera cam;

    private float camFOVStart;
    public float camFOVEnd;

    [Header("Sliding")]
    [Tooltip("The amount of time that you can slide for")]
    public float maxSlideTime;
    [Tooltip("omph of the slide. Feels more forceful than speed")]
    public float slideForce;
    float slideTimer;

    [Tooltip("The size that you shrink to upon sliding")]
    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        camFOVStart = cam.fieldOfView;

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<playerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        if (Input.GetKeyDown(slideKey))
        {
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && pm.isSliding && !pm.underObject)
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
        rb.AddForce(Vector3.down * 5.5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        var cam = PlayerCam.Instance.cam;
        Vector3 inputDirection = playerObj.transform.forward * verticalInput + playerObj.transform.right * horizontalInput;

        //sliding normal style
        if(!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            if (!pm.underObject)
            {
                slideTimer -= Time.deltaTime;
            }
        }
        //sliding down a slope
        else
        {
            Debug.Log("Slope detected");
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0 && !pm.underObject)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        pm.isSliding = false;

        rb.position = rb.position + new Vector3(0, (2.06f * 1.55f) / 2, 0);
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
        
    }
}
