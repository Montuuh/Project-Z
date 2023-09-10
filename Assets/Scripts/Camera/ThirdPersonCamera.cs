using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    #region InspectorProperties
    
    public Transform target; // The player's transform
    [Tooltip("Lerp speed between Camera States")]
    public float smoothCameraRotation = 12f;
    
    public float rightOffset = 0f; // Right offset
    public float height = 1.4f; // Height from the target
    public float smoothFollow = 10f; // Smooth follow speed
    public float xMouseSensitivity = 3f; // Mouse sensitivity in X
    public float yMouseSensitivity = 3f; // Mouse sensitivity in Y
    private float yMinLimit = -40f; // Min Y angle
    private float yMaxLimit = 80f; // Max Y angle
    private float xMinLimit = -360f;
    private float xMaxLimit = 360f;
    public float zoomSpeed = 2f; // Zoom speed
    public float defaultDistance = 4f; // Default distance from the target
    public float minDistance = 1f; // Min distance from target
    public float maxDistance = 8f; // Max distance from target

    #endregion

    #region HideProperties

    private float distance; // Current distance
    private float mouseX = 0f; // Mouse X input
    private float mouseY = 0f; // Mouse Y input

    #endregion


    void Start()
    {
        Init();
    }

    private void Init()
    {
        distance = defaultDistance;
        mouseX = target.eulerAngles.y;
        mouseY = target.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Zoom functionality
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scrollData * zoomSpeed, minDistance, maxDistance);

        // Get mouse input
        mouseX += Input.GetAxis("Mouse X") * xMouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * yMouseSensitivity;

        // Clamp angles
        mouseY = Mathf.Clamp(mouseY, yMinLimit, yMaxLimit);
        mouseX = Mathf.Clamp(mouseX, xMinLimit, xMaxLimit);

        // Calculate rotation and position
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0);
        Vector3 position = rotation * new Vector3(rightOffset, height, -distance) + target.position;

        // Apply rotation and position
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * smoothCameraRotation);
        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothFollow);
    }
}
