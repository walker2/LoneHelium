using System;
using UnityEngine;

public class Character
{
    public Vector2 Position
    {
        get
        {
            return new Vector2(
                Mathf.Lerp(CurrTile.Position.x, DestTile.Position.x, m_movementPercentage),
                Mathf.Lerp(CurrTile.Position.y, DestTile.Position.y, m_movementPercentage));
        }
    }

    public Tile CurrTile { get; private set; }
    public Tile DestTile { get; private set; } // If we aren't moving, then dest = curr
    private float m_movementPercentage;

    public float Speed = 12.0f; // Tiles per second

    public event Action<Character> CbCharacterChanged;


    private Job m_job;

    public Character(Tile tile)
    {
        CurrTile = DestTile = tile;
    }

    public void Update(float deltaTime)
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
            return;
        }

        float distanceToTravel = Vector2.Distance(CurrTile.Position, DestTile.Position);

        float distanceThisFrame = Speed * deltaTime;

        float percThisFrame = distanceThisFrame / distanceToTravel;

        m_movementPercentage += percThisFrame;

        if (m_movementPercentage >= 1.0f)
        {
            CurrTile = DestTile;
            m_movementPercentage = 0;

            // TODO: Do we actually want to retain overshot movement?
        }

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
}