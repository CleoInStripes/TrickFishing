using UnityEngine;

//public class Jumping : MonoBehaviour
//{
//    [Header("References")]
//    public Transform playerObj;
//    private Rigidbody rb;
//    private playerMovement pm;

//    [Header("Jumping")]
//    [Tooltip("Power of the jump up")]
//    public float jumpForce;
//    [Tooltip("Speed while off the ground")]
//    public float airSpeedMultiplier;
//    [Tooltip("Time that you must wait before jumping again")]
//    public float jumpCooldown;
//    bool readyToJump = true;
//    bool readyToDoubleJump = false;

//    [Header(]

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        readyToJump = true;
//        readyToDoubleJump = false;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // START JUMP //
//        if (Input.GetKey(jumpKey) && readyToJump && grounded && !underObject)
//        {
//            readyToJump = false;

//            Jump();

//            Invoke(nameof(ResetJump), jumpCooldown);
//        }
//}
