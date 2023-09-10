using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Singleton pattern
    public static InputManager instance;
    
    // Input controls
    PlayerControls playerControls;
    DebugKeys debugKeys;

    AnimatorManager animatorManager;
    private float moveAmount;

    // Movement
    public Vector2 movementInput;
    public float verticalInput;
    public float horizontalInput;

    // DebugKeys
    public bool toggleExtraHUD;

    // Events
    public static event Action OnToggleExtraHUD;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
        
        animatorManager = GetComponent<AnimatorManager>();
    }

    private void OnEnable()
    {
        if (playerControls == null)
            playerControls = new PlayerControls();
        if (debugKeys == null)
            debugKeys = new DebugKeys();
        
        // Subscribe to input events
        playerControls.PlayerMovement.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();

        // Enable input
        playerControls.Enable();
        debugKeys.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleDebugKeysInput();
        // HandleJumpInput();
        // HandleAttackInput();
        // HandleInventoryInput();
        // HandleInteractInput();
    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmount);
    }

    private void HandleDebugKeysInput()
    {
        if (debugKeys.ExtraHUD.Toggle.triggered)
        {
            OnToggleExtraHUD?.Invoke();
        }
    }
}
