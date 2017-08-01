using System;
using UnityEngine;

public class Furniture
{
    public Tile Tile { get; protected set; }
    public string ObjectType { get; protected set; }

    // This is a multiplier. So value of "2" slows down twice as much. 
    // If equals 0, then it's impassable
    protected float MovementCost;

    protected int Width;
    protected int Height;

    public bool LinksToNeighbour { get; protected set; }

    public event Action<Furniture> CbOnChanged;
    Func<Tile, bool> FuncPositionValidation;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    protected Furniture()
    {
    }

    public static Furniture CreatePrototype(string objectType, float movementCost = 1f, int width = 1,
        int height = 1, bool linksToNeighbour = false)
    {
        var obj = new Furniture
        {
            ObjectType = objectType,
            MovementCost = movementCost,
            Width = width,
            Height = height,
            LinksToNeighbour = linksToNeighbour
        };
        obj.FuncPositionValidation = obj.__IsValidPosition;

        return obj;
    }

    public static Furniture PlaceInstance(Furniture prototype, Tile tile)
    {
        if (prototype.FuncPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position validity function returned FALSE; ");
            return null;
        }
        
        
        Furniture obj = new Furniture
        {
            ObjectType = prototype.ObjectType,
            MovementCost = prototype.MovementCost,
            Width = prototype.Width,
            Height = prototype.Height,
            LinksToNeighbour = prototype.LinksToNeighbour,
            Tile = tile
        };

        // TODO: This assumes we're 1 x 1
        if (obj.Tile.PlaceObject(obj) == false)
        {
            // Tile already has installed object 
            return null;
        }

        if (obj.LinksToNeighbour)
        {
            int x = Mathf.RoundToInt(obj.Tile.Position.x);
            int y = Mathf.RoundToInt(obj.Tile.Position.y);
            // This type of furniture links itself to it's neighbours. Update neighbours by triggering callback
            Tile t = obj.Tile.World.GetTileAt(x, y + 1);

            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }

            t = obj.Tile.World.GetTileAt(x + 1, y);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }

            t = obj.Tile.World.GetTileAt(x, y - 1);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }

            t = obj.Tile.World.GetTileAt(x - 1, y);
            if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }
            
        }

        return obj;
    }

    public bool IsValidPosition(Tile t)
    {
        return FuncPositionValidation(t);
    }

    // TODO: REMOVE THEM FROM HERE
    public bool __IsValidPosition(Tile tile)
    {
        if (tile.Type != TileType.GroundTiles)
        {
            return false;
        }

        if (tile.Furniture != null)
        {
            return false;
        }
        
        return true;
    }

    public bool __IsValidPosition_Door(Tile tile)
    {
        if (__IsValidPosition(tile) == false)
            return false;
        
        // Make sure we have a pair walls 
        return true;
    }
}