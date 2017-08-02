using System.Collections.Generic;
using UnityEngine;


public class FurnitureSpriteController : MonoBehaviour
{
    public static TileSpriteController Instance { get; private set; }

    private Dictionary<Furniture, GameObject> m_furnitureGameObjectMap;

    private Dictionary<string, Sprite> m_furnitureSprites;

    private static World World
    {
        get { return WorldController.Instance.World; }
    }

    private void Start()
    {
        LoadSprites();

        m_furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        World.CbFurnitureCreated += OnFurnitureCreated;
    }

    private void LoadSprites()
    {
        // Load Furniture Sprites
        m_furnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furniture/");

        Debug.Log("Loaded Furniture Sprites");
        foreach (Sprite sprite in sprites)
        {
            m_furnitureSprites[sprite.name] = sprite;
        }
    }

    private void OnFurnitureCreated(Furniture obj)
    {
        // TODO: Does not consider multitiled object nor object rotation

        var furnGameObject = new GameObject(obj.ObjectType + "_" + obj.Tile.Position.x + "_" + obj.Tile.Position.y);

        m_furnitureGameObjectMap.Add(obj, furnGameObject);

        furnGameObject.transform.position = new Vector3(obj.Tile.Position.x, obj.Tile.Position.y, 0);
        furnGameObject.transform.SetParent(this.transform, true);

        var sr = furnGameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(obj);
        sr.sortingLayerName = "Furniture";
        sr.sortingOrder = (int) (obj.Tile.Position.x * obj.Tile.Position.y * -2);

        obj.CbOnChanged += OnFurnitureObjectChanged;
    }

    private void OnFurnitureObjectChanged(Furniture obj)
    {
        // Make sure the furniture's graphics are correct 
        if (m_furnitureGameObjectMap.ContainsKey(obj) == false)
        {
            Debug.LogError("OnFurnitureObjectChanged " +
                           "-- Trying to change visuals for furniture that not in map m_furnitureGameObjectMap");
            return;
        }

        GameObject furnGameObject = m_furnitureGameObjectMap[obj];
        furnGameObject.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(obj);
    }

    public Sprite GetSpriteForFurniture(Furniture obj)
    {
        if (obj.LinksToNeighbour == false)
        {
            return m_furnitureSprites[obj.ObjectType];
        }

        string spriteName = obj.ObjectType + "_";
        var suffix = "";
        int x = Mathf.RoundToInt(obj.Tile.Position.x);
        int y = Mathf.RoundToInt(obj.Tile.Position.y);

        Tile t = World.GetTileAt(x, y + 1);

        if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
        {
            /*if (m_furnitureGameObjectMap[t.Furniture].name == (obj.ObjectType + "_ES"))
            {
                
            }*/
            suffix += "N";
        }

        t = World.GetTileAt(x + 1, y);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
        {
            suffix += "E";
        }

        t = World.GetTileAt(x, y - 1);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
        {
            suffix += "S";
        }

        t = World.GetTileAt(x - 1, y);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == obj.ObjectType)
        {
            suffix += "W";
        }

        spriteName += suffix;
        if (m_furnitureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForFurniture -- No sprite with name: " + spriteName);
            return null;
        }
        return m_furnitureSprites[spriteName];
    }

    public Sprite GetSpriteForFurniture(string furnType)
    {
        if (m_furnitureSprites.ContainsKey(furnType))
            return m_furnitureSprites[furnType];

        if (m_furnitureSprites.ContainsKey(furnType + "_"))
            return m_furnitureSprites[furnType + "_"];


        Debug.LogError("GetSpriteForFurniture -- No sprite with name: " + furnType);
        return null;
    }
}