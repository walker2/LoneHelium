using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour
{
    protected bool BuildModeIsObjects = false;
    protected TileType BuildModeTileType = TileType.Ground;
    protected string BuildModeObjectType;

    public void DoBuild(Tile t)
    {
        if (BuildModeIsObjects)
        {
            // Create the installed object and assign it to the Tile
            // TODO: instantly builds furniture

            // Can we build the furniture in the selected tile
            // Run the valid placement function

            string furnType = BuildModeObjectType;

            if (WorldController.Instance.World.IsFurniturePlacementValid(furnType, t) == false
                || t.PendingFutureJob != null)
                return;

            var j = new Job(t, furnType,
                (job) =>
                {
                    WorldController.Instance.World.PlaceFurniture(furnType, job.Tile);
                    t.PendingFutureJob = null;
                });

            // TODO: MEH
            t.PendingFutureJob = j;
            j.CbJobComplete += (job) => { job.Tile.PendingFutureJob = null; };
            
            WorldController.Instance.World.JobQueue.Enqueue(j);
        }
        else
        {
            // Change TileType
            Tile.ChangeTileType(t, BuildModeTileType);
        }
    }

    public void SetupPathfindingTestExample()
    {
        WorldController.Instance.World.SetupPathfindingTestExample();
    }
    
    public void SetMode_BuildFloor()
    {
        BuildModeIsObjects = false;
        BuildModeTileType = TileType.Ground;
    }

    public void SetMode_Destroy()
    {
        BuildModeIsObjects = false;
        BuildModeTileType = TileType.Empty;
    }

    public void SetMode_BuildFurniture(string objectType)
    {
        BuildModeIsObjects = true;
        BuildModeObjectType = objectType;
    }
}