using UnityEngine;

//----------------------------------------------------------------
//
//                        PLAYER MOVEMENT
//
//----------------------------------------------------------------

public class BasicMovementTemplate : MonoBehaviour {
    [Header("Movement Settings")]
    public float forwardSpeed = 5.0f;
    public float gravity = -30f;
    public float jumpHeight = 1.0f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2.0f;
    public Transform playerCamera;

    // --- Private Variables ---
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float cameraPitch = 0.0f;
    
    private Vector3 verticalVelocity;


    void Start() {
        // Get the CharacterController component
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        HandleMouseLook();
        HandleMovement();
    }

    // =================================================================
    // MOUSE LOOK 
    // =================================================================

    private void HandleMouseLook() {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 1. Rotate the player object horizontally
        transform.Rotate(Vector3.up * mouseX);

        // 2. Rotate the camera vertically (Pitch)
        // Adjust the cameraPitch variable to accumulate vertical movement
        cameraPitch -= mouseY;
        // Clamp the pitch so the player can't look upside down
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        
        // Apply the pitch to the camera's local rotation
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement() {
        // clear previous frame's horizontal move to avoid residual values
        moveDirection.x = 0f;
        moveDirection.z = 0f;

        // --- 1. Get Input ---
        float inputX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float inputZ = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow

        // --- 2. Calculate Horizontal Movement (Velocity) ---
        // We use 'transform.right' and 'transform.forward' to move relative to the player's rotation
        Vector3 desiredMove = transform.right * inputX + transform.forward * inputZ;
        
        // This is the simplified, non-Quake velocity setting:
        moveDirection.x = desiredMove.x * forwardSpeed;
        moveDirection.z = desiredMove.z * forwardSpeed;


        // --- 3. Handle Gravity and Jumping ---

        // Check if the CharacterController is touching the ground
        if (characterController.isGrounded) {
            // Reset vertical velocity when grounded (small negative to keep controller grounded)
            verticalVelocity.y = -1f;

            if (Input.GetButtonDown("Jump") || Input.GetAxis("Mouse ScrollWheel") < 0f) {
                // Calculate jump velocity using basic physics formula (v = sqrt(2 * h * g))
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else {
            // Apply gravity over time
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        // --- 4. Final Movement Application ---
        
        // Combine horizontal speed and vertical velocity (gravity/jump)
        moveDirection.y = verticalVelocity.y;
        
        // Apply the full move vector using the CharacterController
        // Note: Time.deltaTime makes the movement frame-rate independent
        characterController.Move(moveDirection * Time.deltaTime);
    }
}