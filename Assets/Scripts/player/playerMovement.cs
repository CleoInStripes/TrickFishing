using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class playerMovement : MonoBehaviour
{

    [Header("References")] //
    public Transform playerObj;

    [Header("Movement")] //
    private float moveSpeed = 12f;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float dashSpeed;

    [Tooltip("How much the player drags when on the ground (prevents slippery movement)")]
    public float groundDrag;
    private float startYScale;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    [Tooltip("Increase in speed over time while moving")]
    public float speedIncreaseMultiplier;
    [Tooltip("Increase in speed when going down a slope")]
    public float slopeIncreaseMultiplier;

    [Header("Jumping")] //
    [Tooltip("Power of the jump up")]
    public float jumpForce;
    [Tooltip("Speed while off the ground")]
    public float airSpeedMultiplier;
    [Tooltip("Time that you must wait before jumping again")]
    public float jumpCooldown;
    bool readyToJump = true;
    int jumpCounter;
    [Tooltip("The amount of times that you can jump")]
    public int maxJumpCounter = 2;
    [Tooltip("A little extra oomph when you double jump")]
    public float doubleJumpForce;
    private float timeBetweenJumps = 0.2f;
    bool isDoubleJumpReady;

    [Header("Above Head Check")]
    [Tooltip("Whats above you")]
    public LayerMask aboveHeadCheck;
    public Transform aboveHeadCheckObj;
    public bool underObject;

    [Header("Ground Check")]
    [Tooltip("The height of the player. Can't you read?")]
    public float playerHeight;
    [Tooltip("Whats below you")]
    public LayerMask groundCheck;
    bool grounded;

    [Header("Slope Standing")]
    [Tooltip("The steepest angle that youre allowed to stand on")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Keybinds")]
    public KeyCode jumpKey;
    public KeyCode sprintKey;
    public KeyCode crouchKey;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState 
    { 
        walking,
        sprinting,
        crouching,
        sliding,
        dashing,
        air
    }

    public bool isOnSlope;

    public bool isSliding;
    
    public bool isdashing;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        isOnSlope = false;

        readyToJump = true;
        isDoubleJumpReady = false;
        jumpCounter = 0;
        maxJumpCounter = 2;

        //save the default scale of the player
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        if (!PlayerModel.Instance.allowInput)
        {
            return;
        }

        //player rotate
        var cam = PlayerCam.Instance.cam;
        playerObj.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight + 0.3f, groundCheck);

        //activate drag if grounded
        if (state == MovementState.walking || state == MovementState.sprinting)
        {
            rb.linearDamping = groundDrag;
            rb.mass = 1;
        }
        else
        {
            rb.linearDamping = 0;

            if (state == MovementState.air)
            {
                rb.mass = 2;
            }
        }

        //above head check
        underObject = Physics.Raycast(aboveHeadCheckObj.position, Vector3.up, 1.5f, aboveHeadCheck);
        if (underObject)
        {
            Debug.Log("There is something above my head!");
        }

        PlayerInput();
        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate()
    {
        Schmoovement();
    }

    // PLAYER INPUT //

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // JUMPING //
        if(Input.GetKeyDown(jumpKey) && readyToJump && grounded && !underObject && jumpCounter <= 0)
        {
            readyToJump = false;

            jumpCounter += 1;

            //abandon slope physics
            exitingSlope = true;

            //reset y velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            //A big burst of force into the air!
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);

            //gives a short pause before the double jump can activate
            isDoubleJumpReady = false;
            Invoke(nameof(WaitToJumpAgain), timeBetweenJumps);
        }

        //Double (or triple) jump
        if (Input.GetKeyDown(jumpKey) && !readyToJump && (jumpCounter >= 1) && (jumpCounter < maxJumpCounter) && isDoubleJumpReady)
        {
            jumpCounter += 1;

            //reset y velocity
            //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            //A big burst of force into the air!
            rb.AddForce(transform.up * jumpForce * doubleJumpForce, ForceMode.Impulse);

            //Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void WaitToJumpAgain()
    {
        isDoubleJumpReady = true;
    }

    private void ResetJump()
    {
        exitingSlope = false;
        readyToJump = true;

        jumpCounter = 0;
    }

    // STATE HANDLER //
    private void StateHandler()
    {
        
        if (isSliding) // Mode - Sliding
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        else if (isdashing) //Mode - Dashing
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
        }
        else if (Input.GetKey(sprintKey) && grounded)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded) //Mode - Walking
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else if (Input.GetKey(sprintKey) && !grounded) // Mode - Sprinting on air
        {
            state = MovementState.air;
            desiredMoveSpeed = sprintSpeed;
        }
        else //Mode - Air
        {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
                desiredMoveSpeed = walkSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        //check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(LerpMoveSpeed());
        }
        else
        {
            StopAllCoroutines();
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    // SPEED LERP //
    private IEnumerator LerpMoveSpeed()
    {
        //lerp movement speed back to walk speed after speeding up real fast
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;


        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * (slopeIncreaseMultiplier * slopeAngleIncrease);
            }
            else
            {
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time/difference);

                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }
    }

    // WALKING //
    private void Schmoovement()
    {
        if (state == MovementState.dashing) return;

        isOnSlope = false;

        var cam = PlayerCam.Instance.cam;
        //calculate movement direction, so you always move in the direction you are looking
        moveDirection = playerObj.transform.forward * verticalInput + playerObj.transform.right * horizontalInput;

        //on slope
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 10f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 8f, ForceMode.Force);
            }
        }
        else if (grounded) //on ground
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);
        }
        else if(!grounded) //in air
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airSpeedMultiplier, ForceMode.Force);

        //turn off gravity while on a slope
        rb.useGravity = !OnSlope();

    }


    // MAX SPEED LIMIT //
    private void SpeedControl()
    {
        //limit speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //limit spped on ground or air to keep people from going crazy
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    // SLOPES //
    public bool OnSlope()
    {
        isOnSlope = false;

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.3f + 2))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }


    // 


}

