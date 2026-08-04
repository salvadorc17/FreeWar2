using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using DEngine;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Just the turret atop a tank. Should look around for enemies and shoot at them.
    /// </summary>
    public class SmallTankTurret : RTSActor
    {
        Vector2 barrelVector = new Vector2(64, 64);
        protected Actor projectileTemplate;


        #region Public Properties
        public bool EnemyEngaged
        {
            get { return _enemyFound; }
        }
        #endregion


        public SmallTankTurret(FactionsGame game)
            : base(game, "SmallTankTurret")
        {
            _engine = game;
            _fireRate = 200;
            MaxTargetRange = 400;
            _armed = true;
            _useBurst = false;
            _rotateWithDirection = true;
            _selectable = false;
        }


        public override void Initialize()
        {
            base.Initialize();

            this.OnAttackStart += new RTSActorTargetEventHandler(SmallTankTurret_OnAttackStart);
            this.OnAttackEnd += new RTSActorEventHandler(SmallTankTurret_OnAttackEnd);
        }

        protected override void UnloadContent()
        {
            this.OnAttackStart -= SmallTankTurret_OnAttackStart;
            this.OnAttackEnd -= SmallTankTurret_OnAttackEnd;
        }

        void SmallTankTurret_OnAttackEnd()
        {
            this.Rotation = 0;
        }

        void SmallTankTurret_OnAttackStart(RTSActor target)
        {
            // Make a sound
            _engine.PlaySound("LightShot");


            // Get a unit vector of turret direction
            Vector2 shootVector = PointOnCircle(Vector2.Zero, (this.Size.Y) - 2,
                                            (float)(this.Rotation * (180 / Math.PI)));

            shootVector *= -1;
            Vector2 shootVectorNormalized = shootVector;
            shootVectorNormalized.Normalize();

            // Introduce inaccuracy
            //shootVector += FireConeRandomUnitVector(shootVector, gunFireSpread);

            // Make the shot, position and add it
            Projectile tankShot = (Projectile)projectileTemplate.Clone();
            tankShot.Position = new Vector2(AbsolutePosition.X + shootVector.X,
                                        AbsolutePosition.Y + shootVector.Y);
            tankShot.ShootVector = shootVectorNormalized;
            tankShot.TargetVector = _closestTargetVector - shootVector;
            tankShot.Type = Projectile.ProjectileType.Light;
            tankShot.Team = TankBase.Team;
            tankShot.Initialize();
            _engine.EffectsSceneNode1.Children.Add(tankShot);
            //engine.Actors.Add(tankShot);
        }


        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Set our origin in the middle of the tank
            this.Origin = new Vector2(this.Size.X / 2, (this.Size.Y / 2) - 18);

            projectileTemplate = _engine.GetTemplateActorByName("Projectile");
        }
        #endregion


        public override void Stop()
        {
            //base.Stop();
            TankBase.Stop();
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }




    }
}
