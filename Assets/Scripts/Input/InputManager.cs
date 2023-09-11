using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Singleton pattern
    public static InputManager instance { get; private set; }


    // Input controls
    PlayerControls playerControls;
    DebugKeys debugKeys;

    // Movement
    public Vector2 movementInput; // This will be the input movement (auto-set)
    public float verticalInput;
    public float horizontalInput;

    // Camera
    public Vector2 cameraInput; // This will be the input camera (auto-set)
    public float cameraVerticalInput;
    public float cameraHorizontalInput;

    // Camera Zoom
    public float cameraZoomInput;

    // Events
    public static event Action OnToggleExtraHUD;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
    }

    private void OnEnable()
    {
        if (playerControls == null)
            playerControls = new PlayerControls();
        if (debugKeys == null)
            debugKeys = new DebugKeys();
        
        // Subscribe to input events
        playerControls.PlayerMovement.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerMovement.Camera.performed += ctx => cameraInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerMovement.CameraZoom.performed += ctx => cameraZoomInput = ctx.ReadValue<Vector2>().y;

        // Enable input
        playerControls.Enable();
        debugKeys.Enable();
    }

    private void OnDisable()
    {
        if (playerControls != null)
            playerControls.Disable();
        if (debugKeys != null)
            debugKeys.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleCameraInput();
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
    }

    private void HandleCameraInput()
    {
        cameraVerticalInput = cameraInput.y;
        cameraHorizontalInput = cameraInput.x;
    }

    private void HandleDebugKeysInput()
    {
        if (debugKeys.ExtraHUD.Toggle.triggered)
            OnToggleExtraHUD?.Invoke();
    }
}
