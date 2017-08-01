using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour
{
    private Dictionary<Character, GameObject> m_characterGameObjectMap;

    private Dictionary<string, Sprite> m_characterSprites;

    private static World World
    {
        get { return WorldController.Instance.World; }
    }

    private void Start()
    {
        LoadSprites();

        m_characterGameObjectMap = new Dictionary<Character, GameObject>();

        World.CbCharacterCreated += OnCharacterCreated;

        //DEBUG 
        var character = World.CreateCharacter(World.GetTileAt(World.Width / 2, World.Height / 2));
        //character.SetDestination(World.GetTileAt(World.Width / 2 + 5, World.Height / 2));
    }

    private void LoadSprites()
    {
        // Load Furniture Sprites
        m_characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Characters/");

        Debug.Log("Loaded Character Sprites");
        foreach (Sprite sprite in sprites)
        {
            m_characterSprites[sprite.name] = sprite;
        }
    }

    private void OnCharacterCreated(Character character)
    {
        // TODO: Does not consider multitiled object nor object rotation

        var charGameObject = new GameObject("Character");
        m_characterGameObjectMap.Add(character, charGameObject);

        charGameObject.transform.position =
            new Vector3(character.Position.x, character.Position.y, 0);
        charGameObject.transform.SetParent(this.transform, true);

        charGameObject.AddComponent<SpriteRenderer>().sprite = m_characterSprites["Peasant"];
        // TODO: We assume that the object must be a wall
        charGameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Characters";

        character.CbCharacterChanged += OnCharacterChanged;
    }

    private void OnCharacterChanged(Character character)
    {
        // Make sure the furniture's graphics are correct 
        if (m_characterGameObjectMap.ContainsKey(character) == false)
        {
            Debug.LogError("OnCharacterChanged " +
                           "-- Trying to change visuals for character that not in map m_characterGameObjectMap");
            return;
        }

        GameObject charGameObject = m_characterGameObjectMap[character];
        charGameObject.transform.position = new Vector3(character.Position.x, character.Position.y, 0);
    }
}