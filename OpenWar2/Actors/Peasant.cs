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

namespace FactionsGame.Actors
{
    /// <summary>
    /// Basic RTS soldier
    /// </summary>
    public class Peasant : RTSActor
    {
        string shootSuffix = "Attack";

        public const int CORPSE_DISAPPEAR_TIME = 3000;
        protected int corpseTimer = 0;

        protected EightWayDirection lastCompassDirection = EightWayDirection.North;

        public Peasant(FactionsGame game)
            : base(game, "Peasant")
        {
            _engine = game;
            _rotateWithDirection = false;
            _movable = true;

            _moveSpeed = 1.4f;
            _fireRate = 24;
            _damage = 6;
            _burstShotsMax = 6;
            _reloadTime = 140;
            
            _stopOnHostilities = true;
            _armed = true;
            _useBurst = true;
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

            //this.Geom.CollisionGroup = this.Team;

            this.OnAttackStart += new RTSActorTargetEventHandler(Soldier_OnAttack);
            this.OnBurstStart += new RTSActorEventHandler(Soldier_OnBurstStart);
            this.OnBurstEnd += new RTSActorEventHandler(Soldier_OnBurstEnd);
            this.OnAttackEnd += new RTSActorEventHandler(Soldier_OnAttackEnd);
        }

        protected override void UnloadContent()
        {
            this.OnAttackStart -= Soldier_OnAttack;
            this.OnBurstStart -= Soldier_OnBurstStart;
            this.OnBurstEnd -= Soldier_OnBurstEnd;
            this.OnAttackEnd -= Soldier_OnAttackEnd;
            base.UnloadContent();
        }

        void Soldier_OnBurstEnd()
        {
            shootSuffix = string.Empty;
        }

        void Soldier_OnAttackEnd()
        {
            FaceCompassDirectionUsingFrames();
        }

        void Soldier_OnBurstStart()
        {
            shootSuffix = "Attack";
            _engine.PlaySound("LightMG1");
        }

        void Soldier_OnAttack(RTSActor target)
        {

            // Shoot or aim animation, depending on our fire rate (burst fire)
            //string shootSuffix = "Attack";
            //if (_fireRate == _reloadTime)
            //    shootSuffix = string.Empty;

            for (int i = 0; i < this.Sprites.Count; i++)
            {
                if (this.Sprites[i].Name == "Peasant" + _compassDirection.ToString() + shootSuffix)
                {
                    if (_spriteIndex != i)
                        this._spriteIndex = i;
                    break;
                }
            }


            float adjustedDamage = _damage;
            switch (target.Armor)
            {
                case ArmorType.Light:
                    adjustedDamage *= 0.8f;
                    break;
                case ArmorType.Medium:
                    adjustedDamage *= 0.4f;
                    break;
                case ArmorType.Heavy:
                    adjustedDamage *= 0.2f;
                    break;
                case ArmorType.Building:
                    adjustedDamage *= 0.5f;
                    break;
                case ArmorType.Super:
                    adjustedDamage *= 0.05f;
                    break;
                default:
                    break;
            }

            target.Health -= (int)adjustedDamage;


            // Make a bullet/blood splat impact effect
            if (target.Name == "Peasant")
            {
                Actor templateActor = _engine.GetTemplateActorByName("BloodSplat1");
                BloodSplat1 impactEffect = (BloodSplat1)templateActor.Clone();

                // Randomize location of bullet impact slightly
                Random rand = new Random();
                float randomX, randomY;
                randomX = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value
                randomY = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value

                impactEffect.Position
                    = new Vector2(target.Position.X + ((target.Size.X * target.Scale) / 2) + randomX,
                                  target.Position.Y + ((target.Size.Y * target.Scale) / 2) + randomY);
                //impactEffect.Scale = 0.6f;
                impactEffect.TintColor = new Color(255, 255, 255, 200);
                impactEffect.Initialize();

                // Attach the node to the graph
                _engine.EffectsSceneNode1.Children.Add(impactEffect);
            }
            else
            {
                Actor templateActor = _engine.GetTemplateActorByName("BulletImpact");
                BulletImpact impactEffect = (BulletImpact)templateActor.Clone();

                // Randomize location of bullet impact slightly
                Random rand = new Random();
                float randomX, randomY;
                randomX = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value
                randomY = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value

                impactEffect.Position = new Vector2(target.Position.X + randomX, target.Position.Y + randomY);
                //impactEffect.Scale = 0.6f;
                impactEffect.TintColor = new Color(255, 255, 255, 200);
                impactEffect.Initialize();

                // Attach the node to the graph
                _engine.EffectsSceneNode1.Children.Add(impactEffect);
                //engine.Actors.Add(impactEffect);
            }

        }
        #endregion


        /// <summary>
        /// Select the appropriate sprite animation for this compass direction.
        /// </summary>
        protected void FaceCompassDirectionUsingFrames()
        {
            // Find our sprite that has this name
            for (int i = 0; i < _sprites.Count; i++)
            {
                if (_sprites[i].Name == "Peasant" + _compassDirection.ToString())
                {
                    _spriteIndex = i;
                    break;
                }
            }
        }


