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
    /// Spawn point for players upon game start.
    /// </summary>
    public class PlayerStart : Actor
    {
        FactionsGame engine;


        public PlayerStart(FactionsGame game)
            : base(game, "PlayerStart")
        {
            engine = game;
        }

        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            EditorVisibleOnly = true;

            //if (_physicallySimulated)
                //this.Geom.CollisionEnabled = false;
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

    }
}
