using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class PlayerDirectionalMovement : PlayerMovementModifier {

    [SerializeField] float speed;

    public float sprintMultiplier;
    public float crouchingSpeedMultiplier;

    [SerializeField] float smoothSpeed;
    [SerializeField] float movementFloor;

    Vector2 inputDirection;
    Vector2 smoothedInputDirection;
    Vector2 refVelocity = Vector2.zero;

    PlayerCamera camera;

    void Start() {
        camera = player.GetPlayerBehaviour<PlayerCamera>();
        player.input.actions["Move"].performed += (InputAction.CallbackContext context) => { inputDirection = context.ReadValue<Vector2>(); };
        player.input.actions["Move"].canceled += (InputAction.CallbackContext context) => { inputDirection = Vector2.zero; };
        player.input.actions["Sprint"].performed += (InputAction.CallbackContext context) => { if (!player.crouching) player.sprinting = true; };
        player.input.actions["Sprint"].canceled += (InputAction.CallbackContext context) => { player.sprinting = false; };
    }

    void Update() {
        if (!player.sliding) { // !dashAnimPlaying 
            if (inputDirection != Vector2.zero) {
                if (player.sprinting) {
                    camera.SmoothFOV(100);
                } else {
                    camera.SmoothFOV(90);
                }
            } else {
                camera.SmoothFOV(90);
            }

            smoothedInputDirection = Vector2.SmoothDamp(smoothedInputDirection, inputDirection, ref refVelocity, smoothSpeed);

            float targetSpeed = (player.crouching ? speed * crouchingSpeedMultiplier : player.sprinting ? speed * sprintMultiplier : speed) * smoothedInputDirection.magnitude;

            Vector3 targetDirection = (transform.right.normalized * smoothedInputDirection.x + transform.forward.normalized * smoothedInputDirection.y) * targetSpeed;

            if (targetDirection.magnitude > movementFloor) {
                moveDir = targetDirection;
            } else {
                moveDir = Vector2.zero;
            }
        }
    }
}
