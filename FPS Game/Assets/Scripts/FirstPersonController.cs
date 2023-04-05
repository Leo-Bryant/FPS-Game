using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;

    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 5f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftControl;
    [SerializeField] KeyCode crouchKey = KeyCode.C;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;
    [SerializeField] float crouchDrag = 0.5f;

    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;

    [Header("Gravity")]
    [SerializeField] float gravity = -10;

    [Header("Air Strafing")]
    Vector3 currentOrientation;
    Vector3 currentVelocity;
    Vector3 airAcceleration;



    public bool isGrounded { get; private set; }

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;
    ConstantForce constantForceObject;

    RaycastHit slopeHit;

    public bool isCrouching = false;

    [SerializeField] Transform capsuleTransform;
    [SerializeField] Transform groundCheckTransform;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Awake()
    {
        constantForceObject = GetComponent<ConstantForce>();
        Vector3 gravityVector = new Vector3(0, gravity, 0);
        constantForceObject.force = gravityVector;

    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        MyInput();
        ControlDrag();
        ControlSpeed();
        Crouch();

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(crouchKey))
        {
            Crouch();
        }

        //Air Strafing

        currentOrientation = orientation.forward;
        currentVelocity = rb.velocity;

        if (!isGrounded)
        {
            AirMove();
        }



        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y), ForceMode.Impulse);
        }
    }

    void ControlSpeed()
    {
        if (Input.GetKey(sprintKey) && isGrounded)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
            if (isCrouching)
            {
                rb.drag = crouchDrag;
            }
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }

    void Crouch()
    {
        if (Input.GetKey(crouchKey))
        {
            isCrouching = true;
            capsuleTransform.transform.localScale = new Vector3(rb.transform.localScale.x, rb.transform.localScale.y / 2, rb.transform.localScale.z);
        }

        else
        {
            isCrouching = false;
            capsuleTransform.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void AirMove()
    {

    }
}