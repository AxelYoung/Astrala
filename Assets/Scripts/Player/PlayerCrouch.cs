using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : PlayerBehaviour {

    public float crouchingHeightMulitplier;
    [SerializeField] float crouchAnimLength;

    [HideInInspector] public bool crouchAnimPlaying;
    [HideInInspector] public float standingHeight;
    [HideInInspector] public Vector3 standingCenter = Vector3.zero;


    void Start() {
        standingHeight = player.controller.height;
        player.input.actions["Crouch"].performed += (InputAction.CallbackContext context) => { if (!player.sprinting && !crouchAnimPlaying && player.controller.isGrounded) StartCoroutine(Crouch()); };
        player.input.actions["Sprint"].performed += (InputAction.CallbackContext context) => { if (player.crouching) StartCoroutine(Crouch()); };
    }

    IEnumerator Crouch() {
        // Check if object above player before standing
        if (player.crouching && UnityEngine.Physics.Raycast(transform.position, Vector3.up, 2f)) yield break;

        crouchAnimPlaying = true;

        // Reset time of animation to 0 and set variables according to mulipliers
        float elapsedTime = 0;
        float targetHeight = player.crouching ? standingHeight : standingHeight * crouchingHeightMulitplier;
        float currentHeight = player.controller.height;
        Vector3 targetCenter = player.crouching ? standingCenter : standingCenter + (Vector3.up * crouchingHeightMulitplier);
        Vector3 currentCenter = player.controller.center;

        while (elapsedTime < crouchAnimLength) {
            // Lerp both height and center of player controller from starting to goal height and center
            player.controller.height = Mathf.Lerp(currentHeight, targetHeight, elapsedTime / crouchAnimLength);
            player.controller.center = Vector3.Lerp(currentCenter, targetCenter, elapsedTime / crouchAnimLength);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.controller.height = targetHeight;
        player.controller.center = targetCenter;

        player.crouching = !player.crouching;

        crouchAnimPlaying = false;
    }
}
