using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Pathfinding;
using UnityEngine;

public class Character : IXmlSerializable
{
    public Vector2 Position
    {
        get
        {
            return new Vector2(
                Mathf.Lerp(CurrTile.Position.x, m_nextTile.Position.x, m_movementPercentage),
                Mathf.Lerp(CurrTile.Position.y, m_nextTile.Position.y, m_movementPercentage));
        }
    }

    public Tile CurrTile { get; private set; }
    public Tile DestTile { get; private set; } // If we aren't moving, then dest = curr

    private Tile m_nextTile; // Next tile in pathfinding sequence 
    private AStar m_pathAStar;
    private float m_movementPercentage;

    public float Speed = 12.0f; // Tiles per second

    public event Action<Character> CbCharacterChanged;


    private Job m_job;

    public Character(Tile tile)
    {
        CurrTile = DestTile = m_nextTile = tile;
    }

    private void DoJob(float deltaTime)
    {
        // Do I have a job?
        if (m_job == null)
        {
            // Grab a new job
            m_job = CurrTile.World.JobQueue.Dequeue();

            if (m_job != null)
            {
                DestTile = m_job.Tile;
                m_job.CbJobComplete += OnJobEnded;
                m_job.CbJobCancel += OnJobEnded;
            }
        }

        if (CurrTile == DestTile)
        {
            if (m_job != null)
            {
                m_job.DoWork(deltaTime);
            }
        }
    }

    public void AbandonJob()
    {
        m_nextTile = DestTile = CurrTile;
        m_pathAStar = null;
        CurrTile.World.JobQueue.Enqueue(m_job);
        m_job = null;
    }

    private void Move(float deltaTime)
    {
        if (CurrTile == DestTile)
        {
            m_pathAStar = null;
            return;
        }

        if (m_nextTile == null || m_nextTile == CurrTile)
        {
            if (m_pathAStar == null || m_pathAStar.Length() == 0)
            {
                // Generate the path 
                m_pathAStar = new AStar(CurrTile.World, CurrTile, DestTile);
                if (m_pathAStar.Length() == 0)
                {
                    Debug.LogError("AStar -- returned no path to destination");
                    // TODO: Cancel Job? Should be re-enqueued 
                    AbandonJob();
                    m_pathAStar = null;
                    return;
                }
                m_nextTile = m_pathAStar.Dequeue();
            }

            // Grap the next Node from path
            m_nextTile = m_pathAStar.Dequeue();

            if (m_nextTile == CurrTile)
                Debug.LogError("Character::Move -- next Tile is currTile");
        }


        float distanceToTravel = Vector2.Distance(CurrTile.Position, m_nextTile.Position);

        if (m_nextTile.IsEnterable() == EnterState.Never)
        {
            Debug.LogError("FIXME: A character was trying to enter unwalkable tile");
            m_nextTile = null;
            m_pathAStar = null;
            return;
        }

        if (m_nextTile.IsEnterable() == EnterState.Soon)
        {
            return;
        }
        
        
        float distanceThisFrame = Speed / m_nextTile.MovementCost * deltaTime;

        float percThisFrame = distanceThisFrame / distanceToTravel;

        m_movementPercentage += percThisFrame;

        if (m_movementPercentage >= 1.0f)
        {
            // TODO: Get next tile from pathFinding system
            CurrTile = m_nextTile;
            m_movementPercentage = 0;
        }
    }

    public void Update(float deltaTime)
    {
        DoJob(deltaTime);
        Move(deltaTime);

        if (CbCharacterChanged != null)
            CbCharacterChanged(this);
    }

    public void SetDestination(Tile tile)
    {
        if (CurrTile.IsNeighbour(tile, true) == false)
        {
            Debug.Log("Character::SetDestination -- Our destination tile isn't actually our neighbour");
        }

        DestTile = tile;
    }

    private void OnJobEnded(Job j)
    {
        // Job completed or was canceled 

        if (j != m_job)
        {
            Debug.LogError(
                "Character being told about job that isn't his. Must be forgetting about unregistering something");
            return;
        }
        m_job = null;
    }

    /*******************************************************/
    /*                  FOR XML SERIALIZATON               */
    /*******************************************************/

    public Character()
    {
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        return;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("x", CurrTile.Position.x.ToString());
        writer.WriteAttributeString("y", CurrTile.Position.y.ToString());
    }
}