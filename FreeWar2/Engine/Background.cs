using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using DGui;
using DSceneGraph;

namespace DEngine
{
    /// <summary>
    /// A parallax-enabled tiled background.
    /// Maintains an array of duplicates for smooth transition.
    /// </summary>
    public class Background : SceneNode
    {
        ContentManager _content;
        protected string _imageName;                 // Image filename
        protected Texture2D _texture;                // Texture to render  (just one for now)
        protected Vector2 _size;
        protected Vector2 _velocity;                 // Make it move!
        Engine _engine;


        #region Public Properties
        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                _velocity = value;
            }
        }
        public string ImageName
        {
            get
            {
                return _imageName;
            }
            set
            {
                _imageName = value;
            }
        }
        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }
        public Texture2D Texture
        {
            get
            {
                return _texture;
            }
        }
        #endregion


        public Background(Game game) 
            : base(game)
        {
            _content = new ContentManager(game.Services);
            _content.RootDirectory = Path.Combine(Directory.GetCurrentDirectory(),FileAccess.GetBackgroundsDir());
            _engine = (Engine)Game;
            AlwaysVisible = true;
        }

        // ICloneable
        #region Clone
        public Background Clone()
        {
            Background bg = new Background(Game);
            bg.ImageName = ImageName;
            return bg;
        }
        #endregion




        // Xna
        #region Initialize
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion


        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _engine = (Engine)Game;
            try
            {
                //load the display texture
                _texture = _content.Load<Texture2D>(_imageName);
            }
            catch (Exception e)
            {
                Log.Message("Couldn't load background " + _imageName + ": " + e.Message);
            }
        }
        #endregion


        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Perform overlaps here!
            // Ensure our background panels cover the camera's field of view.
            // Delete panels far out of bounds and create new ones in open space.
            Position += _velocity;
        }
        #endregion


        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            // Perform multiple draws to cover the entire screen
            // Start at camera position
            // Mod texture size and camera position
            Vector2 topLeftDrawPos = new Vector2(AbsoluteTransform.X, AbsoluteTransform.Y);

            int earliestX = (int)topLeftDrawPos.X - ((int)topLeftDrawPos.X % _texture.Width);
            int earliestY = (int)topLeftDrawPos.Y - ((int)topLeftDrawPos.Y % _texture.Height);

            topLeftDrawPos -= new Vector2(earliestX, earliestY);

            // Calculate number of times to cover the screen
            int numRepeatsX = Convert.ToInt32(Math.Ceiling((float)_engine.Window.ClientBounds.Width / (float)_texture.Width));
            int numRepeatsY = Convert.ToInt32(Math.Ceiling((float)_engine.Window.ClientBounds.Height / (float)_texture.Height));

            // Give it an extra repeat on either end ( from -1 to x <= numRepeatsX)
            for (int x = -1; x <= numRepeatsX; x++)
            {
                for (int y = -1; y <= numRepeatsY; y++)
                {
                    // Make new draw pos
                    Vector2 drawPos = new Vector2(x * _texture.Width, y * _texture.Height);
                    drawPos += topLeftDrawPos;

                    _engine.SpriteBatch.Draw(_texture,
                                    drawPos,
                                    null,
                                    Color.White,
                                    Rotation,
                                    Vector2.Zero,
                                    1.0f,
                                    SpriteEffects.None,
                                    0);
                }
            }



            //base.Draw(gameTime);
        }
        #endregion

    }
}