        public override void Update(GameTime gameTime)
        {
            if (!IsDead)
            {
                // Use compass direction to figure out sprite name
                if (_compassDirection != lastCompassDirection)
                {
                    FaceCompassDirectionUsingFrames();
                    lastCompassDirection = _compassDirection;
                }



                // Soldier-specific code to pause animation on a certain frame
                if (!_moveCommandIssued && !_enemyFound)
                {
                    Sprite.Animating = false;

                    // Graphic-specific code
                    if (lastCompassDirection == EightWayDirection.North ||
                        lastCompassDirection == EightWayDirection.South ||
                        lastCompassDirection == EightWayDirection.East ||
                        lastCompassDirection == EightWayDirection.West)
                        Sprite.FrameIndex = 0;
                    else
                        if (Sprite.Frames.Count > 3)
                        Sprite.FrameIndex = 3;
                }
                else
                    Sprite.Animating = true;
            }
            
            base.Update(gameTime);

            //CombatUpdate();
        }


        //void CombatUpdate()
        //{
        //    if (!IsDead)
        //    {
        //        // Use compass direction to figure out sprite name
        //        if (_compassDirection != lastCompassDirection)
        //        {
        //            FaceCompassDirectionUsingFrames();
        //            lastCompassDirection = _compassDirection;
        //        }

        //        // Soldier-specific code to pause animation on a certain frame
        //        if (!_moveCommandIssued && !enemyFound)
        //        {
        //            _sprites[_spriteIndex].Animating = false;

        //            // Graphic-specific code
        //            if (lastCompassDirection == EightWayDirection.North ||
        //                lastCompassDirection == EightWayDirection.South ||
        //                lastCompassDirection == EightWayDirection.East ||
        //                lastCompassDirection == EightWayDirection.West)
        //                _sprites[_spriteIndex].FrameIndex = 0;
        //            else if (_sprites[_spriteIndex].Frames.Count > 3)
        //                _sprites[_spriteIndex].FrameIndex = 3;
        //        }
        //        else
        //            _sprites[_spriteIndex].Animating = true;


        //        if (this.PhysicallySimulated)
        //            this.Geom.CollisionGroup = this.Team;


        //        // Look for enemies and engage!
        //        bool attackEngaged = false;
        //        if ((_attackMove && _moveStepComplete) || !_moveCommandIssued)
        //        {
        //            enemyFound = false;
        //            Vector2 closestTargetVector = ClosestEnemyTargetVector(out enemyFound);

        //            // Examine closest target
        //            if (enemyFound && Math.Abs(closestTargetVector.Length()) <= MaxTargetRange && Math.Abs(closestTargetVector.Length()) > 0)
        //            {
        //                // Get absolute angle
        //                float absoluteAngle = (float)(Math.Atan2(closestTargetVector.X, closestTargetVector.Y));
        //                absoluteAngle = (float)(Math.PI / 2) - absoluteAngle;

        //                int quadrant = QuadrantFromVector(closestTargetVector);

        //                float startAngle = 0f;
        //                startAngle += QuadrantToAngle(quadrant);

        //                // Add absolute angle to quadrant start angle
        //                float shootAngle = startAngle + absoluteAngle;

        //                // Stop pathing while we're attacking
        //                MovementSuspended = true;

        //                // Remove and reset all nodes on the current path (we've stopped to attack)
        //                //while (pathStack.Count > 0 && pathStack.Peek() != null)
        //                //{
        //                //    AStarPathNode pathNode = pathStack.Pop();
        //                //    engine.PathMarshal.PathOccupancyGrid[pathNode.Position.X, pathNode.Position.Y] = 0;

        //                //    if (engine.PathDebugEnabled)
        //                //    {
        //                //        Tile thisTile = engine.TileExistenceCheckByGridRef(pathNode.Position);
        //                //        thisTile.TintColor = Color.White;
        //                //    }
        //                //}


        //                // Convert our shoot direction into a compass direction
        //                _compassDirection = VectorToEightWayDirection(closestTargetVector);





        //                // Shoot when the timer is up
        //                if (gunFireCounter >= _fireRate)
        //                {
        //                    string shootSuffix = "Attack";


        //                    // Figure out when burst is up and set fire rate accordingly
        //                    if (_burstShotsCount == 0)
        //                    {
        //                        // Reset original fire rate when done reloading
        //                        _fireRate = _originalFireRate;
        //                        if (!_fireSoundPlayed)
        //                        {
        //                            _fireSoundPlayed = true;
        //                            _engine.PlaySound("LightMG1");
        //                        }
        //                    }
        //                    _burstShotsCount++;
        //                    if (_burstShotsCount == _burstShotsMax)
        //                    {
        //                        _burstShotsCount = 0;
        //                        _fireRate = _reloadTime;
        //                        shootSuffix = string.Empty;
        //                        _fireSoundPlayed = false;
        //                    }


