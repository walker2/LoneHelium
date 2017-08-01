using System;
using UnityEngine;

public enum TileType
{
    Empty,
    GroundTiles
}

public class Tile
{
    public event Action<Tile> CbTileTypeChanged;
    private TileType m_type = TileType.Empty;

    public TileType Type
    {
        get { return m_type; }
        set
        {
            if (m_type == value) return;

            m_type = value;
            //Call the callback
            if (CbTileTypeChanged != null)
            {
                CbTileTypeChanged(this);
            }
        }
    }

    private Inventory m_inventory;

    public Furniture Furniture { get; private set; }

    public Job PendingFutureJob;

    public World World { get; private set; }

    public Vector2 Position { get; private set; }

    public Tile(World world, Vector2 position)
    {
        World = world;
        Position = position;
    }

    public static void ChangeTileType(Tile tileData, TileType tileType)
    {
        tileData.Type = tileType;
        tileData.CbTileTypeChanged(tileData);
        
        int x = Mathf.RoundToInt(tileData.Position.x);
        int y = Mathf.RoundToInt(tileData.Position.y);
        
        // This type of furniture links itself to it's neighbours. Update neighbours by triggering callback
        Tile t = tileData.World.GetTileAt(x, y + 1);

        if (t != null)
        {
            t.CbTileTypeChanged(t);
        }

        t = tileData.World.GetTileAt(x + 1, y);
        if (t != null)
        {
            t.CbTileTypeChanged(t);
        }

        t = tileData.World.GetTileAt(x, y - 1);
        if (t != null)
        {
            t.CbTileTypeChanged(t);
        }

        t = tileData.World.GetTileAt(x - 1, y);
        if (t != null)
        {
            t.CbTileTypeChanged(t);
        }
    }
    
    public bool PlaceObject(Furniture objInstance)
    {
        if (objInstance == null)
        {
            Furniture = null;
            return true;
        }

        if (Furniture != null)
        {
            Debug.LogError("Trying to assign an installed object to tile that already has one");
            return false;
        }

        Furniture = objInstance;
        return true;
    }
}