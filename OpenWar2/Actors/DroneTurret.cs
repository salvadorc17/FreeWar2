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
    /// Drone turret, superficial (for now)
    /// </summary>
    public class DroneTurret : RTSActor
    {
        //public RTSActor TankBase;

        Vector2 barrelVector = new Vector2(64, 64);




        public DroneTurret(FactionsGame game)
            : base(game, "DroneTurret")
        {
            _engine = game;
            //_fireRate = 140;
            MaxTargetRange = 100;
            _armed = false;
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

        void SmallTankTurret_OnAttackEnd()
        {
        }

        void SmallTankTurret_OnAttackStart(RTSActor target)
        {
            
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
            //this.Origin = new Vector2(this.Size.X / 2, (this.Size.Y / 2));

            this.Team = TankBase.Team;
        }
        #endregion


        public override void Stop()
        {
            base.Stop();
            //TankBase.Stop();
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
