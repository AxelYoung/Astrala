using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJumpMovement : PlayerMovementModifier {

    [SerializeField] float jumpHeight;
    [SerializeField] float drag;

    bool groundedPreviousFrame;

    void Start() {
        player.input.actions["Jump"].performed += (InputAction.CallbackContext context) => { if (player.controller.isGrounded) moveDir.y = jumpHeight; };
    }

    // Update is called once per frame
    void Update() {

        if (!groundedPreviousFrame && player.controller.isGrounded) {
            moveDir.y = 0;
        }

        groundedPreviousFrame = player.controller.isGrounded;

        moveDir.y = Mathf.Lerp(moveDir.y, 0, drag * Time.deltaTime);
    }
}
