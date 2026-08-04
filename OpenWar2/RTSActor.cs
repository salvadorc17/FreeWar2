using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DEngine;
using System.Drawing;

using Color = Microsoft.Xna.Framework.Color;

using DSceneGraph;

namespace FactionsGame
{
    public delegate void RTSActorEventHandler();
    public delegate void RTSActorTargetEventHandler(RTSActor target);

    /// <summary>
    /// Actor with a team number, player color, health, max health, and pathfinding/moving capability.
    /// </summary>
    public class RTSActor : Actor, ICloneable
    {
        public enum ArmorType 
        { 
            None, 
            Light, 
            Medium, 
            Heavy, 
            Building, 
            Super 
        };

        // 8-way direction for animation
        public enum EightWayDirection
        {
            North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
        };


        #region Private Variables
        new protected FactionsGame _engine;

        protected bool _movable = false;
        protected bool _selectable = true;
        protected bool _armed = true; // do enemy targetting?
        protected bool _isBuilding = false;
        protected bool _stopOnHostilities = true;
        protected int _playerColor; // Player color of this actor (who it belongs to)
        protected int _health = 100;
        protected int _maxHealth = 100;


        // Movement and pathfinding
        protected Stack<AStarPathNode> _pathStack = new Stack<AStarPathNode>();
        protected bool _moveCommandIssued = false;
        protected GridReference _currentGoal;          // Long-term goal grid ref
        protected GridReference _intermediateGoal;    // Next grid ref to move to from our current location
        protected GridReference _currentLocation;     // The current grid ref we occupy
        protected bool _moveStepComplete = true;              // Whether or not we have completed our current step
        protected float _moveSpeed = 120f;
        protected Vector2 _moveDisplacement = Vector2.Zero;    // Total move displacement for this step
        protected Vector2 _lastPosition = Vector2.Zero;    // Position since last update (used to determine displacement)

        protected bool _rotateWithDirection = false;

        // Non-physics enabled velocity
        protected Vector2 _velocity = Vector2.Zero;

        // Health bar and selection box
        HealthBar _healthBar = null;
        SelectionBox _selectionBox = null;

        
        protected EightWayDirection _compassDirection = EightWayDirection.South;

        // Armor!
        protected ArmorType _armorType = ArmorType.None;

        // Combat variables
        protected int _maxTargetRange = 300;
        protected int _damage = 12;
        protected int _fireRate = 25;

        protected bool _pathFound = false;

        protected bool _movementSuspended = false;
        protected bool _attackMove = false;
        protected RTSActor _targetActor;

        //protected RTSActor _baseActor; // Attached to another unit?



        // Fire rate and control variables
        float gunFireCounter = 0f;
        protected int _burstShotsMax = 3;
        int _burstShotsCount = 0;
        protected int _reloadTime = 105;
        int _originalFireRate;
        protected float inaccuracyValue = 10f;
        protected bool _enemyFound = false;
        protected bool _attackEngaged = false;

        protected bool _useBurst = false;


        protected RTSActor _tankBase;

        public event RTSActorTargetEventHandler OnAttackStart;
        public event RTSActorEventHandler OnAttackEnd;
        public event RTSActorEventHandler OnBurstStart;
        public event RTSActorEventHandler OnBurstEnd;


        protected Vector2 _closestTargetVector;


        int _targetFindUpdateCounter = 0;
        protected int _targetFindUpdateInterval = 5;

        #endregion



