using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

public class World : IXmlSerializable
{
    public Tile[,] Tiles;

    public List<Character> CharactersList { get; protected set; }
    public List<Furniture> FurnitureList { get; protected set; }
    public List<Room> RoomList { get; protected set; }

    private Dictionary<string, Furniture> m_furniturePrototypes;
    public TileGraph TileGraph;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public event Action<Furniture> CbFurnitureCreated;
    public event Action<Character> CbCharacterCreated;
    public event Action<Tile> CbTileChanged;

    public JobQueue JobQueue;

    public World(int width, int height)
    {
        SetUpWorld(width, height);
    }

    public Room GetOuterSpace()
    {
        return RoomList[0];
    }

    public void DeleteRoom(Room room)
    {
        if (room == GetOuterSpace())
        {
            Debug.LogError("Tried to delete outer space");
        }
        RoomList.Remove(room);
        
        room.UnassignAllTiles();
    }

    public void AddRoom(Room room)
    {
        RoomList.Add(room);
    }

    private void SetUpWorld(int width, int height)
    {
        JobQueue = new JobQueue();
        Width = width;
        Height = height;

        Tiles = new Tile[Width, Height];
        RoomList = new List<Room>();
        RoomList.Add(new Room()); // Create the outside room?

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                Tiles[x, y] = new Tile(this, new Vector2(x, y));
                Tiles[x, y].CbTileTypeChanged += OnTileChanged;
                Tiles[x, y].Room = GetOuterSpace(); // Rooms 0 is outside 
            }
        }

        Debug.Log("World created with " + (Width * Height) + " tiles");

        CreateFurniturePrototypes();

        CharactersList = new List<Character>();
        FurnitureList = new List<Furniture>();
    }

    public void Update(float deltaTime)
    {
        foreach (var character in CharactersList)
        {
            character.Update(deltaTime);
        }

        foreach (var furniture in FurnitureList)
        {
            furniture.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile tile)
    {
        var chr = new Character(tile);
        CharactersList.Add(chr);
        if (CbCharacterCreated != null)
            CbCharacterCreated(chr);

        return chr;
    }

    private void CreateFurniturePrototypes()
    {
        // This will be replaced by a function that reads all types of furniture from file
        m_furniturePrototypes =
            new Dictionary<string, Furniture>
            {
                {"Wall", new Furniture("Wall", 0f, 1, 1, true, true)},
                {"Door", new Furniture("Door", 1f, 1, 1, false, true)}
            };

        m_furniturePrototypes["Door"].FurnParameters["openness"] = 0;
        m_furniturePrototypes["Door"].FurnParameters["isOpening"] = 0;
        m_furniturePrototypes["Door"].UpdateActions += FurnitureActions.Door_UpdateAction;
        m_furniturePrototypes["Door"].FuncIsEnterable = FurnitureActions.Door_Is_Enterable;
    }

    public void RandomizeTiles()
    {
        Debug.Log("RandomizeTiles");
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                Tiles[x, y].Type = Random.Range(0, 2) == 0 ? TileType.Empty : TileType.Ground;
            }
        }
    }

    public void SetupPathfindingTestExample()
    {
        Debug.Log("SetupPathfindingTestExample");

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b - 5; y < b + 15; y++)
            {
                Tile.ChangeTileType(Tiles[x, y], TileType.Ground);

                if (x == 1 || x == l + 9 || y == b || y == b + 9)
                {
                    if (x != l + 5 && y != b + 4)
                    {
                        PlaceFurniture("Wall", Tiles[x, y]);
                    }
                }
            }
        }
    }

    public Tile GetTileAt(float x, float y)
    {
        return GetTileAt((int) x, (int) y);
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0)
        {
            //Debug.LogError("Tile (" + x + "," + y + ") is out of range");
            return null;
        }

        return Tiles[x, y];
    }

    public Furniture PlaceFurniture(string objectType, Tile t)
    {
        // TODO: Only 1 x 1 tiles

        if (m_furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("m_furniturePrototypes doesn't contain a prototype for key:" + objectType);
            return null;
        }

        Furniture furn = Furniture.PlaceInstance(m_furniturePrototypes[objectType], t);

        if (furn == null)
        {
            // Failed to place object
            return null;
        }

        FurnitureList.Add(furn);

        if (furn.RoomEnclosure)
        {
            Room.DoRoomFloodFill(furn);
        }

        if (CbFurnitureCreated != null)
        {
            CbFurnitureCreated(furn);

            if (Math.Abs(furn.MovementCost - 1) > 0.0001f)
            {
                InvalidateTileGraph();
            }
        }

        return furn;
    }

    private void OnTileChanged(Tile t)
    {
        if (CbTileChanged == null)
            return;

        CbTileChanged(t);
        InvalidateTileGraph();
    }

    private void InvalidateTileGraph()
    {
        TileGraph = null;
    }

    public bool IsFurniturePlacementValid(string furnType, Tile t)
    {
        return m_furniturePrototypes[furnType].IsValidPosition(t);
    }

    public Furniture GetFurniturePrototype(string furnType)
    {
        if (m_furniturePrototypes.ContainsKey(furnType) == false)
        {
            Debug.LogError("GetFurniturePrototype -- m_furniturePrototypes doesn't contain furniture type: " +
                           furnType);
            return null;
        }

        return m_furniturePrototypes[furnType];
    }

    /*******************************************************/
    /*                  FOR XML SERIALIZATON               */
    /*******************************************************/

    public World()
    {
        // Do not use it 
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        // Load info here
        Debug.Log("ReadXML");
        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));

        SetUpWorld(Width, Height);

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furniture":
                    ReadXml_Furniture(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }
    }

    private void ReadXml_Tiles(XmlReader reader)
    {
        if (reader.ReadToDescendant("Tile"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));

                Tiles[x, y].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));
        }
    }

    private void ReadXml_Furniture(XmlReader reader)
    {
        if (reader.ReadToDescendant("Furn"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("ObjectType"), Tiles[x, y]);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furn"));
        }
    }

    private void ReadXml_Characters(XmlReader reader)
    {
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));

                var c = CreateCharacter(Tiles[x, y]);
                c.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        // Save info here

        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Tiles");

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Tiles[x, y].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    Tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Furniture");

        foreach (var furniture in FurnitureList)
        {
            writer.WriteStartElement("Furn");
            furniture.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("Characters");

        foreach (var character in CharactersList)
        {
            writer.WriteStartElement("Character");
            character.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }
}