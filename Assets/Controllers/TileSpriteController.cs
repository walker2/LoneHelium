using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class TileSpriteController : MonoBehaviour
{
    public static TileSpriteController Instance { get; private set; }

    private Dictionary<Tile, GameObject> m_tileGameObjectMap;

    private Dictionary<string, Sprite> m_tileSprites;

    private static World m_world
    {
        get { return WorldController.Instance.World; }
    }

    private void Start()
    {
        LoadSprites();
        // Init dictionary, which glues Tiles and GameObjets

        m_tileGameObjectMap = new Dictionary<Tile, GameObject>();

        //Create a GameObject for each of our tiles, to show them on screen
        for (var x = 0; x < m_world.Width; x++)
        {
            for (var y = 0; y < m_world.Height; y++)
            {
                Tile tileData = m_world.GetTileAt(x, y);
                var tileGO = new GameObject("Tile_" + x + "_" + y);

                m_tileGameObjectMap.Add(tileData, tileGO);

                tileGO.transform.position = new Vector3(tileData.Position.x, tileData.Position.y, 0);
                tileGO.transform.SetParent(this.transform, true);

                if (Random.Range(0, 100) > 95)
                {
                    int num = Random.Range(2, 5);

                    tileGO.AddComponent<SpriteRenderer>().sprite = m_tileSprites["Darkness_" + num];
                }
                else
                {
                    tileGO.AddComponent<SpriteRenderer>().sprite = null;
                }
            }
        }

        m_world.CbTileChanged += OnTileChanged;
    }

    private void LoadSprites()
    {
        // Load Tile Sprites
        m_tileSprites = new Dictionary<string, Sprite>();
        var sprites = Resources.LoadAll<Sprite>("Images/Tiles/");

        Debug.Log("Loaded Tile Sprites");
        foreach (Sprite sprite in sprites)
        {
            m_tileSprites[sprite.name] = sprite;
        }
    }

    private void OnTileChanged(Tile tileData)
    {
        if (!m_tileGameObjectMap.ContainsKey(tileData))
        {
            Debug.LogError("m_tileGameObjectMap doen't contain the tileData");
            return;
        }

        GameObject tileGO = m_tileGameObjectMap[tileData];

        if (tileGO == null)
        {
            Debug.LogError("m_tileGameObjectMap returned null GameObject");
            return;
        }

        tileGO.GetComponent<SpriteRenderer>().sprite = GetSpriteForTile(tileData);
    }

    private Sprite GetSpriteForTile(Tile tileData)
    {
        if (tileData.Type == TileType.Empty)
            return null;

        string spriteName = tileData.Type + "_";
        var suffix = "";
        int x = Mathf.RoundToInt(tileData.Position.x);
        int y = Mathf.RoundToInt(tileData.Position.y);

        Tile t = m_world.GetTileAt(x, y + 1);

        if (t != null && t.Type == tileData.Type)
        {
            suffix += "N";
        }

        t = m_world.GetTileAt(x + 1, y);
        if (t != null && t.Type == tileData.Type)
        {
            suffix += "E";
        }

        t = m_world.GetTileAt(x, y - 1);
        if (t != null && t.Type == tileData.Type)
        {
            suffix += "S";
        }

        t = m_world.GetTileAt(x - 1, y);
        if (t != null && t.Type == tileData.Type)
        {
            suffix += "W";
        }

        if (suffix == "NESW")
        {
            if (Random.Range(0, 100) > 90)
            {
                int num = Random.Range(1, 6);
                suffix += "_" + num;
            }
        }
        spriteName += suffix;

        /*if (!suffix.Contains("S"))
        {
            t = m_world.GetTileAt(x, y - 1);
            if (t.Type == TileType.Empty)
            {
                var tileGO = m_tileGameObjectMap[t];
                tileGO.GetComponent<SpriteRenderer>().sprite = m_tileSprites[spriteName + "B"];
            }
        }*/
            

        if (m_tileSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForTile -- No sprite with name: " + spriteName);
            return null;
        }
        return m_tileSprites[spriteName];
    }
}