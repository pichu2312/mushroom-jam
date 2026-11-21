using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class scr_player : MonoBehaviour
{
    //Input actions
    public InputActionAsset Actions;
    private InputAction Move;
    private InputAction Jump;
    private InputAction Interact;

    //Refs to other objects
    public GameObject cameraObject;

    //Components
    private Rigidbody2D rigidbody;
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
        //transform.position = newPosition;
        //Movement
        //If not input, trend towards 0
        if (Mathf.Abs(controlMoving.x) < 0.1f)
        {
            //currentVelocity = Mathf.Round(Mathf.Lerp(rigidbody.linearVelocityX, 0f, friction * Time.deltaTime) * 100) / 100;
            currentVelocity = Mathf.Lerp(rigidbody.linearVelocityX, 0f, friction * Time.fixedDeltaTime);
        }
        //If our speed is greater than maxSpeed, trend back towards maxspeed
        else if (Math.Abs(rigidbody.linearVelocityX) > maxSpeed)
        {
            currentVelocity = Mathf.Lerp(rigidbody.linearVelocityX, Mathf.Sign(rigidbody.linearVelocityX) * maxSpeed, airFriction * Time.fixedDeltaTime);
        }
        else 
        {
            //currentVelocity = Mathf.Round(Mathf.Clamp(rigidbody.linearVelocityX + (acceleration * Time.deltaTime * controlMoving.x), -maxSpeed, maxSpeed) * 100) / 100f;
            currentVelocity = Mathf.Clamp(rigidbody.linearVelocityX + (acceleration * Time.fixedDeltaTime * controlMoving.x), -maxSpeed, maxSpeed);
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
