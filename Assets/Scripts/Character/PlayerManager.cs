using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Singleton
    private InputManager inputManager;
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
        inputManager = GetComponent<InputManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void Update()
    {
        inputManager.HandleAllInputs();
        playerLocomotion.HandleAllLocomotion();
    }

    // We use FixedUpdate because we are using Rigidbody physics
    private void FixedUpdate()
    {
    }
}
