using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Drawing;

using Color = Microsoft.Xna.Framework.Color;
using DGui;
using DSceneGraph;

namespace DEngine
{

    public class TransitionTilePosition
    {
        public Tile underlyingTile;
        public TransitionPosition orientation;
        public Vector2 position;

        public TransitionTilePosition()
        {
            underlyingTile = null;
            orientation = TransitionPosition.None;
            position = Vector2.Zero;
        }
    }


    public enum TransitionPosition
    {
        None,
        North, East, South, West,
        NorthEast, SouthEast, SouthWest, NorthWest,
        NorthEastInner, SouthEastInner, SouthWestInner, NorthWestInner,
    }



    /// <summary>
    /// A non-solid, partially transparent overlay tile.
    /// Used to create seamless transitions between terrain types.
    /// See http://www.gamedev.net/reference/articles/article934.asp
    /// </summary>
    public class TransitionOverlayTile : GameSceneNode
    {
        ContentManager content;
        Engine engine;
        protected string imageName;
        protected Texture2D texture;
        protected Vector2 size;
        protected Vector2 origin;
        protected float scale = 1f;
        protected Color tintColor = Color.White;

        protected TransitionPosition orientation = TransitionPosition.None;
        protected Tile parent;
        protected int precedence;

        protected static Dictionary<string, Texture2D> _transitionTileTextureVault = new Dictionary<string, Texture2D>();
        

        #region Public Properties
        public Tile Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }
        public int Precedence
        {
            get
            {
                return precedence;
            }
            set
            {
                precedence = value;
            }
        }
        public TransitionPosition Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
            }
        }
        public Color TintColor
        {
            get
            {
                return tintColor;
            }
            set
            {
                tintColor = value;
            }
        }
        public string ImageName
        {
            get
            {
                return imageName;
            }
            set
            {
                imageName = value;
            }
        }
        public Vector2 Origin
        {
            get
            {
                return origin;
            }
            set
            {
                origin = value;
            }
        }
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
        }
        #endregion
    
        
        #region Constructors
        public TransitionOverlayTile(Engine game)
            : base(game)
        {
            content = new ContentManager(game.Services);
            content.RootDirectory = FileAccess.GetTileTransitionsDir();

            // Get the engine config values
            engine = game;
            size = new Vector2(engine.TileHeight, engine.TileHeight);
        }

        public TransitionOverlayTile(Engine game, float x, float y)
            : this(game)
        {
            Position = new Vector2(x, y);
        }
        #endregion


        // ICloneable
        #region Clone
        public TransitionOverlayTile Clone()
        {
            TransitionOverlayTile t = new TransitionOverlayTile(engine, Position.X, Position.Y);
            t.Precedence = Precedence;
            t.ImageName = ImageName;
            return t;
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
            engine = (Engine)Game;

            //load the display texture
            if (_transitionTileTextureVault.ContainsKey(imageName))
            {
                texture = _transitionTileTextureVault[imageName];
            }
            else
            {
                texture = content.Load<Texture2D>(imageName);
                _transitionTileTextureVault.Add(imageName, texture);
            }

            // Set alignment and size
            origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // Center
            //origin = new Vector2(texture.Width / 2, texture.Height / 2);
            scale = size.X / texture.Width;

            _rectangle = new RectangleF(Position.X, Position.Y, size.X, size.Y);
        }
        #endregion


        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
        }
        #endregion


        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            // Must take into account camera
            Vector2 drawPos = new Vector2(Position.X, Position.Y); // AbsoluteTransform.X ...
            drawPos += new Vector2(engine.SceneGraph.Camera.Position.X, engine.SceneGraph.Camera.Position.Y);

            //draw the boxes using the position and rotation of the bodies.
            engine.SpriteBatch.Draw(texture,
                                drawPos,
                                null,
                                tintColor,
                                Rotation,
                                origin,
                                scale,
                                SpriteEffects.None,
                                0);
        }
        #endregion

    }
}
