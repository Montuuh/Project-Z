using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Singleton pattern
    public static PlayerManager instance;

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
    }

    // We use FixedUpdate because we are using Rigidbody physics
    private void FixedUpdate()
    {
        playerLocomotion.HandleAllLocomotion();
    }

    private void LateUpdate()
    {
        //CameraManager.instance.FollowTarget();
    }
}
