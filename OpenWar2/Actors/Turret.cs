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
    /// Anti-tank fixed gun.
    /// </summary>
    public class Turret : RTSActor
    {
        // Engine and spawnable object templates
        //FactionsGame engine;

        TurretGun turretGun;


        public Turret(FactionsGame game)
            : base(game, "Turret")
        {
            _engine = game;

            turretGun = new TurretGun(_engine);
            this.Movable = false;
            _health = 150;
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

            // Gimme a turret!
            turretGun = (TurretGun)_engine.GetTemplateActorByName("TurretGun").Clone();
            turretGun.Team = Team;
            turretGun.Base = this;
            turretGun.Position = new Vector2(0, -2);
            turretGun.Initialize();

            if (_engine.ActorsPhysicallySimulated)
            {
                //turretGun.Geom.CollisionGroup = 1;
                //this.Geom.CollisionGroup = 1;
            }

            this.Children.Add(turretGun);
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //turretGun.Update(gameTime);

            turretGun.Team = _team;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

    }
}
