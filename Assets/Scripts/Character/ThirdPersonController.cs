using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    #region InspectorProperties
    // Movement variables
    public float moveSpeed = 4f;
    public float rotationSpeed = 16f;

    // Jumping variables
    public float jumpHeight = 4f;
    public float jumpTimer = 0.3f;

    #endregion InspectorProperties

    #region HideProperties

    // Components
    private Animator animator;
    private Rigidbody _rigidbody;

    // Internal variables
    private Vector3 moveDirection;
    private bool isGrounded;
    private float jumpCooldown;
    private bool isJumping;

    #endregion

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        // Initialize components
        animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //transform.position += new Vector3(0, 0, 0.05f); // This should move the GameObject forward
        
        // Handle input and movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(horizontal, 0, vertical).normalized;

        Debug.Log("Input: " + input);
        Debug.Log("IsGrounded: " + isGrounded);

        // Rotate character
        if (input != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
        }

        // Move character
        if (isGrounded && !isJumping)
        {
            moveDirection = input * moveSpeed;
            _rigidbody.velocity = new Vector3(moveDirection.x, _rigidbody.velocity.y, moveDirection.z);
            Debug.Log("Rigidbody Velocity: " + _rigidbody.velocity);
        }

        // Handle jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpCooldown = jumpTimer;
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, jumpHeight, _rigidbody.velocity.z);
        }

        // Update animator parameters
        UpdateAnimator(input);

        // Update jump state
        if (isJumping)
        {
            jumpCooldown -= Time.deltaTime;
            if (jumpCooldown <= 0)
            {
                isJumping = false;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void UpdateAnimator(Vector3 input)
    {
        animator.SetFloat("InputHorizontal", input.x);
        animator.SetFloat("InputVertical", input.z);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsJumping", isJumping);
    }
}

