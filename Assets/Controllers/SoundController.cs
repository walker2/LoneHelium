using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{

	private float m_soundCooldown = 0;
	// Use this for initialization
	void Start ()
	{
		WorldController.Instance.World.CbFurnitureCreated += OnFurnitureCreated;
		WorldController.Instance.World.CbTileChanged += OnTileTypeChanged;
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_soundCooldown -= Time.deltaTime;
	}
	
	private void OnTileTypeChanged(Tile tileData)
	{
		// TODO:!
		if (m_soundCooldown > 0)
			return;
		
		AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
		AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
		m_soundCooldown = 0.1f;
	}
	
	private void OnFurnitureCreated(Furniture obj)
	{
		if (m_soundCooldown > 0)
			return;
		
		AudioClip ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
		AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
		m_soundCooldown = 0.1f;
	}
}