        #region Public Properties
        public EightWayDirection CompassDirection
        {
            get { return _compassDirection; }
            set { _compassDirection = value; }
        }
        public RTSActor TargetActor
        {
            get { return _targetActor; }
            set { _targetActor = value; }
        }
        public RTSActor TankBase
        {
            get { return _tankBase; }
            set { _tankBase = value; }
        }
        public bool Selectable
        {
            get { return _selectable; }
            set { _selectable = value; }
        }
        public bool IsBuilding
        {
            get { return _isBuilding; }
            set { _isBuilding = value; }
        }
        public ArmorType Armor
        {
            get
            {
                return _armorType;
            }
            set
            {
                _armorType = value;
            }
        }
        public bool MovementSuspended
        {
            get
            {
                return _movementSuspended;
            }
            set
            {
                _movementSuspended = value;
            }
        }
        public int MaxTargetRange
        {
            get
            {
                return _maxTargetRange;
            }
            set
            {
                _maxTargetRange = value;
            }
        }
        public int Damage
        {
            get
            {
                return _damage;
            }
            set
            {
                _damage = value;
            }
        }
        public int FireRate
        {
            get
            {
                return _fireRate;
            }
            set
            {
                _fireRate = value;
            }
        }
        public SelectionBox SelectionBox
        {
            get
            {
                return _selectionBox;
            }
            set
            {
                _selectionBox = value;
            }
        }
        public HealthBar HealthBar
        {
            get
            {
                return _healthBar;
            }
            set
            {
                _healthBar = value;
            }
        }
        public GridReference CurrentLocation
        {
            get
            {
                return _currentLocation;
            }
        }
        public bool IsDead
        {
            get
            {
                return _health <= 0f;
            }
        }
        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
            set
            {
                _maxHealth = value;
            }
        }
        public int Health
        {
            get
            {
                return _health;
            }
            set
            {
                _health = value;
            }
        }
        public int PlayerColor
        {
            get
            {
                return _playerColor;
            }
            set
            {
                _playerColor = value;
            }
        }
        public bool Movable
        {
            get
            {
                return _movable;
            }
            set
            {
                _movable = value;
            }
        }
        public bool IsOurs
        {
            get
            {
                if (this.Team == _engine.LocalPlayer.Team && this.MaskColor == _engine.PlayerColors[_engine.LocalPlayer.Color - 1])
                    return true;
                return false;
            }
        }
        public bool IsOurTeam
        {
            get
            {
                if (this.Team == _engine.LocalPlayer.Team)
                    return true;
                return false;
            }
        }
        #endregion



        #region Constructor
        public RTSActor(FactionsGame game, string _name)
            : base(game, _name)
        {
            _engine = game;
            _subtype = "RTSActor";
        }
        #endregion



        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            _originalFireRate = _fireRate;


            Random random = new Random();
            _targetFindUpdateCounter = random.Next(_targetFindUpdateInterval - 1);


            // Don't allow any bouncing or physical interaction. Only collision detection.
            //if (_engine.ActorsPhysicallySimulated)
            //    this.Geom.CollisionResponseEnabled = false;

