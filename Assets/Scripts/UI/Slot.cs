using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/* DEPRECATED

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {

    public int index;
    public CraftingGUI inventory;
    public byte type = 0;

    bool entered = false;

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            switch (type) {
                case 0:
                    inventory.OnLeftButtonClick(index);
                    return;
                case 1:
                    inventory.OnLeftButtonClickCrafting(index);
                    return;
                case 2:
                    inventory.OnLeftButtonClickOutput();
                    return;
            }
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            switch (type) {
                case 0:
                    inventory.OnRightButtonClick(index);
                    return;
                case 1:
                    inventory.OnRightButtonClickCrafting(index);
                    return;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (Input.GetKey(KeyCode.Mouse1)) {
            if (entered == false) {
                switch (type) {
                    case 0:
                        inventory.OnRightButtonClick(index);
                        return;
                    case 1:
                        inventory.OnRightButtonClickCrafting(index);
                        return;
                }
                entered = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        entered = false;
    }
}

*/