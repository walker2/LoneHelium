using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; private set; }

    public World World { get; protected set; }

    private static bool m_loadWorld = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;


        if (m_loadWorld)
        {
            m_loadWorld = false;
            CreateWorldFromFile();
        }
        else
        {
            CreateEmptyWorld();
        }
    }

    public void NewWorld()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        CreateEmptyWorld();
    }

    public void SaveWorld()
    {
        var xmlSerializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        xmlSerializer.Serialize(writer, World);
        writer.Close();

        Debug.Log(writer.ToString());

        PlayerPrefs.SetString("Save000", writer.ToString());
    }

    public void LoadWorld()
    {
        WorldController.m_loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the scene and clean all screen
    }

    private void Update()
    {
        // TODO: Add pause/unpause, speed controls, etc.
        World.Update(Time.deltaTime);
    }

    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x);
        int y = Mathf.RoundToInt(coord.y);

        return WorldController.Instance.World.GetTileAt(x, y);
    }

    private void CreateEmptyWorld()
    {
        World = new World(100, 100);

        Camera.main.transform.Translate(World.Width / 2f, World.Height / 2f, -10);
        
        World.CreateCharacter(World.GetTileAt(World.Width / 2, World.Height / 2));
    }

    private void CreateWorldFromFile()
    {
        Debug.Log("Created World from file");
        
        var xmlSerializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("Save000"));
        World = (World) xmlSerializer.Deserialize(reader);
        reader.Close();

        Camera.main.transform.Translate(World.Width / 2f, World.Height / 2f, -10);
    }
}