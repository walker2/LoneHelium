using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class World
{
    private Tile[,] m_tiles;

    private List<Character> m_characters;
    private Dictionary<string, Furniture> m_furniturePrototypes;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public event Action<Furniture> CbFurnitureCreated;
    public event Action<Character> CbCharacterCreated; 
    public event Action<Tile> CbTileChanged;

    public JobQueue JobQueue;


    public World(int width = 100, int height = 100)
    {
        JobQueue = new JobQueue();
        Width = width;
        Height = height;

        m_tiles = new Tile[Width, Height];

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                m_tiles[x, y] = new Tile(this, new Vector2(x, y));
                m_tiles[x, y].CbTileTypeChanged += OnTileChanged;
            }
        }

        Debug.Log("World created with " + (Width * Height) + " tiles");

        CreateFurniturePrototypes();

        m_characters = new List<Character>();
    }

    public void Update(float deltaTime)
    {
        foreach (var character in m_characters)
        {
            character.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile tile)
    {
        var chr = new Character(m_tiles[Width / 2, Height / 2]);
        m_characters.Add(chr);
        if (CbCharacterCreated != null) 
            CbCharacterCreated(chr);

        return chr;
    }
    private void CreateFurniturePrototypes()
    {
        m_furniturePrototypes =
            new Dictionary<string, Furniture>
            {
                {"Walls", Furniture.CreatePrototype("Walls", 0f, 1, 1, true)},
                {"Door", Furniture.CreatePrototype("Door", 1f, 1, 1, true)}
            };
    }

    public void RandomizeTiles()
    {
        Debug.Log("RandomizeTiles");
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                m_tiles[x, y].Type = Random.Range(0, 2) == 0 ? TileType.Empty : TileType.GroundTiles;
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

        return m_tiles[x, y];
    }

    public void PlaceFurniture(string objectType, Tile t)
    {
        // TODO: Only 1 x 1 tiles

        if (m_furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("m_furniturePrototypes doesn't contain a prototype for key:" + objectType);
            return;
        }

        Furniture obj = Furniture.PlaceInstance(m_furniturePrototypes[objectType], t);

        if (obj == null)
        {
            // Failed to place object
            return;
        }

        if (CbFurnitureCreated != null)
        {
            CbFurnitureCreated(obj);
        }
    }

    private void OnTileChanged(Tile t)
    {
        if (CbTileChanged == null)
            return;

        CbTileChanged(t);
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
}