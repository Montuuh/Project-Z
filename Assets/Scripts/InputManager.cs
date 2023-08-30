using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Events
    public static event Action OnToggleExtraHUD;

    private void Update()
    {
        // Listen for F1 key press
        if (Input.GetKeyDown(KeyCode.F1)) // Toggle extra HUD info
        {
            OnToggleExtraHUD?.Invoke();
        }
    }
}
