using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using DEngine;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Basic controllable, selectable RTS unit.
    /// Sticks to the tile grid.
    /// Uses the tile grid and tile solidity for navigation.
    /// Uses A* for pathfinding.
    /// </summary>
    public class RTSUnit : Actor
    {
        // Engine and spawnable object templates
        protected Engine engine;


        // Movement and pathfinding
        protected Stack<AStarPathNode> pathStack;
        protected bool moveCommandIssued = false;
        protected Vector2 currentGoal;          // Long-term goal grid ref
        protected Vector2 intermediateGoal;    // Next grid ref to move to from our current location
        protected Vector2 currentLocation;     // The current grid ref we occupy
        protected bool moveStepComplete = true;              // Whether or not we have completed our current step
        protected float moveSpeed = 12000f;
        protected Vector2 moveDisplacement = Vector2.Zero;    // Total move displacement for this step
        protected Vector2 lastPosition = Vector2.Zero;    // Position since last update (used to determine displacement)

        protected bool rotateWithDirection = true;

        // Non-physics enabled velocity
        protected Vector2 velocity = Vector2.Zero;



        //protected string playerColor = null;


        #region Public Properties
        //public int Team
        //{
        //    get
        //    {
        //        return team;
        //    }
        //    set
        //    {
        //        team = value;
        //    }
        //}
        //public string PlayerColor
        //{
        //    get
        //    {
        //        return playerColor;
        //    }
        //    set
        //    {
        //        playerColor = value;
        //    }
        //}
        #endregion



        public RTSUnit(Engine game, string _name)
            : base(game, _name)
        {
            engine = (Engine)game;

            pathStack = new Stack<AStarPathNode>();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Determine our current grid reference!
            Tile t = engine.TileExistenceCheckByGeometry(this.Position);
            if (t != null)
            {
                // Set our current grid ref and last location
                currentLocation = new Vector2(t.GridReference().X, t.GridReference().Y);
                lastPosition = t.Position;

                // tint it for debug
                //t.TintColor = Color.RosyBrown;
            }
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.Body.AngularVelocity = 0f;

            PathingUpdate();
        }


        /// <summary>
        /// Move along allotted A* search path
        /// </summary>
        protected void PathingUpdate()
        {
            // If we've been given a command
            if (moveCommandIssued)
            {
                // If we haven't reached the current goal
                if (currentLocation != currentGoal)
                {
                    // If we haven't reached the next tile in the path sequence
                    if (!moveStepComplete)
                    {
                        // Moving between tiles.
                        // Determine when move is up and set moveStepComplete to true to trigger next path read.

                        // Do a manual move not using physics engine
                        //Position += velocity;
                        //moveDisplacement += velocity;

                        //Vector2 currentDisplacement = new Vector2(Math.Abs(Body.LinearVelocity.X), Math.Abs(Body.LinearVelocity.Y));

                        // Get distance moved since last update, and update total displacement for this step
                        Vector2 currentDisplacement = Position - lastPosition;
                        lastPosition = Position;
                        moveDisplacement += new Vector2(Math.Abs(currentDisplacement.X), Math.Abs(currentDisplacement.Y));

                        // Figure out if we've moved a tile distance on X or Y
                        if (moveDisplacement.X >= engine.TileWidth || moveDisplacement.Y >= engine.TileHeight)
                        {
                            // Set the move step complete and get the next step in the path
                            this.Body.LinearVelocity = Vector2.Zero;
                            moveStepComplete = true;
                            moveDisplacement = Vector2.Zero;

                            // Update our location
                            Tile t = engine.TileExistenceCheckByGeometry(this.Position);
                            if (t != null)
                            {
                                // Set our current grid ref.
                                currentLocation = new Vector2(t.GridReference().X, t.GridReference().Y);

                                // Reset path debug color
                                t.TintColor = Color.White;
                            }
                            else
                            {
                                currentLocation = intermediateGoal;
                            }
                        }
                    }
                    else
                    {
                        if (pathStack.Count > 0)
                        {
                            AStarPathNode pathNode = pathStack.Pop();
                            //if (pathNode.Position == currentLocation)  // Remove first node, it's us
                            //    pathNode = pathStack.Pop();

                            // Check solidity
                            Tile pathNodeTile = engine.TileExistenceCheckByGridRef(new GridReference((int)pathNode.Position.X, (int)pathNode.Position.Y));
                            if (pathNodeTile != null && pathNodeTile.Solid == false)
                            {
                                // Set this as our next goal and make it go!
                                intermediateGoal = new Vector2(pathNode.Position.X, pathNode.Position.Y);
                                moveStepComplete = false;
                                moveDisplacement = Vector2.Zero;

                                // Establish velocity for this move
                                Vector2 relativeMove = new Vector2(pathNode.Position.X, pathNode.Position.Y) - currentLocation;
                                this.Body.ApplyForce(relativeMove * moveSpeed);
                                //this.Position += relativeMove;
                                //velocity = relativeMove * moveSpeed;

                                if (rotateWithDirection)
                                {
                                    // Set our angle to the direction!
                                    if (relativeMove.X == 0f) // Exclude divide by zero; set north or south manually
                                    {
                                        // Either north or south.
                                        if (relativeMove.Y == 1f)
                                            this.Body.Rotation = (float)(Math.PI / 2);
                                        else
                                            this.Body.Rotation = (float)-(Math.PI / 2);
                                    }
                                    else
                                    {
                                        this.Body.Rotation = (float)Math.Atan(relativeMove.Y / relativeMove.X);
                                    }
                                    this.Body.Rotation -= (float)(Math.PI / 2);  // Adjust for art orientation
                                }

                                
                            }
                        }
                        else
                        // Stack is empty. Move has been cut short (target tile was solid)
                        {
                            moveCommandIssued = false;
                            this.Body.LinearVelocity = Vector2.Zero;
                        }
                    }
                }
                else
                {
                    // Break out of the movement loop.
                    moveCommandIssued = false;
                    this.Body.LinearVelocity = Vector2.Zero;
                }
            }
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }



        /// <summary>
        /// Move from grid square to grid square until the destination is reached.
        /// 
        /// </summary>
        /// <param name="pos"></param>
        public void MoveToGridLocation(Vector2 target)
        {
            //moveCommandIssued = false;

            // Set our new goal
            currentGoal = target;
            pathStack.Clear();

            Vector2 startTarget = currentLocation;
            if (moveCommandIssued)
                startTarget = intermediateGoal;


            //AStarSearch search = new AStarSearch(engine);
            AStarPathNode path = new AStarPathNode();
            path.Position = new GridReference(target.X, target.Y);  // currentLocation
            while (path != null)
            {
                pathStack.Push(path);
                Tile thisTile = engine.TileExistenceCheckByGridRef(new GridReference((int)path.Position.X, (int)path.Position.Y));

                // Some color debugging
                //thisTile.TintColor = Color.LightGray;

                path = path.Predecessor;
            }

            moveCommandIssued = true;
        }

    }
}
