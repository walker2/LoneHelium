using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MouseOverFurnitureText : MonoBehaviour
{
    private TextMeshProUGUI m_text;
    public MouseController MouseController;

    private void Start()
    {
        m_text = GetComponent<TextMeshProUGUI>();

        if (m_text == null)
        {
            Debug.LogError("MouseOverRoomIDText: No 'TextMeshPro' UI component on this object.");
            enabled = false;
            return;
        }

        MouseController = GameObject.FindObjectOfType<MouseController>();
    }

    void Update()
    {
        Tile tile = MouseController.GetTileUnderMouse();

        var s = "null";
        if (tile.Furniture != null)
        {
            s = tile.Furniture.ObjectType;
        }
        m_text.SetText("Furniture: " + s);
    }
}