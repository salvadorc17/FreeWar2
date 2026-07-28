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
using DGui;
using System.Drawing;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Barracks for building infantry
    /// </summary>
    public class Barracks : RTSActor
    {
        public Barracks(FactionsGame game)
            : base(game, "Barracks")
        {
            _engine = game;
            _movable = false;
            MaxHealth = 300;
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
