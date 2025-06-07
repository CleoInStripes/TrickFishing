using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{

    [Header("Movement")]
    private float moveSpeed = 12f;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpMultiplier;
    public float jumpCooldown;
    bool readyToJump = true;
    bool readyToDoubleJump = false;

    //i dont want the player to crouch. This is getting removed later
    //I am keeping it as a temporary replacement to ground sliding
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundCheck;
    bool grounded;

    [Header("Slope Standing")]
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
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        readyToDoubleJump = false;

        //save the default scale of the player
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        //ground check. 
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight + 0.2f, groundCheck);

        //activate drag if grounded. Drag works but leaves much to be desired
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        PlayerInput();
        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate()
    {
        Schmoovement();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //double jump. //double jump is not working. I will come back to it/
        /*
        if (Input.GetKey(jumpKey))
        {
            //double jump
            if (readyToDoubleJump && !readyToJump && !grounded)
            {
                readyToDoubleJump = false;

                Jump();

                return;

                //Invoke(nameof(ResetJump), doubleJumpCoolDown);
            }
            else if (readyToJump && grounded)
            {
                readyToJump = false;
                readyToDoubleJump = true;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
                return;
            }
        }
        */

        // START JUMP //
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        } 

        // CROUCHING //
        //start crouch 
        if (Input.GetKeyDown(crouchKey))
        {
            //shrink down player
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            //a small push down so the shrunken player hits the ground
            rb.AddForce(Vector3.down * 4f, ForceMode.Impulse);
        }

        //stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

    }

    // STATE HANDLER //
    private void StateHandler()
    {
        //Mode - Crouching
        if(Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        //Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        //Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    
    // WALKING //
    private void Schmoovement()
    {
        var cam = PlayerCam.Instance.cam;

        //calculate movement direction, so you always move in the direction you are looking
        moveDirection = cam.transform.forward * verticalInput + cam.transform.right * horizontalInput;

        //on slope
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 8f, ForceMode.Force);
            }
        }

        //on ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);
        }
        
        //in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * jumpMultiplier, ForceMode.Force);

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


    // JUMPING //
    public void Jump()
    {
        exitingSlope = true;

        //reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        exitingSlope = false;

        readyToJump = true;
        readyToDoubleJump = false;
    }

    private void ResetDoubleJump()
    {
        readyToDoubleJump = true;
    }

    // SLOPES //
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

}

