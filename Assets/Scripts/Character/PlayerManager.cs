using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Singleton pattern
    public static PlayerManager instance { get; private set; }
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Sprinting,
        Falling
    }

    private PlayerLocomotion playerLocomotion;
    private Rigidbody _rigidbody;

    [Header("Player Flags")]
    public bool isGrounded;
    public float groundedSensitivity = 0.4f;
    public float fallingThreshold = -1.5f;
    private float lastTimeGrounded;
    public PlayerState playerState;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        playerLocomotion = GetComponent<PlayerLocomotion>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        InputManager.instance.HandleAllInputs();
        float moveInputAmount = Mathf.Clamp01(Mathf.Abs(InputManager.instance.horizontalInput) + Mathf.Abs(InputManager.instance.verticalInput));

        if (moveInputAmount > 0)
        {
            if (InputManager.instance.inputIsWalking)
                playerState = PlayerState.Walking;
            else if (InputManager.instance.inputIsSprinting)
                playerState = PlayerState.Sprinting;
            else
                playerState = PlayerState.Running;
        }
        else
        {
            playerState = PlayerState.Idle;
        }

        // Grounded sensitivity
        if (isGrounded)
            lastTimeGrounded = Time.time;

        bool isCloseToGound = Physics.Raycast(transform.position, Vector3.down, 0.1f, LayerMask.GetMask("Ground"));
        bool isActuallyFalling = _rigidbody.velocity.y < fallingThreshold;
        bool shouldFall = !isGrounded && !isCloseToGound && isActuallyFalling;

        AnimatorManager.instance.UpdateAnimatorValues(0, moveInputAmount, playerState);
        AnimatorManager.instance.UpdateIsFalling(shouldFall);
    }

    // We use FixedUpdate because we are using Rigidbody physics
    private void FixedUpdate()
    {
        playerLocomotion.HandleAllLocomotion();
    }

    private void LateUpdate()
    {
        CameraManager.instance.HandleAllCameraMovement();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }
}
