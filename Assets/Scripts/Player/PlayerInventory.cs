using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour {

    public GameObject inventoryObject;
    public Image[] hotbarSlots;
    [HideInInspector] public ItemSlot[] inventoryItems = new ItemSlot[37];

    [HideInInspector] public int currentItem;
    public RawImage selectedItemImage;

    [HideInInspector] public bool inventoryOpen = false;

    Player player;
    PlayerHand hand;
    PlayerMovementHandler movementHandler;

    void Start() {
        player = GetComponent<Player>();
        hand = player.GetPlayerBehaviour<PlayerHand>();
        movementHandler = player.GetPlayerBehaviour<PlayerMovementHandler>();
        player.input.actions["Inventory"].performed += (InputAction.CallbackContext context) => { ToggleInventory(); };
        player.input.actions["HotbarSelect"].performed += (InputAction.CallbackContext context) => { InventorySelect(context.ReadValue<float>()); };
        selectedItemImage.transform.position = hotbarSlots[currentItem].rectTransform.position;
    }

    void InventorySelect(float scroll) {
        if (scroll != 0) {
            if (scroll > 0) currentItem--;
            if (scroll < 0) currentItem++;

            if (currentItem < 0) {
                currentItem = 6;
            }
            if (currentItem > 6) {
                currentItem = 0;
            }
            hand.UpdateArm();
            selectedItemImage.transform.position = hotbarSlots[currentItem].rectTransform.position;
        }
    }

    void ToggleInventory() {
        inventoryOpen = !inventoryOpen;
        Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        inventoryObject.SetActive(inventoryOpen);
        movementHandler.moveDir = new Vector3(0, movementHandler.moveDir.y, 0);
    }

    public int PickUpItem(byte ID, byte amount) {
        for (int i = 0; i < 37; i++) {
            if (inventoryItems[i].ID == ID) {
                inventoryItems[i].quantity += amount;
                UpdateText(i);
                return i;
            }
        }
        for (int i = 0; i < 37; i++) {
            if (inventoryItems[i].quantity == 0) {
                inventoryItems[i].ID = ID;
                inventoryItems[i].quantity += amount;
                UpdateSlotImage(i);
                UpdateText(i);
                return i;
            }
        }
        return 0;
    }

    public void RemoveItem(int index, byte quantity) {
        inventoryItems[index].quantity -= quantity;
        if (inventoryItems[index].quantity == 0) {
            inventoryItems[index].ID = 0;
            if (index <= 6) {
                hotbarSlots[index].gameObject.SetActive(false);
            }
        } else {
            UpdateText(index);
        }
    }

    public void UpdateSlotImage(int index) {
        if (index <= 6) {
            if (inventoryItems[index].quantity > 0) {
                hotbarSlots[index].gameObject.SetActive(true);
                hotbarSlots[index].sprite = Resources.Load<Sprite>("Sprites/" + inventoryItems[index].ID.ToString());
            } else {
                hotbarSlots[index].gameObject.SetActive(false);
            }
            if (index == currentItem) {
                hand.UpdateArm();
            }
        }
    }

    public void UpdateText(int index) {
        if (index <= 6) {
            Text[] hotbarAmount = hotbarSlots[index].GetComponentsInChildren<Text>();
            foreach (Text amount in hotbarAmount) {
                amount.text = (inventoryItems[index].quantity > 1) ? inventoryItems[index].quantity.ToString() : "";
            }
        }
    }
}

public struct ItemSlot {
    public byte ID;
    public byte quantity;

    public ItemSlot(byte ID = 0, byte quantity = 0) {
        this.ID = ID;
        this.quantity = quantity;
    }
}