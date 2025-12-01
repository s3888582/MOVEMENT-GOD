using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    public Camera playerCamera;
    public float forwardSpeed = 6f;
    public float jumpPower = 7f;
    public float gravity = 25f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    // CS-style tuning variables for bhop/strafing
    public float groundAccel = 50f;  // how fast we accelerate on ground
    public float airAccel = 3f;     // how fast we accelerate in air
    public float strafeAccel = 25f;  // boost when strafing perpendicular (bhop gain)
    public float airControl = 1f;  // how much air control (tweak for feel)
    // turning suppression: fast mouse yaw reduces air acceleration (CS-like)
    public float turnSpeedThreshold = 4f; // mouseX*lookSpeed at which suppression reaches minimum
    public float minTurnAccelMultiplier = 0.05f; // minimum multiplier applied to air accel when turning fast
    public float friction = 8f;      // ground slowing when no input
    public float maxSpeed = 50f;      // cap horizontal speed

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    // helper: accelerate current velocity toward desired using max accel per second
    private Vector3 Accelerate(Vector3 current, Vector3 desired, float accel)
    {
        Vector3 delta = desired - current;
        float maxChange = accel * Time.deltaTime;
        float deltaMag = delta.magnitude;
        if (deltaMag > maxChange && deltaMag > 0f)
            delta = delta.normalized * maxChange;
        return current + delta;
    }

    void Start() {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Input direction (not forced to zero when small)
        Vector3 inputDir = (forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal"));
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();
        Vector3 targetVelocity = inputDir * forwardSpeed;

        float movementDirectionY = moveDirection.y;

        // current horizontal velocity
        Vector3 horizontal = new Vector3(moveDirection.x, 0f, moveDirection.z);

        if (characterController.isGrounded) {
            // apply ground friction when no input
            if (inputDir.sqrMagnitude < 0.0001f) {
                horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, friction * Time.deltaTime);
            }

            // accelerate toward target velocity using groundAccel
            horizontal = Accelerate(horizontal, targetVelocity, groundAccel);

            // clamp horizontal speed
            if (horizontal.magnitude > maxSpeed) horizontal = horizontal.normalized * maxSpeed;

            // Jump (single press)
            if (Input.GetButton("Jump") && canMove) {
                moveDirection.y = jumpPower;
            } else {
                moveDirection.y = -1f; // small downward force to keep grounded
            }

        } else {
            // AIR MOVEMENT - Source-style accelerate + air control
            // build wishdir/wishspeed from input (world-space)
            Vector3 wishdir = inputDir;
            float wishspeed = wishdir.magnitude * forwardSpeed;
            if (wishspeed > 0.0001f) wishdir.Normalize();

            // choose accel: prefer strafeAccel when input is roughly perpendicular to velocity
            float accelToUse = airAccel;

            // reduce air accel when the player is turning the camera too fast (CS-like behavior)
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float turnSpeed = Mathf.Abs(mouseX);
            float turnFactor = Mathf.Clamp01(1f - (turnSpeed / turnSpeedThreshold));
            float turnMultiplier = Mathf.Lerp(minTurnAccelMultiplier, 1f, turnFactor);
            if (horizontal.sqrMagnitude > 0.0001f && wishspeed > 0.0001f) {
                float dot = Vector3.Dot(horizontal.normalized, wishdir);
                if (Mathf.Abs(dot) < 0.5f) accelToUse = strafeAccel;
            }

            // apply turn suppression to accelToUse
            accelToUse *= turnMultiplier;

            // Source-style accelerate: only add the component of velocity along wishdir up to wishspeed
            float currentspeed = Vector3.Dot(horizontal, wishdir);
            float addspeed = wishspeed - currentspeed;
            if (addspeed > 0f) {
                float accelspeed = accelToUse * Time.deltaTime * wishspeed;
                if (accelspeed > addspeed) accelspeed = addspeed;
                horizontal += wishdir * accelspeed;
            }

            // AirControl: gives extra directional change when moving forward in air
            if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.001f && wishspeed > 0f) {
                Vector3 vel = horizontal;
                float zspeed = vel.y;
                vel.y = 0f;
                float speed = vel.magnitude;
                if (speed > 0.0001f) {
                    vel.Normalize();
                    float dot = Vector3.Dot(vel, wishdir);
                    float k = 32f * airControl * dot * dot * Time.deltaTime;
                    if (dot > 0f) {
                        vel = vel * speed + wishdir * k;
                        vel.Normalize();
                        horizontal.x = vel.x * speed;
                        horizontal.z = vel.z * speed;
                    }
                }
                // preserve any vertical component (should be none for horizontal)
                moveDirection.y = movementDirectionY - gravity * Time.deltaTime;
            } else {
                // normal gravity application
                moveDirection.y = movementDirectionY - gravity * Time.deltaTime;
            }

            // clamp horizontal speed (ground will clamp on landing)
            if (horizontal.magnitude > maxSpeed) horizontal = horizontal.normalized * maxSpeed;
        }

        moveDirection.x = horizontal.x;
        moveDirection.z = horizontal.z;

        // Crouch logic
        if (Input.GetKey(KeyCode.R) && canMove) {
            characterController.height = crouchHeight;
            forwardSpeed = crouchSpeed;
        } else {
            characterController.height = defaultHeight;
            forwardSpeed = 6f;
        }

        // Apply movement
        characterController.Move(moveDirection * Time.deltaTime);

        // Camera rotation
        if (canMove) {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}
