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
    /// Large rock of valuable minerals!
    /// Planning on having this break down into smaller chunks
    /// </summary>
    public class GemRockLarge : MineralRock
    {
        // Engine and spawnable object templates
        FactionsGame engine;




        public GemRockLarge(FactionsGame game)
            : base(game, "GemRockLarge")
        {
            engine = game;
            //movable = false;

            _mineralsMax = 25000;
            _minerals = _mineralsMax;
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

            Random rand = new Random(DateTime.Now.Millisecond);
            SpriteIndex = rand.Next(Sprites.Count - 1);
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
