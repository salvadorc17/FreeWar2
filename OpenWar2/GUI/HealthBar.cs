using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaInput = Microsoft.Xna.Framework.Input;
using FactionsGame.Actors;

using DSceneGraph;

namespace FactionsGame
{
    /// <summary>
    /// RTS-style health bar.
    /// </summary>
    public class HealthBar : SceneNode
    {
        Engine engine;

        protected Texture2D healthBarTexture;                // Texture to render   
        protected Texture2D outlineTexture;         // Camera's view outline
        protected Vector2 drawPos;
        protected float scale = 1f;                 // Body & render scale
        protected Color outlineColor = Color.White;
        protected Color barColor = new Color(0, 255, 0, 150);
        protected Rectangle healthDrawRect;

        protected RTSActor parentActor;
        protected int borderWidth = 1;
        protected int verticalPos = 14;
        protected int barHeight = 6;

        protected float percentHealth = 1f;
        public Vector2 Size;
        bool _hasUpdated = false;


        #region Public Properties
        public float PercentHealth
        {
            get
            {
                return percentHealth;
            }
            set
            {
                percentHealth = value;
            }
        }
        public RTSActor ParentActor
        {
            get
            {
                return parentActor;
            }
            set
            {
                parentActor = value;
            }
        }
        public Color OutlineColor
        {
            get
            {
                return outlineColor;
            }
            set
            {
                outlineColor = value;
            }
        }
        public int BorderWidth
        {
            get
            {
                return borderWidth;
            }
            set
            {
                borderWidth = value;
            }
        }
        #endregion



        #region Constructor
        public HealthBar(Engine game)
            : base(game)
        {
            engine = game;
        }
        #endregion



        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            CreateHealthBarTexture();
        }
        #endregion



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



        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Get percentage health, update draw rect
            if (ParentActor.Health > ParentActor.MaxHealth)
                ParentActor.MaxHealth = ParentActor.Health;

            percentHealth = (float)ParentActor.Health / (float)ParentActor.MaxHealth;
            healthDrawRect = new Rectangle(0, 0, (int)(healthBarTexture.Width * percentHealth), (int)healthBarTexture.Height);
            _hasUpdated = true;
        }
        #endregion



        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            if (_hasUpdated)
            {
                // Must take into account parent size and padding.
                drawPos = new Vector2(AbsoluteTransform.X, AbsoluteTransform.Y);
                drawPos -= ((parentActor.Size * parentActor.Scale) / 2);
                drawPos -= new Vector2(0, verticalPos);

                // draw the minimap texture
                engine.SpriteBatch.Draw(outlineTexture,
                                    drawPos,
                                    null,
                                    Color.White,
                                    Rotation,
                                    Vector2.Zero,
                                    scale,
                                    SpriteEffects.None,
                                    0);

                // draw the minimap texture
                drawPos += new Vector2(borderWidth, borderWidth);




                engine.SpriteBatch.Draw(healthBarTexture,
                                    drawPos,
                                    healthDrawRect,
                                    Color.White,
                                    Rotation,
                                    Vector2.Zero,
                                    scale,
                                    SpriteEffects.None,
                                    0);
            }
        }
        #endregion



        // Additional methods
        #region CreateHealthBarTexture
        /// <summary>
        /// Creates the health bar outline and scalable health bar texture.
        /// </summary>
        protected void CreateHealthBarTexture()
        {
            if (parentActor != null)
            {
                int boxWidth = (int)(parentActor.Size.X * parentActor.Scale);
                int boxHeight = barHeight;

                int x;
                int y = -1;
                int count = boxWidth * boxHeight;
                Color[] colorArray = new Color[count];
                for (int i = 0; i < count; i++)
                {
                    if (i % boxWidth == 0) { y += 1; }
                    x = i % boxWidth;
                    {
                        if ((x >= 0 && x < borderWidth)|| 
                            (y >= 0 && y < borderWidth) || 
                            (x <= boxWidth - 1 && x > (boxWidth - 1) - borderWidth) ||
                            (y <= boxHeight - 1 && y > (boxHeight - 1) - borderWidth))
                            colorArray[i] = outlineColor;
                        else
                            colorArray[i] = Color.Transparent;
                    }
                }

                outlineTexture = new Texture2D(engine.GraphicsDevice, boxWidth, boxHeight);
                outlineTexture.SetData<Color>(colorArray);

                // Create box for the health bar
                int healthBarWidth, healthBarHeight;
                healthBarWidth = boxWidth - (2 * borderWidth);
                healthBarHeight = boxHeight - (2 * borderWidth);
                count = healthBarWidth * healthBarHeight;
                colorArray = new Color[count];
                for (int i = 0; i < count; i++)
                {
                    colorArray[i] = barColor;
                }
                healthBarTexture = new Texture2D(engine.GraphicsDevice, healthBarWidth, healthBarHeight);
                healthBarTexture.SetData<Color>(colorArray);
                healthDrawRect = new Rectangle(0, 0, (int)(healthBarWidth * percentHealth), (int)healthBarHeight);
            }
        }
        #endregion


    }
}
