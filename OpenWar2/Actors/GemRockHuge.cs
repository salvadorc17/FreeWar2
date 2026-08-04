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
    /// Big rock of valuable minerals!
    /// Planning on having this break down into smaller chunks
    /// </summary>
    public class GemRockHuge : MineralRock
    {
        // Engine and spawnable object templates
        FactionsGame engine;




        public GemRockHuge(FactionsGame game)
            : base(game, "GemRockHuge")
        {
            engine = game;
            //movable = false;

            _mineralsMax = 50000;
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
