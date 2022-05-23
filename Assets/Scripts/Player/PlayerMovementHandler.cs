using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementHandler : PlayerBehaviour {

    List<PlayerMovementModifier> movementModifiers = new List<PlayerMovementModifier>();

    [HideInInspector] public Vector3 moveDir;

    void Update() {
        moveDir = Vector3.zero;

        foreach (PlayerMovementModifier modifier in movementModifiers) {
            moveDir += modifier.moveDir;
        }

        player.controller.Move(moveDir * Time.deltaTime);
    }

    public void AddMovementModifier(PlayerMovementModifier modifier) => movementModifiers.Add(modifier);
    public void RemoveMovementModifier(PlayerMovementModifier modifier) => movementModifiers.Remove(modifier);
}
