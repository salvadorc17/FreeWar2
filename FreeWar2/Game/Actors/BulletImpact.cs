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
    /// Tiny bullet impact animation
    /// </summary>
    public class BulletImpact : Actor
    {
        int lifeCounter = 20;
        FactionsGame engine;



        #region Public Properties
        #endregion


        public BulletImpact(FactionsGame game)
            : base(game, "BulletImpact")
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

            // Keep it travelling in a straight line
            //Geom.CollisionResponseEnabled = false;
            //Body.IgnoreGravity = true;
            //Body.IsStatic = true;
            //Geom.CollisionGroup = 13;
        }
        #endregion



        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            
            // Do some actual exploding


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
                //engine.Actors.Remove(this);
                //Dispose();
            }
        }



    }
}
