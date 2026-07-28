using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DEngine;


namespace FactionsGame
{
    public delegate void TickHandler();

    /// <summary>
    /// Implementation of ant-based pathfinding system described in a paper I found:
    /// Ant Based Pathfinding - David Gordon
    /// 
    /// Uses the engine's TileGrid as a search space.
    /// Tile solidity is considered.
    /// Keeps its own set of search nodes with pheromone trails (and perhaps influence maps in the future)
    /// </summary>
    public class AntsSystem
    {
        protected Engine engine;

        // Global search properties
        protected GridReference startLocation;
        protected GridReference endLocation;
        //protected int concurrentAntCount = 5;             // How many ants search at the same time.
        protected int maxAntCount = 10;                     // How many ants total in a search.
        protected int maxAntPathLength;                     // Total distance an ant can move without finding the goal before it expires.
        protected float maxAntPathLengthMultiplier = 2f;    // Multiplier of minimum distance to goal to allow sub-optimal routes.
        protected AntGraphNode[,] antGraph;                 // Navigation graph
        protected float pheromoneDecayValue = 0.1f;

        // Internal search variables
        private int antCounter = 0;


        // Debug
        protected bool antPathDebugEnabled = false;


        public event TickHandler AntTick;


        #region Public Properties
        /// <summary>
        /// Rate at which ant pheromone decays
        /// </summary>
        public float PheromoneDecayValue
        {
            get
            {
                return pheromoneDecayValue;
            }
            set
            {
                pheromoneDecayValue = value;
            }
        }
        /// <summary>
        /// Ant navigation graph
        /// </summary>
        public AntGraphNode[,] AntGraph
        {
            get
            {
                return antGraph;
            }
        }
        /// <summary>
        /// Game engine to reference tile grid.
        /// </summary>
        public Engine Engine
        {
            get
            {
                return engine;
            }
        }
        /// <summary>
        /// Start point for ant search (the hive).
        /// </summary>
        public GridReference StartLocation
        {
            get
            {
                return startLocation;
            }
            set
            {
                startLocation = value;
            }
        }
        /// <summary>
        /// Target end point for ant search (the food).
        /// </summary>
        public GridReference EndLocation
        {
            get
            {
                return endLocation;
            }
            set
            {
                endLocation = value;
            }
        }
        ///// <summary>
        ///// How many ants search at the same time.
        ///// </summary>
        //public int ConcurrentAntCount
        //{
        //    get
        //    {
        //        return concurrentAntCount;
        //    }
        //    set
        //    {
        //        concurrentAntCount = value;
        //    }
        //}
        /// <summary>
        /// How many ants total in a search.
        /// </summary>
        public int MaxAntCount
        {
            get
            {
                return maxAntCount;
            }
            set
            {
                maxAntCount = value;
            }
        }
        /// <summary>
        /// Total distance an ant can move without finding the goal before it expires.
        /// </summary>
        public int MaxAntPathLength
        {
            get
            {
                return maxAntPathLength;
            }
            set
            {
                maxAntPathLength = value;
            }
        }
        #endregion



        public AntsSystem(Engine game, GridReference _startLocation, GridReference _endLocation)
        {
            startLocation = _startLocation;
            endLocation = _endLocation;

            // Set the max length as a percentage of the relative distance
            GridReference relativeLength = endLocation - startLocation;
            maxAntPathLength = (int)(Math.Abs(relativeLength.X) + Math.Abs(relativeLength.Y));
            maxAntPathLength = (int)(maxAntPathLength * maxAntPathLengthMultiplier);

            // If the path length is colossal, restrict to just two ants
            if (maxAntPathLength > 250)
            {
                maxAntCount = 2;
            }

            engine = game;

            // Load an array of AntGraphNodes the same size as the tile array.
            ClearAntGraph();
        }


