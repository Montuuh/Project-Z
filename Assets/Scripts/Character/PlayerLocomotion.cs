using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    private Vector3 moveDirection;
    private Transform cameraObject;
    private Rigidbody playerRigidbody;

    [SerializeField] private float currentSpeed;
    private float walkSpeed = 2f;    // Walking speed
    private float runningSpeed = 4f; // Default speed running
    private float sprintSpeed = 6f;  // Sprinting speed
    private float rotationSpeed = 16f;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllLocomotion()
    {
        if (PlayerManager.instance.playerState == PlayerManager.PlayerState.Falling)
            return;
        
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        moveDirection = cameraObject.forward * InputManager.instance.verticalInput;
        moveDirection += cameraObject.right * InputManager.instance.horizontalInput;
        moveDirection.Normalize();

        if (PlayerManager.instance.playerState == PlayerManager.PlayerState.Walking)
            currentSpeed = walkSpeed;
        else if (PlayerManager.instance.playerState == PlayerManager.PlayerState.Sprinting)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = runningSpeed;
        moveDirection *= currentSpeed;
        playerRigidbody.velocity = new Vector3(moveDirection.x, playerRigidbody.velocity.y, moveDirection.z);
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;
        targetDirection = cameraObject.forward * InputManager.instance.verticalInput;
        targetDirection += cameraObject.right * InputManager.instance.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        transform.rotation = playerRotation;
    }
}
