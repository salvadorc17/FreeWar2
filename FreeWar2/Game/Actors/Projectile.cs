using System;
using System.Collections.Generic;
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
    /// 120mm tank shot.
    /// Has a damage value & radius, velocity, and an expiry timer.
    /// Will spawn an EnergyExplosion on detonation.
    /// 
    /// </summary>
    public class Projectile : Actor
    {
        public enum ProjectileType
        {
            Light,
            Medium,
            Heavy,
            ArmorPiercing,
            AntiPersonnel,
            Nuclear
        };

        float rotationOffset = 0;        // Rotation correction (change the default direction)

        protected Vector2 _shootVector; // normalized

        protected Vector2 _targetVector;
        protected float _velocityMultiplier = 10f; // velocity multiplier
        protected bool _forceApplied;
        protected int _lifeCounter = 180;
        protected Vector2 _initialVelocity;
        protected Vector2 _shootVelocity;
        protected int _damage = 0;
        protected float _distanceTraveled = 0;
        protected float _damageRadius = 12;
        protected ProjectileType _projectileType = ProjectileType.Light;

        bool _dissipated;
        string _effectName = "EnergyExplosion";
        float _effectScale = 0.6f;
        Color _effectColor = new Color(255, 255, 255, 200);



        FactionsGame engine;

        // Explosion template;
        Actor explosionTemplate;



        #region Public Properties
        public ProjectileType Type
        {
            get
            {
                return _projectileType;
            }
            set
            {
                _projectileType = value;
                SetProjectileType(value);
            }
        }
        public Vector2 TargetVector
        {
            get
            {
                return _targetVector;
            }
            set
            {
                _targetVector = value;
            }
        }
        /// <summary>
        /// Unit vector for shoot direction
        /// </summary>
        public Vector2 ShootVector
        {
            get
            {
                return _shootVector;
            }
            set
            {
                _shootVector = value;
            }
        }
        public Vector2 InitialVelocity
        {
            get
            {
                return _initialVelocity;
            }
            set
            {
                _initialVelocity = value;
            }
        }
        #endregion


        public Projectile(FactionsGame game)
            : base(game, "Projectile")
        {
            engine = game;
            _isEffect = true;
            SetProjectileType(_projectileType);
        }



        void SetProjectileType(ProjectileType projectileType)
        {
            _projectileType = projectileType;

            switch (_projectileType)
            {
                case ProjectileType.Light:
                    _damage = 60;
                    _velocityMultiplier = 10;
                    _damageRadius = 12;
                    _effectColor = Color.Yellow;
                    _effectScale = 0.3f;
                    break;
                case ProjectileType.Medium:
                    _damage = 90;
                    _velocityMultiplier = 12;
                    _damageRadius = 16;
                    _effectColor = Color.Orange;
                    _effectScale = 0.6f;
                    break;
                case ProjectileType.Heavy:
                    _damage = 120;
                    _velocityMultiplier = 16;
                    _damageRadius = 24;
                    _effectColor = Color.Red;
                    _effectScale = 1f;
                    break;
                case ProjectileType.AntiPersonnel:
                    _damage = 140;
                    _velocityMultiplier = 5;
                    _damageRadius = 150;
                    _effectColor = Color.Purple;
                    _effectScale = 1f;
                    break;
                case ProjectileType.ArmorPiercing:
                    _damage = 200;
                    _velocityMultiplier = 24;
                    _damageRadius = 5;
                    _effectColor = Color.Blue;
                    _effectScale = 0.3f;
                    break;
                case ProjectileType.Nuclear:
                    _damage = 25000;
                    _velocityMultiplier = 1;
                    _lifeCounter = 2000;
                    _damageRadius = 1300;
                    _effectColor = Color.LimeGreen;
                    _effectScale = 25f;
                    break;
                default:
                    break;
            }
        }



        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            explosionTemplate = engine.GetTemplateActorByName(_effectName);
        }
        #endregion



        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private float RadianToDegree(float angle)
        {
            return angle * (180.0f / (float)Math.PI);
        }


        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private float DegreeToRadian(float angle)
        {
            float radians = ((float)Math.PI / 180f) * angle;
            return radians;
        }

        /// <summary>
        /// Convert a unit vector to radians
        /// </summary>
        /// <param name="unitVector"></param>
        /// <returns></returns>
        private float UnitVectorToRadian(Vector2 unitVector)
        {
            return (float)Math.Atan2(unitVector.X, unitVector.Y);
        }

        /// <summary>
        /// Convert radians to a unit vector.
        /// 2 Pi will yield a 90 degree unit vector
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Vector2 RadianToUnitVector(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Apply shoot force only once
            if (!_forceApplied)
            {
                // Set rotation
                float angle = (float)(Math.PI - Math.Atan2(_shootVector.X, _shootVector.Y));
                Rotation = angle + rotationOffset;

                // Apply the multiplied unit vector velocity!
                _shootVelocity = new Vector2(_shootVector.X * _velocityMultiplier,
                                                _shootVector.Y * _velocityMultiplier);
                _shootVelocity += _initialVelocity;

                if (engine.ActorsPhysicallySimulated)
                    Body.LinearVelocity = _shootVelocity;


                _forceApplied = true;

                // Make a sound
                //engine.PlaySound("HunFire");
            }

            // Maintain the current velocity
            //Body.AngularVelocity = 0;
            //Body.ClearTorque();

            this.Position += _shootVelocity;
            _distanceTraveled += _shootVelocity.Length();


            // If we have reached the distance of the target, explode!
            if (_distanceTraveled >= _targetVector.Length())
            {
                if (!_dissipated)
                {
                    // Lookup actor
                    RectangleF targetRectangle = new RectangleF(this.Position.X - _damageRadius, this.Position.Y - _damageRadius, _damageRadius * 2, _damageRadius * 2);
                    List<GameSceneNode> targetsHit = engine.ActorQuadTree.Query(targetRectangle);

                    // Damage all!
                    foreach (GameSceneNode node in targetsHit)
                    {
                        if (node is RTSActor)
                        {
                            RTSActor rtsActor = (RTSActor)node;
                            if (rtsActor.Team != this.Team)
                            {
                                // Apply adjustment to damage value depending on target
                                float adjustedDamage = _damage;

                                switch (rtsActor.Armor)
                                {
                                    case RTSActor.ArmorType.None:
                                        // Make Antipersonnel kill it
                                        adjustedDamage *= 2;
                                        break;
                                    case RTSActor.ArmorType.Light:
                                        if (_projectileType == ProjectileType.ArmorPiercing)
                                            adjustedDamage *= 4;
                                        break;
                                    case RTSActor.ArmorType.Medium:
                                        if (_projectileType == ProjectileType.ArmorPiercing)
                                            adjustedDamage *= 3;
                                        if (_projectileType == ProjectileType.AntiPersonnel)
                                            adjustedDamage *= 0.2f;
                                        break;
                                    case RTSActor.ArmorType.Heavy:
                                        if (_projectileType == ProjectileType.ArmorPiercing)
                                            adjustedDamage *= 2;
                                        if (_projectileType == ProjectileType.AntiPersonnel)
                                            adjustedDamage *= 0.1f;
                                        break;
                                    case RTSActor.ArmorType.Building:
                                        if (_projectileType == ProjectileType.AntiPersonnel)
                                            adjustedDamage *= 0.05f;
                                        break;
                                    case RTSActor.ArmorType.Super:
                                        adjustedDamage *= 0.001f;
                                        break;
                                    default:
                                        break;
                                }


                                rtsActor.Health -= (int)adjustedDamage;
                            }
                        }
                    }
                    

                    // Make an explosion
                    EnergyExplosion explosion = (EnergyExplosion)explosionTemplate.Clone();
                    explosion.Position = new Vector2(Position.X, Position.Y);
                    explosion.Scale = _effectScale;
                    explosion.TintColor = _effectColor;
                    explosion.Initialize();

                    // Attach the node to the graph
                    engine.EffectsSceneNode1.Children.Add(explosion);

                    engine.EffectsSceneNode1.Children.Remove(this);

                    //Dispose();
                    //engine.PlaySound("HunHit2");
                    _dissipated = true;
                }
            }
            



            // Destroy it after a set time
            _lifeCounter--;
            if (_lifeCounter < 0)
            {
                engine.EffectsSceneNode1.Children.Remove(this);

                if (engine.ActorsPhysicallySimulated)
                    engine.PhysicsSimulator.RemoveBody(this.Body);

                //engine.Actors.Remove(this);
                //Body.Dispose();

                //Dispose();
            }
        }
    }
}
