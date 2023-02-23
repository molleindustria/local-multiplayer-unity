using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Tooltip("Normal speed")]
    public float movementSpeed = 10;

    [Tooltip("Rotation speed if not instant")]
    public float rotationSpeed = 100;

    [Tooltip("Sprint speed")]
    public float sprintSpeed = 10.0f;

    [Tooltip("Look/rotation sensitivity for first person movement")]
    public float lookSensitivity = 5.0f;

    [Tooltip("The height the player can jump")]
    public float jumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15.0f;

    [Tooltip("The angle the player can look up and down")]
    public float upDownRange = 60.0f;

    //the analog values read from the controller
    public Vector2 leftStick = Vector2.zero;
    public Vector2 rightStick = Vector2.zero;

    //a complex component that facilitates the control of a character
    public CharacterController controller;

    public PlayerInput playerInput;

    public float animationSpeed = 1;
    public float animationSprintSpeed = 2;

    private bool sprinting = false;
    private bool jumpedThisFrame = false;
    private float verticalVelocity = 0;
    //for FPS camera controls
    private float lookVelocity = 0.0f;
    private float lookSmoothing = 0.3f;

    public Game game;

    public Animator animator;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        //find the "brain" and notify it of the new player
        game = FindObjectOfType<Game>();


        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();


        if (game != null)
        {
            //pass a reference to this script
            game.PlayerJoined(this);
        }
    }

    void Update()
    {
        //uncomment only one at the time! 

        SingleStickMovement();

        //FirstPersonMovement();

        //TwinStickMovement();

        //PlayerRelativeMovement();



        //basic example of controlling an animated character
        if (animator != null && controller.velocity.magnitude > 0.1f)
        {
            if (controller.isGrounded)
            {
                //change the playing speed based on 
                if (sprinting)
                    animator.speed = animationSprintSpeed;
                else
                    animator.speed = animationSpeed;

                animator.Play("Walk");
            }
            else
            {
                //if jumping freeze the walking animation at time 0
                //I didn't have time to make a jumping animation
                animator.Play("Walk", 0, 0);
                animator.speed = 0;
            }
        }
        else
        {
            animator.speed = 1;
            animator.Play("Idle");
        }
    }


    //left stick determines the movement, character faces the direction
    void SingleStickMovement()
    {
        float targetSpeed = movementSpeed;

        //change speed based on sprint input 
        if (sprinting)
        {
            targetSpeed = sprintSpeed;
        }

        //factor gravity
        verticalVelocity += gravity * Time.deltaTime;

        if (controller.isGrounded && jumpedThisFrame)
        {
            //calculate the jump velocity based on the desired jump height
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //combining the left stick input and the vertical velocity
        //absolute coordinates movement: up means +z in the world, left means -x
        Vector3 movement = new Vector3(leftStick.x * targetSpeed, verticalVelocity, leftStick.y * targetSpeed);

        //since it's in update and continuous the vector has to be multiplied by Time.deltaTime to be frame independent
        controller.Move(movement * Time.deltaTime);

        //absolute rotation: controller rotates in the direction of the stick
        //Keep the rotation if the stick is not being moved 
        //magnitude is the lenght of the vector, small magnitude = stick in the dead zone 
        if (leftStick.magnitude > 0.1f)
        {
            Vector3 lookDirection = new Vector3(leftStick.x, 0, leftStick.y);
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        //end of the frame reset the bool
        jumpedThisFrame = false;
    }

    //left stick controls thrust, right direction based on the player orientation, think vehicle control in 3d person
    void PlayerRelativeMovement()
    {
        //factor gravity
        verticalVelocity += gravity * Time.deltaTime;

        if (controller.isGrounded && jumpedThisFrame)
        {
            //calculate the jump velocity based on the desired jump height
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //incremental rotation: right stick horizontal increases the rotation 
        transform.Rotate(0, rightStick.x * Time.deltaTime * rotationSpeed, 0);

        Vector3 speed = new Vector3(0, verticalVelocity, leftStick.y * movementSpeed);

        //relative movement: up means move in the direction the player is facing
        speed = transform.rotation * speed;

        controller.Move(speed * Time.deltaTime);

        //end of the frame reset the bool
        jumpedThisFrame = false;
    }




    //left stick movement, right stick rotation, independent from each other. Think Robotron, Binding of Isaac, Overcooked 
    void TwinStickMovement()
    {
        float targetSpeed = movementSpeed;

        //change speed based on sprint input 
        if (sprinting)
        {
            targetSpeed = sprintSpeed;
        }

        //factor gravity
        verticalVelocity += gravity * Time.deltaTime;

        if (controller.isGrounded && jumpedThisFrame)
        {
            //calculate the jump velocity based on the desired jump height
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //combining the left stick input and the vertical velocity
        //absolute coordinates movement: up means +z in the world, left means -x
        Vector3 movement = new Vector3(leftStick.x * targetSpeed, verticalVelocity, leftStick.y * targetSpeed);

        //since it's in update and continuous the vector has to be multiplied by Time.deltaTime to be frame independent
        controller.Move(movement * Time.deltaTime);

        //absolute rotation: controller rotates in the direction of the stick
        //Keep the rotation if the stick is not being moved 
        //magnitude is the lenght of the vector, small magnitude = stick in the dead zone 
        if (rightStick.magnitude > 0.1f)
        {
            Vector3 lookDirection = new Vector3(rightStick.x, 0, rightStick.y);
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        //end of the frame reset the bool
        jumpedThisFrame = false;
    }




    //simple FPS style with jump and dash: left stick movement/strafe right stick rotation/look
    void FirstPersonMovement()
    {
        //incremental rotation: right stick horizontal increases the rotation 
        transform.Rotate(0, rightStick.x * Time.deltaTime * rotationSpeed, 0);

        //looking up and down: multiply the look range by the stick position (-1 to 1)
        float targetLookAngle = -rightStick.y * upDownRange;


        if (playerInput.camera != null)
        {
            //smoothing the up down look so it's not to sudden
            float smoothedLookAngle = Mathf.SmoothDampAngle(playerInput.camera.transform.localRotation.eulerAngles.x, targetLookAngle, ref lookVelocity, lookSmoothing);

            //apply to x axis of the camera
            playerInput.camera.transform.localRotation = Quaternion.Euler(smoothedLookAngle, 0, 0);
        }

        float targetSpeed = movementSpeed;


        if (sprinting)
        {
            targetSpeed = sprintSpeed;
        }


        // Movement
        float forwardSpeed = leftStick.y * targetSpeed;
        float sideSpeed = leftStick.x * targetSpeed;

        //gravity/jump
        verticalVelocity += gravity * Time.deltaTime;

        if (controller.isGrounded && jumpedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); ;
        }

        Vector3 speed = new Vector3(sideSpeed, verticalVelocity, forwardSpeed);

        speed = transform.rotation * speed;

        controller.Move(speed * Time.deltaTime);

        //end of the frame reset the bool
        jumpedThisFrame = false;
    }


    //it's probably useful for this script to know what team this player belongs (if any)
    //in this case it's only used to change the color of the placeholder graphics
    public void ChangeColor(Color c)
    {
        //this depends on the player model, quick & dirty solution: get all the renderers 
        //and change the color of their first material
        SkinnedMeshRenderer[] smr = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer s in smr)
            s.material.color = c;

        Renderer[] mr = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in mr)
            r.material.color = c;

    }

    /*
     * Every time an Input Action fires, the Player Input Component will trigger functions 
     * on the same game object that match the name of that Action, prefixed with “On”.
     */

    //this is the proper way to name an input, after the action so it can be remapped for different devices
    public void OnKick()
    {
        print("Kick action");
    }

    //this is a less proper naming but more intuitive if you are used to just check an axis
    void OnLeftStickMove(InputValue value)
    {
        leftStick = value.Get<Vector2>();
    }

    void OnRightStickMove(InputValue value)
    {
        rightStick = value.Get<Vector2>();
    }


    //button action called with value 
    //check the control scheme to see the configuration needed to detect press and release
    public void OnSprint(InputValue value)
    {
        //horrible but that's one of the many ways to do it
        if (value.Get<float>() >= 0.5f)
        {
            print("sprint pressed");
            sprinting = true;
        }
        else
        {
            print("sprint released");
            sprinting = false;
        }
    }


    //just another way to use an input setting a temporary boolean 
    //that I can use in the update and that gets reset at the end of it
    public void OnJump()
    {
        jumpedThisFrame = true;
    }


}
