using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class InventoryUI : MonoBehaviour
{
    private TextMeshProUGUI keyText;    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        keyText = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateKeyText(PlayerInventory playerInventory)
    {
        keyText.text = playerInventory.NumberOfKeys.ToString();
    }
}
