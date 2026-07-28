using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Web;
using System.IO;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaInput = Microsoft.Xna.Framework.Input;

using DSceneGraph;

namespace DEngine
{

    public delegate void OnMiniMapDragHandler(bool dragging);

    public class MiniMap : DPanel
    {
        Engine engine;

        //protected Texture2D texture;                // Texture to render   
        protected Texture2D cameraOutlineTexture;   // Camera's view outline
        protected Vector2 drawPos;
        protected float scale = 1f;                 // Body & render scale
        protected Color tintColor = Color.White;
        protected Color backgroundColor = Color.Black;
        protected Color cameraOutlineColor = Color.White;
        protected bool minimapDragging = false;

        protected Level level;

        //public Vector2 Size;
        protected bool staticMap = false; // If this is enabled, update loop is disabled and no extras such as camera or units are shown.
        //public bool ShowStartPoints = false;
        protected Collection<DText> startPoints = new Collection<DText>();


        public event OnMiniMapDragHandler OnMiniMapDrag;

        protected Vector2 clickOffset;


        #region Public Properties
        public Vector2 ClickOffset
        {
            get
            {
                return clickOffset;
            }
            set
            {
                clickOffset = value;
            }
        }
        public bool StaticMap
        {
            get
            {
                return staticMap;
            }
            set
            {
                staticMap = value;
            }
        }
        public bool MouseDragging
        {
            get
            {
                return minimapDragging;
            }
        }
        public Collection<DText> StartPoints
        {
            get
            {
                return startPoints;
            }
        }
        public Texture2D Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                _texture = value;
            }
        }
        #endregion



        #region Constructor
        public MiniMap(Engine game, Level _level)
            : base(game.GuiManager)
        {
            engine = game;
            level = _level;
        }
        #endregion



        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a texture
            if (level != null)
            {
                // Find if the level image file exists
                string levelFile = System.IO.Path.Combine(DEngine.FileAccess.GetLevelsDir(), level.LevelName);
                string mapImageFile = level.LevelName;
                mapImageFile = mapImageFile.Replace(".xml", ".png");
                string mapImageDir = System.IO.Path.Combine(engine.Content.RootDirectory, "levels");
                mapImageFile = System.IO.Path.Combine(mapImageDir, mapImageFile);

                // Load it as the image if it does
                if (System.IO.File.Exists(mapImageFile))
                {
                    FileStream fileStream = new FileStream(mapImageFile, FileMode.Open);
                    _texture = Texture2D.FromStream(engine.GraphicsDevice, fileStream);
                    fileStream.Close();

                    // Scale it automatically
                    scale = Size.X / level.Width;
                }
                else
                {
                    CreateMinimapTexture(level);
                }

                if (!StaticMap)
                {
                    CreateCameraBoxTexture();
                }

                //if (ShowStartPoints)
                //{
                    //CreateStartPoints();
                //}
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
            if (!StaticMap)
            {
                //DMouseState dMouseState = DMouse.GetState();
                //if (!dMouseState.InputHandled)
                {
                    MouseState ms = Mouse.GetState();//dMouseState.MouseState;
                    Vector2 mousePos = new Vector2(ms.X, ms.Y);
                    mousePos += ClickOffset;
                    Vector2 absPos = new Vector2(AbsoluteTransform.X, AbsoluteTransform.Y);

                    //base.Update(gameTime);


                    if ((mousePos.X > absPos.X && mousePos.X < (absPos.X + Size.X) &&
                         mousePos.Y > absPos.Y && mousePos.Y < (absPos.Y + Size.Y)))
                    {
                        IsMouseHoveringOver = true;
                    }
                    else
                        IsMouseHoveringOver = false;


                    if (ms.LeftButton == ButtonState.Pressed)
                    {
                        // Is mouse hovering over, or have we not yet released from a minimap click-drag?
                        if (minimapDragging == true ||
                            IsMouseHoveringOver)
                        {
                            //engine.CameraMoving = true;
                            if (minimapDragging == false && OnMiniMapDrag != null)
                                OnMiniMapDrag(true);

                            minimapDragging = true;

                            Vector2 scaledCameraPosition;
                            // Get mouseclick relative to minimap position
                            scaledCameraPosition = mousePos - this.Position;

                            // Displace by half of camera outline box dimensions so view is centered
                            scaledCameraPosition -= new Vector2((cameraOutlineTexture.Width / 2), (cameraOutlineTexture.Height / 2));

                            // Consider minimap scale
                            scaledCameraPosition /= scale;

                            // Upscale to tile size
                            scaledCameraPosition *= engine.TileHeight;

                            // Apply to camera position
                            engine.MoveCameraTo(scaledCameraPosition.X * -1, scaledCameraPosition.Y * -1);
                        }
                    }
                    else
                    {
                        if (minimapDragging == true && OnMiniMapDrag != null)
                            OnMiniMapDrag(false);
                        minimapDragging = false;
                    }
                }
            }
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
            drawPos = new Vector2(AbsoluteTransform.X, AbsoluteTransform.Y);

            // draw the minimap texture
            engine.SpriteBatch.Draw(_texture,
                                drawPos,
                                null,
                                tintColor,
                                Rotation,
                                Vector2.Zero,
                                scale,
                                SpriteEffects.None,
                                0);

            if (!StaticMap)
            {
                // Draw camera view outline
                Camera cam = engine.SceneGraph.Camera;

                // Create sized rectangle
                int camX = (int)((cam.Position.X / engine.TileHeight) * scale) * -1;
                int camY = (int)((cam.Position.Y / engine.TileHeight) * scale) * -1;

                Vector2 outlineDrawPos = new Vector2(drawPos.X + camX, drawPos.Y + camY);

                // Draw camera outline
                engine.SpriteBatch.Draw(cameraOutlineTexture,
                                    outlineDrawPos,
                                    null,
                                    tintColor,
                                    0f,
                                    Vector2.Zero,
                                    1f,
                                    SpriteEffects.None,
                                    0);
            }

            //base.Draw(gameTime);
        }
        #endregion



        #region CreateCameraBoxTexture
        /// <summary>
        /// Draws an outline of the camera's current view on the minimap texture.
        /// </summary>
        protected void CreateCameraBoxTexture()
        {
            
            int camBoxWidth = (int)(scale * (engine.Window.ClientBounds.Width / engine.TileHeight));  // Each tile is one pixel on the minimap.
            int camBoxHeight = (int)(scale * (engine.Window.ClientBounds.Height / engine.TileHeight));

  
            int x;
            int y = -1;
            int count = camBoxWidth * camBoxHeight;
            Color[] colorArray = new Color[count];
            for (int i = 0; i < count; i++)
            {
                if (i % camBoxWidth == 0) { y += 1; }
                x = i % camBoxWidth;
                {
                    if (x == 0 || y == 0 || x == camBoxWidth - 1 || y == camBoxHeight - 1)
                    {
                        colorArray[i] = cameraOutlineColor;
                    }
                    else
                    {
                        colorArray[i] = Color.Transparent;
                    }
                }
            }

            cameraOutlineTexture = new Texture2D(engine.GraphicsDevice, camBoxWidth, camBoxHeight);
            cameraOutlineTexture.SetData<Color>(colorArray);
        }
        #endregion



        #region CreateMinimapTexture
        /// <summary>
        /// Creates a scaled minimap texture of this level.
        /// Chooses the color of the middle pixel of each tile to create the pixel map.
        /// </summary>
        /// <returns></returns>
        public void CreateMinimapTexture(Level level)
        {
            int x;
            int y = -1;
            int count = level.Width * level.Height;
            Color[] colorArray = new Color[count];


            for (int i = 0; i < count; i++)
            {
                if (i % level.Width == 0) { y += 1; }
                x = i % level.Width;

                // Get this tile
                Tile t = engine.TileExistenceCheckByGridRef(new GridReference(x, y));
                if (t != null)
                {
                    // Set this pixel of the minimap teture.
                    colorArray[i] = t.CenterPixelColor;
                }
                else
                {
                    colorArray[i] = backgroundColor;
                }
            }

            // Scale it automatically
            scale = Size.X / level.Width;

            if (_texture != null)
                _texture.Dispose();
            _texture = new Texture2D(engine.GraphicsDevice, level.Width, level.Height);
            _texture.SetData<Color>(colorArray);
        }
        #endregion


    }
}
