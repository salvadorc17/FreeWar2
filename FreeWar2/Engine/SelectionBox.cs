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

using DSceneGraph;

namespace DEngine
{
    /// <summary>
    /// Box surrounding unit.
    /// </summary>
    public class SelectionBox : SceneNode
    {
        Engine engine;

        protected Texture2D texture;                // Texture to render   
        protected Texture2D outlineTexture;         // Camera's view outline
        protected Vector2 drawPos;
        protected float scale = 1f;                 // Body & render scale
        protected Color outlineColor = new Color(255, 255, 255, 150);

        protected Actor parentActor;
        protected int padding = 6;
        protected int borderWidth = 2;
        protected Vector2 size;

        protected int _flickerCounter;
        protected int _flickerInterval = 10;
        protected int _flickerLifeSpan = 70;
        protected bool _flicker = false;
        bool _hasUpdated = false;

        #region Public Properties
        public Vector2 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        public Actor ParentActor
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
        public int Padding
        {
            get
            {
                return padding;
            }
            set
            {
                padding = value;
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
        public SelectionBox(Engine game)
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

            CreateOutlineTexture();
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
            base.Update(gameTime);


            // Do some flickering
            if (_flicker)
            {
                if (_flickerCounter % _flickerInterval == 0)
                {
                    //_flickerCounter = 0;

                    this.Visible = !this.Visible;
                }
                if (_flickerCounter == _flickerLifeSpan)
                {
                    _flickerCounter = 0;
                    _flicker = false;
                    this.ParentActor.Children.Remove(this);
                }

                _flickerCounter++;
            }

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
                drawPos -= new Vector2(padding, padding);

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
            }

        }
        #endregion



        #region CreateOutlineTexture
        /// <summary>
        /// Draws an outline around the actor.
        /// </summary>
        protected void CreateOutlineTexture()
        {
            if (parentActor != null)
            {
                int boxWidth = (int)(parentActor.Size.X * parentActor.Scale) + (2 * padding);
                int boxHeight = (int)(parentActor.Size.Y * parentActor.Scale) + (2 * padding);

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
            }
        }
        #endregion



        /// <summary>
        /// Blink the selection box on and off
        /// </summary>
        public void Flicker()
        {
            _flicker = true;
        }

    }
}
