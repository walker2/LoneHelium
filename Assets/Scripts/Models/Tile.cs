using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public enum TileType
{
    Empty,
    Ground
}

public enum EnterState
{
    Yes,
    Never,
    Soon
}

public class Tile : IXmlSerializable
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

    public Room Room;

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
        return Math.Abs(Mathf.Abs(tile.Position.x - this.Position.x) + Mathf.Abs(tile.Position.y - this.Position.y) -
                        1) < 0.00001f
               || (includeCorner && Math.Abs(Mathf.Abs(tile.Position.x - this.Position.x) - 1) < 0.00001f
                   && Math.Abs(Mathf.Abs(tile.Position.y - this.Position.y) - 1) < 0.00001f);
    }

    public Tile[] GetNeighbours(bool includeCorner = false)
    {
        // Tile order [N E S W]<NE SE SW NW>
        // Tile[] ns = includeCorner == false ? new Tile[4] : new Tile[8];

        Tile[] tiles = includeCorner == false ? new Tile[4] : new Tile[8];
        
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
    public bool IsClippingCorner(Tile neighbourTile)
    {
        float dX = this.Position.x - neighbourTile.Position.x;
        float dY = this.Position.y - neighbourTile.Position.y;

        if (Math.Abs(Mathf.Abs(dX) + Mathf.Abs(dY) - 2) < 0.00001f)
        {
            // We are diagonal
            if (World.GetTileAt(Position.x - dX, Position.y).MovementCost < 0.00001f)
            {
                // East or West is unwalkable, therefore this would be a clipped movement.
                return true;
            }

            if (Math.Abs(World.GetTileAt(Position.x, Position.y - dY).MovementCost) < 0.00001f)
            {
                // North or South is unwalkable, therefore this would be a clipped movement.
                return true;
            }

            // If we reach here, we are diagonal, but not clipping
        }

        // If we are here, we are either not clipping, or not diagonal
        return false;
    }

    public EnterState IsEnterable()
    {
        if (MovementCost == 0)
            return EnterState.Never;

        // Check furniture to see if has no restrictions to enter it

        if (Furniture != null && Furniture.FuncIsEnterable != null)
        {
            return Furniture.FuncIsEnterable(Furniture);
        }

        return EnterState.Yes;
    }

    public Tile North()
    {
        return World.GetTileAt(Position.x, Position.y + 1);
    }

    public Tile South()
    {
        return World.GetTileAt(Position.x, Position.y - 1);
    }

    public Tile East()
    {
        return World.GetTileAt(Position.x + 1, Position.y);
    }

    public Tile West()
    {
        return World.GetTileAt(Position.x - 1, Position.y);
    }

    /*******************************************************/
    /*                  FOR XML SERIALIZATON               */
    /*******************************************************/

    public Tile()
    {
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        TileType tileType = (TileType) Enum.Parse(typeof(TileType), reader.GetAttribute("Type"));
        ChangeTileType(this, tileType);
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("x", Position.x.ToString());
        writer.WriteAttributeString("y", Position.y.ToString());
        writer.WriteAttributeString("Type", Type.ToString());
    }
}