        //                    // Find our shoot/aim animation
        //                    for (int i = 0; i < this.Sprites.Count; i++)
        //                    {
        //                        if (this.Sprites[i].Name == "Soldier" + _compassDirection.ToString() + shootSuffix)
        //                        {
        //                            this._spriteIndex = i;
        //                            break;
        //                        }
        //                    }



        //                    gunFireCounter = 0f;


        //                    RTSActor targetActor = ClosestEnemyTarget();
        //                    if (targetActor != null)
        //                    {
        //                        float adjustedDamage = _damage;
        //                        switch (targetActor.Armor)
        //                        {
        //                            case ArmorType.Light:
        //                                adjustedDamage *= 0.8f;
        //                                break;
        //                            case ArmorType.Medium:
        //                                adjustedDamage *= 0.4f;
        //                                break;
        //                            case ArmorType.Heavy:
        //                                adjustedDamage *= 0.2f;
        //                                break;
        //                            case ArmorType.Building:
        //                                adjustedDamage *= 0.5f;
        //                                break;
        //                            case ArmorType.Super:
        //                                adjustedDamage *= 0.05f;
        //                                break;
        //                            default:
        //                                break;
        //                        }

        //                        targetActor.Health -= (int)adjustedDamage;

        //                        // Make a bullet/blood splat impact effect
        //                        if (targetActor.Name == "Soldier")
        //                        {
        //                            Actor templateActor = _engine.GetTemplateActorByName("BloodSplat1");
        //                            BloodSplat1 impactEffect = (BloodSplat1)templateActor.Clone();

        //                            // Randomize location of bullet impact slightly
        //                            Random rand = new Random();
        //                            float randomX, randomY;
        //                            randomX = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value
        //                            randomY = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value

        //                            impactEffect.Position
        //                                = new Vector2(targetActor.Position.X + ((targetActor.Size.X * targetActor.Scale) / 2) + randomX,
        //                                              targetActor.Position.Y + ((targetActor.Size.Y * targetActor.Scale) / 2) + randomY);
        //                            //impactEffect.Scale = 0.6f;
        //                            impactEffect.TintColor = new Color(255, 255, 255, 200);
        //                            impactEffect.Initialize();

        //                            // Attach the node to the graph
        //                            _engine.EffectsSceneNode1.Children.Add(impactEffect);
        //                        }
        //                        else
        //                        {
        //                            Actor templateActor = _engine.GetTemplateActorByName("BulletImpact");
        //                            BulletImpact impactEffect = (BulletImpact)templateActor.Clone();

        //                            // Randomize location of bullet impact slightly
        //                            Random rand = new Random();
        //                            float randomX, randomY;
        //                            randomX = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value
        //                            randomY = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value

        //                            impactEffect.Position = new Vector2(targetActor.Position.X + randomX, targetActor.Position.Y + randomY);
        //                            //impactEffect.Scale = 0.6f;
        //                            impactEffect.TintColor = new Color(255, 255, 255, 200);
        //                            impactEffect.Initialize();

        //                            // Attach the node to the graph
        //                            _engine.EffectsSceneNode1.Children.Add(impactEffect);
        //                            //engine.Actors.Add(impactEffect);

        //                        }

        //                    }

        //                }
        //                attackEngaged = true;
        //            }
        //            else
        //            {
        //                // Closest target was out of range.
        //                enemyFound = false;
        //                attackEngaged = false;
        //                MovementSuspended = false;

        //                // Reset burst count
        //                _burstShotsCount = 0;
        //            }

        //            // Weren't in range or step was not complete.
        //            if (!attackEngaged)
        //            {
        //                FaceCompassDirectionUsingFrames();
        //            }
        //        }

        //        gunFireCounter++;
        //    }
        //}



        public override void Die()
        {
            // Death animation
            if (_sprites[_spriteIndex].Name != "SoldierDieSoft")
            {
                for (int i = 0; i < _sprites.Count; i++)
                {
                    if (_sprites[i].Name == "SoldierDieSoft")
                    {
                        _spriteIndex = i;

                        if (_engine.ActorsPhysicallySimulated)
                            Body.Enabled = false;
                        break;
                    }
                }
            }

            // Destroy our health bar and selection box.
            if (SelectionBox != null)
            {
                _engine.SceneGraph.RemoveNode(SelectionBox);
                this.Children.Remove(SelectionBox);
                SelectionBox.Dispose();
                SelectionBox = null;
            }
            if (HealthBar != null)
            {
                _engine.SceneGraph.RemoveNode(HealthBar);
                this.Children.Remove(HealthBar);
                HealthBar.Dispose();
                HealthBar = null;
            }
            _engine.SelectedActors.Remove(this);

            // Only destroy corpse after a set period
            if (corpseTimer >= CORPSE_DISAPPEAR_TIME)
                base.Die();
            corpseTimer++;

            
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
