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
    /// Ant system search node.
    /// Has a set of edges ranked according to heuristic value to the goal.
    /// </summary>
    public class AntGraphNode
    {
        protected AntsSystem antsSystem;
        public GridReference gridReference;
        protected Collection<AntGraphEdge> rankedEdges = new Collection<AntGraphEdge>();



        #region Public Properties
        public Collection<AntGraphEdge> RankedEdges
        {
            get
            {
                return rankedEdges;
            }
        }
        #endregion



        public AntGraphNode(AntsSystem _antsSystem, GridReference _gridReference)
        {
            antsSystem = _antsSystem;
            gridReference = _gridReference;

            // Create a set of AntGraphEdges for available edges from this reference.
            Collection<AntGraphEdge> adjacentEdges = new Collection<AntGraphEdge>();
            Tile currentTile = antsSystem.Engine.TileExistenceCheckByGridRef(gridReference);
            if (currentTile != null)
            {
                Collection<GridReference> adjacentRefs = currentTile.AdjacentGridReferences();
                foreach (GridReference adjacentRef in adjacentRefs)
                {
                    Tile adjacentTile = antsSystem.Engine.TileExistenceCheckByGridRef(adjacentRef);
                    if (adjacentTile != null && adjacentTile.Solid == false)
                    {
                        AntGraphEdge edge = new AntGraphEdge(adjacentRef, antsSystem.EndLocation);
                        adjacentEdges.Add(edge);
                    }
                }
            }


            // Rank by heuristic value
            while (adjacentEdges.Count > 0)
            {
                float lowestHeuristic = -1f;
                int lowestIndex = -1;
                for (int i = 0; i < adjacentEdges.Count; i++)
                {
                    if (lowestHeuristic == -1f || adjacentEdges[i].Heuristic < lowestHeuristic)
                    {
                        lowestHeuristic = adjacentEdges[i].Heuristic;
                        lowestIndex = i;
                    }
                }

                if (lowestIndex > -1)
                {
                    rankedEdges.Add(adjacentEdges[lowestIndex]);
                    adjacentEdges.Remove(adjacentEdges[lowestIndex]);
                }
            }
            adjacentEdges.Clear();
        }



    }

}
