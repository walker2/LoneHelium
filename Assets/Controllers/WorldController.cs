using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; private set; }
    
    public World World { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;

        //Create a world with Empty tiles
        World = new World();
        
        // Center the camera
        Camera.main.transform.Translate(World.Width / 2f, World.Height / 2f, -10);
    }
    
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x);
        int y = Mathf.RoundToInt(coord.y);

        return WorldController.Instance.World.GetTileAt(x, y);
    }
}