using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hide : MonoBehaviour {

	// Use this for initialization
	public void Button_HideShow()
	{
		gameObject.SetActive(!gameObject.activeSelf);
	}
}
