using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class TileGraph
    {
        // This class constructs a simple pathfinding graph of our world.
        public Dictionary<Tile, Node<Tile>> Nodes;

        public TileGraph(World world)
        {
            Nodes = new Dictionary<Tile, Node<Tile>>();
            // For each tile create a node 
            for (int x = 0; x < world.Width; x++)
            {
                for (int y = 0; y < world.Height; y++)
                {
                    Tile t = world.GetTileAt(x, y);

                    //if (t.MovementCost > 0f)
                    //{
                    var node = new Node<Tile>
                    {
                        Data = t
                    };

                    Nodes.Add(t, node);
                    //}
                }
            }

            foreach (Tile t in Nodes.Keys)
            {
                Node<Tile> node = Nodes[t];

                var edges = new List<Edge<Tile>>();

                Tile[] tiles = t.GetNeighbours(true);

                // Get a list of neighbours for the tile
                // If neighbour is walkable -- create an edge with movement cost
                foreach (Tile neighbourTile in tiles)
                {
                    if (neighbourTile != null && neighbourTile.MovementCost > 0 && t.IsClippingCorner(neighbourTile) == false)
                    {
                        var edge = new Edge<Tile>
                        {
                            Cost = neighbourTile.MovementCost,
                            Node = Nodes[neighbourTile]
                        };

                        edges.Add(edge);
                    }
                }

                node.Edges = edges.ToArray();
            }
        }
    }
}