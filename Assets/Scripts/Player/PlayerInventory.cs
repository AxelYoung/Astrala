using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour {

    public GameObject inventoryObject;
    public Image[] hotbarSlots;
    [HideInInspector] public ItemSlot[] inventoryItems = new ItemSlot[37];

    [HideInInspector] public int currentItem;
    public RawImage selectedItemImage;

    [Header("Controls")]
    [SerializeField] KeyCode inventoryKey = KeyCode.E;

    [HideInInspector] public bool inventoryOpen = false;

    PlayerController player;

    void Awake() {
        player = GetComponent<PlayerController>();
    }

    void Update() {
        if (!player.craftingOpen) {
            ToggleInventory();
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") != 0) {
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0) currentItem--;
            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0) currentItem++;

            if (currentItem < 0) {
                currentItem = 6;
            }
            if (currentItem > 6) {
                currentItem = 0;
            }
            player.UpdateArm();
        }

        selectedItemImage.transform.position = hotbarSlots[currentItem].rectTransform.position;
    }

    void ToggleInventory() {
        if (Input.GetKeyDown(inventoryKey)) {
            inventoryOpen = !inventoryOpen;
            Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
            inventoryObject.SetActive(inventoryOpen);
            player.moveDir = new Vector3(0, player.moveDir.y, 0);
        }
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
                player.UpdateArm();
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