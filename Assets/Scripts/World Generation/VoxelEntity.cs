using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelEntity : MonoBehaviour {

    public Transform player;
    public byte ID;

    bool pickupable = false;
    bool enroute = false;

    Vector3 startPosition;
    float timeToReach = 0.4f;
    float currentTime = 0;
    bool finishedMovement = false;

    public void Setup(Transform player, byte ID) {
        this.player = player;
        this.ID = ID;
        Invoke("Activate", 0.5f);
    }

    void Activate() {
        pickupable = true;
    }

    void Update() {
        if (pickupable) {
            if (Vector3.Distance(player.position, transform.position) <= 2.5f) {
                if (enroute == false) {
                    startPosition = transform.position;
                    enroute = true;
                }
            }
        }
        if (enroute) {
            currentTime += Time.deltaTime / timeToReach;
            transform.position = Vector3.Lerp(startPosition, player.position, currentTime);
            if (currentTime >= timeToReach) {
                player.GetComponent<PlayerInventory>().PickUpItem(ID, 1);
                Destroy(gameObject);
            }
        }
    }

}
