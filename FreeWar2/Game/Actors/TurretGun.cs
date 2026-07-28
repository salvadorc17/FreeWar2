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

namespace FactionsGame.Actors
{
    /// <summary>
    /// Just the turret atop a fixed gun turret location.
    /// </summary>
    public class TurretGun : RTSActor
    {
        public Turret Base;

        Vector2 barrelVector = new Vector2(64, 64);


        Actor projectileTemplate;


        public TurretGun(FactionsGame game)
            : base(game, "TurretGun")
        {
            _engine = game;
            _fireRate = 120;
            MaxTargetRange = 500;
            _rotateWithDirection = true;
            _selectable = false;
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

            this.Origin = new Vector2(this.Size.X / 2, (this.Size.Y / 2) - 10);

            projectileTemplate = _engine.GetTemplateActorByName("Projectile");

            this.OnAttackStart += new RTSActorTargetEventHandler(TurretGun_OnAttackStart);
            this.OnAttackEnd += new RTSActorEventHandler(TurretGun_OnAttackEnd);
        }

        protected override void UnloadContent()
        {
            this.OnAttackStart -= TurretGun_OnAttackStart;
            this.OnAttackEnd -= TurretGun_OnAttackEnd;
            base.UnloadContent();
        }

        void TurretGun_OnAttackEnd()
        {
            this.Rotation = 0;
        }

        void TurretGun_OnAttackStart(RTSActor target)
        {
            _engine.PlaySound("HeavyShot");

            // Get a unit vector of turret direction
            Vector2 shootVector = PointOnCircle(Vector2.Zero, (this.Size.X) + 22,
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
            tankShot.Type = Projectile.ProjectileType.Medium;
            tankShot.Team = Base.Team;
            tankShot.Initialize();
            _engine.EffectsSceneNode1.Children.Add(tankShot);
            //engine.Actors.Add(tankShot);
        }
        #endregion


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
