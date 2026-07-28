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
    /// Explosion that looks like a yellow plasma bolt detonation
    /// </summary>
    public class EnergyExplosion : Actor
    {
        int lifeCounter = 30;
        FactionsGame engine;



        #region Public Properties
        #endregion


        public EnergyExplosion(FactionsGame game)
            : base(game, "EnergyExplosion")
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
