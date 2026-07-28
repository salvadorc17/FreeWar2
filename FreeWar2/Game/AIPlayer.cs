using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using DEngine;

namespace FactionsGame
{
    /// <summary>
    /// Computer-controlled opponent.
    /// Will (eventually) use an ants system to devise navigation paths and rally points.
    /// Possibly defensive locations?
    /// </summary>
    public class AIPlayer : FactionsPlayer
    {
        public enum AIState { Idle, Waiting, Defending, Attacking };

        protected FactionsGame _engine;

        protected Collection<AntsSystem> _antsSystems = new Collection<AntsSystem>();
        protected Collection<GridReference> _enemyStartPoints = new Collection<GridReference>();
        protected GridReference _startRef;
        protected AIState _aiState = AIState.Idle;
        protected Collection<RTSActor> _units = new Collection<RTSActor>();
        
        protected GridReference _rallyPoint;
        protected Collection<GridReference> _antAttackPath = new Collection<GridReference>();
        protected Collection<AStarPathNode> _smoothedAttackPath = new Collection<AStarPathNode>();

        // Timing variables
        int _attackCounter = 0;
        int _attackTimer = 1500;





        #region Public Properties
        public AIState State
        {
            get
            {
                return _aiState;
            }
            set
            {
                _aiState = value;
            }
        }
        #endregion


        public AIPlayer(FactionsGame game)
            : base(game)
        {
            _engine = game;
        }


        public override void Initialize()
        {
            base.Initialize();

            GetStartPoints();
            CreateAntsSystems();
            GetRallyPoint();
            GetControlledUnits();
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // AI update loop.
            // I guess we're going to need some states to control things?
            switch (_aiState)
            {
                case AIState.Idle:

                    // Grab all units and send them to the rally point.
                    if (_rallyPoint != new GridReference(0, 0))
                    {
                        // Get the required number of free adjacent nodes surrounding the target square.
                        Collection<GridReference> validTargetGridRefs = _engine.GetFreeNodes(_rallyPoint, _units.Count);

                        // Apply move & pathing.
                        for (int i = 0; i < _units.Count; i++)
                        {
                            _units[i].MoveToGridLocation(validTargetGridRefs[i], false);
                        }

                        _aiState = AIState.Waiting;
                    }
                    break;


                case AIState.Waiting:

                    // Count for a while, until attack timer is up.
                    // Then attack and reset the timer.
                    _attackCounter++;
                    if (_attackCounter >= _attackTimer)
                    {
                        _attackCounter = 0;

                        // Grab all units and send them to the end of the attack path!
                        GridReference attackPoint = _antAttackPath[_antAttackPath.Count - 1];

                        // Get the required number of free adjacent nodes surrounding the target square.
                        Collection<GridReference> validTargetGridRefs = _engine.GetFreeNodes(attackPoint, _units.Count);

                        // Apply move & pathing.
                        if (validTargetGridRefs.Count > 0)
                        {
                            for (int i = 0; i < _units.Count; i++)
                            {
                                _units[i].MoveToGridLocation(validTargetGridRefs[i], true);
                            }

                            _aiState = AIState.Attacking;
                        }
                    }


                    break;
                case AIState.Attacking:

                    // Wait and evaluate result of attack

                    break;
                case AIState.Defending:

                    // How to trigger defending state? In waiting phase I suppose.

                    break;
                default:
                    break;
            }
        }



