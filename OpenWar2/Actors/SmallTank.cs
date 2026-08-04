using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;
using DEngine;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Basic RTS tank unit
    /// </summary>
    public class SmallTank : RTSActor
    {
        SmallTankTurret turret;

        public SmallTank(FactionsGame game)
            : base(game, "SmallTank")
        {
            _engine = game;

            turret = new SmallTankTurret(_engine);
            _movable = true;
            _moveSpeed = 2f;
            _health = 400;
            _maxHealth = 400;
            _armorType = ArmorType.Medium;
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
            turret = (SmallTankTurret)_engine.GetTemplateActorByName("SmallTankTurret").Clone();
            turret.Team = Team;
            turret.TankBase = this;
            turret.Initialize();

            //if (this._physicallySimulated)
            //    turret.Geom.CollisionEnabled = false;

            turret.Position = new Vector2(0,0);
            this.Children.Add(turret);
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Keep the team updated since we might have recolored (this is a bit of a hack!)
            turret.Team = Team;

            turret.TargetActor = _targetActor;

            if (turret.EnemyEngaged && (_attackMove || _targetActor != null))
                MovementSuspended = true;
            else
                MovementSuspended = false;
        }



        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
