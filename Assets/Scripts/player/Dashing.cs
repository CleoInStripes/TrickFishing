using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform playerObj;
    public Transform playerCam;
    private Rigidbody rb;
    private playerMovement pm;

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
    }

    private void ResetDash()
    {
        pm.isdashing = false;
        rb.useGravity = true;

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
