﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Responsible for the inventory slot the script is attached to.
/// </summary>

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;                      // reference to the icon image component of this slot
    public InventoryType slotType;          // reference to the inventory type of this slot
    public bool isActionButton;             // whether or not this slot is an action button

    [SerializeField] GameObject removeBtn;  // reference to the remove item button of this slot
    [SerializeField] TextMeshProUGUI stackCount;                // reference to the stack count text component

    ObservableStack<Item> items = new ObservableStack<Item>();  // a stack of items in this slot

    /// <summary>
    /// Returns the stack of items in this slot.
    /// </summary>

    public ObservableStack<Item> GetItems
    {
        get
        {
            return items;
        }
    }

    private void Awake()
    {
        items.OnPop += new UpdateStackEvent(UpdateSlot);
        items.OnPush += new UpdateStackEvent(UpdateSlot);
        items.OnClear += new UpdateStackEvent(UpdateSlot);
    }

    /// <summary>
    /// Returns whether or not the slot is empty.
    /// </summary>

    public bool IsEmpty
    {
        get
        {
            return items.Count == 0;
        }
    }

    /// <summary>
    /// Returns whether or not the slot is full.
    /// </summary>

    public bool IsFull
    {
        get
        {
            if (IsEmpty || items.Count < item.GetStackSize())
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Responsible for adding an item to this inventory slot.
    /// </summary>
    /// <param name="item">the new item to add</param>

    public bool AddItem(Item item)
    {
        if (item.GetInventoryType() != slotType && slotType != InventoryType.None)
        {
            return false;
        }

        items.Push(item);

        icon.sprite = item.GetIcon();
        icon.enabled = true;
        removeBtn.SetActive(true);
        item.Slot = this;
        return true;
    }

    /// <summary>
    /// Responsible for clearing this inventory slot.
    /// </summary>

    public void RemoveItem(Item item)
    {
        if (!IsEmpty)
        {
            items.Pop();
        }
    }

    /// <summary>
    /// To peek at the top item of the stack, returns null if stack is empty.
    /// </summary>

    public Item item
    {
        get
        {
            if (!IsEmpty)
            {
                return items.Peek();
            }

            return null;
        }
    }

    /// <summary>
    /// Handles click events for the remove item button.
    /// </summary>

    public void OnRemoveButton()
    {
        Clear();
    }

    /// <summary>
    /// Responsible for clearing the slot.
    /// </summary>

    public void Clear()
    {
        if (items.Count > 0)
        {
            items.Clear();
        }
    }

    /// <summary>
    /// Handles button clicks on inventory items.
    /// </summary>

    public void UseItem()
    {
        if (item != null)
        {
            item.Use();
        }
    }

    /// <summary>
    /// Handles click events on this slot button.
    /// </summary>

    public void OnPointerClick(PointerEventData eventData)
    {
        // if this slot is an action button, it will override right clicks

        if (!isActionButton)
        {
            // if right mouse click, use item

            if (eventData.button == PointerEventData.InputButton.Right && MoveUI.instance.moveable == null)
            {
                UseItem();
            }
        }

        // if left mouse click, enable move

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // if we don't have anything to move

            if (InventoryManager.instance.MovingSlot == null && !IsEmpty)
            {
                if (MoveUI.instance.moveable != null)
                {
                    if (MoveUI.instance.moveable is Equipment)
                    {
                        if (item is Equipment && (item as Equipment).GetEquipmentType() ==
                            (MoveUI.instance.moveable as Equipment).GetEquipmentType())
                        {
                            (item as Equipment).Use();
                            UIManager.instance.RefreshTooltip();
                            MoveUI.instance.Drop();
                        }
                    }
                }
                else
                {
                    MoveUI.instance.TakeMoveable(item as Moveable);
                    InventoryManager.instance.MovingSlot = this;
                }
            }
            else if (InventoryManager.instance.MovingSlot == null && IsEmpty)
            {
                if (MoveUI.instance.moveable is Equipment)
                {
                    Equipment equipment = (Equipment)MoveUI.instance.moveable;

                    if (AddItem(equipment))
                    {
                        CharacterPanel.instance.currentlySelectedSlot.DequipItem();
                        MoveUI.instance.Drop();
                    }
                }
            }
            else if (InventoryManager.instance.MovingSlot != null)      // if we do have something to move
            {
                // if the same slot is clicked, item is to be put back

                if (PutItemBack() || MergeItems(InventoryManager.instance.MovingSlot) ||
                    SwapItems(InventoryManager.instance.MovingSlot) ||
                    AddItems(InventoryManager.instance.MovingSlot.items))
                {
                    MoveUI.instance.Drop();
                    InventoryManager.instance.MovingSlot = null;
                }
            }
        }
    }

    /// <summary>
    /// Handles mouse enter events on this slot button.
    /// </summary>

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsEmpty)
        {
            UIManager.instance.ShowTooltip(new Vector2(1,0), transform.position, item);
        }
    }

    /// <summary>
    /// Handles mouse exit events on this slot button.
    /// </summary>

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.HideTooltip();
    }

    /// <summary>
    /// Returns whether or not the moved item should be put back.
    /// </summary>

    bool PutItemBack()
    {
        if (InventoryManager.instance.MovingSlot == this)
        {
            InventoryManager.instance.MovingSlot.icon.color = Color.white;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns whether or not it was possible to swap the two item stacks.
    /// </summary>
    /// <param name="from">the first clicked slot</param>

    bool SwapItems(InventorySlot from)
    {
        if (from.slotType != slotType && slotType != InventoryType.None)
        {
            return false;
        }

        if (IsEmpty)
        {
            return false;
        }

        if (from.item.GetName() != item.GetName() ||
            from.items.Count + items.Count > item.GetStackSize())
        {
            ObservableStack<Item> tmpFrom = new ObservableStack<Item>(from.items);

            from.items.Clear();
            from.AddItems(items);

            items.Clear();
            AddItems(tmpFrom);

            Debug.Log("Item Swap");
            return true;
        }

        Debug.Log("Swap failed");

        return false;
    }

    /// <summary>
    /// Responsible for trying to add the items to the slot.
    /// </summary>
    /// <param name="newItems">the stack of new items</param>
    /// <returns></returns>

    public bool AddItems(ObservableStack<Item> newItems)
    {
        if (newItems.Peek().GetInventoryType() != slotType && slotType != InventoryType.None)
        {
            return false;
        }

        if (IsEmpty || newItems.Peek().GetName() == item.GetName())
        {
            int count = newItems.Count;

            for (int i = 0; i < count; i++)
            {
                if (IsFull)
                {
                    return false;
                }

                AddItem(newItems.Pop());
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Resposnible for trying to merge two items.
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>

    bool MergeItems(InventorySlot from)
    {
        if (IsEmpty)
        {
            return false;
        }

        if (from.item.GetName() == item.GetName() && !IsFull)
        {
            // calculate how many free spaces we have in the stack

            int space = item.GetStackSize() - items.Count;

            for (int i = 0; i < space; i++)
            {
                AddItem(from.items.Pop());
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Responsible for trying to stack an item into this bag slot.
    /// </summary>
    /// <param name="item">the item to stack</param>
    /// <returns>whether or not the stack was succesful</returns>

    public bool StackItem(Item item)
    {
        if (!IsEmpty && item.GetName() == this.item.GetName() && items.Count < this.item.GetStackSize())
        {
            items.Push(item);
            item.Slot = this;
            UpdateSlot();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Responsible for updating the stack size.
    /// </summary>

    public void UpdateStackSize()
    {
        if (items.Count > 1)
        {
            stackCount.text = items.Count.ToString();

            icon.color = Color.white;
            removeBtn.SetActive(true);
        }
        else
        {
            stackCount.text = "";

            icon.color = Color.white;
            removeBtn.SetActive(true);
        }

        if (IsEmpty)
        {
            stackCount.text = "";

            icon.sprite = null;
            icon.enabled = false;
            removeBtn.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the slot whenever any changes are made to it.
    /// </summary>

    void UpdateSlot()
    {
        UpdateStackSize();
    }
}
