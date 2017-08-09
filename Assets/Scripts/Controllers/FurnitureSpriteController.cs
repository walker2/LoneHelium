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

        foreach (Furniture furniture in World.FurnitureList)
        {
            OnFurnitureCreated(furniture);
        }
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

    private void OnFurnitureCreated(Furniture furniture)
    {
        // TODO: Does not consider multitiled object nor object rotation

        var furnGameObject =
            new GameObject(furniture.ObjectType + "_" + furniture.Tile.Position.x + "_" + furniture.Tile.Position.y);

        m_furnitureGameObjectMap.Add(furniture, furnGameObject);

        furnGameObject.transform.position = new Vector3(furniture.Tile.Position.x, furniture.Tile.Position.y, 0);
        furnGameObject.transform.SetParent(this.transform, true);

        var sr = furnGameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furniture);
        sr.sortingLayerName = furniture.ObjectType == "Door" ? "Door" : "Furniture";
        sr.sortingOrder = (int) (furniture.Tile.Position.y * -2);

        furniture.CbOnChanged += OnFurnitureObjectChanged;
    }

    private void OnFurnitureObjectChanged(Furniture furniture)
    {
        // Make sure the furniture's graphics are correct 
        if (m_furnitureGameObjectMap.ContainsKey(furniture) == false)
        {
            Debug.LogError("OnFurnitureObjectChanged " +
                           "-- Trying to change visuals for furniture that not in map m_furnitureGameObjectMap");
            return;
        }

        GameObject furnGameObject = m_furnitureGameObjectMap[furniture];
        furnGameObject.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furniture);
    }

    public Sprite GetSpriteForFurniture(Furniture furniture)
    {
        // If this is a DOOR, check it's openness and update the sprite
        // TODO: Fix this hardcoding


        string spriteName = furniture.ObjectType + "_";
        var suffix = "";
        int x = Mathf.RoundToInt(furniture.Tile.Position.x);
        int y = Mathf.RoundToInt(furniture.Tile.Position.y);

        if (furniture.ObjectType == "Door")
        {
            Tile tileN = World.GetTileAt(x, y + 1);
            Tile tileS = World.GetTileAt(x, y - 1);
            Tile tileE = World.GetTileAt(x + 1, y);
            Tile tileW = World.GetTileAt(x - 1, y);

            if (tileN != null && tileS != null &&
                tileN.Furniture != null && tileS.Furniture != null &&
                tileN.Furniture.ObjectType == "Wall" &&
                tileS.Furniture.ObjectType == "Wall")
            {
                suffix += "vert_";
            }
            else if (tileE != null && tileW != null &&
                     tileE.Furniture != null && tileW.Furniture != null &&
                     tileE.Furniture.ObjectType == "Wall" &&
                     tileW.Furniture.ObjectType == "Wall")
            {
                suffix += "hor_";
            }
            else
            {
                Debug.Log("You can't build door here");
                furniture = null;
                return null;
            }
                
            if (furniture.FurnParameters["openness"] < 0.1f)
            {
                // Door is closed 
                suffix += "closed";
                //return m_furnitureSprites[furniture.ObjectType];
            }
            else if (furniture.FurnParameters["openness"] < 0.5f)
            {
                // Door is opening 
                suffix += "opening";
                //return m_furnitureSprites[furniture.ObjectType + "_opening"];
            }
            else
            {
                suffix += "open";
            }
            spriteName += suffix;

            if (m_furnitureSprites.ContainsKey(spriteName) == false)
            {
                Debug.LogError("GetSpriteForFurniture -- No sprite with name: " + spriteName);
                return null;
            }

            return m_furnitureSprites[spriteName];
        }

        if (furniture.LinksToNeighbour == false)
        {
            return m_furnitureSprites[furniture.ObjectType];
        }


        Tile t = World.GetTileAt(x, y + 1);

        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furniture.ObjectType)
        {
            suffix += "N";
        }

        t = World.GetTileAt(x + 1, y);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furniture.ObjectType)
        {
            suffix += "E";
        }

        t = World.GetTileAt(x, y - 1);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furniture.ObjectType)
        {
            suffix += "S";
        }

        t = World.GetTileAt(x - 1, y);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furniture.ObjectType)
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