        #region GetStartPoints
        /// <summary>
        /// Get location of our start point.
        /// Get locations of all player start points not belonging to our team.
        /// Create ants systems for each enemy start point
        /// </summary>
        protected void GetStartPoints()
        {
            // Obtain enemy start locations
            // Create ants systems for each
            // Deploy ants!
            if (_engine.CurrentLevel != null)
            {
                Level level = _engine.CurrentLevel;
                // Find our start point's ref
                bool gridRefFound = false;
                foreach (Actor startPoint in level.StartPoints)
                {
                    if (startPoint.Team == this.Team && startPoint.MaskColor == _engine.PlayerColors[this.Color - 1])
                    {
                        _startRef = startPoint.GridReference;
                        gridRefFound = true;
                        break;
                    }
                }

                if (gridRefFound)
                {
                    // Get enemy startpoints
                    foreach (Actor enemyPoint in level.StartPoints)
                    {
                        if (enemyPoint.Team != this.Team)
                        {
                            _enemyStartPoints.Add(enemyPoint.GridReference);
                        }
                    }
                }
            }
        }
        #endregion



        #region CreateAntsSystems
        protected void CreateAntsSystems()
        {
            foreach (GridReference enemyRef in _enemyStartPoints)
            {
                // Create ants system, perform ants search and add to the ant system list.
                AntsSystem antsSystem = new AntsSystem(_engine, _startRef, enemyRef);
                antsSystem.AntTick += new TickHandler(antsSystem_AntTick);
                antsSystem.DoAntsSearch();
                _antsSystems.Add(antsSystem);
            }
        }

        void antsSystem_AntTick()
        {
            _engine.Tick();
        }
        #endregion



        #region GetRallyPoint
        /// <summary>
        /// Search along the ant attack path for a node which has free space in a 2-tile radius around it.
        /// Establish this as the rally point.
        /// </summary>
        protected void GetRallyPoint()
        {
            // Perform tasks using ants system for AI battle planning
            foreach (AntsSystem antSys in _antsSystems)
            {
                // Send out a pilot ant to devise an attack path!
                _antAttackPath = antSys.PilotAntPath();

                // Debug coloration of pilot ant path
                foreach (GridReference gr in _antAttackPath)
                {
                    Tile t = _engine.TileExistenceCheckByGridRef(gr);
                    if (_engine.PathDebugEnabled)
                    {
                        if (t != null)
                            t.TintColor = Microsoft.Xna.Framework.Color.BlueViolet;
                    }
                }

                // Devise a rally point
                int minimumRallyPointDistance = 10;
                int minimumRallyPointCounter = 0;
                foreach (GridReference gr in _antAttackPath)
                {
                    // Find a rally point with 21 free adjacent nodes where 0 nodes were occupied when searching (empty field)
                    // 45 is good too for a large circle
                    int nodesOccupied = 0;
                    Collection<GridReference> adjacentNodes = _engine.GetFreeAdjacentNodes(gr, 21, ref nodesOccupied);
                    if (nodesOccupied == 0 && minimumRallyPointCounter >= minimumRallyPointDistance)
                    {
                        _rallyPoint = gr;

                        if (_engine.PathDebugEnabled)
                        {
                            // Debug color of rally point(s)
                            foreach (GridReference adjacentNode in adjacentNodes)
                            {
                                Tile rallyTile = _engine.TileExistenceCheckByGridRef(adjacentNode);
                                if (rallyTile != null)
                                {
                                    rallyTile.TintColor = Microsoft.Xna.Framework.Color.Firebrick;
                                }
                                if (adjacentNode == gr)
                                    rallyTile.TintColor = Microsoft.Xna.Framework.Color.HotPink;
                            }
                        }

                        break;
                    }
                    minimumRallyPointCounter++;
                }
            }
        }
        #endregion



        #region GetControlledUnits
        /// <summary>
        /// Obtain a collection of RTSActors under the players' control.
        /// </summary>
        protected void GetControlledUnits()
        {
            // Obtain our units!
            foreach (Actor actor in _engine.CurrentLevel.Actors)
            {
                if (actor.Team == this.Team && actor.MaskColor == _engine.PlayerColors[this.Color - 1])
                {
                    if (actor is RTSActor)
                    {
                        RTSActor rtsActor = (RTSActor)actor;
                        _units.Add(rtsActor);
                    }
                }
            }
        }
        #endregion


    }
}