        #region ClearAntGraph
        /// <summary>
        /// Clear all influences from the ant graph.
        /// Load an array of AntGraphNodes the same size as the level.
        /// </summary>
        public void ClearAntGraph()
        {
            if (engine.CurrentLevel != null)
            {
                antGraph = new AntGraphNode[engine.CurrentLevel.Width, engine.CurrentLevel.Height];
                for (int column = 0; column < engine.CurrentLevel.Width; column++)
                {
                    for (int row = 0; row < engine.CurrentLevel.Height; row++)
                    {
                        antGraph[column, row] = new AntGraphNode(this, engine.TileGrid[column,row].GridReference());
                    }
                }
            }
        }
        #endregion



        #region DoAntsSearch
        /// <summary>
        /// Perform the ant system search using the class variables for number of ants and max path length.
        /// Specify the start and end points.
        /// Potentially hefty - new thread? (eventually)
        /// </summary>
        public void DoAntsSearch()
        {
            antCounter = 0;

            // Release the hounds!
            while (antCounter < maxAntCount)
            {
                DeployAnt();
                antCounter++;

                // Apply pheromone decay to any and all edges
                ApplyDecay();

                if (AntTick != null)
                    AntTick();
            }
        }
        #endregion



        #region ApplyDecay
        /// <summary>
        /// Apply pheromone decay to all edges in the graph!
        /// </summary>
        protected void ApplyDecay()
        {
            foreach (AntGraphNode node in antGraph)
            {
                foreach (AntGraphEdge edge in node.RankedEdges)
                {
                    edge.PheromoneValue *= (1 - pheromoneDecayValue);
                }
            }
        }
        #endregion



        /// <summary>
        /// Send an ant out into the grid to navigate to the goal (hopefully).
        /// </summary>
        protected Collection<AntGraphEdge> DeployAnt()
        {
            Ant ant = new Ant(this);
            Collection<AntGraphEdge> pathEdges = ant.AntSearch();
            if (pathEdges.Count > 0)
            {
                // We found a path!
                // Mark pheromones on this path.
                foreach (AntGraphEdge pathEdge in pathEdges)
                {
                    // Add to existing pheromone value
                    pathEdge.PheromoneValue += ant.PheromoneStrength;

                    // Debug coloration
                    if (antPathDebugEnabled)
                    {
                        Tile pathTile = engine.TileExistenceCheckByGridRef(pathEdge.targetRef);
                        if (pathTile != null)
                        {
                            pathTile.TintColor = new Color(pathTile.TintColor.R,
                                                           (byte)(pathTile.TintColor.G - Convert.ToByte(10)),
                                                           (byte)(pathTile.TintColor.B - Convert.ToByte(10)),
                                                           pathTile.TintColor.A);
                        }
                    }
                }
            }
            return pathEdges;
        }




        /// <summary>
        /// Send a pilot ant to navigate to the goal.
        /// Remove pointless loops.
        /// </summary>
        /// <returns></returns>
        public Collection<GridReference> PilotAntPath()
        {
            // Not all ants succeed so send ants until we do
            Collection<AntGraphEdge> antPathEdges = new Collection<AntGraphEdge>();
            while (antPathEdges.Count == 0)
            {
                antPathEdges = DeployAnt();
            }

            // Create list of target refs
            Collection<GridReference> antPath = new Collection<GridReference>();
            foreach (AntGraphEdge edge in antPathEdges)
            {
                antPath.Add(edge.targetRef);
            }

            // Prune the path - exclude loops
            // For each node in the path
            for (int i = 0; i < antPath.Count; i++)
            {
                // Check all other nodes to see if this is referenced later.
                bool foundAlready = false;
                for (int subIndex = 0; subIndex < antPath.Count; subIndex++)
                {
                    if (antPath[subIndex] == antPath[i])
                    {
                        if (foundAlready)
                        {
                            // Clip all nodes between index and subindex
                            for (int clipIndex = i + 1; clipIndex <= subIndex; clipIndex++)
                            {
                                antPath.RemoveAt(clipIndex);
                                clipIndex--;
                                subIndex--;
                            }
                        }
                        else
                            foundAlready = true;
                    }
                }
            }


            return antPath;
        }
    }

    
}
