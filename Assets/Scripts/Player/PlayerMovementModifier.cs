using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementHandler))]
public abstract class PlayerMovementModifier : PlayerBehaviour {
    [HideInInspector] public Vector3 moveDir;

    PlayerMovementHandler movementHandler;

    void Awake() => movementHandler = GetComponent<PlayerMovementHandler>();

    void OnEnable() => movementHandler.AddMovementModifier(this);
    void OnDisable() => movementHandler.RemoveMovementModifier(this);
}
