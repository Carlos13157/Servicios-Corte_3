using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour{
    private CharacterController charCtrl;
    private PlayerCam camCtrl;
    private WeaponManager weaponManager;

    // private GameMenuManager gameMenuManager;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpCrouchAction;
    
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;
    public float jumpForce;
    public float runMultiplier = 2;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;
    public bool allowGravity = true;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    float horizontalInput;
    float verticalInput;

    private readonly float gravity = -9.81f;

    private Vector3 velocity;

    private void Start(){
        readyToJump = true;

        playerInput = GetComponent<PlayerInput>();
        charCtrl = GetComponent<CharacterController>();
        camCtrl = GetComponent<PlayerCam>();
        weaponManager = GetComponent<WeaponManager>();

        playerHeight = transform.GetComponentInChildren<CapsuleCollider>().height;
        // gameMenuManager = GetComponent<GameMenuManager>();       

        moveAction = playerInput.actions.FindAction("Move");
        jumpCrouchAction = playerInput.actions.FindAction("JumpCrouch");
        runAction = playerInput.actions.FindAction("Run");
    }

    private void Update(){
        if(allowGravity){
            Fall();
        }
        
        MyInput();
        // SpeedControl();
    }

    private void FixedUpdate(){
        MovePlayer();
    }

    private void MyInput(){
    Vector2 moveInput = moveAction.ReadValue<Vector2>();
    verticalInput = moveInput.y;
    horizontalInput = moveInput.x;

    float jumpCrouchValue = jumpCrouchAction.ReadValue<float>();

    if (jumpCrouchValue > 0){
        if (allowGravity){
            // if (readyToJump && grounded){
            //     readyToJump = false;
            //     Jump();
            //     Invoke(nameof(ResetJump), jumpCooldown);
            // }

            if(grounded){
                Jump();
            }
        }else{
            Fly();
        }
    }else if (jumpCrouchValue < 0){
        if (allowGravity && grounded){
            // Acciones para agacharse (si se implementa)
            // Crouch(); (Agregar lógica si se requiere)
        }else{
            Land();
        }
    }else{
        if(!allowGravity){
            velocity.y = 0;
        }
    }
}

    private void MovePlayer(){
        Vector3 moveDirection;

        if (allowGravity){
            moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        }else{
            moveDirection = Camera.main.transform.rotation * new Vector3(horizontalInput, 0, verticalInput);
        }

        if (runAction.ReadValue<float>() > 0){
            // Debug.Log("Running");
            moveDirection *= runMultiplier;
        }      

        charCtrl.Move(moveSpeed * Time.deltaTime * moveDirection);

        charCtrl.Move(velocity * Time.deltaTime);
    }

    private void SpeedControl(){
        Vector3 flatVel = new(velocity.x, 0f, velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed){
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            velocity = new(limitedVel.x, velocity.y, limitedVel.z);
        }
    }

    private void Jump(){
        // Debug.Log("Jumping");
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    private void ResetJump(){
        readyToJump = true;
    }

    private void Fly(){
        // Debug.Log("Flying");
        velocity.y = moveSpeed;
    }

    private void Land(){
        // Debug.Log("Landing");
        velocity.y = -moveSpeed;
    }

    private void Fall(){
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight/2, whatIsGround);
        if(grounded && velocity.y < 0){
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    // private void OnOpenUI(){
    //     var menu = gameMenuManager.Menu;
        
    //     menu.SetActive(!menu.activeSelf);
        
    //     camCtrl.CanUpdate = !menu.activeSelf;
    // }

    public void OnCycleWeapons(InputValue inputValue){
        var auxIndex = (int)inputValue.Get<float>() >= 1 ? 1 : -1;
        weaponManager.CycleWeapons(auxIndex);
    }
}