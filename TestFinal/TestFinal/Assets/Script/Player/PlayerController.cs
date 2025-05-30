using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : NetworkBehaviour, IHealth
{
    private CharacterController charCtrl;
    private PlayerCam camCtrl;
    private WeaponManager weaponManager;
    [SerializeField] private GameObject playerCanvasPrefab;
    private GameObject playerCanvasInstance;


    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction jumpCrouchAction;
    [Header("Health")]

    [Networked, OnChangedRender(nameof(HealthChanged))]
    public float NetworkedHealth { get; set; } = 100;

    [Header("Movement")]
    public float moveSpeed = 2f;

    public float groundDrag;
    public float jumpForce;
    public float runMultiplier = 2;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;

    private float GroundedOffset;

    public bool allowGravity = true;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    float horizontalInput;
    float verticalInput;

    private readonly float gravity = -9.81f*2;

    private Vector3 velocity;

    private Vector2 move = new();

    private bool isAlive = true;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        charCtrl = GetComponent<CharacterController>();
        camCtrl = Camera.main.GetComponent<PlayerCam>();
        weaponManager = GetComponent<WeaponManager>();
        playerHeight = transform.GetComponentInChildren<CapsuleCollider>().height;

        moveAction = playerInput.actions.FindAction("Move");
        jumpCrouchAction = playerInput.actions.FindAction("JumpCrouch");
        runAction = playerInput.actions.FindAction("Run");

        var uiManagerPrefab = playerCanvasPrefab.GetComponent<UIManager>();
        // if (!uiManagerPrefab.hasAuthority) return;

        var canvasInstance = Instantiate(playerCanvasPrefab);
        var uiManager = canvasInstance.GetComponent<UIManager>();

        weaponManager.OnCurrentWeaponBulletsChanged += uiManager.UpdateCurrentBullets;
        weaponManager.OnCurrentWeaponReserveChanged += uiManager.UpdateCurrentReserve;
        weaponManager.OnCurrentWeaponAmmoTypeChanged += uiManager.UpdateAmmoType;
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            camCtrl.Target = transform.GetChild(0).transform;
            camCtrl.playerInput = GetComponent<PlayerInput>();
        }
    }

    void Update(){
        GroundedCheck();
    }

    public override void FixedUpdateNetwork()
    {
        // Quaternion cameraRotationY = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        // Vector3 newMove = cameraRotationY * new Vector3(move.x, 0, move.y) * Runner.DeltaTime * moveSpeed;

        // charCtrl.Move(newMove);

        // if (newMove != Vector3.zero)
        // {
        //     gameObject.transform.forward = newMove;
        // }
        if (allowGravity)
        {
            Fall();
        }

        MyInput();
        MovePlayer();
    }

    private void MyInput()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        verticalInput = moveInput.y;
        horizontalInput = moveInput.x;

        float jumpCrouchValue = jumpCrouchAction.ReadValue<float>();

        if (jumpCrouchValue > 0)
        {
            if (allowGravity)
            {
                // if (readyToJump && grounded){
                //     readyToJump = false;
                //     Jump();
                //     Invoke(nameof(ResetJump), jumpCooldown);
                // }

                if (grounded)
                {
                    Jump();
                }
            }
            else
            {
                Fly();
            }
        }
        else if (jumpCrouchValue < 0)
        {
            if (allowGravity && grounded)
            {
                // Acciones para agacharse (si se implementa)
                // Crouch(); (Agregar lógica si se requiere)
            }
            else
            {
                Land();
            }
        }
        else
        {
            if (!allowGravity)
            {
                velocity.y = 0;
            }
        }
    }

    private void MovePlayer()
    {
        Vector3 moveDirection;
        Quaternion cameraRotationY = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

        if (allowGravity)
        {
            moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        }
        else
        {
            moveDirection = Camera.main.transform.rotation * new Vector3(horizontalInput, 0, verticalInput);
        }

        if (runAction.ReadValue<float>() > 0)
        {
            // Debug.Log("Running");
            moveDirection *= runMultiplier;
        }

        moveDirection = cameraRotationY * moveDirection;
        charCtrl.Move(moveSpeed * Runner.DeltaTime * moveDirection);

        charCtrl.Move(velocity * Runner.DeltaTime);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new(velocity.x, 0f, velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            velocity = new(limitedVel.x, velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Debug.Log("Jumping");
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Fly()
    {
        // Debug.Log("Flying");
        velocity.y = moveSpeed;
    }

    private void Land()
    {
        // Debug.Log("Landing");
        velocity.y = -moveSpeed;
    }

    private void Fall(){
        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    // private void OnOpenUI(){
    //     var menu = gameMenuManager.Menu;

    //     menu.SetActive(!menu.activeSelf);

    //     camCtrl.CanUpdate = !menu.activeSelf;
    // }

    void HealthChanged()
    {
        Debug.Log($"{transform.name} health changed to: {NetworkedHealth}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcDecreaseHealthBy(float damageValue){
        if(isAlive){
            if (NetworkedHealth - damageValue <= 0){
                NetworkedHealth = 0;
                isAlive = false;
                Destroy(gameObject, 6f);
            }else{
                NetworkedHealth -= damageValue;
            }
        }
    }

    public void OnCycleWeapons(InputValue inputValue)
    {
        var auxIndex = (int)inputValue.Get<float>() >= 1 ? 1 : -1;
        weaponManager.CycleWeapons(auxIndex);
    }
    
    protected void GroundedCheck() {
        Vector3 spherePosition = new(transform.position.x, transform.position.y - playerHeight/2,
            transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, charCtrl.radius, whatIsGround,
            QueryTriggerInteraction.Ignore);
    }

    protected void OnDrawGizmosSelected(){
        if (!charCtrl) return;
        Gizmos.color = grounded ? new(0.0f, 1.0f, 0.0f, 0.35f) : new(1.0f, 0.0f, 0.0f, 0.35f);

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - playerHeight/2, transform.position.z),
            charCtrl.radius);
    }
}