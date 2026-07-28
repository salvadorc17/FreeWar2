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
using DGui;
using System.Drawing;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Main building for a side.
    /// Produces units.
    /// </summary>
    public class Headquarters : RTSActor
    {
        public Headquarters(FactionsGame game)
            : base(game, "Headquarters")
        {
            _engine = game;
            _movable = false;
            MaxHealth = 1200;
            Health = MaxHealth;
            Armor = ArmorType.Building;
            _isBuilding = true;
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
