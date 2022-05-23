using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerSlideMovement : PlayerMovementModifier {

    [SerializeField] float slideSpeed;
    [SerializeField] float slideLength;

    PlayerCrouch crouch;
    PlayerCamera camera;

    // Start is called before the first frame update
    void Start() {
        crouch = player.GetPlayerBehaviour<PlayerCrouch>();
        camera = player.GetPlayerBehaviour<PlayerCamera>();
        player.input.actions["Crouch"].performed += (InputAction.CallbackContext context) => { if (player.sprinting && !crouch.crouchAnimPlaying && player.controller.isGrounded && !player.dashing) StartCoroutine(Slide()); };
    }


    IEnumerator Slide() {
        player.sliding = true;

        // Reset time of animation to 0 and set variables according to mulipliers
        float elapsedTime = 0;
        float currentHeight = player.controller.height;
        Vector3 currentCenter = player.controller.center;

        float newElapsedTime = 0;

        float crouchedHeight = crouch.standingHeight * crouch.crouchingHeightMulitplier;
        Vector3 crouchedCenter = crouch.standingCenter + (Vector3.up * crouch.crouchingHeightMulitplier);

        player.controller.height = crouchedHeight;
        player.controller.center = crouchedCenter;

        float standTransitionLength = 0.2f;

        Vector2 input = player.input.actions["Move"].ReadValue<Vector2>();

        if (input != Vector2.zero) {
            Vector3 dashDir = (transform.right * input.x + transform.forward * input.y) * slideSpeed;

            while (elapsedTime < slideLength) {
                if (elapsedTime >= slideLength - standTransitionLength) {
                    if (!UnityEngine.Physics.Raycast(transform.position, Vector3.up, 1f)) {
                        player.controller.height = Mathf.Lerp(crouchedHeight, crouch.standingHeight, newElapsedTime / standTransitionLength);
                        player.controller.center = Vector3.Lerp(crouchedCenter, crouch.standingCenter, newElapsedTime / standTransitionLength);
                        newElapsedTime += Time.deltaTime;
                        elapsedTime += Time.deltaTime;
                    }
                } else {
                    elapsedTime += Time.deltaTime;
                }

                moveDir = new Vector3(dashDir.x, 0, dashDir.z);
                camera.SmoothFOV(110);
                yield return null;
            }

            player.controller.height = crouch.standingHeight;
            player.controller.center = crouch.standingCenter;
        }

        moveDir = Vector3.zero;
        player.sliding = false;
    }
}
