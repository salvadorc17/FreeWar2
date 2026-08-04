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
    /// Blood Splat (very small)
    /// </summary>
    public class BloodSplat1 : Actor
    {
        int lifeCounter = 20;
        FactionsGame engine;



        #region Public Properties
        #endregion


        public BloodSplat1(FactionsGame game)
            : base(game, "BloodSplat1")
        {
            engine = game;
            _isEffect = true;
        }

        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.PhysicallySimulated = false;

            base.LoadContent();
        }
        #endregion



        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            // Destroy it after a set time
            lifeCounter--;
            if (lifeCounter < 0)
            {
                engine.EffectsSceneNode1.Children.Remove(this);
                if (this.PhysicallySimulated)
                {
                    engine.PhysicsSimulator.RemoveBody(this.Body);
                    Body.Dispose();
                }
            }
        }



    }
}
