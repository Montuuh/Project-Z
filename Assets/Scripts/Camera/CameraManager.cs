using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Singleton pattern
    public static CameraManager instance { get; private set; }

    private Transform targetTransform;          // The target transform
    private Transform cameraTransform;
    private float rightOffset = 0f;
    private float heightOffset = 1.5f;
    private float cameraFollowSpeed = 10f;
    private float cameraRotationSpeed = 12f;
    private float cameraZoomSpeed = 2f;
    private float cameraSmoothTime = 0.2f;
    private float currentCameraVelocity;

    // Desired direction of the rotation
    private float angleY;  // Rotation around the Y axis
    private float minAngleY = 0f; // Min rotation around the Y axis
    private float maxAngleY = 50f; // Max rotation around the Y axis
    private float sensitivityAngleY = .3f;
    private float angleX; // Rotation around the X axis
    private float sensitivityAngleX = .3f;

    // Distance from the camera to the target
    private float distance;
    private float defaultDistance = 4f;
    private float minDistance = .5f;
    private float maxDistance = 9f;

    private Quaternion rotation;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        targetTransform = FindObjectOfType<PlayerManager>().transform;
        cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void Start()
    {
        distance = defaultDistance;
        angleY = targetTransform.eulerAngles.y;
        angleX = targetTransform.eulerAngles.x;
    }

    public void HandleAllCameraMovement()
    {
        HandleCameraRotation();
        HandleCameraZoom();
        HandleCameraCollisions();
        HandleCameraMovement();
    }

    private void HandleCameraRotation()
    {
        angleX += InputManager.instance.cameraHorizontalInput * sensitivityAngleX;
        angleY -= InputManager.instance.cameraVerticalInput * sensitivityAngleY;

        // Boundaries
        angleY = Mathf.Clamp(angleY, minAngleY, maxAngleY);

        rotation = Quaternion.Euler(angleY, angleX, 0);
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, rotation, Time.deltaTime * cameraRotationSpeed);
    }

    private void HandleCameraZoom()
    {
        // Zoom functionality
        float scrollData = InputManager.instance.cameraZoomInput;
        scrollData = Mathf.Clamp(scrollData, -1f, 1f);
        distance = Mathf.Clamp(distance - scrollData * cameraZoomSpeed, minDistance, maxDistance);
    }

    private void HandleCameraMovement()
    {
        Vector3 position = targetTransform.position + rotation * new Vector3(rightOffset, heightOffset, -distance);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, position, Time.deltaTime * cameraFollowSpeed);
    }

    private void HandleCameraCollisions()
    {
        // The direction from the camera to the player
        Vector3 toPlayer = targetTransform.position - cameraTransform.position;

        // Perform the raycast
        RaycastHit hit;
        float newDistance = defaultDistance;  // Initialize newDistance with defaultDistance

        if (Physics.Raycast(targetTransform.position, -toPlayer.normalized, out hit, defaultDistance))
        {
            // If something was hit, set the new distance to the hit point
            if (hit.collider.gameObject != targetTransform.gameObject)
            {
                newDistance = hit.distance;
            }
        }

        // Smoothly interpolate between the current distance and the new distance
        distance = Mathf.SmoothDamp(distance, newDistance, ref cameraSmoothTime, currentCameraVelocity);
    }
}
