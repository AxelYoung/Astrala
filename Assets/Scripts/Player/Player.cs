using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour {

    [HideInInspector] public PlayerBehaviour[] playerBehaviours;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public PlayerInput input;

    [HideInInspector] public bool crouching;
    [HideInInspector] public bool sprinting;
    [HideInInspector] public bool sliding;
    [HideInInspector] public bool dashing;

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        playerBehaviours = GetComponents<PlayerBehaviour>();
        foreach (PlayerBehaviour playerBehaviour in playerBehaviours) {
            playerBehaviour.player = this;
        }
    }

    public T GetPlayerBehaviour<T>() {
        return playerBehaviours.OfType<T>().First();
    }
}
