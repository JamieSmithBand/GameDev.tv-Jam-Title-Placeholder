﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manages the equipment part of the slot.
/// </summary>

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] EquipmentType equipmentType;       // to determine which type of equipment belongs to this slot

    [SerializeField] Equipment equipment;               // reference to the equipment item of this slot

    [SerializeField] Image icon;                        // reference to the image component on this slot

    /// <summary>
    /// To handle click events on this slot.
    /// </summary>

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (MoveUI.instance.moveable is Equipment)
            {
                Equipment tmp = (Equipment)MoveUI.instance.moveable;

                if (tmp.GetEquipmentType() == equipmentType)
                {
                    EquipItem(tmp);
                }
            }
        }
    }

    /// <summary>
    /// Responsible for equiping an item.
    /// </summary>
    /// <param name="newEquipment"></param>

    void EquipItem(Equipment newEquipment)
    {
        icon.enabled = true;
        icon.sprite = newEquipment.icon;
        equipment = newEquipment;
        MoveUI.instance.DeleteItem();
    }
}
