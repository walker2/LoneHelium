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

    public float MovementCost
    {
        get
        {
            if (Type == TileType.Empty)
                return 0; // 0 is unwalkable

            if (Furniture == null)
                return 1;

            return Furniture.MovementCost;
        }
    }

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

    public bool IsNeighbour(Tile tile, bool includeCorner = false)
    {
        return Mathf.Abs(tile.Position.x - this.Position.x) + Mathf.Abs(tile.Position.y - this.Position.y) == 1
               || (includeCorner && Mathf.Abs(tile.Position.x - this.Position.x) == 1
                   && Mathf.Abs(tile.Position.y - this.Position.y) == 1);
    }

    public Tile[] GetNeighbours(bool includeCorner = false)
    {
        // Tile order [N E S W]<NE SE SW NW>
        //Tile[] ns = includeCorner == false ? new Tile[4] : new Tile[8];

        var tiles = new Tile[10];
        tiles[0] = World.GetTileAt(Position.x, Position.y + 1);
        tiles[1] = World.GetTileAt(Position.x + 1, Position.y);
        tiles[2] = World.GetTileAt(Position.x, Position.y - 1);
        tiles[3] = World.GetTileAt(Position.x - 1, Position.y);

        if (includeCorner)
        {
            tiles[4] = World.GetTileAt(Position.x + 1, Position.y + 1);
            tiles[5] = World.GetTileAt(Position.x + 1, Position.y - 1);
            tiles[6] = World.GetTileAt(Position.x - 1, Position.y - 1);
            tiles[7] = World.GetTileAt(Position.x - 1, Position.y + 1);
        }

        return tiles;
    }
}