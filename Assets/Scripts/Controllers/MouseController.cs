using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    public GameObject TileSelectedPrefab;
    
    private Vector3 m_lastFramePosition;
    private Vector3 m_dragStartPosition;
    private Vector3 m_currentFramePosition;
    private List<GameObject> m_draggedGameObjects;

    void Start()
    {
        m_draggedGameObjects = new List<GameObject>();
    }

    void Update()
    {
        m_currentFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        m_currentFramePosition.z = 0;

        UpdateDragging();
        UpdateCameraMovement();

        m_lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        m_lastFramePosition.z = 0;
    }

    public void UpdateDragging()
    {
        // If we are over UI - stop
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Start drag
        if (Input.GetMouseButtonDown(0))
        {
            m_dragStartPosition = m_currentFramePosition;
        }

        // Cacluate indexes
        var start = new Vector2(Mathf.RoundToInt(m_dragStartPosition.x), Mathf.RoundToInt(m_dragStartPosition.y));
        var end = new Vector2(Mathf.RoundToInt(m_currentFramePosition.x), Mathf.RoundToInt(m_currentFramePosition.y));

        CalculateIndexes(ref start.x, ref end.x);
        CalculateIndexes(ref start.y, ref end.y);
        
        // Clean up old drag previews 

        foreach (GameObject obj in m_draggedGameObjects)
        {
            SimplePool.Despawn(obj);
        }
        m_draggedGameObjects.Clear();

        if (Input.GetMouseButton(0))
        {
            // Display a preview of the drag area 
            for (var x = start.x; x <= end.x; x++)
            {
                for (var y = start.y; y <= end.y; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        // Display the building hint 
                        GameObject go = SimplePool.Spawn(TileSelectedPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        go.transform.SetParent(this.transform, true);
                        m_draggedGameObjects.Add(go);
                    }
                }
            }
        }

        // End drag
        if (Input.GetMouseButtonUp(0))
        {
            var bmc = GameObject.FindObjectOfType<BuildModeController>();
            
            for (var x = start.x; x <= end.x; x++)
            {
                for (var y = start.y; y <= end.y; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        bmc.DoBuild(t);
                    }
                }
            }
        }
    }

    private void UpdateCameraMovement()
    {
        // Handle camera movement with RMB dragging
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            Vector3 diff = m_lastFramePosition - m_currentFramePosition;
            Camera.main.transform.Translate(diff);
        }

        // Zooming
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 5f, 25f);
    }

    private void CalculateIndexes(ref float start, ref float end)
    {
        if (end >= start) 
            return;
        
        var tmp = end;
        end = start;
        start = tmp;
    }
}