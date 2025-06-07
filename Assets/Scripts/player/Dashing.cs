using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform playerObj;
    public Transform playerCam;
    public Camera cam;
    private Rigidbody rb;
    private playerMovement pm;

    private float camFOVStart;
    public float camFOVEnd;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    [Header("Cooldown")]
    public float dashCooldown;
    private float dashCooldownTimer;

    [Header("Input")]
    public KeyCode dashKey;


    private void Start()
    {
        camFOVStart = cam.fieldOfView;

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<playerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
        {
            Dash();
        }

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        //prevents dash if cooldown timer is still going, otherwise start timer
        if (dashCooldownTimer > 0) return;
        else dashCooldownTimer = dashCooldown;

        pm.isdashing = true;

        Transform forwardT;
        forwardT = playerObj.transform;
        Vector3 direction = GetDirection(forwardT);
        Vector3 forceToApply = playerObj.transform.forward * dashForce + playerObj.transform.up * dashUpwardForce;

        rb.useGravity = false;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);

        //smooth camera zoom?
        float time = 0;
        float camFOVChange = Mathf.Abs(camFOVStart - camFOVEnd);

        while (time < camFOVChange)
        {
            cam.fieldOfView = Mathf.Lerp(camFOVStart, camFOVEnd, time / camFOVChange);

            time += Time.deltaTime;
            Debug.Log(time);

        }
    }

    private void ResetDash()
    {
        pm.isdashing = false;
        rb.useGravity = true;

        //smooth camera zoom?
        float time = 0;
        float camFOVChange = Mathf.Abs(camFOVEnd - camFOVStart); 

        while (time < camFOVChange)
        {
            cam.fieldOfView = Mathf.Lerp(camFOVEnd, camFOVStart, time / camFOVChange);

            time += Time.deltaTime;
            Debug.Log(time);
        }
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;

        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }

}
