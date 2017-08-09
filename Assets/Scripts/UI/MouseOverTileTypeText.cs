using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MouseOverTileTypeText : MonoBehaviour
{
    private TextMeshProUGUI m_text;
    public MouseController MouseController;

    private void Start()
    {
        m_text = GetComponent<TextMeshProUGUI>();

        if (m_text == null)
        {
            Debug.LogError("MouseOverTileTypeText: No 'TextMeshPro' UI component on this object.");
            enabled = false;
            return;
        }

        MouseController = GameObject.FindObjectOfType<MouseController>();
    }

    void Update()
    {
        Tile tile = MouseController.GetTileUnderMouse();
        m_text.SetText("Tile type: " + tile.Type.ToString());
    }
}