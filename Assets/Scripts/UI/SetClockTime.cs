using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class SetClockTime : MonoBehaviour {

	private TextMeshProUGUI m_text;

	private void Start()
	{
		m_text = GetComponent<TextMeshProUGUI>();

		if (m_text == null)
		{
			Debug.LogError("MouseOverRoomIDText: No 'TextMeshPro' UI component on this object.");
			enabled = false;
			return;
		}
	}

	void Update()
	{
		m_text.SetText(DateTime.Now.ToString("HH:mm"));
	}
}