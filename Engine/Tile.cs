using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;
using System.Drawing;
using QuadTreeLib;

using Color = Microsoft.Xna.Framework.Color;
using DGui;
using DSceneGraph;

namespace DEngine
{
    /// <summary>
    /// A static tile on the game field.
    /// Has a physical body.
    /// Has a collection of transition tiles for seamless transitions between other tiles.
    /// </summary>
    public class Tile : GameSceneNode
    {
        ContentManager content;
        Engine engine;
        protected int id;                           // Tile ID, used when saving levels to match transition tiles to their parent.
        protected string imageName;                 // Image filename
        protected Texture2D texture;                // Texture to render   
        protected Body body;                        // Physical body
        //protected Geom geom;                        // On-screen geometry
        protected int precedence;                   // Precedence in the scene render order
        protected Vector2 size;                     // Render size
        protected float scale = 1f;                 // Body & render scale
        protected Vector2 origin;                   // Draw offset
        protected bool physicallySimulated = false; // Use the physics simulator or just cosmetic
        protected Color tintColor = Color.White;    // Overall tint color
        Color centerPixelColor = Color.White;       // Determined center pixel color
        protected bool solid = false;               // Whether the tile's body is solid or not.

        // Transition tiles (North, NE, E, SE, S, SW, W, NW)
        protected Collection<TransitionOverlayTile> transitionOverlayTiles = new Collection<TransitionOverlayTile>();

        protected static Dictionary<string, Texture2D> _tileTextureVault = new Dictionary<string, Texture2D>();





