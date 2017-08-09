using System.Collections.Generic;
using UnityEngine;

public class Room
{
    // Placeholder 
    public float AtmosO2 = 0;

    public float AtmosN = 0;
    public float AtmosCO2 = 0;

    private List<Tile> m_tiles;

    public Room()
    {
        m_tiles = new List<Tile>();
    }

    public void AssignTile(Tile tile)
    {
        if (m_tiles.Contains(tile))
            return;

        if (tile.Room != null)
            tile.Room.m_tiles.Remove(tile);

        tile.Room = this;
        m_tiles.Add(tile);
    }

    public void UnassignAllTiles()
    {
        foreach (Tile t in m_tiles)
        {
            t.Room = t.World.GetOuterSpace(); // Assign to outside
        }
        m_tiles = new List<Tile>();
    }

    public static void DoRoomFloodFill(Furniture furniture)
    {
        // Check NESW neighbours of furniture's tile
        // and do flood fill from them

        World world = furniture.Tile.World;
        Room oldRoom = furniture.Tile.Room;

        // Try building a new Rooms for each dir
        foreach (Tile tile in furniture.Tile.GetNeighbours())
        {
            if (tile != null)
                FloodFill(tile, oldRoom);
        }

        furniture.Tile.Room = null;
        oldRoom.m_tiles.Remove(furniture.Tile);

        // If furniture was added to an existing Room
        if (oldRoom != world.GetOuterSpace())
        {
            if (oldRoom.m_tiles.Count > 0)
                Debug.LogError("Old Room still has tiles assigned to it");
            world.DeleteRoom(oldRoom);
        }
    }

    private static void FloodFill(Tile tile, Room oldRoom)
    {
        if (tile == null || tile.Room != oldRoom || tile.Furniture != null && tile.Furniture.RoomEnclosure)
            return;

        if (tile.Type == TileType.Empty)
            return;

        var newRoom = new Room();
        var tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);

        while (tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();


            if (t.Room == oldRoom)
            {
                newRoom.AssignTile(t);

                Tile[] ns = t.GetNeighbours();
                foreach (Tile t2 in ns)
                {
                    if (t2 == null || t2.Type == TileType.Empty)
                    {
                        newRoom.UnassignAllTiles();
                        return;
                    }

                    if (t2.Room == oldRoom && (t2.Furniture == null || t2.Furniture.RoomEnclosure == false))
                        tilesToCheck.Enqueue(t2);
                }
            }
        }

        Debug.Log("Add new room");
        // Tell the world that the new Room has been formed
        tile.World.AddRoom(newRoom);
    }
}