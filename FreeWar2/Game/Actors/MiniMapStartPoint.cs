using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using DEngine;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Colorable minimap actor for multiplayer game setup.
    /// </summary>
    public class MiniMapStartPoint : Actor
    {
        FactionsGame engine;


        public MiniMapStartPoint(FactionsGame game)
            : base(game, "MiniMapStartPoint")
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

            EditorVisibleOnly = false;
            //this.Geom.CollisionEnabled = false;
            this.Body.IsStatic = true;
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

    }
}