        #region Public Properties
        [ContentSerializerIgnore]
        public Color CenterPixelColor
        {
            get
            {
                return centerPixelColor;
            }
        }
        public bool Solid
        {
            get
            {
                return solid;
            }
            set
            {
                solid = value;
            }
        }
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public int Precedence
        {
            set
            {
                precedence = value;
            }
            get
            {
                return precedence;
            }
        }
        public Collection<TransitionOverlayTile> TransitionOverlayTiles
        {
            get
            {
                return transitionOverlayTiles;
            }
        }
        public bool PhysicallySimulated
        {
            get
            {
                return physicallySimulated;
            }
            set
            {
                physicallySimulated = value;
                if (body != null)// && geom != null)
                {
                    body.Enabled = physicallySimulated;
                    //geom.CollisionResponseEnabled = physicallySimulated;
                }
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
        public Body Body
        {
            get
            {
                return body;
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
        public Tile(Engine game)
            : base(game)
        {
            content = new ContentManager(game.Services);
            content.RootDirectory = FileAccess.GetTilesDir();

            // Get the engine config values
            engine = game;
            size = new Vector2(engine.TileWidth, engine.TileHeight);
            physicallySimulated = engine.TilesPhysicallySimulated;
        }

        public Tile(Engine game, float x, float y)
            : this(game)
        {
            Position = new Vector2(x, y);
        }
        #endregion


        // ICloneable
        #region Clone
        public Tile Clone()
        {
            Tile t = new Tile(engine, Position.X, Position.Y);
            t.ImageName = ImageName;
            t.Precedence = Precedence;
            t.Solid = Solid;
            t.Rotation = Rotation;
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
            if (_tileTextureVault.ContainsKey(imageName))
            {
                texture = _tileTextureVault[imageName];
            }
            else
            {
                texture = content.Load<Texture2D>(imageName);
                _tileTextureVault.Add(imageName, texture);
            }

            GetCenterPixelColorAveraged();

            // Set alignment and size
            origin = new Vector2(texture.Width / 2, texture.Height / 2);


            //create body
            if (physicallySimulated)
            {
                body = BodyFactory.CreateBody(engine.PhysicsSimulator, new Vector2(Position.X, Position.Y));
                body.IsStatic = true;
                body.Enabled = physicallySimulated;

                //Fixture fixture = FixtureFactory.CreateRectangle(engine.PhysicsSimulator, size.X, size.Y, 1.0f);

                //create the geometry
                //geom = GeomFactory.Instance.CreateRectangleGeom(engine.PhysicsSimulator, body, size.X, size.Y);
                //geom.CollisionResponseEnabled = physicallySimulated;
            }

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
            // Update our position in the scene graph
            if (physicallySimulated)
                Position = body.Position;
        }
        #endregion



        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            // Must take into account camera; we're avoiding update which would usually give us our absolute position
            Vector2 drawPos = new Vector2(Position.X, Position.Y);
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



        // Public tile functions
        #region PointHitCheck
        /// <summary>
        /// Check if a point touches this tile.
        /// Does using FarseerPhysics Geom.Collide() method - inexact!
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Tile PointHitCheck(Vector2 point)
        {
            //Fixture fixture;
            //if (.Collide(point))
            //    return this;
            return null;
        }
        #endregion



        #region GridReference
        /// <summary>
        /// Return a zero-based index of this tile for the engine's tile grid.
        /// </summary>
        /// <returns></returns>
        public GridReference GridReference()
        {
            // Determine the grid reference of this tile
            int gridX, gridY;
            gridX = (int)(Position.X + (engine.TileHeight / 2)) / engine.TileHeight;
            gridY = (int)(Position.Y + (engine.TileHeight / 2)) / engine.TileHeight;
            gridX--;
            gridY--;

            return new GridReference(gridX, gridY);
        }
        #endregion



        #region AdjacentGridReferences
        /// <summary>
        /// Returns grid references of adjacent tiles.
        /// </summary>
        /// <returns></returns>
        public Collection<GridReference> AdjacentGridReferences()
        {
            GridReference thisGridRef = GridReference();

            Collection<GridReference> adjacentGridReferences = new Collection<GridReference>();

            // Manually set up iteration of adjacent grid squares.
            // Get north, east, south, west squares before diagonals.
            Collection<GridReference> candidateGridReferences = new Collection<GridReference>();
            candidateGridReferences.Add(new GridReference(0, -1));
            candidateGridReferences.Add(new GridReference(0, 1));
            candidateGridReferences.Add(new GridReference(-1, 0));
            candidateGridReferences.Add(new GridReference(1, 0));
            candidateGridReferences.Add(new GridReference(1, 1));
            candidateGridReferences.Add(new GridReference(1, -1));
            candidateGridReferences.Add(new GridReference(-1, -1));
            candidateGridReferences.Add(new GridReference(-1, 1));

            foreach (GridReference adjGridRef in candidateGridReferences)
            {
                int gridX, gridY;
                gridX = thisGridRef.X + adjGridRef.X;
                gridY = thisGridRef.Y + adjGridRef.Y;

                if (gridX >= 0 && gridX < engine.TileGrid.GetLength(0) &&
                    gridY >= 0 && gridY < engine.TileGrid.GetLength(1))
                {
                    if (engine.TileGrid[gridX, gridY] != null)
                        adjacentGridReferences.Add(new GridReference(gridX, gridY));
                }
            }
            candidateGridReferences.Clear();
            return adjacentGridReferences;
        }
        #endregion



        #region GetCenterPixelColor
        /// <summary>
        /// The color of the central pixel of this tile.
        /// </summary>
        /// <returns></returns>
        protected void GetCenterPixelColor()
        {
            if (texture != null)
            {
                // Get this tile's texture color data
                Color[] tileColors = new Color[texture.Width * texture.Height];
                texture.GetData<Color>(tileColors, 0, texture.Width * texture.Height);

                // Pick the middle color of the tile texture.
                int middleColorIndex = (texture.Width / 2) * (texture.Height / 2);

                // Set this pixel of the minimap teture.
                centerPixelColor = tileColors[middleColorIndex];
            }
        }
        #endregion

        protected void GetCenterPixelColorAveraged()
        {
            if (texture != null)
            {
                // Get this tile's texture color data
                Color[] tileColors = new Color[texture.Width * texture.Height];
                texture.GetData<Color>(tileColors, 0, texture.Width * texture.Height);

                Random rand = new Random();
                int pixelCount = 6;
                Color runningColor = Color.White;

                for (int i = 0; i < pixelCount; i++)
                {
                    int xOffset = rand.Next((int)(engine.TileWidth / 4));
                    xOffset -= (xOffset / 2);
                    int yOffset = rand.Next((int)(engine.TileHeight / 4));
                    yOffset -= (yOffset / 2);
                    int middleColorIndex = ((texture.Width / 2) + xOffset) * ((texture.Height / 2) + yOffset);
                    
                    if (i == 0)
                        runningColor = tileColors[middleColorIndex];
                    else
                        runningColor = Color.Lerp(runningColor, tileColors[middleColorIndex], 1 / pixelCount);

                }
                // Set this pixel of the minimap teture.
                centerPixelColor = runningColor;
            }
        }
    }
}