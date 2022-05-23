using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerDashMovement : PlayerMovementModifier {

    [SerializeField] float dashSpeed;
    [SerializeField] float dashLength;
    [SerializeField] ParticleSystem dashParticle;

    ParticleSystem.VelocityOverLifetimeModule dashParticleVel;

    PlayerCamera camera;

    // Start is called before the first frame update
    void Start() {
        dashParticleVel = dashParticle.velocityOverLifetime;
        camera = player.GetPlayerBehaviour<PlayerCamera>();
        player.input.actions["Dash"].performed += (InputAction.CallbackContext context) => { if (!player.dashing && !player.sliding && !player.crouching) StartCoroutine(Dash()); };
    }

    IEnumerator Dash() {
        player.dashing = true;

        // Reset time of animation to 0 and set variables according to mulipliers
        float elapsedTime = 0;

        Vector2 input = player.input.actions["Move"].ReadValue<Vector2>();

        //smoothMove = 0f;

        if (input != Vector2.zero) {
            Vector3 dashDir = (transform.right * input.x + transform.forward * input.y) * dashSpeed;

            while (elapsedTime < dashLength) {
                moveDir = new Vector3(dashDir.x, 0, dashDir.z);
                camera.SmoothFOV(120);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            dashParticleVel.x = -input.x * 3;
            dashParticleVel.z = -input.y * 3;
            dashParticle.Emit(100);
        }

        //smoothMove = smoothMoveDefault;

        moveDir = Vector3.zero;
        player.dashing = false;
    }
}
