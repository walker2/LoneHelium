using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class TileGraph
    {
        // This class constructs a simple pathfinding graph of our world.
        private Dictionary<Tile, Node<Tile>> m_nodes;

        public TileGraph(World world)
        {
            m_nodes = new Dictionary<Tile, Node<Tile>>();
            // For each tile create a node 
            for (int x = 0; x < world.Width; x++)
            {
                for (int y = 0; y < world.Height; y++)
                {
                    Tile t = world.GetTileAt(x, y);

                    if (t.MovementCost > 0f)
                    {
                        var node = new Node<Tile>
                        {
                            Data = t
                        };

                        m_nodes.Add(t, node);
                        Debug.DrawLine(new Vector3(x - 0.1f, y - 0.25f, 0), new Vector3(x + 0.1f, y + 0.25f, 0), Color.red,
                            999f);
                    }
                }
            }

            foreach (Tile t in m_nodes.Keys)
            {
                Node<Tile> node = m_nodes[t];

                var edges = new List<Edge<Tile>>();

                Tile[] tiles = t.GetNeighbours(true);

                // Get a list of neighbours for the tile
                // If neighbour is walkable -- create an edge with movement cost
                foreach (Tile neighbourTile in tiles)
                {
                    if (neighbourTile != null && neighbourTile.MovementCost > 0)
                    {
                        var edge = new Edge<Tile>
                        {
                            Cost = neighbourTile.MovementCost,
                            Node = m_nodes[neighbourTile]
                        };

                        edges.Add(edge);
                        Debug.DrawLine(new Vector3(t.Position.x, t.Position.y, 0),
                            new Vector3(edge.Node.Data.Position.x, edge.Node.Data.Position.y, 0), Color.green, 999f);
                    }
                }

                node.Edges = edges.ToArray();
            }
        }
    }
}