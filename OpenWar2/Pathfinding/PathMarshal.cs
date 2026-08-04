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
    /// Provides pathing considering unit-occupied grid refs and path-occupied grid refs.
    /// Should take in all path requests from RTSActors and consider other unit's paths before returning a result.
    /// 
    /// Works with the engine's TileGrid and AStarPathNode to perform search tasks
    /// </summary>
    public class PathMarshal : GameComponent
    {
        protected FactionsGame engine;
        
        // Grid squares are indexed based off the Engine's TileGrid.
        protected bool[,] occupancyGrid; // Grid squares occupied with no intention to move
        protected int[,] pathOccupancyGrid; // Grid squares that currently form part of a unit's movement path.
        protected bool considerOccupied = true;
        protected bool considerPathOccupied = true;


        #region Public Properties
        public bool ConsiderPathOccupied
        {
            get
            {
                return considerPathOccupied;
            }
            set
            {
                considerPathOccupied = value;
            }
        }
        public bool ConsiderOccupied
        {
            get
            {
                return considerOccupied;
            }
            set
            {
                considerOccupied = value;
            }
        }
        public int[,] PathOccupancyGrid
        {
            get
            {
                return pathOccupancyGrid;
            }
        }
        public bool[,] OccupancyGrid
        {
            get
            {
                return occupancyGrid;
            }
        }
        #endregion



        #region Constructor
        public PathMarshal(FactionsGame game)
            : base(game)
        {
            engine = game;
        }
        #endregion



        #region Initialize
        /// <summary>
        /// Populate occupancy grids from the current level's actors.
        /// Assumes level is loaded.
        /// </summary>
        public override void Initialize()
        {
            if (engine.CurrentLevel != null)
            {
                int x, y;
                x = engine.CurrentLevel.Width;
                y = engine.CurrentLevel.Height;
                occupancyGrid = new bool[x, y];
                pathOccupancyGrid = new int[x, y];

                foreach (Actor a in engine.CurrentLevel.Actors)
                {
                    if (a is RTSActor)
                    {
                        RTSActor rtsActor = (RTSActor)a;
                        occupancyGrid[(int)rtsActor.CurrentLocation.X, (int)rtsActor.CurrentLocation.Y] = true;
                    }
                }
            }
        }
        #endregion


        // A* search function
        #region DoSearch
        /// <summary>
        /// A* search algorithm that acts upon the tile grid.
        /// Considers solidity of the tiles.
        /// Also considers other unit's search paths.
        /// Returns a path from the start to goal.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public Collection<AStarPathNode> DoSearch(GridReference start, GridReference goal)
        {
            AStarPathNode result = null;

            int openListCountMinimum = 25;
            bool goalFoundInInitialSpaceCheck = false;
            // Note that we are reversing the start and goal for this call - we want to see if the target area has enough free nodes to access it
            Collection<AStarPathNode> pathNodes = GetFreePathNodesRecursive(start, goal, openListCountMinimum, ref goalFoundInInitialSpaceCheck);

            // There were at least X free tiles accessile at the target, or the goal was within the free nodes, OK to do path move.
            if (pathNodes.Count >= openListCountMinimum || goalFoundInInitialSpaceCheck)
            {
                // A* variables
                Collection<AStarPathNode> openList = new Collection<AStarPathNode>();
                Collection<AStarPathNode> closedList = new Collection<AStarPathNode>();


                // Get children of start node and add them to the open list
                Tile startTile = engine.TileExistenceCheckByGridRef(start);
                if (startTile != null && startTile.Solid == false)
                {
                    // Get the adjacent grid refs for this tile
                    Collection<GridReference> adjacentRefs = startTile.AdjacentGridReferences();
                    foreach (GridReference adjacentRef in adjacentRefs)
                    {
                        // Make a new node for the open list with updated path length and heuristic
                        AStarPathNode pathNode = new AStarPathNode();
                        pathNode.Position = adjacentRef;
                        pathNode.Predecessor = null;
                        pathNode.PathLength = 1;

                        // Get estimated cost to goal (rel. x + rel. y)
                        GridReference relDist = goal - adjacentRef;
                        pathNode.Heuristic = (int)Math.Abs(relDist.X) + (int)Math.Abs(relDist.Y);

                        // Consider occupancy
                        if ((!considerOccupied || occupancyGrid[adjacentRef.X, adjacentRef.Y] == false) &&
                            (!considerPathOccupied || pathOccupancyGrid[adjacentRef.X, adjacentRef.Y] == 0))
                        {
                            // Add this adjacent node to the open list
                            openList.Add(pathNode);
                        }

                        // Drop out if this adjacent node is the goal.
                        if (adjacentRef == goal)
                        {
                            result = pathNode;
                            openList.Clear();
                            break;
                        }
                    }
                }


                // Begin traversal of the open list!
                while (openList.Count > 0 && closedList.Count < 1000)
                {
                    // Get the lowest cost node
                    AStarPathNode lowestNode = null;
                    int lowestCost = -1;
                    foreach (AStarPathNode pathNode in openList)
                    {
                        if (lowestCost < 0 || pathNode.TotalCost() < lowestCost)
                        {
                            lowestCost = pathNode.TotalCost();
                            lowestNode = pathNode;
                        }
                    }
                    openList.Remove(lowestNode);
                    closedList.Add(lowestNode);



                    // Check if we've found it!
                    if (lowestNode.Position == goal)
                    {
                        result = lowestNode;
                        break;
                    }



                    Tile thisTile = engine.TileExistenceCheckByGridRef(new GridReference(lowestNode.Position.X, lowestNode.Position.Y));
                    if (thisTile != null && thisTile.Solid == false)
                    {
                        // Get the adjacent grid refs for this tile
                        Collection<GridReference> adjacentRefs = thisTile.AdjacentGridReferences();
                        foreach (GridReference adjacentRef in adjacentRefs)
                        {
                            bool validRef = true; // Exclude predecessor, closed list, and occupied nodes.

                            // Exclude occupied nodes
                            if (!considerOccupied || occupancyGrid[adjacentRef.X, adjacentRef.Y] == true)
                                validRef = false;

                            // Exclude path occupied nodes
                            if (!considerPathOccupied || occupancyGrid[adjacentRef.X, adjacentRef.Y] == true)
                                validRef = false;

                            // Exclude predecessor
                            if (adjacentRef == lowestNode.Position && validRef)
                                validRef = false;

                            // Exclude items on the closed list
                            bool isClosed = false;
                            if (validRef)
                            {
                                foreach (AStarPathNode closedNode in closedList)
                                {
                                    if (closedNode.Position == adjacentRef)
                                    {
                                        isClosed = true;
                                        break;
                                    }
                                }
                                if (isClosed)
                                    validRef = false;
                            }



                            // If the adjacent node is valid or is the goal
                            if (validRef || adjacentRef == goal)
                            {
                                // Make a new node for the open list with updated path length and heuristic
                                AStarPathNode pathNode = new AStarPathNode();
                                pathNode.Position = adjacentRef;
                                pathNode.Predecessor = lowestNode;
                                pathNode.PathLength = lowestNode.PathLength + 1;

                                // Get estimated cost to goal (rel. x + rel. y)
                                GridReference relDist = goal - adjacentRef;
                                pathNode.Heuristic = (int)Math.Abs(relDist.X) + (int)Math.Abs(relDist.Y);

                                // Add it to the open list
                                openList.Add(pathNode);

                                // Drop out if it's the goal
                                if (adjacentRef == goal)
                                    break;
                            }
                        }
                        adjacentRefs.Clear();
                        adjacentRefs = null;
                    }
                }

                openList.Clear();
                closedList.Clear();
                openList = null;
                closedList = null;
            }

            // Build the path from the result path!
            Collection<AStarPathNode> resultPath = new Collection<AStarPathNode>();
            while (result != null)
            {
                resultPath.Add(result);
                result = result.Predecessor;
            }

            return resultPath;
        }
        #endregion



        Collection<AStarPathNode> GetFreePathNodesRecursive(GridReference goal, GridReference gridRef, int count, ref bool goalFound)
        {
            return GetFreePathNodesRecursive(goal, gridRef, count, new Collection<AStarPathNode>(), new Collection<GridReference>(), ref goalFound);
        }

        Collection<AStarPathNode> GetFreePathNodesRecursive(GridReference goal, GridReference gridRef, int count, Collection<AStarPathNode> pathNodeCollection, Collection<GridReference> gridRefCollection, ref bool goalFound)
        {
            // Get children of start node and add them to the open list
            Tile startTile = engine.TileExistenceCheckByGridRef(gridRef);
            if (startTile != null && startTile.Solid == false)
            {
                // Get the adjacent grid refs for this tile
                Collection<GridReference> adjacentRefs = startTile.AdjacentGridReferences();
                Collection<GridReference> validAdjacentRefs = new Collection<GridReference>();
                foreach (GridReference adjacentRef in adjacentRefs)
                {
                    // Consider occupancy
                    if ((!considerOccupied || occupancyGrid[adjacentRef.X, adjacentRef.Y] == false)) // &&
                        //(!considerPathOccupied || pathOccupancyGrid[adjacentRef.X, adjacentRef.Y] == 0))
                    {
                        if (adjacentRef == goal)
                        {
                            goalFound = true;
                            break;
                        }

                        if (!gridRefCollection.Contains(adjacentRef))
                        {
                            gridRefCollection.Add(adjacentRef);
                            validAdjacentRefs.Add(adjacentRef);

                            // Make a new node for the open list with updated path length and heuristic
                            AStarPathNode pathNode = new AStarPathNode();
                            pathNode.Position = adjacentRef;
                            pathNode.Predecessor = null;
                            pathNode.PathLength = 1;

                            // Get estimated cost to goal (rel. x + rel. y)
                            GridReference relDist = goal - adjacentRef;
                            pathNode.Heuristic = (int)Math.Abs(relDist.X) + (int)Math.Abs(relDist.Y);

                            // Add this adjacent node to the open list if not already exists
                            pathNodeCollection.Add(pathNode);

                            if (pathNodeCollection.Count == count)
                                break;
                        }
                    }
                }

                if (pathNodeCollection.Count < count)
                {
                    foreach (GridReference adjacentRef in validAdjacentRefs)
                    {
                        pathNodeCollection = GetFreePathNodesRecursive(goal, adjacentRef, count, pathNodeCollection, gridRefCollection, ref goalFound);

                        if (pathNodeCollection.Count == count)
                            break;
                    }
                }
            }
            return pathNodeCollection;
        }

    }
}
