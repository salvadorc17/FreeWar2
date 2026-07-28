using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using DEngine;

namespace FactionsGame
{
    public class RTSSprite : Sprite
    {
        protected Color teamColor;
        protected bool teamColorsEnabled = true;


        #region Public Properties

        #endregion



        public RTSSprite(Engine game, string actorName)
            : base(game, actorName)
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

            //ApplyTeamColors();
        }
        #endregion





        
    }
}
