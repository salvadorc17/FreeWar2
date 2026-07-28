using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using DEngine;

namespace FactionsGame
{
    /// <summary>
    /// Path node for A* path search.
    /// </summary>
    public class AStarPathNode
    {
        public GridReference Position;
        public int PathLength;
        public int Heuristic;
        public AStarPathNode Predecessor;

        public bool DebugMode = false;  // Tint tiles

        public AStarPathNode()
        {
        }

        public int TotalCost()
        {
            return Heuristic + PathLength;
        }
    }
}
