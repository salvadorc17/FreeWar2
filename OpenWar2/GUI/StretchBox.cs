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

namespace FactionsGame
{
    /// <summary>
    /// Strechy selection box to grab a bunch of units.
    /// </summary>
    public class StretchBox : GameSceneNode
    {
        protected FactionsGame engine;

        protected Texture2D outlineTexture;   // Camera's view outline
        protected Vector2 drawPos;
        protected Color outlineColor = Color.LimeGreen;
        protected Color fillColor = new Color(0, 0, 255, 50);
        protected bool useTeamColor = false;
        protected Vector2 startPos;

        protected Rectangle drawRect;
        protected Vector2 size;
        protected int borderWidth = 1;


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
        public Rectangle DrawRect
        {
            get
            {
                return drawRect;
            }
            set
            {
                drawRect = value;
            }
        }
        public bool UseTeamColor
        {
            get
            {
                return useTeamColor;
            }
            set
            {
                useTeamColor = value;
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
        public StretchBox(FactionsGame game, MouseState ms)
            : base(game)
        {
            engine = game;

            // Store the point where we first clicked
            startPos = new Vector2(ms.X, ms.Y);
            startPos = engine.AbsoluteCoordinates(startPos);
            //startPos += new Vector2(engine.SceneGraph.Camera.Position.X, engine.SceneGraph.Camera.Position.Y);

            this.AlwaysVisible = true; // don't let occluder clip this
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
        }
        #endregion



        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            //DMouseState dMouseState = DMouse.GetState();
            //if (!dMouseState.InputHandled)
            {
                MouseState ms = Mouse.GetState();//dMouseState.MouseState;
                Vector2 newPos = new Vector2(ms.X, ms.Y);
                newPos += AbsolutePosition;
                //newPos = engine.AbsoluteCoordinates(newPos);

                // Flip coordinate systems, sigh
                //newPos = new Vector2(newPos.X * -1, newPos.Y * -1);
                Vector2 shiftedStartPos = startPos + new Vector2(AbsoluteTransform.X, AbsoluteTransform.Y);

                // Place it at the top left point of the box
                // Find whichever corner is top-left most
                int boxTop, boxLeft;
                if (newPos.X > shiftedStartPos.X)
                    boxLeft = (int)shiftedStartPos.X;
                else
                    boxLeft = (int)newPos.X;
                if (newPos.Y > shiftedStartPos.Y)
                    boxTop = (int)shiftedStartPos.Y;
                else
                    boxTop = (int)newPos.Y;


                int boxWidth = (int)Math.Abs(newPos.X - shiftedStartPos.X);
                int boxHeight = (int)Math.Abs(newPos.Y - shiftedStartPos.Y);
                if (boxWidth == 0)
                    boxWidth = 1;
                if (boxHeight == 0)
                    boxHeight = 1;



                drawPos = new Vector2(boxLeft, boxTop);
                drawRect = new Rectangle((int)drawPos.X, (int)drawPos.Y, boxWidth, boxHeight);

                // draw the texture
                engine.SpriteBatch.Draw(outlineTexture,
                                    drawPos,
                                    drawRect,
                                    Color.White,
                                    Rotation,
                                    Vector2.Zero,
                                    1.0f,
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
            Color[] colorArray = new Color[] { fillColor };
            outlineTexture = new Texture2D(engine.GraphicsDevice, 1, 1);
            outlineTexture.SetData<Color>(colorArray);
        }
        #endregion


    }
}
