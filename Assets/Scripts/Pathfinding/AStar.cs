using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UnityEngine;

namespace Pathfinding
{
    public class AStar
    {
        private Queue<Tile> m_path;

        public AStar(World world, Tile tileStart, Tile tileEnd)
        {
            if (world.TileGraph == null) // If graph is invalidated -- create it
            {
                world.TileGraph = new TileGraph(world);
            }

            Dictionary<Tile, Node<Tile>> nodes = world.TileGraph.Nodes; // Dictionary of all valid walkable nodes      

            if (nodes.ContainsKey(tileStart) == false)
            {
                Debug.LogError("AStar: The starting tile isn't in list of nodes");
                return;
            }

            if (nodes.ContainsKey(tileEnd) == false)
            {
                Debug.LogError("AStar: The ending tile isn't in list of nodes");
                return;
            }

            var start = nodes[tileStart];
            var end = nodes[tileEnd];
            var closedSet = new HashSet<Node<Tile>>();
            var openSet = new SimplePriorityQueue<Node<Tile>>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<Node<Tile>, Node<Tile>>();
            var gScore = new Dictionary<Node<Tile>, float>();
            foreach (Node<Tile> n in nodes.Values)
            {
                gScore[n] = Mathf.Infinity;
            }
            gScore[start] = 0;

            var fScore = new Dictionary<Node<Tile>, float>();
            foreach (Node<Tile> n in nodes.Values)
            {
                fScore[n] = Mathf.Infinity;
            }
            fScore[start] = HeuristicCostEstimate(start, end);

            while (openSet.Count > 0)
            {
                Node<Tile> current = openSet.Dequeue();

                if (current == end)
                {
                    ReconstructPath(cameFrom, current);
                    return;
                }

                closedSet.Add(current);

                foreach (Edge<Tile> edgeNeighbour in current.Edges)
                {
                    var neighbour = edgeNeighbour.Node;
                    if (closedSet.Contains(neighbour))
                        continue;

                    float tentativeGScore =
                        gScore[current] + DistBetween(current, neighbour) * neighbour.Data.MovementCost;

                    if (openSet.Contains(neighbour) && tentativeGScore >= gScore[neighbour])
                        continue;

                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeGScore;
                    fScore[neighbour] = gScore[neighbour] + HeuristicCostEstimate(neighbour, end);
                    if (openSet.Contains(neighbour) == false)
                    {
                        openSet.Enqueue(neighbour, fScore[neighbour]);
                    }
                    else
                    {
                        openSet.UpdatePriority(neighbour, fScore[neighbour]);
                    }
                }
            }

            // There's no path from start to end, Path would be null
        }

        private void ReconstructPath(Dictionary<Node<Tile>, Node<Tile>> cameFrom, Node<Tile> current)
        {
            var totalPath = new Queue<Tile>();

            totalPath.Enqueue(current.Data);
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Enqueue(current.Data);
            }

            m_path = new Queue<Tile>(totalPath.Reverse());
        }

        private static float HeuristicCostEstimate(Node<Tile> a, Node<Tile> b)
        {
            if (b == null)
                return 0f;

            return ((a.Data.Position.x - b.Data.Position.x) * (a.Data.Position.x - b.Data.Position.x) +
                    (a.Data.Position.y - b.Data.Position.y) * (a.Data.Position.y - b.Data.Position.y));
        }

        private float DistBetween(Node<Tile> a, Node<Tile> b)
        {
            if (Mathf.Abs(a.Data.Position.x - b.Data.Position.x)
                + Mathf.Abs(a.Data.Position.y - b.Data.Position.y) == 1)
            {
                return 1f;
            }

            // Diag neighbours have a distance of 1.41421356237
            if (Mathf.Abs(a.Data.Position.x - b.Data.Position.x) == 1 &&
                Mathf.Abs(a.Data.Position.y - b.Data.Position.y) == 1)
            {
                return 1.41421356237f;
            }

            // Up/Down neighbors have a distance of 1
            if (a.Data.Position.x == b.Data.Position.x && a.Data.Position.y == b.Data.Position.y)
            {
                return 1f;
            }

            // Otherwise, do the actual math.
            return ((a.Data.Position.x - b.Data.Position.x) * (a.Data.Position.x - b.Data.Position.x) +
                    (a.Data.Position.y - b.Data.Position.y) * (a.Data.Position.y - b.Data.Position.y));
        }

        public Tile Dequeue()
        {
            return m_path.Dequeue();
        }

        public int Length()
        {
            return m_path == null ? 0 : m_path.Count;
        }
    }
}