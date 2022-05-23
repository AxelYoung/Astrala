using System.Collections;
using UnityEngine;

public class PlayerCamera : PlayerBehaviour {

    public Camera cam;
    [SerializeField] float sensitivity;
    [SerializeField] float bobSpeed;
    [SerializeField] float bobAmount;
    [SerializeField] Camera overlayCam;
    [SerializeField] float fovSmoothing;

    float refVel = 0f;
    float xRotation;
    float defaultCamPos;
    float cameraTimer;

    PlayerMovementHandler movementHandler;
    PlayerDirectionalMovement directionalMovement;

    void Start() {
        defaultCamPos = cam.transform.localPosition.y;
        movementHandler = player.GetPlayerBehaviour<PlayerMovementHandler>();
        directionalMovement = player.GetPlayerBehaviour<PlayerDirectionalMovement>();
    }

    void Update() {
        CameraMovement();
    }

    void CameraMovement() {
        // Recive input from two axises, adjust sensitivity
        Vector2 input = new Vector2(player.input.actions["LookX"].ReadValue<float>() * sensitivity, player.input.actions["LookY"].ReadValue<float>() * sensitivity);

        // Clamp X rotation
        xRotation -= input.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotations to cam
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * input.x);

        Headbob();
    }

    void Headbob() {
        // Head bob only when grounded and moving
        if (player.controller.isGrounded && !player.sliding) {
            if (movementHandler.moveDir.x != 0 || movementHandler.moveDir.z != 0) {
                // Calculate bob speed and amount based on mulitiplers, bob is simple sin wave
                cameraTimer += Time.deltaTime * (player.crouching ? bobSpeed * directionalMovement.crouchingSpeedMultiplier : player.sprinting ? bobSpeed * directionalMovement.sprintMultiplier : bobSpeed);
                float bob = defaultCamPos + Mathf.Sin(cameraTimer) * (player.crouching ? bobAmount * directionalMovement.crouchingSpeedMultiplier : player.sprinting ? bobAmount * directionalMovement.sprintMultiplier : bobAmount);

                // Apply bob and pass through to footstep function
                cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, bob, cam.transform.localPosition.z);
                //Footsteps(bob);
            }
        }
    }

    public void SmoothFOV(int goal) {
        if (cam.fieldOfView != goal) {
            float fov = Mathf.SmoothDamp(cam.fieldOfView, goal, ref refVel, fovSmoothing);
            cam.fieldOfView = fov;
            overlayCam.fieldOfView = fov;
        }
    }
}