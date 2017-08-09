using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class Furniture : IXmlSerializable
{
    public Dictionary<string, float> FurnParameters;
    public event Action<Furniture, float> UpdateActions;

    public void Update(float deltaTime)
    {
        if (UpdateActions != null)
            UpdateActions(this, deltaTime);
    }

    public Tile Tile { get; protected set; }
    public string ObjectType { get; protected set; }

    // This is a multiplier. So value of "2" slows down twice as much. 
    // If equals 0, then it's impassable
    public float MovementCost { get; protected set; }

    public bool RoomEnclosure { get; protected set; }

    protected int Width;
    protected int Height;

    public bool LinksToNeighbour { get; protected set; }

    public event Action<Furniture> CbOnChanged;
    public Func<Tile, bool> FuncPositionValidation;
    public Func<Furniture, EnterState> FuncIsEnterable;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    protected Furniture(Furniture furniture)
    {
        ObjectType = furniture.ObjectType;
        MovementCost = furniture.MovementCost;
        RoomEnclosure = furniture.RoomEnclosure;
        Width = furniture.Width;
        Height = furniture.Height;
        LinksToNeighbour = furniture.LinksToNeighbour;
        Tile = furniture.Tile;

        FurnParameters = new Dictionary<string, float>(furniture.FurnParameters);
        if (furniture.UpdateActions != null)
            UpdateActions = (Action<Furniture, float>) furniture.UpdateActions.Clone();

        FuncIsEnterable = furniture.FuncIsEnterable;
    }

    public virtual Furniture Clone()
    {
        return new Furniture(this);
    }

    public Furniture(string objectType, float movementCost = 1f, int width = 1,
        int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false)
    {
        ObjectType = objectType;
        MovementCost = movementCost;
        Width = width;
        Height = height;
        LinksToNeighbour = linksToNeighbour;
        RoomEnclosure = roomEnclosure;

        FuncPositionValidation = __IsValidPosition;
        FurnParameters = new Dictionary<string, float>();
    }

    public static Furniture PlaceInstance(Furniture prototype, Tile tile)
    {
        if (prototype.FuncPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position validity function returned FALSE; ");
            return null;
        }

        Furniture obj = prototype.Clone();
        obj.Tile = tile;

        // TODO: This assumes we're 1 x 1!
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

            if (t != null && t.Furniture != null && t.Furniture.CbOnChanged != null &&
                t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }

            t = obj.Tile.World.GetTileAt(x + 1, y);
            if (t != null && t.Furniture != null && t.Furniture.CbOnChanged != null &&
                t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }

            t = obj.Tile.World.GetTileAt(x, y - 1);
            if (t != null && t.Furniture != null && t.Furniture.CbOnChanged != null &&
                t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }

            t = obj.Tile.World.GetTileAt(x - 1, y);
            if (t != null && t.Furniture != null && t.Furniture.CbOnChanged != null &&
                t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.CbOnChanged(t.Furniture);
            }
        }

        return obj;
    }

    public void Changed()
    {
        if (CbOnChanged != null)
            CbOnChanged(this);
    }

    public bool IsValidPosition(Tile t)
    {
        return FuncPositionValidation(t);
    }

    // TODO: REMOVE THEM FROM HERE
    public bool __IsValidPosition(Tile tile)
    {
        if (tile.Type != TileType.Ground)
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

        // Make sure we have a pair Wall 
        return true;
    }


    /*******************************************************/
    /*                  FOR XML SERIALIZATON               */
    /*******************************************************/

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        MovementCost = int.Parse(reader.GetAttribute("MovementCost"));

        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string key = reader.GetAttribute("Name");
                float value = float.Parse(reader.GetAttribute("Value"));
                FurnParameters[key] = value;
            } while (reader.ReadToNextSibling("Param"));
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("x", Tile.Position.x.ToString());
        writer.WriteAttributeString("y", Tile.Position.y.ToString());
        writer.WriteAttributeString("ObjectType", ObjectType);
        writer.WriteAttributeString("MovementCost", MovementCost.ToString());

        foreach (string key in FurnParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("Name", key);
            writer.WriteAttributeString("Value", FurnParameters[key].ToString());
            writer.WriteEndElement();
        }
    }
}