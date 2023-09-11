using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Singleton pattern
    public static PlayerManager instance { get; private set; }

    private PlayerLocomotion playerLocomotion;

    [Header("Player Flags")]
    [SerializeField] private bool isInteracting;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isFalling;
    [SerializeField] private bool isAttacking;
    
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

        float moveAmount = Mathf.Clamp01(Mathf.Abs(InputManager.instance.horizontalInput) + Mathf.Abs(InputManager.instance.verticalInput));
        AnimatorManager.instance.UpdateAnimatorValues(0, moveAmount);
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
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
            isGrounded = false;
    }
}
