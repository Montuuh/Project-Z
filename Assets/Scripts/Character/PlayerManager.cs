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

    [Header("Player Flags")]
    //[SerializeField] private bool isInteracting;
    //[SerializeField] private bool isAttacking;
    [SerializeField] private bool isGrounded;
    [SerializeField] public PlayerState playerState;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        playerLocomotion = GetComponent<PlayerLocomotion>();
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

        if (!isGrounded)
            playerState = PlayerState.Falling;

        AnimatorManager.instance.UpdateAnimatorValues(0, moveInputAmount, playerState);
        AnimatorManager.instance.UpdateIsFalling(!isGrounded);
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
        if (collision.gameObject.CompareTag("Terrain"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            isGrounded = false;
        }
    }
}