            // Determine our current grid reference!
            Vector2 tilePos = _engine.GetTileGridPosition(this.Position);
            Tile t = _engine.TileExistenceCheckByExactLocation(tilePos);
            if (t != null)
            {
                // Set our current grid ref and last location
                _currentLocation = t.GridReference();
                _lastPosition = t.Position;

                // Move it to the center of this tile!
                this.Position = t.Position;

                // tint it for debug
                if (_engine.PathDebugEnabled)
                    t.TintColor = Color.LimeGreen;

                // Set correct collision group
                //if (_engine.ActorsPhysicallySimulated)
                //    this.Geom.CollisionGroup = this.Team;
            }
            //this.Geom.CollisionEnabled = false;
        }
        #endregion



        #region Update
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_health <= 0)
            {
                Die();

                // Also remove from occupancy grid
                _engine.PathMarshal.OccupancyGrid[_currentLocation.X, _currentLocation.Y] = false;

                // Reset path debug color
                if (_engine.PathDebugEnabled)
                {
                    Tile t = _engine.TileExistenceCheckByGridRef(_currentLocation);
                    if (t != null)
                        t.TintColor = Color.White;
                }
            }

            if (this._physicallySimulated)
                this.Body.AngularVelocity = 0f;


            // Repeat pathfind if no path found.
            if (_movable && !IsDead)
            {
                // Repeat pathfind if no path found. (is this working as intended?)
                if (_moveCommandIssued && !_pathFound)
                    MoveToGridLocation(_currentGoal, _attackMove);

                // Update pathing
                PathingUpdate();
            }

            // Enemy targetting and firing
            if (!IsDead && _armed)
            {
                CombatUpdate();
            }
        }
        #endregion



        /// <summary>
        /// Look for enemies and engage
        /// </summary>
        void CombatUpdate()
        {
            // Look for enemies and engage!
            _attackEngaged = false;
            if ((_attackMove && _moveStepComplete) || !_moveCommandIssued || _targetActor != null)
            {

                // Check if our target is dead
                if (_targetActor != null && _targetActor.IsDead)
                {
                    _targetActor = null;
                    Stop();
                }


                // If we have a target, disregard all enemies except the target
                if (_targetActor != null)
                {
                    _enemyFound = false;
                    _closestTargetVector = (_targetActor.AbsolutePosition + (this.Size / 2)) - this.AbsolutePosition;
                    if (Math.Abs(_closestTargetVector.Length()) <= MaxTargetRange && Math.Abs(_closestTargetVector.Length()) > 0)
                    {
                        _enemyFound = true;
                        // Get absolute angle
                        float absoluteAngle = (float)(Math.Atan2(-_closestTargetVector.X, _closestTargetVector.Y));

                        if (_rotateWithDirection)
                            this.Rotation = absoluteAngle;
                    }
                }
                // No specific target - kill anything in sight
                else
                {
                    if (_targetFindUpdateCounter == _targetFindUpdateInterval)
                    {
                        _targetFindUpdateCounter = 0;

                        _enemyFound = false;
                        // Find the angle of this target
                        _closestTargetVector = ClosestEnemyTargetVector(out _enemyFound);
                        if (_enemyFound)
                        {
                            if (Math.Abs(_closestTargetVector.Length()) <= MaxTargetRange && Math.Abs(_closestTargetVector.Length()) > 0)
                            {
                                // Get absolute angle
                                float absoluteAngle = (float)(Math.Atan2(-_closestTargetVector.X, _closestTargetVector.Y));

                                if (_rotateWithDirection)
                                    this.Rotation = absoluteAngle;
                            }
                            else
                                // If our cheap rectangle check retrieved a target outside our attack radius, disregard!
                                _enemyFound = false;
                        }
                    }
                    _targetFindUpdateCounter++;
                }









                // Examine closest target
                if (_enemyFound) //&& Math.Abs(closestTargetVector.Length()) <= MaxTargetRange && Math.Abs(closestTargetVector.Length()) > 0)
                {
                    // Get absolute angle
                    float absoluteAngle = (float)(Math.Atan2(_closestTargetVector.X, _closestTargetVector.Y));
                    absoluteAngle = (float)(Math.PI / 2) - absoluteAngle;

                    int quadrant = QuadrantFromVector(_closestTargetVector);

                    float startAngle = 0f;
                    startAngle += QuadrantToAngle(quadrant);

                    // Add absolute angle to quadrant start angle
                    float shootAngle = startAngle + absoluteAngle;

                    // Stop pathing while we're attacking
                    if (_stopOnHostilities)
                        MovementSuspended = true;

                    // Convert our shoot direction into a compass direction
                    _compassDirection = VectorToEightWayDirection(_closestTargetVector);

                    // Shoot when the timer is up
                    if (gunFireCounter >= _fireRate)
                    {
                        if (_useBurst)
                        {
                            // Figure out when burst is up and set fire rate accordingly
                            if (_burstShotsCount == 0)
                            {
                                // Reset original fire rate when done reloading
                                _fireRate = _originalFireRate;

                                // Trigger fire-burst start event
                                if (OnBurstStart != null)
                                    OnBurstStart();
                            }
                            _burstShotsCount++;
                            if (_burstShotsCount == _burstShotsMax)
                            {
                                _burstShotsCount = 0;
                                _fireRate = _reloadTime;

                                if (OnBurstEnd != null)
                                    OnBurstEnd();
                            }
                        }

                        gunFireCounter = 0f;

                        // We're looking up the closest actor twice. Once for the vector and once for the target.
                        // Fix!

                        RTSActor targetActor = _targetActor != null ? _targetActor : ClosestEnemyTarget();
                        if (targetActor != null)
                        {
                            if (OnAttackStart != null)
                                OnAttackStart(targetActor);
                        }

                    }
                    _attackEngaged = true;
                }
                else
                {
                    // Closest target was out of range.
                    _enemyFound = false;
                    _attackEngaged = false;
                    MovementSuspended = false;

                    // Reset burst count
                    _burstShotsCount = 0;
                }

                // Weren't in range or step was not complete.
                if (!_attackEngaged)
                {
                    if (OnAttackEnd != null)
                        OnAttackEnd();
                }
            }

            gunFireCounter++;
        }





        #region PathingUpdate
        /// <summary>
        /// Move along allotted A* search path
        /// </summary>
        protected void PathingUpdate()
        {
            this.Position += _velocity;

            // If we've been given a command
            if (_moveCommandIssued)
            {
                // If we haven't reached the current goal
                if (_currentLocation != _currentGoal)
                {
                    // If we haven't reached the next tile in the path sequence
                    if (!_moveStepComplete)
                    {
                        // Moving between tiles.
                        // Determine when move is up and set moveStepComplete to true to trigger next path read.

                        // Get distance moved since last update, and update total displacement for this step
                        Vector2 currentDisplacement = Position - _lastPosition;
                        _lastPosition = Position;

                        // If we've somehow moved nowhere, set to move step complete.
                        if (currentDisplacement == Vector2.Zero)
                            _moveStepComplete = true;

                        _moveDisplacement += new Vector2(Math.Abs(currentDisplacement.X), Math.Abs(currentDisplacement.Y));

                        // Figure out if we've moved a tile distance on X or Y
                        if (_moveDisplacement.X >= _engine.TileWidth || _moveDisplacement.Y >= _engine.TileHeight)
                        {

                            // Set the move step complete and get the next step in the path
                            //this.Body.LinearVelocity = Vector2.Zero;
                            _velocity = Vector2.Zero;


                            _moveStepComplete = true;
                            _moveDisplacement = Vector2.Zero;

                            Tile t = _engine.TileExistenceCheckByGridRef(_intermediateGoal);
                            // Set our current grid ref.
                            _currentLocation = t.GridReference();

                            // Place ourselves in the direct center of this tile.
                            //this.Body.Position = t.Position;
                            this.Position = t.Position;

                            // Reset path debug color
                            if (_engine.PathDebugEnabled)
                                t.TintColor = Color.White;
                        }
                    }
                    else
                    {
                        // Don't allow another step to be taken if we've decided to pause
                        if (!_movementSuspended)
                        {
                            if (_pathStack.Count > 0)
                            {
                                AStarPathNode pathNode = _pathStack.Pop();

                                // Set PathMarshal path occupancy grid to zero
                                _engine.PathMarshal.PathOccupancyGrid[pathNode.Position.X, pathNode.Position.Y] = 0;

                                // Check solidity
                                Tile pathNodeTile = _engine.TileExistenceCheckByGridRef(pathNode.Position);
                                if (pathNodeTile != null && pathNodeTile.Solid == false)
                                // Also check PathMarshal's occupancy grid.
                                //&& engine.PathMarshal.OccupancyGrid[pathNode.Position.X, pathNode.Position.Y] == false)
                                {
                                    // Set this as our next goal and make it go!
                                    _intermediateGoal = pathNode.Position;
                                    _moveStepComplete = false;
                                    _moveDisplacement = Vector2.Zero;

                                    // Set our last position to here
                                    _lastPosition = new Vector2(Position.X, Position.Y);

                                    // Establish velocity for this move
                                    GridReference relativeMove = pathNode.Position - _currentLocation;

                                    Vector2 relativeMoveVector = new Vector2(relativeMove.X, relativeMove.Y);


                                    //this.Body.ApplyForce(relativeMoveVector * moveSpeed);
                                    _velocity = relativeMoveVector * _moveSpeed;



                                    // Figure out 8-way direction
                                    if (relativeMove.X == 1 && relativeMove.Y == 1)
                                        _compassDirection = EightWayDirection.SouthEast;
                                    else if (relativeMove.X == 0 && relativeMove.Y == 1)
                                        _compassDirection = EightWayDirection.South;
                                    else if (relativeMove.X == -1 && relativeMove.Y == 1)
                                        _compassDirection = EightWayDirection.SouthWest;
                                    else if (relativeMove.X == 1 && relativeMove.Y == 0)
                                        _compassDirection = EightWayDirection.East;
                                    else if (relativeMove.X == -1 && relativeMove.Y == 0)
                                        _compassDirection = EightWayDirection.West;
                                    else if (relativeMove.X == 1 && relativeMove.Y == -1)
                                        _compassDirection = EightWayDirection.NorthEast;
                                    else if (relativeMove.X == 0 && relativeMove.Y == -1)
                                        _compassDirection = EightWayDirection.North;
                                    else if (relativeMove.X == -1 && relativeMove.Y == -1)
                                        _compassDirection = EightWayDirection.NorthWest;


                                    if (_rotateWithDirection)
                                    {
                                        // Set our angle to the direction!
                                        if (relativeMove.X == 0f) // Exclude divide by zero; set north or south manually
                                        {
                                            // Either north or south.
                                            if (relativeMove.Y == 1f)
                                                this.Rotation = (float)(Math.PI / 2);  // Body
                                            else
                                                this.Rotation = (float)-(Math.PI / 2);
                                        }
                                        else
                                        {
                                            this.Rotation = (float)Math.Atan(relativeMove.Y / relativeMove.X);
                                        }
                                        this.Rotation -= (float)(Math.PI / 2);  // Adjust for art orientation
                                    }
                                }
                                else if (pathNodeTile == null)
                                {
                                    // Found null tile when assigning path move
                                    Log.Message("Found null tile when assigning path move!");
                                }
                            }
                            else
                            // Stack is empty. Move has been cut short (target tile was solid)
                            {
                                _moveCommandIssued = false;


                                if (_engine.ActorsPhysicallySimulated)
                                    this.Body.LinearVelocity = Vector2.Zero;

                                _velocity = Vector2.Zero;
                            }
                        }
                    }
                }
                else
                {
                    // Break out of the movement loop.
                    _moveCommandIssued = false;

                    //this.Body.LinearVelocity = Vector2.Zero;
                    _velocity = Vector2.Zero;

                    // Inform PathMarshal this grid is permanently occupied.
                    _engine.PathMarshal.OccupancyGrid[CurrentLocation.X, CurrentLocation.Y] = true;

                    // tint it for debug
                    if (_engine.PathDebugEnabled)
                        _engine.TileExistenceCheckByGridRef(_currentLocation).TintColor = Color.LimeGreen;
                }
            }
        }
        #endregion



        #region Die
        /// <summary>
        /// Remove from the game world
        /// </summary>
        public virtual void Die()
        {
            this._movable = false;
            this._selectable = false;
            this._armed = false;

            _engine.ActorQuadTree.Remove(this);
            _engine.SceneGraph.RemoveNode(this);
            _engine.ActorsSceneNode.Children.Remove(this);
            _engine.CurrentLevel.Actors.Remove(this);

            if (this._physicallySimulated)
            {
                _engine.PhysicsSimulator.RemoveBody(this.Body);
                //_engine.PhysicsSimulator.Remove(this.Geom);
                this.Body.Dispose();
            }
            //this.Dispose();
        }
        #endregion



        public virtual void Stop()
        {
            _movementSuspended = true;
            ClearPathOccupancy();
            _targetActor = null;
            //intermediateGoal = currentGoal;
        }


        #region MoveToGridLocation
    /// <summary>
        /// Move from grid square to grid square until the destination is reached.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="attackMove"></param>
    /// <returns>Number of nodes in the path.</returns>
        public int MoveToGridLocation(GridReference target, bool attackMove)
        {
            _attackMove = attackMove;

            if (_movable)
            {
                _moveCommandIssued = false;
                //moveStepComplete = false;

                // Set our new goal
                _currentGoal = target;

                ClearPathOccupancy();

                // If we're already pathing, make our current location the intermediate goal
                GridReference startTarget = _currentLocation;
                if (!_moveStepComplete)
                    startTarget = _intermediateGoal;


                Collection<AStarPathNode> pathNodes = _engine.PathMarshal.DoSearch(startTarget, target);  // currentLocation
                if (pathNodes.Count > 0)
                {
                    // Reset occupied node color
                    if (_engine.PathDebugEnabled)
                        _engine.TileExistenceCheckByGridRef(_currentLocation).TintColor = Color.White;

                    // Inform PathMarshal this node is empty now.
                    _engine.PathMarshal.OccupancyGrid[CurrentLocation.X, CurrentLocation.Y] = false;


                    // Add search nodes to our path stack
                    _pathFound = true;
                    foreach (AStarPathNode pathNode in pathNodes)
                    {
                        // Add to PathMarshal path occupancy grid
                        _engine.PathMarshal.PathOccupancyGrid[pathNode.Position.X, pathNode.Position.Y] = 1;

                        _pathStack.Push(pathNode);

                        // Some color debugging
                        if (_engine.PathDebugEnabled)
                        {
                            Tile thisTile = _engine.TileExistenceCheckByGridRef(pathNode.Position);
                            if (thisTile.TintColor == Color.White)
                                thisTile.TintColor = Color.Tan;
                        }
                    }
                }
                else
                    _pathFound = false;

                // Set us to run!
                _moveCommandIssued = true;
                _movementSuspended = false;
            }

            return _pathStack.Count;
        }
        #endregion



        /// <summary>
        /// Remove all grid occupancies
        /// </summary>
        protected void ClearPathOccupancy()
        {
            // Pop all nodes off the path stack and remove them from the PathMarshal's path occupancy list.
            while (_pathStack.Count > 0 && _pathStack.Peek() != null)
            {
                AStarPathNode pathNode = _pathStack.Pop();
                _engine.PathMarshal.PathOccupancyGrid[pathNode.Position.X, pathNode.Position.Y] = 0;

                if (_engine.PathDebugEnabled)
                {
                    Tile thisTile = _engine.TileExistenceCheckByGridRef(pathNode.Position);
                    thisTile.TintColor = Color.White;
                }
            }
            _pathStack = new Stack<AStarPathNode>();
        }



        #region ClosestEnemyTarget
        /// <summary>
        /// Returns the closest RTSActor that is not on our team and not dead.
        /// </summary>
        /// <returns></returns>
        public RTSActor ClosestEnemyTarget()
        {
            float leastDistance = -1f;
            Vector2 relativePos = Vector2.Zero;
            RTSActor closestActor = null;

            RectangleF drawRect = new RectangleF((AbsolutePosition.X + ((this.Size.X * this.Scale) / 2)) - MaxTargetRange,
                                                 (AbsolutePosition.Y + ((this.Size.Y * this.Scale) / 2)) - MaxTargetRange,
                    (MaxTargetRange * 2), (MaxTargetRange * 2));
            List<GameSceneNode> quadTreeRenderNodes = _engine.ActorQuadTree.Query(drawRect);

            foreach (GameSceneNode n in quadTreeRenderNodes)
            {
                if (n is RTSActor)
                {
                    RTSActor rtsActor = (RTSActor)n;
                    if (rtsActor != null)
                    {
                        if (!rtsActor.IsDead &&
                            rtsActor.Team != this.Team && !rtsActor.Name.Contains("Turret")) // total hack, make it exclude turrets
                        {
                            // Get it's position relative to us
                            relativePos = rtsActor.AbsolutePosition - this.AbsolutePosition;
                            if (leastDistance < 0f || Math.Abs(relativePos.Length()) < leastDistance)
                            {
                                leastDistance = Math.Abs(relativePos.Length());
                                closestActor = rtsActor;
                            }
                        }
                    }
                }
            }
            return closestActor;
        }
        #endregion



        #region ClosestEnemyTargetVector
        /// <summary>
        /// Get a vector pointing at the closest valid target (i.e. alive, not on our team, not an effect)
        /// </summary>
        /// <returns></returns>
        public Vector2 ClosestEnemyTargetVector(out bool enemyFound)
        {
            RTSActor closestEnemy = ClosestEnemyTarget();

            if (closestEnemy != null)
            {
                // Get it's position relative to us
                Vector2 relativePos = Vector2.Zero;
                relativePos = closestEnemy.AbsolutePosition - this.AbsolutePosition;
                enemyFound = true;
                return relativePos;
            }
            else
            {
                enemyFound = false;
                return Vector2.Zero;
            }
        }
        #endregion



        #region QuadrantFromVector
        /// <summary>
        /// Return the quadrant of this 2D vector.
        /// </summary>
        /// <param name="vector">Vector to examine.</param>
        /// <returns></returns>
        public static int QuadrantFromVector(Vector2 vector)
        {
            // Discover quadrant of this vector
            int quadrant = 0;
            if (vector.X > 0)
            {
                // Quadrants 1 or 4
                if (vector.Y > 0)
                    quadrant = 4;
                else
                    quadrant = 1;
            }
            else
            {
                // Quadrants 2 or 3
                if (vector.Y > 0)
                    quadrant = 3;
                else
                    quadrant = 2;
            }
            return quadrant;
        }
        #endregion



        #region QuadrantToAngle
        /// <summary>
        /// Starting angle of this quadrant number.
        /// Returns angle in radians ranging from 0 to 2*Pi
        /// </summary>
        /// <param name="quadrant"></param>
        /// <returns></returns>
        public static float QuadrantToAngle(int quadrant)
        {
            float angle = 0f;
            switch (quadrant)
            {
                case 1:
                    angle = 0f; //-(Math.PI / 2);
                    break;
                case 2:
                    angle = (float)(Math.PI / 2); //((3 * Math.PI) / 2);
                    break;
                case 3:
                    angle = (float)Math.PI; //-(Math.PI / 2);
                    break;
                case 4:
                    angle = (float)((3 * Math.PI) / 2); //((3 * Math.PI) / 2);
                    break;
                default:
                    break;
            }
            return angle;
        }
        #endregion



        #region VectorToEightWayDirection
        /// <summary>
        /// Convert a 2D vector to our 8-way orientation
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public EightWayDirection VectorToEightWayDirection(Vector2 vector)
        {
            EightWayDirection direction = EightWayDirection.North;

            float angle = (float)(Math.Atan2(vector.X, vector.Y));
            angle += (float)(Math.PI);

            if ((angle >= 0 && angle <= (Math.PI / 8)) || // First and last eighths
                (angle >= (15 * (Math.PI / 8)) && angle <= 2 * (Math.PI)))
                direction = EightWayDirection.North;
            else if (angle > (Math.PI / 8) && angle <= (3 * (Math.PI / 8)))
                direction = EightWayDirection.NorthWest;
            else if (angle > (3 * (Math.PI / 8)) && angle <= (5 * (Math.PI / 8)))
                direction = EightWayDirection.West;
            else if (angle > (5 * (Math.PI / 8)) && angle <= (7 * (Math.PI / 8)))
                direction = EightWayDirection.SouthWest;
            else if (angle > (7 * (Math.PI / 8)) && angle <= (9 * (Math.PI / 8)))
                direction = EightWayDirection.South;
            else if (angle > (9 * (Math.PI / 8)) && angle <= (11 * (Math.PI / 8)))
                direction = EightWayDirection.SouthEast;
            else if (angle > (11 * (Math.PI / 8)) && angle <= (13 * (Math.PI / 8)))
                direction = EightWayDirection.East;
            else if (angle > (13 * (Math.PI / 8)) && angle <= (15 * (Math.PI / 8)))
                direction = EightWayDirection.NorthEast;
            else
                direction = EightWayDirection.North;

            return direction;
        }
        #endregion


        #region PointOnCircle
        /// <summary>
        /// Gets a point created by rotating a point over a circle.
        /// Used for figuring out where the end of the turret is
        /// </summary>
        /// <param name="center">Center of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>A point on the circle specified by <paramref name="center"/> and <paramref name="radius"/> moved by a specifed
        /// <paramref name="angle"/>.</returns>
        ///
        protected Vector2 PointOnCircle(Vector2 center, float radius, float angle)
        {
            float angleInRadians = angle * (float)Math.PI / 180;
            Vector2 top = Vector2.Add(center, new Vector2(0, radius));
            return new Vector2(
                (float)(center.X + Math.Cos(angleInRadians) * (center.X - top.X)
                - Math.Sin(angleInRadians) * (center.Y - top.Y)),
                (float)(center.Y + Math.Sin(angleInRadians) * (center.X - top.X)
                + Math.Cos(angleInRadians) * (center.Y - top.Y)));
        }
        #endregion
    }
}
