using System.Collections.Generic;

namespace Pathfinding
{
    public class AStar
    {
        private Queue<Tile> m_path;

        public AStar(World world, Tile start, Tile end)
        {
        }

        public Tile GetNextTile()
        {
            return m_path.Dequeue();
        }
    }
}