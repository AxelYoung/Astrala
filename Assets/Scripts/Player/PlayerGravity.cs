using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : PlayerMovementModifier {

    [SerializeField] float gravity;
    [SerializeField] float groundedGravity;

    bool groundedPreviousFrame;

    void Update() {
        if (player.controller.isGrounded) {
            moveDir.y = -groundedGravity;
        } else if (groundedPreviousFrame) {
            moveDir.y = 0;
        } else {
            moveDir.y = moveDir.y - (gravity * Time.deltaTime);
        }

        groundedPreviousFrame = player.controller.isGrounded;
    }
}
