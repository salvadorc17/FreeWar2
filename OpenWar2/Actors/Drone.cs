using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;
using DEngine;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;

using DSceneGraph;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Basic RTS builder unit
    /// </summary>
    public class Drone : RTSActor
    {
        const int _HARVEST_TIME = 600;

        DroneTurret turret;

        MineralRock _mineralsTarget;
        bool _carryingMinerals;
        bool _harvestCommandDone;
        bool _autoHarvestCommandDone;

        Headquarters _nearestHeadquarters;
        int _headquartersIdentifyCounter = 0;
        int _headquartersIdentifyTime = 2000000;


        int _unloadCounter = 0;
        int _unloadTime = 200;

        int _harvestCounter = 0;
        int _harvestTime = _HARVEST_TIME;

        


        int _mineralsMax = 20;

        public Drone(FactionsGame game)
            : base(game, "Drone")
        {
            _engine = game;

            turret = new DroneTurret(_engine);
            _movable = true;
            _moveSpeed = 1.6f;
            _health = 90;
            _maxHealth = 90;
            _armorType = ArmorType.Light;
            _rotateWithDirection = true;
            _armed = false;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Die()
        {
            turret.Die();

            base.Die();
        }


        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Gimme a turret!
            turret = (DroneTurret)_engine.GetTemplateActorByName("DroneTurret").Clone();
            turret.Team = Team;
            turret.TankBase = this;
            turret.Initialize();

            //if (this._physicallySimulated)
            //    turret.Geom.CollisionEnabled = false;

            //turret.Geom.CollisionGroup = Team;
            //this.Geom.CollisionGroup = Team;

            turret.Position = new Vector2(0,0);
            this.Children.Add(turret);


            // Find HQ immediately upon first update
            _headquartersIdentifyCounter = _headquartersIdentifyTime;

            //turret.Rotation = 0f;

            _engine.OnMoveCommand += new FactionsGameEventHandler(_engine_OnMoveCommand);
        }

        void _engine_OnMoveCommand()
        {
            if (_engine.SelectedActors.Contains(this) && this.IsOurs)
            {
                _harvestCommandDone = false;

                //if (!_carryingMinerals)
                //    _mineralsTarget = null;
            }
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Keep the team updated since we might have recolored (this is a bit of a hack!)
            turret.Team = Team;
            turret.TargetActor = _targetActor;

            UpdateNearestHeadquarters();

            UpdateHarvesting();

            // Update turret rotation
            if (_mineralsTarget != null && !_carryingMinerals)
            {
                Vector2 closestTargetVector = (_mineralsTarget.AbsolutePosition + (this.Size / 2)) - this.AbsolutePosition;
                float absoluteAngle = (float)(Math.Atan2(-closestTargetVector.X, closestTargetVector.Y));
                turret.Rotation = absoluteAngle;
            }
            else
                turret.Rotation = 0;
        }


        /// <summary>
        /// Watch for right clicks on minerals.
        /// Perform harvesting and returning to HQ.
        /// </summary>
        private void UpdateHarvesting()
        {
            // Having trouble re-acquiring a target once we dump our minerals.
            // _mineralsTarget is null - being nulled out by engine OnMove command?


            // Watch for right click on minerals
            MouseState ms = Mouse.GetState();
            if (ms.RightButton == ButtonState.Pressed && _engine.SelectedActors.Contains(this) && this.IsOurs)
            {
                if (!_harvestCommandDone)
                {
                    // Single select of this node
                    Actor actor = _engine.ActorMouseSingleSelect(ms);
                    if (actor != null)
                    {
                        if (actor is GemRockHuge || actor is GemRockLarge || actor is GemRockSmall)
                        {
                            // Find out how many selected actors are drones
                            int droneCount = 0;
                            foreach (Actor selectedActor in _engine.SelectedActors)
                            {
                                if (selectedActor is Drone)
                                    droneCount++;
                            }


                            // If we're a single drone, target the node we selected
                            if (droneCount == 1)
                            {
                                _mineralsTarget = actor as MineralRock;
                                if (!_mineralsTarget.DroneClients.Contains(this))
                                    _mineralsTarget.DroneClients.Add(this);
                                //t = _engine.TileExistenceCheckByGridRef(_mineralsTarget.GridReference);
                                MoveToLocationOtherUnitsAware(_mineralsTarget.GridReference);
                                _harvestCommandDone = true;
                            }
                            else if (droneCount > 1)
                            {
                                // Multiple drones selected
                                // Find all nearby minerals and target drones on the ones that have the least clients
                                RectangleF mineralsRect = new RectangleF(actor.Position.X - (_engine.Graphics.PreferredBackBufferWidth / 2),
                                                                        actor.Position.Y - (_engine.Graphics.PreferredBackBufferHeight / 2),
                                                                        _engine.Graphics.PreferredBackBufferWidth,
                                                                        _engine.Graphics.PreferredBackBufferHeight);
                                List<GameSceneNode> actorList = _engine.ActorQuadTree.Query(mineralsRect);
                                List<MineralRock> mineralsList = new List<MineralRock>();
                                foreach (GameSceneNode node in actorList)
                                {
                                    if (node is GemRockHuge || node is GemRockLarge || node is GemRockSmall)
                                    {
                                        mineralsList.Add(node as MineralRock);
                                    }
                                }

                                if (mineralsList.Count > 0)
                                {
                                    MineralRock leastClientsRock = actor as MineralRock;
                                    foreach (MineralRock rock in mineralsList)
                                    {
                                        if (rock.DroneClients.Count < leastClientsRock.DroneClients.Count)
                                        {
                                            leastClientsRock = rock;
                                        }
                                    }
                                    _mineralsTarget = leastClientsRock;

                                    //Random rand = new Random(DateTime.Now.Millisecond);
                                    //int randIndex = rand.Next(mineralsList.Count);
                                    //_mineralsTarget = mineralsList[randIndex];
                                    if (!_mineralsTarget.DroneClients.Contains(this))
                                        _mineralsTarget.DroneClients.Add(this);

                                    MoveToLocationOtherUnitsAware(_mineralsTarget.GridReference);
                                    _harvestCommandDone = true;
                                }
                                mineralsList.Clear();
                                mineralsList = null;
                            }
                        }
                        else if (actor is Headquarters && _carryingMinerals)
                        {
                            Headquarters hq = actor as Headquarters;
                            if (hq.IsOurs)
                            {
                                MoveToLocationOtherUnitsAware(hq.GridReference);
                                _harvestCommandDone = true;
                            }
                        }
                    }
                }
            }
            else
            {
                _harvestCommandDone = false;
            }



            if (_engine.SelectedActors.Contains(this))
                _engine = _engine;

            float minimumDistanceMultiplier = 2.5f;

            // Perform the actual logic of going and getting minerals and returning them to the HQ
            if (_mineralsTarget != null)
            {
                if (_mineralsTarget.Minerals <= 0)
                {
                    // Find closest mineral node to this one!
                    if (HarvestMineralNodeWithLeastClients(_mineralsTarget))
                        MoveToLocationOtherUnitsAware(_mineralsTarget.GridReference);
                }


                if (!_carryingMinerals)
                {
                    // See if we are near the node to be harvested, and harvest it.
                    Vector2 relativePos = this.AbsolutePosition - _mineralsTarget.AbsolutePosition;
                    float mineralsDistance = Math.Abs(relativePos.Length());
                    if (mineralsDistance < (_engine.TileWidth < _engine.TileHeight ? _engine.TileHeight : _engine.TileWidth) * minimumDistanceMultiplier) // Check distance on stop. Not too prohibitive a value here!
                    {
                        _harvestCounter++;
                        if (_harvestCounter == _harvestTime)
                        {
                            _harvestCounter = 0;
                            Random rand = new Random();
                            _harvestTime = _HARVEST_TIME + (rand.Next(64) - 32);

                            if (_mineralsTarget.Minerals > 0)
                            {
                                _mineralsTarget.Minerals -= _mineralsMax;
                                _carryingMinerals = true;
                                if (_nearestHeadquarters != null)
                                {
                                    MoveToLocationOtherUnitsAware(_nearestHeadquarters.GridReference);
                                }
                            }
                            //else
                            //{
                            //    HarvestMineralNodeWithLeastClients(_mineralsTarget);
                            //    if (_mineralsTarget != null && !_moveCommandIssued)
                            //        MoveToLocationOtherUnitsAware(_mineralsTarget.GridReference);
                            //}
                        }
                    }
                    else
                    {
                        // Find a new mineral target that's closer
                        //HarvestMineralNodeWithLeastClients(_mineralsTarget);
                    }
                }
                else
                {
                    if (_nearestHeadquarters != null)
                    {
                        Vector2 relativePos = this.AbsolutePosition - _nearestHeadquarters.AbsolutePosition;
                        float hqDistance = Math.Abs(relativePos.Length());
                        if (hqDistance < (_engine.TileWidth < _engine.TileHeight ? _engine.TileHeight : _engine.TileWidth) * minimumDistanceMultiplier) 
                        {
                            _unloadCounter++;
                            if (_unloadCounter == _unloadTime)
                            {
                                _unloadCounter = 0;

                                DepositMinerals();

                                HarvestMineralNodeWithLeastClients(_mineralsTarget);
                                if (_mineralsTarget != null)
                                    MoveToLocationOtherUnitsAware(_mineralsTarget.GridReference);
                            }
                        }
                    }
                }
            }
        }








        void MoveToLocationOtherUnitsAware(GridReference targetTileGridRef)
        {
            Collection<GridReference> validTargetGridRefs = _engine.GetFreeNodes(targetTileGridRef, 1);

            // Apply move & pathing.
            if (validTargetGridRefs.Count > 0)
                this.MoveToGridLocation(validTargetGridRefs[0], false);
        }


        /// <summary>
        /// Remove the last target tile from the path stack.
        /// </summary>
        private void ClipEndOfMineralsPath()
        {
            AStarPathNode[] pathNodeArray = _pathStack.ToArray();
            Stack<AStarPathNode> newStack = new Stack<AStarPathNode>(_pathStack.Count - 1);
            for (int i = _pathStack.Count - 1; i > 1; i--)
            {
                newStack.Push(pathNodeArray[i]);
            }
            _pathStack = newStack;
        }


        private void DepositMinerals()
        {
            if (_carryingMinerals)
            {
                _carryingMinerals = false;
                _engine.LocalPlayer.Resources += _mineralsMax;
                _engine.TopInfoBar.ResourcesAmount = _engine.LocalPlayer.Resources.ToString();
            }
        }



        bool HarvestNearestMineralNode()
        {
            bool result = false;
            // Find closest mineral node to this one!
            MineralRock newRock = FindNearestMineralNode();
            if (newRock != null)
            {
                if (newRock != _mineralsTarget)
                {
                    _mineralsTarget.DroneClients.Remove(this);
                    _mineralsTarget = newRock;
                    newRock.DroneClients.Add(this);
                    result = true;
                }
            }
            return result;
        }


        private MineralRock FindNearestMineralNode()
        {
            List<MineralRock> minerals = new List<MineralRock>();
            foreach (Actor a in _engine.Actors)
            {
                if (a is MineralRock && (a as MineralRock).Minerals > 0)
                {
                    minerals.Add(a as MineralRock);
                }
            }

            float lowestDistance = 0;
            MineralRock nearestRock = null;
            if (minerals.Count > 0)
            {
                foreach (MineralRock mr in minerals)
                {
                    Vector2 relativePos = mr.AbsolutePosition - this.AbsolutePosition;
                    if (nearestRock == null)
                    {
                        nearestRock = mr;
                        lowestDistance = (float)Math.Abs(relativePos.Length());
                    }
                    else
                    {
                        float thisLength = (float)Math.Abs(relativePos.Length());
                        if (thisLength < lowestDistance)
                        {
                            lowestDistance = thisLength;
                            nearestRock = mr;
                        }
                    }
                }
            }
            return nearestRock;
        }


        bool HarvestMineralNodeWithLeastClients(MineralRock rock)
        {
            bool result = false;
            // Find closest mineral node to this one!
            MineralRock newRock = FindMineralRockWithLeastClients(rock);
            if (newRock != null)
            {
                if (newRock != _mineralsTarget)
                {
                    _mineralsTarget.DroneClients.Remove(this);
                    _mineralsTarget = newRock;
                    newRock.DroneClients.Add(this);
                    result = true;
                }
                
                if (newRock.DroneClients.Count >= 5)
                {
                    //Stop();
                    //newRock.DroneClients.Remove(this);
                }
            }
            return result;
        }


        private MineralRock FindMineralRockWithLeastClients(MineralRock targetRock)
        {
            RectangleF mineralsRect = new RectangleF(targetRock.Position.X - (_engine.Graphics.PreferredBackBufferWidth / 4),
                                                targetRock.Position.Y - (_engine.Graphics.PreferredBackBufferHeight / 4),
                                                _engine.Graphics.PreferredBackBufferWidth / 2,
                                                _engine.Graphics.PreferredBackBufferHeight / 2);
            List<GameSceneNode> actorList = _engine.ActorQuadTree.Query(mineralsRect);
            List<MineralRock> mineralsList = new List<MineralRock>();
            foreach (GameSceneNode node in actorList)
            {
                if (node is GemRockHuge || node is GemRockLarge || node is GemRockSmall)
                {
                    if ((node as MineralRock).Minerals > 0)
                        mineralsList.Add(node as MineralRock);
                }
            }

            MineralRock lowestClientsRock = null;
            if (targetRock.Minerals > 0)
                lowestClientsRock = targetRock;

            if (mineralsList.Count > 0)
            {
                foreach (MineralRock rock in mineralsList)
                {
                    // Find out how many clients are on this rock, excluding ourselves
                    int countMinusOurselves = rock.DroneClients.Count;
                    if (rock.DroneClients.Contains(this))
                        countMinusOurselves--;
                    
                    // Find out how many clients are on the current lowest
                    int currentLowestCountMinusOurselves = 0;
                    if (lowestClientsRock != null)
                    {
                        currentLowestCountMinusOurselves = lowestClientsRock.DroneClients.Count;
                        if (lowestClientsRock.DroneClients.Contains(this))
                            currentLowestCountMinusOurselves--;
                    }

                    // If this rock has a lower count, choose it
                    if (countMinusOurselves < currentLowestCountMinusOurselves || lowestClientsRock == null)
                    {
                        lowestClientsRock = rock;
                    }
                }
            }
            return lowestClientsRock;
        }




        /// <summary>
        /// Every so many ticks, find the headquarters that is closest to us.
        /// </summary>
        private void UpdateNearestHeadquarters()
        {
            _headquartersIdentifyCounter++;
            if (_headquartersIdentifyCounter >= _headquartersIdentifyTime || _nearestHeadquarters == null)
            {
                _headquartersIdentifyCounter = 0;

                // Find our nearest headquarters
                List<Headquarters> ourHeadquarters = new List<Headquarters>();
                foreach (Actor a in _engine.Actors)
                {
                    if (a is Headquarters && (a as Headquarters).IsOurs)
                    {
                        ourHeadquarters.Add(a as Headquarters);
                    }
                }

                float lowestDistance = 0;
                Headquarters nearestHQ = null;
                if (ourHeadquarters.Count > 0)
                {
                    foreach (Headquarters hq in ourHeadquarters)
                    {
                        Vector2 relativePos = hq.AbsolutePosition - this.AbsolutePosition;
                        if (nearestHQ == null)
                        {
                            nearestHQ = hq;
                            lowestDistance = (float)Math.Abs(relativePos.Length());
                        }
                        else
                        {
                            float thisLength = (float)Math.Abs(relativePos.Length());
                            if (thisLength < lowestDistance)
                            {
                                lowestDistance = thisLength;
                                nearestHQ = hq;
                            }
                        }
                    }
                    _nearestHeadquarters = nearestHQ;
                }
                else
                    _nearestHeadquarters = null;
            }
        }


        public override void Stop()
        {
           base.Stop();

           _harvestCommandDone = false;
           _mineralsTarget = null;
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
