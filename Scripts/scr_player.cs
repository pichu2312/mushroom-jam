using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class scr_player : MonoBehaviour
{
    [SerializeField] public bool DEBUG; 
    //Input actions
    public InputActionAsset Actions;
    private InputAction Move;
    private InputAction Jump;
    private InputAction Interact;

    //Refs to other objects
    public GameObject cameraObject;

    //Components
    public Rigidbody2D rigidbody;
    private BoxCollider2D collider;

    //Movement
    //Vector2 newPosition;
    float currentVelocity;
    //Values to change
    [SerializeField] float acceleration = 0.1f;
    [SerializeField] float maxSpeed = 4f;

    [SerializeField] float jumpAmount = 4f;
    [SerializeField] float friction = 1f;
    [SerializeField] float airFriction = 0.5f;

    [SerializeField] float distanceFromGround = 1f;
    [SerializeField] float jumpForce = 1.5f;

    //Mode
    //Jogging is the same as sprinting, except you go a bit faster
    public enum Mode
    {
        Walking, Jogging, Sprinting
    }
    public Mode mode = Mode.Walking;
    private float timeWalking;
    private bool continusMove;

    [SerializeField] float walkToJog = 3f;
    [SerializeField] float jogToSprint = 3f;

    
    [SerializeField] float walkSpeed = 1f;
    [SerializeField] float jogSpeed = 3f;






    //Jumping
    bool jump = false;
    bool jumpHeld = false;
    [SerializeField] private float fallLongMult = 0.85f;
    [SerializeField] private float fallShortMult = 1.55f;
    public LayerMask groundLayer;


    //Inputs
    Vector2 controlMoving;
    bool controlJumping;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Actions.Enable();

        //Assign inputactions
        Move = Actions.FindAction("Movement");
        Jump = Actions.FindAction("Jump");
        Interact = Actions.FindAction("Interact");


        //Assign components
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();

    }

    // Update is called once per frame
    void Update()
    {
        CameraControls();
        GatherInputs();
    }

    void FixedUpdate()
    {
        Movement();
    }

    private void CameraControls()
    {

        Vector3 playerPosition = transform.position;
        Vector3 cameraPosition = cameraObject.transform.position;

        cameraObject.transform.position = new Vector3(
            playerPosition.x,
            playerPosition.y,
            cameraPosition.z
        );

    }

    private void GatherInputs()
    {
        //Left right stuff
        controlMoving = Move.ReadValue<Vector2>();

        if (Move.WasPressedThisFrame())
        {
            timeWalking = 0;
            if (Math.Abs(controlMoving.x) < 0.1f)
            {
                mode = Mode.Walking;
            }
        }


        //Jump stuff
        if (Jump.WasPressedThisFrame()) {
            bool grounded = IsGrounded();
            jump = grounded && Jump.WasPressedThisFrame();
            jumpHeld = true;
        }

        if (jumpHeld)
        {
            if (Jump.WasReleasedThisFrame())
            {
                jumpHeld = false;
            }
        }


    }

    private void Movement()
    {
        if (Mathf.Abs(controlMoving.x) < 0.1f)
        {
            //currentVelocity = Mathf.Round(Mathf.Lerp(rigidbody.linearVelocityX, 0f, friction * Time.deltaTime) * 100) / 100;
            currentVelocity = Mathf.Lerp(rigidbody.linearVelocityX, 0f, friction * Time.fixedDeltaTime);

            // Stop tiny residual velocities
            if (Mathf.Abs(currentVelocity) < 0.05f)
            {
                currentVelocity = 0f;
            }
        }
        else
        {
            //Update how long we've held a key for
            timeWalking += Time.fixedDeltaTime;

            switch (mode)
            {
                case Mode.Walking:
                currentVelocity = walkSpeed * controlMoving.x * Time.fixedDeltaTime;
                if (timeWalking >= walkToJog)
                {
                    mode = Mode.Jogging;
                }
                break;
                case Mode.Jogging:
                currentVelocity = jogSpeed * controlMoving.x * Time.fixedDeltaTime;

                break;
                case Mode.Sprinting:
                break;
            }
        }

        
        //Apply speed
        rigidbody.linearVelocityX = currentVelocity;
    }

    private void OLDMovement()
    {
        //transform.position = newPosition;
        //Movement
        //If no input, trend towards 0
        if (Mathf.Abs(controlMoving.x) < 0.1f)
        {
            //currentVelocity = Mathf.Round(Mathf.Lerp(rigidbody.linearVelocityX, 0f, friction * Time.deltaTime) * 100) / 100;
            currentVelocity = Mathf.Lerp(rigidbody.linearVelocityX, 0f, friction * Time.fixedDeltaTime);

            // Stop tiny residual velocities
            if (Mathf.Abs(currentVelocity) < 0.05f)
            {
                currentVelocity = 0f;
            }
        }
        else
        {
            // Input exists: accelerate towards the target velocity
            float targetVelocity = controlMoving.x * maxSpeed;

            float currentAcceleration = acceleration;

            // Apply stronger deceleration when reversing direction
            if ((Mathf.Sign(controlMoving.x) != Mathf.Sign(rigidbody.linearVelocityX)) && (rigidbody.linearVelocityX != 0))
            {
                currentAcceleration = airFriction;
            }

            currentVelocity = Mathf.Lerp(
                rigidbody.linearVelocityX,
                targetVelocity,
                currentAcceleration * Time.fixedDeltaTime
            );
        }




        

        // Jumping
        if (jump)
        {
            rigidbody.linearVelocityY = jumpAmount;
            jump = false;

            //Add some speed too
            currentVelocity *= jumpForce;
        }

        // Jumping High...
        if (jumpHeld && rigidbody.linearVelocityY > 0)
        {
            rigidbody.linearVelocityY += Physics2D.gravity.y * (fallLongMult - 1) * Time.fixedDeltaTime;
        }
        // Jumping Low...
        else if (!jumpHeld && rigidbody.linearVelocityY > 0)
        {
            rigidbody.linearVelocityY += Physics2D.gravity.y * (fallShortMult - 1) * Time.fixedDeltaTime;
        }


        //Apply speed
        rigidbody.linearVelocityX = currentVelocity;

    }

    bool IsGrounded() {
        float halfColliderTransform = collider.size.x/2;
        return Physics2D.Raycast(transform.position + new Vector3(halfColliderTransform,0), Vector2.down, distanceFromGround, groundLayer) || 
        Physics2D.Raycast(transform.position - new Vector3(halfColliderTransform, 0), Vector2.down, distanceFromGround, groundLayer);
    }

}
