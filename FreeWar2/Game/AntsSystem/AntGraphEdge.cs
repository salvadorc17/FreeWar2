using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DEngine;


namespace FactionsGame
{
    /// <summary>
    /// Edge between AntGraphNodes.
    /// Has a pheromone value and heuristic.
    /// </summary>
    public class AntGraphEdge
    {
        public float PheromoneValue = 0;
        public GridReference targetRef;    // The ref this edge is pointing to
        public GridReference goalRef;       // The ref of the goal node


        #region Public Properties
        /// <summary>
        /// Plain X + Y distance
        /// </summary>
        public float Heuristic
        {
            get
            {
                GridReference relDist = goalRef - targetRef;
                float heuristic = (float)(Math.Abs(relDist.X) + Math.Abs(relDist.Y));
                return heuristic;
            }
        }
        #endregion


        /// <summary>
        /// New edge. Specify the reference at the end point of the edge, and the overall goal.
        /// </summary>
        /// <param name="_targetRef"></param>
        /// <param name="_goalRef"></param>
        public AntGraphEdge(GridReference _targetRef, GridReference _goalRef)
        {
            targetRef = _targetRef;
            goalRef = _goalRef;
        }



    }
}