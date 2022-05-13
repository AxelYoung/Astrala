using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingGUI : MonoBehaviour {

    /* DEPRECATED

    public Slot[] inventorySlots;
    public Slot[] craftingSlots;
    public Slot outputSlot;
    public Image[] inventoryIcons;
    public Image[] craftingIcons;
    public Image outputIcon;

    public ItemSlot[] crafting;
    public ItemSlot output = new ItemSlot();

    public bool uiOpen;

    public GameObject cursor;
    public Image cursorImage;
    public Text cursorText;
    public Text cursorTextShadow;
    public ItemSlot cursorItem;
    public bool cursorHasItem;

    public PlayerInventory inventoryClass;
    ItemSlot[] inventory;

    void Awake() {
        inventory = inventoryClass.inventoryItems;
        inventoryIcons = new Image[inventorySlots.Length];
        crafting = new ItemSlot[craftingSlots.Length];
        for (int i = 0; i < inventorySlots.Length; i++) {
            inventoryIcons[i] = inventorySlots[i].transform.GetChild(0).GetComponent<Image>();
            int index = i;
            inventorySlots[i].index = index;
            inventorySlots[i].inventory = this;
        }
        craftingIcons = new Image[craftingSlots.Length];
        for (int i = 0; i < craftingSlots.Length; i++) {
            craftingIcons[i] = craftingSlots[i].transform.GetChild(0).GetComponent<Image>();
            int index = i;
            craftingSlots[i].index = index;
            craftingSlots[i].inventory = this;
            craftingSlots[i].type = 1;
        }
        outputIcon = outputSlot.transform.GetChild(0).GetComponent<Image>();
        outputSlot.inventory = this;
        outputSlot.type = 2;
    }

    void Update() {
        cursor.transform.position = Input.mousePosition;
    }

    void OnEnable() {
        for (int i = 0; i < inventorySlots.Length; i++) {
            UpdateSlotImage(i);
            UpdateText(i);
        }
    }

    public void OnRightButtonClick(int index) {
        if (cursorHasItem) {
            if (inventory[index].quantity == 0) {
                inventory[index].ID = cursorItem.ID;
                inventory[index].quantity = 1;
                UpdateSlotImage(index);
                UpdateText(index);
                cursorItem.quantity -= 1;
                if (cursorItem.quantity > 1) {
                    cursorText.text = cursorItem.quantity.ToString();
                    cursorTextShadow.text = cursorItem.quantity.ToString();
                } else {
                    cursorText.text = "";
                    cursorTextShadow.text = "";
                }
                if (cursorItem.quantity == 0) {
                    cursorItem.ID = 0;
                    cursor.SetActive(false);
                    cursorHasItem = false;
                }
            } else if (inventory[index].ID == cursorItem.ID) {
                inventory[index].quantity++;
                UpdateText(index);
                cursorItem.quantity--;
                if (cursorItem.quantity > 1) {
                    cursorText.text = cursorItem.quantity.ToString();
                    cursorTextShadow.text = cursorItem.quantity.ToString();
                } else {
                    cursorText.text = "";
                    cursorTextShadow.text = "";
                }
                if (cursorItem.quantity == 0) {
                    cursorItem.ID = 0;
                    cursor.SetActive(false);
                    cursorHasItem = false;
                }
            }
        }
    }

    public void OnLeftButtonClick(int index) {
        if (inventory[index].quantity != 0) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                ItemSlot temp = inventory[index];
                inventory[index].quantity = 0;
                inventory[index].ID = 0;
                int newSlot = inventoryClass.PickUpItem(temp.ID, temp.quantity);
                UpdateSlotImage(index);
                UpdateText(index);
                UpdateSlotImage(newSlot);
                UpdateText(newSlot);
            } else {
                if (cursorHasItem == false) {
                    cursorItem = inventory[index];
                    cursorImage.sprite = Resources.Load<Sprite>("Sprites/" + inventory[index].ID.ToString());
                    if (inventory[index].quantity > 1) {
                        cursorText.text = inventory[index].quantity.ToString();
                        cursorTextShadow.text = inventory[index].quantity.ToString();
                    } else {
                        cursorText.text = "";
                        cursorTextShadow.text = "";
                    }
                    cursor.SetActive(true);
                    inventory[index].quantity = 0;
                    inventory[index].ID = 0;
                    UpdateSlotImage(index);
                    UpdateText(index);
                    cursorHasItem = true;
                } else {
                    if (inventory[index].ID == cursorItem.ID) {
                        inventory[index].quantity += cursorItem.quantity;
                        UpdateText(index);
                        cursorItem.ID = 0;
                        cursorItem.quantity = 0;
                        cursor.SetActive(false);
                        cursorHasItem = false;
                    } else {
                        ItemSlot cursorSlot = cursorItem;
                        cursorItem = inventory[index];
                        cursorImage.sprite = Resources.Load<Sprite>("Sprites/" + inventory[index].ID.ToString());
                        if (inventory[index].quantity > 1) {
                            cursorText.text = inventory[index].quantity.ToString();
                            cursorTextShadow.text = inventory[index].quantity.ToString();
                        } else {
                            cursorText.text = "";
                            cursorTextShadow.text = "";
                        }
                        inventory[index] = cursorSlot;
                        UpdateText(index);
                        UpdateSlotImage(index);
                    }
                }
            }
        } else {
            if (cursorHasItem) {
                ItemSlot cursorSlot = cursorItem;
                cursorItem = inventory[index];
                cursorImage.sprite = null;
                cursorText.text = null;
                cursorTextShadow.text = null;
                cursor.SetActive(false);
                inventory[index] = cursorSlot;
                UpdateText(index);
                UpdateSlotImage(index);
                cursorHasItem = false;
            }
        }
    }

    public void OnRightButtonClickCrafting(int index) {
        if (cursorHasItem) {
            if (crafting[index].quantity == 0) {
                crafting[index].ID = cursorItem.ID;
                craftingIcons[index].gameObject.SetActive(true);
                craftingIcons[index].sprite = Resources.Load<Sprite>("Sprites/" + cursorItem.ID.ToString());
                crafting[index].quantity = 1;
                UpdateTextCrafting(index);
                cursorItem.quantity -= 1;
                if (cursorItem.quantity > 1) {
                    cursorText.text = cursorItem.quantity.ToString();
                    cursorTextShadow.text = cursorItem.quantity.ToString();
                } else {
                    cursorText.text = "";
                    cursorTextShadow.text = "";
                }
                if (cursorItem.quantity == 0) {
                    cursorItem.ID = 0;
                    cursor.SetActive(false);
                    cursorHasItem = false;
                }
                UpdateCraftingOutput();
            } else if (crafting[index].ID == cursorItem.ID) {
                crafting[index].quantity++;
                UpdateTextCrafting(index);
                cursorItem.quantity--;
                if (cursorItem.quantity > 1) {
                    cursorText.text = cursorItem.quantity.ToString();
                    cursorTextShadow.text = cursorItem.quantity.ToString();
                } else {
                    cursorText.text = "";
                    cursorTextShadow.text = "";
                }
                if (cursorItem.quantity == 0) {
                    cursorItem.ID = 0;
                    cursor.SetActive(false);
                    cursorHasItem = false;
                }
                UpdateCraftingOutput();
            }
        }
    }

    public void OnLeftButtonClickCrafting(int index) {
        if (crafting[index].quantity != 0) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                ItemSlot temp = crafting[index];
                crafting[index].quantity = 0;
                crafting[index].ID = 0;
                int newSlot = inventoryClass.PickUpItem(temp.ID, temp.quantity);
                craftingIcons[index].gameObject.SetActive(false);
                UpdateTextCrafting(index);
                UpdateSlotImage(newSlot);
                UpdateText(newSlot);
            } else {
                if (cursorHasItem == false) {
                    cursorItem = crafting[index];
                    cursorImage.sprite = Resources.Load<Sprite>("Sprites/" + crafting[index].ID.ToString());
                    if (crafting[index].quantity > 1) {
                        cursorText.text = crafting[index].quantity.ToString();
                        cursorTextShadow.text = crafting[index].quantity.ToString();
                    } else {
                        cursorText.text = "";
                        cursorTextShadow.text = "";
                    }
                    cursor.SetActive(true);
                    craftingIcons[index].gameObject.SetActive(false);
                    crafting[index].quantity = 0;
                    crafting[index].ID = 0;
                    UpdateCraftingOutput();
                    UpdateTextCrafting(index);
                    cursorHasItem = true;
                } else {
                    if (crafting[index].ID == cursorItem.ID) {
                        crafting[index].quantity += cursorItem.quantity;
                        UpdateTextCrafting(index);
                        cursorItem.ID = 0;
                        cursorItem.quantity = 0;
                        cursor.SetActive(false);
                        UpdateCraftingOutput();
                        cursorHasItem = false;
                    } else {
                        ItemSlot cursorSlot = cursorItem;
                        cursorItem = crafting[index];
                        cursorImage.sprite = Resources.Load<Sprite>("Sprites/" + crafting[index].ID.ToString());
                        if (crafting[index].quantity > 1) {
                            cursorText.text = crafting[index].quantity.ToString();
                            cursorTextShadow.text = crafting[index].quantity.ToString();
                        } else {
                            cursorText.text = "";
                            cursorTextShadow.text = "";
                        }
                        craftingIcons[index].sprite = Resources.Load<Sprite>("Sprites/" + cursorSlot.ID.ToString());
                        crafting[index] = cursorSlot;
                        UpdateCraftingOutput();
                        UpdateTextCrafting(index);
                    }
                }
            }
        } else {
            if (cursorHasItem) {
                ItemSlot cursorSlot = cursorItem;
                cursorItem = crafting[index];
                cursorImage.sprite = null;
                cursorText.text = null;
                cursorTextShadow.text = null;
                cursor.SetActive(false);
                craftingIcons[index].sprite = Resources.Load<Sprite>("Sprites/" + cursorSlot.ID.ToString());
                craftingIcons[index].gameObject.SetActive(true);
                crafting[index] = cursorSlot;
                UpdateCraftingOutput();
                UpdateTextCrafting(index);
                cursorHasItem = false;
            }
        }
    }

    public void OnLeftButtonClickOutput() {
        if (output.quantity != 0) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                ItemSlot temp = output;
                output.quantity = 0;
                output.ID = 0;
                int newSlot = inventoryClass.PickUpItem(temp.ID, temp.quantity);
                outputIcon.gameObject.SetActive(false);
                UpdateTextOutput();
                UpdateSlotImage(newSlot);
                UpdateText(newSlot);
                RemoveMaterials();
            } else {
                if (cursorHasItem == false) {
                    cursorItem = output;
                    cursorImage.sprite = Resources.Load<Sprite>("Sprites/" + output.ID.ToString());
                    if (output.quantity > 1) {
                        cursorText.text = output.quantity.ToString();
                        cursorTextShadow.text = output.quantity.ToString();
                    } else {
                        cursorText.text = "";
                        cursorTextShadow.text = "";
                    }
                    cursor.SetActive(true);
                    outputIcon.gameObject.SetActive(false);
                    output.quantity = 0;
                    output.ID = 0;
                    UpdateTextOutput();
                    RemoveMaterials();
                    cursorHasItem = true;
                } else {
                    if (output.ID == cursorItem.ID) {
                        cursorItem.quantity += output.quantity;
                        cursorText.text = cursorItem.quantity.ToString();
                        cursorTextShadow.text = cursorItem.quantity.ToString();
                        outputIcon.gameObject.SetActive(false);
                        output.quantity = 0;
                        output.ID = 0;
                        UpdateTextOutput();
                        RemoveMaterials();
                    }
                }
            }
            UpdateCraftingOutput();
        }
    }

    void RemoveMaterials() {
        for (int i = 0; i < crafting.Length; i++) {
            if (crafting[i].quantity != 0) {
                crafting[i].quantity--;
                UpdateTextCrafting(i);
                if (crafting[i].quantity == 0) {
                    craftingIcons[i].gameObject.SetActive(false);
                    crafting[i].ID = 0;
                }
            }
        }
    }

    void UpdateCraftingOutput() {
        byte[,] table;
        table = new byte[,] { { crafting[0].ID, crafting[1].ID, 0 }, { crafting[2].ID, crafting[3].ID, crafting[4].ID }, { crafting[5].ID, crafting[6].ID, 0 } };
        byte[,] resizedTable = ResizeTable(table);
        foreach (Recipe recipe in Recipies.recipes) {
            if (Compare2DArray(resizedTable, recipe.itemIDs)) {
                output.ID = recipe.outputID;
                output.quantity = recipe.quantity;
                outputIcon.sprite = Resources.Load<Sprite>("Sprites/" + output.ID.ToString());
                UpdateTextOutput();
                outputIcon.gameObject.SetActive(true);
                break;
            } else {
                output.ID = 0;
                output.quantity = 0;
                UpdateTextOutput();
                outputIcon.gameObject.SetActive(false);
            }
        }
    }

    byte[,] ResizeTable(byte[,] table) {
        int xStart = 0;
        for (int xStartTest = 0; xStartTest < table.GetLength(1); xStartTest++) {
            bool empty = false;
            for (int y = 0; y < table.GetLength(0); y++) {
                if (table[xStartTest, y] != 0) {
                    empty = false;
                    break;
                } else {
                    empty = true;
                    continue;
                }
            }
            if (empty) {
                xStart = xStartTest + 1;
                continue;
            } else {
                xStart = xStartTest;
                break;
            }
        }
        int xEnd = 0;
        for (int xEndTest = table.GetLength(1) - 1; xEndTest >= 0; xEndTest--) {
            bool empty = false;
            for (int y = 0; y < table.GetLength(0); y++) {
                if (table[xEndTest, y] != 0) {
                    empty = false;
                    break;
                } else {
                    empty = true;
                    continue;
                }
            }
            if (empty) {
                xEnd = xEndTest;
                continue;
            } else {
                xEnd = xEndTest + 1;
                break;
            }
        }
        int yStart = 0;
        for (int yStartTest = 0; yStartTest < table.GetLength(0); yStartTest++) {
            bool empty = false;
            for (int x = 0; x < table.GetLength(1); x++) {
                if (table[x, yStartTest] != 0) {
                    empty = false;
                    break;
                } else {
                    empty = true;
                    continue;
                }
            }
            if (empty) {
                yStart = yStartTest + 1;
                continue;
            } else {
                yStart = yStartTest;
                break;
            }
        }
        int yEnd = 0;
        for (int yEndTest = table.GetLength(0) - 1; yEndTest >= 0; yEndTest--) {
            bool empty = false;
            for (int x = 0; x < table.GetLength(1); x++) {
                if (table[x, yEndTest] != 0) {
                    empty = false;
                    break;
                } else {
                    empty = true;
                    continue;
                }
            }
            if (empty) {
                yEnd = yEndTest;
                continue;
            } else {
                yEnd = yEndTest + 1;
                break;
            }
        }
        if (xStart == table.GetLength(1)) {
            return table;
        }
        byte[,] resizedTable = new byte[xEnd - xStart, yEnd - yStart];
        int resizeY = 0;
        for (int y = yStart; y < yEnd; y++) {
            int resizeX = 0;
            for (int x = xStart; x < xEnd; x++) {
                resizedTable[resizeX, resizeY] = table[x, y];
                resizeX++;
            }
            resizeY++;
        }
        return resizedTable;
    }

    bool Compare2DArray(byte[,] table, byte[,] recipe) {
        if (table.GetLength(0) != recipe.GetLength(0)) return false;
        if (table.GetLength(1) != recipe.GetLength(1)) return false;
        for (int y = 0; y < table.GetLength(0); y++) {
            for (int x = 0; x < table.GetLength(1); x++) {
                if (table[y, x] != recipe[y, x]) return false;
            }
        }
        return true;
    }

    public void UpdateSlotImage(int index) {
        if (index <= 6) {
            inventoryClass.UpdateSlotImage(index);
        }
        if (inventory[index].quantity > 0) {
            inventoryIcons[index].gameObject.SetActive(true);
            inventoryIcons[index].sprite = Resources.Load<Sprite>("Sprites/" + inventory[index].ID.ToString());
        } else {
            inventoryIcons[index].gameObject.SetActive(false);
        }

    }

    void UpdateText(int index) {
        if (index <= 6) {
            inventoryClass.UpdateText(index);
        }
        Text[] amounts = inventorySlots[index].GetComponentsInChildren<Text>();
        foreach (Text amount in amounts) {
            amount.text = (inventory[index].quantity > 1) ? inventory[index].quantity.ToString() : "";
        }
    }

    void UpdateTextCrafting(int index) {
        Text[] amounts = craftingSlots[index].GetComponentsInChildren<Text>();
        foreach (Text amount in amounts) {
            amount.text = (crafting[index].quantity > 1) ? crafting[index].quantity.ToString() : "";
        }
    }

    void UpdateTextOutput() {
        Text[] amounts = outputSlot.GetComponentsInChildren<Text>();
        foreach (Text amount in amounts) {
            amount.text = (output.quantity > 1) ? output.quantity.ToString() : "";
        }
    }

    */
}
