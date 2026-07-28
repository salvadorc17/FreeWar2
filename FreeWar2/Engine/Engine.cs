using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading;
using System.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using DGui;
using QuadTreeLib;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using DSceneGraph;


namespace DEngine
{
    /// <summary>
    /// A 2D physics-enabled sprite engine - Physics simulator courtesy of 
    /// Saves and loads levels to/from XML.
    /// Equipped with a scenegraph and camera.
    /// Uses a tile precedence order to draw transition tiles over adjacent tiles appropriately.
    /// Camera is equipped with an occluder to increase draw times
    /// Accompanying DEditor creates levels for the DEngine.
    /// 
    /// In the future the DEngine will be a fully-fledged 2D game engine API.
    /// 
    /// Points to explain:
    /// - SceneGraphs
    /// - Nodes for scenegraph
    /// - XML formats
    /// - Actors, Tiles, Backgrounds
    /// - Inheriting from DEngine
    /// </summary>
    public class Engine : Microsoft.Xna.Framework.Game
    {
        public const float DEFAULT_GRAVITY = 5f;
        //public const float ALLOWED_PENETRATION = 0.02f;
        public enum GameState { Running, Paused, Stopped };

        public Color[] PlayerColors
            = new Color[] {Color.MediumBlue, 
                           Color.Crimson, 
                           Color.BlueViolet, 
                           Color.Yellow, 
                           Color.LimeGreen, 
                           Color.Orange, 
                           Color.LightGray, 
                           Color.SaddleBrown, 
                           new Color(64, 64, 64), // Dark gray, darker than what web colors afford.
                           Color.MediumTurquoise,
                           Color.Tan,
                           Color.Magenta};



        public string[] PlayerColorNames
            = new string[] {"Blue", 
                           "Red", 
                           "Purple", 
                           "Yellow", 
                           "Green", 
                           "Orange", 
                           "White", 
                           "Brown", 
                           "Gray",
                           "Aqua",
                           "Tan",
                           "Pink"};

        public int PlayerColorHueMask = 205;       // THIS WILL NEED TO BE A SETTING
        // The hue to apply color shifting to.
        // Used to generate team-specific textures.


        protected GraphicsDeviceManager _graphics;   // Graphics device to draw to
        SpriteBatch _spriteBatch;
        DGuiManager _guiManager;

        protected QuadTree<GameSceneNode> _tileQuadTree;        // Quad tree for tile rendering
        protected QuadTree<GameSceneNode> _actorQuadTree;       // Quad tree for actor rendering

        protected GameSceneGraph _backgroundSceneGraph;     // Background scene graph for updating & drawing backgrounds exclusively
        protected SceneNode _backgroundSceneNode1;
        protected SceneNode _backgroundSceneNode2;

        protected GameSceneGraph _sceneGraph;               // Scene graph for actors, update only! Draw handled using quad tree
        protected SceneNode _actorsSceneNode;
        protected GameSceneGraph _effectsSceneGraph;        // Scene graph for effects, update and draw. QuadTree insert is too slow for effects.
        protected SceneNode _effectsSceneNode1;
        protected SceneNode _effectsSceneNode2;


        protected SceneGraph _staticSceneGraph;
        protected World _physicsSimulator;          // Physics simulator courtesy of Farseer Physics: http://www.codeplex.com/FarseerPhysics

        // Game object template lists
        protected List<Tile> _tileTemplates = null;
        protected List<Actor> _actorTemplates = null;
        protected List<Background> _backgroundTemplates = null;

        // Tile transition order http://www.gamedev.net/reference/articles/article934.asp
        protected List<string> _tilePrecedenceOrder = null;
        protected List<TransitionOverlayTile> _transitionOverlayTileTemplates = null;

        // Game object lists
        protected List<Tile> _tiles = null;
        protected List<TransitionOverlayTile> _transitionTiles = null;
        protected List<Actor> _actors = null;
        protected List<Background> _backgrounds = null;
        protected List<Level> _levels = null;
        protected List<Player> _players = new List<Player>();

        // Engine variables
        protected GameState _gameState = GameState.Running;
        protected float _gravity = DEFAULT_GRAVITY;
        protected Actor _cameraFollowActor;                  // Make the camera follow an actor.
        protected bool _editorMode;                          // Show usually invisible ators
        protected Color _backgroundColor = new Color(48, 48, 48);
        protected Level _currentLevel;

        // Configurable Settings
        protected bool _tilesPhysicallySimulated = true;
        protected bool _actorsPhysicallySimulated = true;
        protected int _tileHeight = 32;
        protected int _tileWidth = 32;
        protected Microsoft.Xna.Framework.Point _screenSize = new Microsoft.Xna.Framework.Point(1280,960);
        protected string _allowedImageTypes = "*.xnb";

        // Console
        protected DConsole _console;

        protected DebugPanel _debugPanel;

        // Movement & pathfinding
        Tile[,] _tileGrid;   // A 2D array for easy finding of tiles by index. Initialized upon level load.

        // XML save/load
        EngineIO _engineIO;

        // Debug variables
        protected double _updateCount = 0;
        protected double _drawCount = 0;
        protected double _totalUpdateDuration = 0;
        protected double _totalDrawDuration = 0;
        protected AppSettingsReader _appSettingsReader;
        protected bool _useCameraRectangleBounding = true;
        protected bool _renderEnabled = true;
        protected bool _showDebugInfo = false;

        // Audio objects
        protected AudioEngine _audioEngine;
        protected SoundBank _soundBank;
        protected WaveBank _waveBank;
        // Volume & effects control
        protected AudioCategory _soundEffectsCategory;
        

        #region Public Properties
        public GraphicsDeviceManager Graphics
        {
            get
            {
                return _graphics;
            }
        }
        /// <summary>
        /// Engine input/output functions class.
        /// </summary>
        public EngineIO EngineIO
        {
            get
            {
                return _engineIO;
            }
        }

        public DGuiManager GuiManager
        {
            get
            {
                return _guiManager;
            }
        }

        /// <summary>
        /// Command line console.
        /// </summary>
        public DConsole Console
        {
            get
            {
                return _console;
            }
        }

        /// <summary>
        /// Toggle engine draw output
        /// </summary>
        public bool RenderEnabled
        {
            get
            {
                return _renderEnabled;
            }
            set
            {
                _renderEnabled = value;
            }
        }

        /// <summary>
        /// Current players in the game
        /// </summary>
        public List<Player> Players
        {
            get
            {
                return _players;
            }
            set
            {
                _players = value;
            }
        }

        /// <summary>
        /// Current level being played
        /// </summary>
        public Level CurrentLevel
        {
            get
            {
                return _currentLevel;
            }
            set
            {
                _currentLevel = value;
            }
        }

        /// <summary>
        /// Toggle whether the camera can move outside the bounds of the level
        /// </summary>
        public bool UseCameraRectangleBounding
        {
            get
            {
                return _useCameraRectangleBounding;
            }
            set
            {
                _useCameraRectangleBounding = value;
                _sceneGraph.Camera.UseRectangleBounding = value;
            }
        }

        /// <summary>
        /// Height of tiles
        /// </summary>
        public int TileHeight
        {
            get
            {
                return _tileHeight;
            }
        }

        /// <summary>
        /// Width of tiles
        /// </summary>
        public int TileWidth
        {
            get
            {
                return _tileWidth;
            }
        }

        /// <summary>
        /// Toggle whether tiles have a physical body, if solid.
        /// </summary>
        public bool TilesPhysicallySimulated
        {
            get
            {
                return _tilesPhysicallySimulated;
            }
        }

        /// <summary>
        /// Toggle whether actors have a physical body, if solid.
        /// </summary>
        public bool ActorsPhysicallySimulated
        {
            get
            {
                return _actorsPhysicallySimulated;
            }
        }

        #region Scene Nodes
        /// <summary>
        /// Scene node for actors (update only)
        /// </summary>
        public SceneNode ActorsSceneNode
        {
            get
            {
                return _actorsSceneNode;
            }
        }
        /// <summary>
        /// Scene node for short term effects (bullets, explosions, etc)
        /// </summary>
        public SceneNode EffectsSceneNode1
        {
            get
            {
                return _effectsSceneNode1;
            }
        }
        /// <summary>
        /// Scene node for short term effects (bullets, explosions, etc)
        /// </summary>
        public SceneNode EffectsSceneNode2
        {
            get
            {
                return _effectsSceneNode2;
            }
        }
        /// <summary>
        /// Scene node for backgrounds (rearmost)
        /// </summary>
        public SceneNode BackgroundSceneNode1
        {
            get
            {
                return _backgroundSceneNode1;
            }
        }
        /// <summary>
        /// Scene node for backgrounds
        /// </summary>
        public SceneNode BackgroundSceneNode2
        {
            get
            {
                return _backgroundSceneNode2;
            }
        }
        #endregion

        /// <summary>
        /// Background fill color if no background present
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
            }
        }

        /// <summary>
        /// List of tile precedence orders (height)
        /// </summary>
        public List<string> TilePrecedenceOrder
        {
            get
            {
                return _tilePrecedenceOrder;
            }
        }

        /// <summary>
        /// Background scene graph for items rendered before the Tiles.
        /// </summary>
        public GameSceneGraph BackgroundSceneGraph
        {
            get
            {
                return _backgroundSceneGraph;
            }
            protected set
            {
                _backgroundSceneGraph = value;
            }
        }


        /// <summary>
        /// Main scene graph for actors and other in-game objects
        /// </summary>
        public GameSceneGraph SceneGraph
        {
            get
            {
                return _sceneGraph;
            }
            protected set
            {
                _sceneGraph = value;
            }
        }


        /// <summary>
        /// Scene graph for GUI items and other items that do not move with the camera.
        /// </summary>
        public SceneGraph StaticSceneGraph
        {
            get
            {
                return _staticSceneGraph;
            }
            protected set
            {
                _staticSceneGraph = value;
            }
        }

        public GameSceneGraph EffectsSceneGraph
        {
            get
            {
                return _effectsSceneGraph;
            }
            protected set
            {
                _effectsSceneGraph = value;
            }
        }

        /// <summary>
        /// Engine's sprite batch for each draw
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get
            {
                return _spriteBatch;
            }
        }

        /// <summary>
        /// List of loaded tile templates to clone from
        /// </summary>
        public List<Tile> TileTemplates
        {
            get
            {
                return _tileTemplates;
            }
        }

        /// <summary>
        /// List of loaded actor templates to clone from
        /// </summary>
        public List<Actor> ActorTemplates
        {
            get
            {
                return _actorTemplates;
            }
        }

        /// <summary>
        /// List of loaded background templates to load from
        /// </summary>
        public List<Background> BackgroundTemplates
        {
            get
            {
                return _backgroundTemplates;
            }
        }


        /// <summary>
        /// List of loaded transition tile templates to load from.
        /// </summary>
        public List<TransitionOverlayTile> TransitionOverlayTileTemplates
        {
            get
            {
                return _transitionOverlayTileTemplates;
            }
        }


        /// <summary>
        /// Current list of levels
        /// </summary>
        public List<Level> Levels
        {
            get
            {
                return _levels;
            }
        }

        /// <summary>
        /// Tiles loaded in the engine.
        /// </summary>
        public List<Tile> Tiles
        {
            get
            {
                return _tiles;
            }
        }

        /// <summary>
        /// Transition tiles loaded in the engine.
        /// </summary>
        public List<TransitionOverlayTile> TransitionTiles
        {
            get
            {
                return _transitionTiles;
            }
        }

        /// <summary>
        /// Actors loaded in the engine.
        /// </summary>
        public List<Actor> Actors
        {
            get
            {
                return _actors;
            }
        }

        /// <summary>
        /// Backgrounds loaded in the engine.
        /// </summary>
        public List<Background> Backgrounds
        {
            get
            {
                return _backgrounds;
            }
        }


        /// <summary>
        /// Current game state
        /// </summary>
        public GameState State
        {
            get
            {
                return _gameState;
            }
            set
            {
                _gameState = value;
            }
        }

        /// <summary>
        /// Specify an actor for the camera to follow.
        /// </summary>
        public Actor CameraFollowActor
        {
            get
            {
                return _cameraFollowActor;
            }
            set
            {
                _cameraFollowActor = value;
            }
        }

        /// <summary>
        /// In-game gravity.
        /// </summary>
        public float Gravity
        {
            get
            {
                return _gravity;
            }
            set
            {
                _gravity = value;
            }
        }

        /// <summary>
        /// FarSeer Physics simulator instance
        /// </summary>
        public World PhysicsSimulator
        {
            get
            {
                return _physicsSimulator;
            }
        }

        /// <summary>
        /// Flag to put the engine in editor mode (no actor logic running, etc)
        /// </summary>
        public bool EditorMode
        {
            get
            {
                return _editorMode;
            }
            set
            {
                _editorMode = value;
            }
        }

        
        /// <summary>
        /// Tile array for map navigation
        /// </summary>
        public Tile[,] TileGrid
        {
            get
            {
                return _tileGrid;
            }
        }


        /// <summary>
        /// Flag to show engine debug info
        /// </summary>
        public bool ShowDebugInfo
        {
            get { return _showDebugInfo; }
            set
            {
                if (value != _showDebugInfo && value == true)
                    _debugPanel.ShowForm();
                else if (value != _showDebugInfo && value == false)
                    _debugPanel.HideForm();

                _showDebugInfo = value;
            }
        }



        /// <summary>
        /// Quad tree for tile rendering
        /// </summary>
        public QuadTree<GameSceneNode> TileQuadTree
        {
            get { return _tileQuadTree; }
            set { _tileQuadTree = value; }
        }

        /// <summary>
        /// Quad tree for actor 
        /// </summary>
        public QuadTree<GameSceneNode> ActorQuadTree
        {
            get { return _actorQuadTree; }
            set { _actorQuadTree = value; }
        }
        #endregion
        

        #region Constructor
        public Engine()
        {
            // Purge application log
            Log.PurgeLog();

            // Try to load app settings.
            // This is <application name>.exe.config.
            // If it chucks a spaz then the app settings file is missing.
            _appSettingsReader = new AppSettingsReader();
            try
            {
                _gravity = Convert.ToSingle(_appSettingsReader.GetValue("Gravity", typeof(String)));
                _tilesPhysicallySimulated = Convert.ToBoolean(_appSettingsReader.GetValue("PhysicallySimulateTiles", typeof(String)));
                _actorsPhysicallySimulated = Convert.ToBoolean(_appSettingsReader.GetValue("PhysicallySimulateActors", typeof(String)));
                _tileHeight = Convert.ToInt32(_appSettingsReader.GetValue("TileHeight", typeof(String)));
                _tileWidth = Convert.ToInt32(_appSettingsReader.GetValue("TileWidth", typeof(String)));
                _screenSize.X = Convert.ToInt32(_appSettingsReader.GetValue("ScreenWidth", typeof(String)));
                _screenSize.Y = Convert.ToInt32(_appSettingsReader.GetValue("ScreenHeight", typeof(String)));
                _allowedImageTypes = _appSettingsReader.GetValue("ImageFileTypes", typeof(String)).ToString();
            }
            catch (Exception ex)
            {
                Log.Message("Failure to open application settings: " + ex.Message);
            }


            _graphics = new GraphicsDeviceManager(this);
            //change window size in the game class
            this._graphics.PreferredBackBufferWidth = _screenSize.Y;
            this._graphics.PreferredBackBufferHeight = _screenSize.X;
            this.Window.AllowUserResizing = true;

            //change Window title in game class
            this.Window.Title = "DEngine";

            //content = new ContentManager(Services);
            Content.RootDirectory = "Content";

            _tileTemplates = new List<Tile>();
            _actorTemplates = new List<Actor>();
            _backgroundTemplates = new List<Background>();

            // Tile transitions
            _tilePrecedenceOrder = new List<string>();
            _transitionOverlayTileTemplates = new List<TransitionOverlayTile>();

            _tiles = new List<Tile>();
            _transitionTiles = new List<TransitionOverlayTile>();
            _actors = new List<Actor>();
            _backgrounds = new List<Background>();
            _levels = new List<Level>();

            ResetSceneGraph();  // Clear and get new scene graph (layered)
            _sceneGraph.CullingDistance = _screenSize.X;
        }
        #endregion


        // XNA Game overrides
        // Initialize, Update, Draw

        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;

            //set update to be fixed at 100 fps
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10); //10 ms --> 100 fps for physics update          

            //create the simulator
            _physicsSimulator = new World(new Vector2(0, _gravity));
            
            //_physicsSimulator.EnableDiagnostics = true;
            
            //physicsSimulator.AllowedPenetration = ALLOWED_PENETRATION; // 0.05 default
            //physicsSimulator.BiasFactor = 0.2f;

            // Xml save/load
            _engineIO = new EngineIO(this);

            base.Initialize();

            
        }
        #endregion


        #region OnExiting
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
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


            Log.Message("DEngine 1.0 started on " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            Log.Message("-----------------------------------------");

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            //DGuiCommon.SpriteBatch = this.SpriteBatch;
            _guiManager = new DGuiManager(this, this.SpriteBatch);

            // Load tile and actor templates
            _engineIO.LoadTileTemplates();
            _engineIO.LoadOverlayTilesAndPrecedenceOrder();
            _engineIO.LoadTileProperties();
            _engineIO.LoadActorTemplates();
            _engineIO.LoadBackgroundTemplates();

            // Load console object
            _console = new DConsole(this);
            _console.Initialize();

            _debugPanel = new DebugPanel(this);


        }
        #endregion


        #region UnloadContent
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Log.Message("-----------------------------------------");
            Log.Message("DEngine 1.0 unloaded on " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            Log.Message("Average update time (milliseconds): " + _totalUpdateDuration / _updateCount);
            Log.Message("Average draw time (milliseconds): " + _totalDrawDuration / _drawCount);

            //soundBank.Dispose();
            //waveBank.Dispose();
            //audioEngine.Dispose();

            //_physicsSimulator.Clear();




            Content.Unload();
        }
        #endregion


        // Main engine loop
        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Reset mouse state
            //DMouse.InputHandled = false;

            // Update static scenegraph
            if (_staticSceneGraph != null)
                _staticSceneGraph.Update(gameTime);

            _guiManager.Update(gameTime);

            if (_gameState == GameState.Running || EditorMode)
            {
                //step the simulator. must convert the value of dt to seconds.   
                if (!EditorMode)  
                    _physicsSimulator.Step(gameTime.ElapsedGameTime.Milliseconds * .001f);


                _backgroundSceneGraph.Camera.Position = _sceneGraph.Camera.Position;
                if (_backgroundSceneGraph != null)
                    _backgroundSceneGraph.Update(gameTime);

                

                // Update the scene-graph
                if (_sceneGraph != null)
                    _sceneGraph.Update(gameTime);

                _effectsSceneGraph.Camera.Position = _sceneGraph.Camera.Position;
                if (_effectsSceneGraph != null)
                    _effectsSceneGraph.Update(gameTime);

                _totalUpdateDuration += gameTime.ElapsedGameTime.TotalMilliseconds;
                _updateCount++;

            }



            // Focus the camera on an actor (if necessary)
            if (_cameraFollowActor != null)
            {
                Vector2 pos = _cameraFollowActor.Position;

                // Center the camera view
                Vector3 centeredPosition = new Vector3(pos.X - (_graphics.PreferredBackBufferWidth / 2),
                                                        pos.Y - (_graphics.PreferredBackBufferHeight / 2), 0);
                _sceneGraph.Camera.Position = new Vector3(-centeredPosition.X, -centeredPosition.Y, 0);
            }


            

            //base.Update(gameTime);
        }
        #endregion


        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(_backgroundColor);

            if (_renderEnabled)
            {
                //_spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
                _spriteBatch.Begin();

                if (_backgroundSceneGraph != null)
                    _backgroundSceneGraph.Draw(gameTime);

                DrawTileQuadTree(gameTime);

                // Draw the game scene graph
                //if (_sceneGraph != null)
                //    _sceneGraph.Draw(gameTime);

                DrawActorQuadTree(gameTime);

                if (_effectsSceneGraph != null)
                    _effectsSceneGraph.Draw(gameTime);

                // Draw the hud
                if (_staticSceneGraph != null)
                    _staticSceneGraph.Draw(gameTime);

                // Draw the GUI
                _guiManager.Draw(gameTime);

                //base.Draw(gameTime);

                _spriteBatch.End();
            }



            _totalDrawDuration += gameTime.ElapsedGameTime.TotalMilliseconds;
            _drawCount++;
        }
        #endregion



        #region RemoveTile
        /// <summary>
        /// Remove a tile from the scene graph and the engine's collection.
        /// </summary>
        /// <param name="tile"></param>
        public void RemoveTile(Tile tile)
        {
            if (_tiles.Contains(tile))
            { 
                // Also remove its transition tiles
                foreach (TransitionOverlayTile tt in tile.TransitionOverlayTiles)
                {
                    //_sceneGraph.RemoveNode(tt);
                    _tileQuadTree.Remove(tt);
                    _transitionTiles.Remove(tt);
                    tt.Dispose();
                }

                //_sceneGraph.RemoveNode(tile);
                _tileQuadTree.Remove(tile);
                _currentLevel.Tiles.Remove(tile);
                
                _tiles.Remove(tile);
                tile.Dispose();
            }
        }
        #endregion


        #region LoadQuadTree
        /// <summary>
        /// Insert all items from the scenegraph into the quad tree for rendering.
        /// Ideally we should automatically insert into the quad tree upon adding to the scene graph,
        /// but we are manually inserting children and it is difficult to add quad tree insert code at that level
        /// </summary>
        private void LoadQuadTree(RectangleF rectangle)
        {
            _tileQuadTree = new QuadTree<GameSceneNode>(rectangle);
            _actorQuadTree = new QuadTree<GameSceneNode>(rectangle);
        }
        #endregion


        #region DrawTileQuadTree
        private void DrawTileQuadTree(GameTime gameTime)
        {
            // Query quad tree to get only nodes that need rendering
            if (_tileQuadTree != null)
            {
                RectangleF drawRect = new RectangleF(-_sceneGraph.Camera.Position.X, -_sceneGraph.Camera.Position.Y,
                    GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
                List<GameSceneNode> quadTreeRenderNodes = _tileQuadTree.Query(drawRect);

                // Order the nodes by precedence
                // Should this be done in draw?
                // Currently this is needed because we are getting a subset of tiles which do not respect the order
                // in which they were added to the quad tree.
                // We get massive performance gains from avoiding an update of all the tiles but we must re-create
                // the render order each draw.
                // I am assuming the cost/benefit ratio will be worth it for larger maps.

                List<SceneNode> tileRenderList = new List<SceneNode>();
                List<SceneNode> edgeRenderList = new List<SceneNode>();
                List<SceneNode> cornerRenderList = new List<SceneNode>();

                foreach (SceneNode sceneNode in quadTreeRenderNodes)
                {
                    if (sceneNode is TransitionOverlayTile)
                    {
                        TransitionOverlayTile transitionTile = (TransitionOverlayTile)sceneNode;

                        if (transitionTile.Orientation == TransitionPosition.North ||
                            transitionTile.Orientation == TransitionPosition.East ||
                            transitionTile.Orientation == TransitionPosition.South ||
                            transitionTile.Orientation == TransitionPosition.West)
                        {
                            //tile.TransitionOverlayTiles.Insert(0, newTransitionOverlayTile);

                            //if (_tileTransitionsSceneNode1.Children.Count == 0)
                            if (edgeRenderList.Count == 0)
                            {
                                // Add it straight up. We're the first one here!
                                //_tileTransitionsSceneNode1.Children.Add(newTransitionOverlayTile);
                                edgeRenderList.Add(transitionTile);
                            }
                            else
                            {
                                // Else, add the transition tile to the scenegraph in order of precedence!
                                bool indexFound = false;
                                //for (int i = 0; i < _tileTransitionsSceneNode1.Children.Count; i++)
                                for (int i = 0; i < edgeRenderList.Count; i++)
                                {
                                    TransitionOverlayTile listTransitionTile = (TransitionOverlayTile)edgeRenderList[i];
                                    if (transitionTile.Precedence >= listTransitionTile.Precedence)
                                    {
                                        // Add it to the transition node 1 (draw edges first)
                                        //_tileTransitionsSceneNode1.Children.Insert(i, newTransitionOverlayTile);
                                        edgeRenderList.Insert(i, transitionTile);
                                        indexFound = true;
                                        break;
                                    }
                                }
                                if (!indexFound)
                                {
                                    // Add last.
                                    //_tileTransitionsSceneNode1.Children.Add(newTransitionOverlayTile);
                                    edgeRenderList.Add(transitionTile);
                                }
                            }
                        }

                        else
                        {
                            //tile.TransitionOverlayTiles.Add(transitionTile);

                            //if (_tileTransitionsSceneNode1.Children.Count == 0)
                            if (cornerRenderList.Count == 0)
                            {
                                // Add it straight up. We're the first one here!
                                //_tileTransitionsSceneNode1.Children.Add(newTransitionOverlayTile);
                                cornerRenderList.Add(transitionTile);
                            }
                            else
                            {
                                // Else, add the transition tile to the scenegraph in order of precedence!
                                bool indexFound = false;
                                //for (int i = 0; i < _tileTransitionsSceneNode1.Children.Count; i++)
                                for (int i = 0; i < cornerRenderList.Count; i++)
                                {
                                    TransitionOverlayTile listTransitionTile = (TransitionOverlayTile)cornerRenderList[i];
                                    if (transitionTile.Precedence >= listTransitionTile.Precedence)
                                    {
                                        // Add it to the transition node 1 (draw edges first)
                                        //_tileTransitionsSceneNode1.Children.Insert(i, newTransitionOverlayTile);
                                        cornerRenderList.Insert(i, transitionTile);
                                        indexFound = true;
                                        break;
                                    }
                                }
                                if (!indexFound)
                                {
                                    // Add last.
                                    //_tileTransitionsSceneNode1.Children.Add(newTransitionOverlayTile);
                                    cornerRenderList.Add(transitionTile);
                                }
                            }
                        }
                    }
                    else // tile
                    {
                        tileRenderList.Insert(0, sceneNode);
                    }

                }


                tileRenderList.AddRange(edgeRenderList);
                tileRenderList.AddRange(cornerRenderList);


                foreach (SceneNode n in tileRenderList)
                {
                    if (n.Visible)
                        n.Draw(gameTime);
                }
            }


        }
        #endregion



        #region DrawActorQuadTree
        private void DrawActorQuadTree(GameTime gameTime)
        {
            // Query quad tree to get only nodes that need rendering
            if (_actorQuadTree != null)
            {
                RectangleF drawRect = new RectangleF(-_sceneGraph.Camera.Position.X, -_sceneGraph.Camera.Position.Y,
                    GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height);
                List<GameSceneNode> quadTreeRenderNodes = _actorQuadTree.Query(drawRect);

                foreach (SceneNode n in quadTreeRenderNodes)
                {
                    if (n.Visible || n.AlwaysVisible)
                        n.DrawRecursive(gameTime);
                }
            }
        }
        #endregion


        // Public engine functions

        #region SetAspectRatio
        /// <summary>
        /// Change the viewport to this width and height.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetAspectRatio(int width, int height)
        {
            if (width != 0 && height != 0)
            {
                _graphics.PreferredBackBufferWidth = width;
                _graphics.PreferredBackBufferHeight = height;
                
                _sceneGraph.Camera.AspectRatio = (float)width / (float)height;
                //graphics.GraphicsDevice.Reset();
                _graphics.ApplyChanges();
            }
        }
        #endregion



        #region RunLevel
        /// <summary>
        /// Add pre-loaded level to the scenegraph. Reloads the tile grid.
        /// </summary>
        /// <param name="level"></param>
        public void RunLevel(Level level)
        {
            ResetSceneGraph();

            _currentLevel = level;
            _tileGrid = new Tile[_currentLevel.Width + 1, _currentLevel.Height + 1];

            // Load Quad Tree
            // If it's in editor mode we want a very big board
            if (EditorMode)
            {
                float size = 200 * TileWidth;
                LoadQuadTree(new RectangleF(0, 0, size, size));
            }
            else // restrict it to the level size
                LoadQuadTree(new RectangleF(0, 0, (level.Width + 1) * TileWidth, (level.Height + 1) * TileHeight));



            foreach (Tile t in level.Tiles)
            {
                // Calculate this tile's grid position
                int xCoord = (int)t.Position.X / _tileWidth;
                int yCoord = (int)t.Position.Y / _tileHeight;
                _tileGrid[xCoord, yCoord] = t;

                _tileQuadTree.Insert(t);
                //_tilesSceneNode.Children.Add(t);
            }



            List<TransitionOverlayTile> edgeTransitionsList = new List<TransitionOverlayTile>();
            List<TransitionOverlayTile> cornerTransitionsList = new List<TransitionOverlayTile>();


            foreach (TransitionOverlayTile overlayTile in level.TransitionTiles)
            {
                // Add edges to the lower scenegraph node for transition tiles
                if (overlayTile.Orientation == TransitionPosition.East ||
                    overlayTile.Orientation == TransitionPosition.West ||
                    overlayTile.Orientation == TransitionPosition.North ||
                    overlayTile.Orientation == TransitionPosition.South)
                {
                    edgeTransitionsList.Add(overlayTile);
                }
                else
                {
                    cornerTransitionsList.Add(overlayTile);
                }
            }

            foreach (TransitionOverlayTile tile in edgeTransitionsList)
            {
                //_tileTransitionsSceneNode1.Children.Add(tile);
                _tileQuadTree.Insert(tile);
            }

            foreach (TransitionOverlayTile tile in cornerTransitionsList)
            {
                //_tileTransitionsSceneNode2.Children.Add(tile);
                _tileQuadTree.Insert(tile);
            }

            foreach (Actor a in level.Actors)
            {
                if (a.PhysicallySimulated)
                {
                    //a.Geom.CollisionGroup = a.Team;
                }

                _actorQuadTree.Insert(a);
                _actorsSceneNode.Children.Add(a);

                if (a.Body != null)
                    a.Body.Position = a.Position;
            }

            



            if (!EditorMode)
            {
                // Set up a camera bounding rect on this level
                int maxDisplaceX, maxDisplaceY;
                maxDisplaceX = (level.Width * TileHeight) - (Window.ClientBounds.Width);
                maxDisplaceY = (level.Height * TileHeight) - (Window.ClientBounds.Height);
                _sceneGraph.Camera.BoudingRectangle = new Rectangle(0, 0, maxDisplaceX, maxDisplaceY);
                _sceneGraph.Camera.UseRectangleBounding = true;
            }
            



            _updateCount = 0;
            _totalUpdateDuration = 0;
            _drawCount = 0;
            _totalDrawDuration = 0;


            if (_showDebugInfo)
            {
                _debugPanel.ShowForm();
            }
        }
        #endregion


        #region EndLevel
        /// <summary>
        /// Destroy the scenegraph and recreate
        /// Empty the tile and actor lists
        /// </summary>
        public void EndLevel()
        {
            ResetSceneGraph();
            _physicsSimulator.ClearForces();

            // Destroy each tile and actor
            //foreach (Tile t in _tiles)
            //{
            //    t.Dispose();
            //}

            foreach (TransitionOverlayTile tt in _transitionTiles)
            {
                tt.Dispose();
            }

            foreach (Actor a in _actors)
            {
                a.Dispose();
            }

            foreach (Background b in _backgrounds)
            {
                b.Dispose();
            }

            _tiles.Clear();
            _transitionTiles.Clear();
            _actors.Clear();
            _backgrounds.Clear();


            if (_showDebugInfo)
            {
                _debugPanel.HideForm();
            }
        }
        #endregion


        #region MoveCameraBy
        /// <summary>
        /// Move camera this distance from its current position
        /// </summary>
        /// <param name="x">X distance</param>
        /// <param name="y">Y distance</param>
        public void MoveCameraBy(float x, float y)
        {
            _sceneGraph.Camera.Position = new Vector3(_sceneGraph.Camera.Position.X + x,
                                                        _sceneGraph.Camera.Position.Y + y,
                                                        _sceneGraph.Camera.Position.Z);
        }
        #endregion


        #region MoveCameraTo
        /// <summary>
        /// Set camera to this position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveCameraTo(float x, float y)
        {
            _sceneGraph.Camera.Position = new Vector3(x, y, _sceneGraph.Camera.Position.Z);
        }
        #endregion


        #region TilePointHitCheck
        /// <summary>
        /// Check for the existence of a tile at a point
        /// </summary>
        /// <param name="point">The coord to check</param>
        /// <returns></returns>
        public Tile TileExistenceCheckByGeometry(Vector2 point)
        {
            foreach (Tile tile in _tiles)
            {
                if (tile.PointHitCheck(point) != null)
                    return tile;
            }
            return null;
        }
        #endregion


        #region TileGridCheck
        /// <summary>
        /// Check if there is a tile at this grid location rather than by geometry check.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Tile TileExistenceCheckByExactLocation(Vector2 pos)
        {
            foreach (Tile tile in _tiles)
            {
                if (tile.Position.X == pos.X && tile.Position.Y == pos.Y)
                {
                    return tile;
                }
            }
            return null;
        }
        #endregion


        #region TileExistenceCheckByGridRef
        /// <summary>
        /// Search for a tile by its grid location relative to zero.
        /// Takes into account tile size.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Tile TileExistenceCheckByGridRef(GridReference gridRef)
        {
            int x = gridRef.X;
            int y = gridRef.Y;

            Tile tile = _tileGrid[x, y];
            return tile;
        }
        #endregion


        #region ActorPointHitCheck
        /// <summary>
        /// Check for the existence of an actor at a point
        /// </summary>
        /// <param name="point">The 2D point to check</param>
        /// <returns></returns>
        public Actor ActorPointHitCheck(Vector2 point)
        {
            foreach (Actor actor in _actors)
            {
                if (actor.PointHitCheck(point) != null)
                    return actor;
            }
            return null;
        }
        #endregion


        #region MakeTiledGameBoard
        /// <summary>
        /// Make a blank game board of a defined size using a specified tile name.
        /// Beware of large numbers!
        /// </summary>
        public void MakeTiledGameBoard(int width, int height, string tileName)
        {
            EndLevel();
            _levels.Clear();

            // Make a new level
            Level newLevel = new Level(this, "NewLevel.xml");
            newLevel.Width = width;
            newLevel.Height = height;

            // Get the named tile
            Tile tileTemplate = GetTileTemplateByName(tileName);

            // Failsafe: use the first template tile if we can't find this one
            if (tileTemplate == null && _tileTemplates.Count > 0)
            {
                tileTemplate = _tileTemplates[0];
            }

            if (tileTemplate != null)
            {
                // Warning! Big numbers will make this explode!
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Tile newTile = tileTemplate.Clone();
                        int tileX, tileY;
                        tileX = (x * _tileHeight) + (_tileHeight / 2);
                        tileY = (y * _tileHeight) + (_tileHeight / 2);
                        newTile.Position = new Vector2(tileX, tileY);
                        newTile.Initialize();
                        _tiles.Add(newTile);
                        newLevel.Tiles.Add(newTile);
                    }
                }

                _levels.Add(newLevel);
            }

            RunLevel(newLevel);
        }
        #endregion


        #region GetTileTemplateByName
        /// <summary>
        /// Get a tile template by its image name (minus extension).
        /// </summary>
        /// <param name="tileName"></param>
        /// <returns></returns>
        public Tile GetTileTemplateByName(string tileName)
        {
            // Figure out which tile it is
            Tile tile = null;
            for (int i = 0; i < _tileTemplates.Count; i++)
            {
                if (_tileTemplates[i].ImageName == tileName)
                {
                    tile = _tileTemplates[i];
                    break;
                }
            }
            return tile;
        }
        #endregion


        #region ResetCameraPosition
        /// <summary>
        /// Stop following all actors and return to 0,0
        /// </summary>
        public void ResetCameraPosition()
        {
            _cameraFollowActor = null;
            _sceneGraph.Camera.Position = Vector3.Zero;
        }
        #endregion


        #region AbsoluteCoordinates
        /// <summary>
        /// Get coordinates relative to the camera
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector2 AbsoluteCoordinates(float x, float y)
        {
            return AbsoluteCoordinates(new Vector2(x, y));
        }
        

        /// <summary>
        /// Get coordinates relative to the camera
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public Vector2 AbsoluteCoordinates(Vector2 coordinates)
        {
            Vector2 abs = new Vector2(  coordinates.X - _sceneGraph.Camera.Position.X, 
                                        coordinates.Y - _sceneGraph.Camera.Position.Y);
            return abs;
        }
        #endregion


        #region GetTileGridPosition
        /// <summary>
        /// Get the grid position of the tile from local window coordinates.
        /// Takes into account the engine's camera position.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetTileGridPosition(Vector2 pos)
        {
            // Take into account camera.
            pos = AbsoluteCoordinates(pos);

            float x, y;
            x = pos.X;
            y = pos.Y;

            // Align to grid
            float gridX, gridY;
            gridX = (Math.Abs(x) % _tileHeight) - (_tileHeight / 2);
            gridY = (Math.Abs(y) % _tileHeight) - (_tileHeight / 2);

            // Place with respect to cartesian plane  
            if (x >= 0)
                x -= gridX;
            else
                x += gridX;

            if (y >= 0)
                y -= gridY;
            else
                y += gridY;


            return new Vector2(x, y);
        }
        #endregion


        #region GetTemplateActorByName
        public Actor GetTemplateActorByName(string actorName)
        {
            Actor tempActor = null;
            foreach (Actor a in _actorTemplates)
            {
                if (a.Name == actorName)
                {
                    tempActor = a;
                    break;
                }
            }
            return tempActor;
        }
        #endregion


        #region Sound
        public Cue PlaySound(string Name)
        {
            Cue returnValue = _soundBank.GetCue(Name);
            returnValue.Play();
            return returnValue;
        }

        public static void StopSound(Cue cue)
        {
            cue.Stop(AudioStopOptions.Immediate);
        }
        #endregion


        // Protected engine functions

        // Virtual
        #region AddTemplateActor
        /// <summary>
        /// Virtual function for overriding actor template load.
        /// Override to insert your own derived actor classes.
        /// </summary>
        /// <param name="actor"></param>
        public virtual void AddTemplateActor(Actor actor)
        {
            // Add the actor to the template list!
            _actorTemplates.Add(actor);
        }
        #endregion


        #region ResetSceneGraph
        /// <summary>
        /// Destroy the scenegraph and recreate.
        /// </summary>
        protected void ResetSceneGraph()
        {
            if (_backgroundSceneGraph != null)
                _backgroundSceneGraph.Dispose();

            _tileQuadTree = null;
            _actorQuadTree = null;

            if (_sceneGraph != null)
                _sceneGraph.Dispose();

            if (_staticSceneGraph != null)
                _staticSceneGraph.Dispose();

            if (_effectsSceneGraph != null)
                _effectsSceneGraph.Dispose();

            _backgroundSceneGraph = GetNewBackgroundSceneGraph();
            _sceneGraph = GetNewSceneGraph();

            _staticSceneGraph = new SceneGraph(this);

            _effectsSceneGraph = new GameSceneGraph(this);
            _effectsSceneNode1 = new SceneNode(this);
            _effectsSceneNode2 = new SceneNode(this);
            _effectsSceneGraph.RootNode.Children.Add(_effectsSceneNode1);
            _effectsSceneGraph.RootNode.Children.Add(_effectsSceneNode2);

            MoveCameraTo(0, 0);
        }
        #endregion


        #region GetNewSceneGraph
        /// <summary>
        /// Set up a layered scenegraph.
        /// </summary>
        /// <returns></returns>
        protected GameSceneGraph GetNewSceneGraph()
        {
            GameSceneGraph sg = new GameSceneGraph(this);
            _actorsSceneNode = new SceneNode(this);
            sg.RootNode.Children.Add(_actorsSceneNode);

            return sg;
        }

        protected GameSceneGraph GetNewBackgroundSceneGraph()
        {
            GameSceneGraph sg = new GameSceneGraph(this);
            _backgroundSceneNode1 = new SceneNode(this);
            _backgroundSceneNode2 = new SceneNode(this);
            sg.RootNode.Children.Add(_backgroundSceneNode1);
            sg.RootNode.Children.Add(_backgroundSceneNode2);

            return sg;
        }

        #endregion



        #region AddTile
        /// <summary>
        /// Place a tile on the board at these absolute coords. Will replace a tile at this position if it's not already the same type.
        /// Will take into account camera displacement.
        /// </summary>
        /// <param name="tilePos">Position of tile to be placed.</param>
        /// <returns>Tile that was added.</returns>
        public Tile AddTile(Vector2 tilePos, Tile tileTemplate, float rotation)
        {
            Tile returnTile = null;
            //Vector2 newTilePos = AbsoluteCoordinates(tilePos);

            // Get any tile at this grid ref and overwrite it, if it's different.
            bool placeTileAllowed = true; // Flag: Only allow a tile placement if the existing tile is different to the selected tile.
            Vector2 newTilePos = GetTileGridPosition(tilePos);
            Tile existingTile = TileExistenceCheckByExactLocation(newTilePos);
            if (existingTile != null)
            {
                // Remove the tile if the image names aren't the same
                if (existingTile.ImageName != tileTemplate.ImageName)
                {
                    RemoveTile(existingTile);
                    _currentLevel.Tiles.Remove(existingTile);
                }
                else // Otherwise, we're trying to overwrite a tile of the same type. Disallow the placement.
                {
                    placeTileAllowed = false;
                }
            }


            // Positioning and placement.
            if (placeTileAllowed)
            {
                // Clone from the tile template!
                Tile newTile = tileTemplate.Clone();
                newTile.Position = new Vector2(newTilePos.X, newTilePos.Y);
                newTile.Rotation = rotation;

                // Initialize & attach the node to the graph
                newTile.Initialize();

                _tileQuadTree.Insert(newTile);
                _tiles.Add(newTile);
                _currentLevel.Tiles.Add(newTile);

                GridReference gridRef = newTile.GridReference();
                if (gridRef.X < _currentLevel.Width && gridRef.X >= 0 && gridRef.Y < _currentLevel.Height && gridRef.Y >= 0)
                    _tileGrid[gridRef.X, gridRef.Y] = newTile;

                // Some tiles might be part of the precedence order for tile transitions.
                // All surrounding tiles must be examined when adding a tile, in order to determine if
                // transition tiles need to be created.
                // These must also have their transition tile update routines called
                CalculateTileTransitions(newTile);

                returnTile = newTile;
            }
            return returnTile;
        }
        #endregion


        #region TransitionPositionFromUnitVector
        /// <summary>
        /// Accepts a unit vector from -1 to 1 to give an 8-way nautical direction.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TransitionPosition TransitionPositionFromUnitVector(int x, int y)
        {
            TransitionPosition transOverlayPosition;

            // Figure out the orientation
            // No way to do this but the long way!
            if (x == -1 && y == -1)
                transOverlayPosition = TransitionPosition.NorthWest;
            else if (x == 0 && y == -1)
                transOverlayPosition = TransitionPosition.North;
            else if (x == 1 && y == -1)
                transOverlayPosition = TransitionPosition.NorthEast;
            else if (x == -1 && y == 0)
                transOverlayPosition = TransitionPosition.West;
            else if (x == 1 && y == 0)
                transOverlayPosition = TransitionPosition.East;
            else if (x == -1 && y == 1)
                transOverlayPosition = TransitionPosition.SouthWest;
            else if (x == 0 && y == 1)
                transOverlayPosition = TransitionPosition.South;
            else if (x == 1 && y == 1)
                transOverlayPosition = TransitionPosition.SouthEast;
            else
                transOverlayPosition = TransitionPosition.None;

            return transOverlayPosition;
        }
        #endregion


        #region CalculateTileTransitions
        /// <summary>
        /// Recalculate tile transitions for this tile.
        /// </summary>
        public void CalculateTileTransitions(Tile tile)
        {
            CalculateTileTransitionsRecursive(tile, tile, 0, 2);
        }

        /// <summary>
        /// Recursively calculate transition tiles for a tile and adjacent tiles.
        /// I wouldn't give it any more than 2 bounces if I were you.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="rootTile"></param>
        /// <param name="bounceCount"></param>
        /// <param name="maxBounces"></param>
        private void CalculateTileTransitionsRecursive(Tile tile, Tile rootTile, int bounceCount, int maxBounces)
        {
            // This method triggers terrain changes as each individual tile changes.
            // The engine will draw the tiles, then the tile edges, then the corner tiles (as inner corners overlap).
            // This is done using the appropriate SceneNodes in the engine.


            // Clear everything first.
            foreach (TransitionOverlayTile tt in tile.TransitionOverlayTiles)
            {
                _tileQuadTree.Remove(tt);
                _transitionTiles.Remove(tt);
            }
            tile.TransitionOverlayTiles.Clear();


            bounceCount++;

            // Calc new tile transitions
            // Get the precedence of this tile
            int currentTilePrecedence = _tilePrecedenceOrder.IndexOf(tile.ImageName);
            Tile tileTemplate = GetTileTemplateByName(tile.ImageName);

            // List of transition positions & orientations required
            List<TransitionTilePosition> transitionTileList = new List<TransitionTilePosition>();

            // Get all tiles surrounding the current tile
            for (int column = -1; column < 2; column++)
            {
                for (int row = -1; row < 2; row++)
                {
                    if (column == 0 && row == 0)
                        continue; // Exclude ourselves

                    // Find grid ref of this tile
                    int transTileX = (int)tile.Position.X + (column * _tileHeight);
                    int transTileY = (int)tile.Position.Y + (row * _tileHeight);

                    // Get the tile at this grid ref
                    TransitionTilePosition transitionTile = new TransitionTilePosition();
                    transitionTile.position = new Vector2(transTileX, transTileY);
                    transitionTile.underlyingTile = TileExistenceCheckByExactLocation(transitionTile.position);

                    if (transitionTile.underlyingTile != null)
                    {
                        // Get the precedence of the nearby tile
                        int surroundingTilePrecedence = _tilePrecedenceOrder.IndexOf(transitionTile.underlyingTile.ImageName);

                        // Get the orientation of the transition tile needed for this nearby grid square.
                        transitionTile.orientation = TransitionPositionFromUnitVector(column, row);

                        // Get the adjacent grid squares if this transition tile is on a diagonal
                        List<Tile> adjacentTiles = new List<Tile>();
                        if (row != 0 && column != 0)
                        {
                            // It's a diagonal! Get the positions of the adjacent tiles
                            List<Vector2> adjacentTilePositions = new List<Vector2>();
                            adjacentTilePositions.Add(new Vector2(tile.Position.X, transitionTile.position.Y));
                            adjacentTilePositions.Add(new Vector2(transitionTile.position.X, tile.Position.Y));

                            // Obtain these adjacent tiles and add them to the list.
                            foreach (Vector2 adjacentTilePosition in adjacentTilePositions)
                            {
                                Tile adjacentTile = TileExistenceCheckByExactLocation(adjacentTilePosition);
                                adjacentTiles.Add(adjacentTile);
                            }
                        }



                        // Figure out if this adjacent tile is lower on the precedence chain.
                        // If it is lower, draw a transition tile at this edge/corner.
                        if (surroundingTilePrecedence > currentTilePrecedence)
                        {
                            // If this transition is on a diagonal from the parent tile, need to check the two tiles
                            // that share an edge with both transition and tile (i.e. think opposite colors on a chessboard)
                            // This will determine whether the corner is an interior corner, exterior corner, or no corner.
                            if (row != 0 && column != 0)
                            {
                                int squaresLowerPrecedenceCount = 0; // How many adjacent tiles have lower precedence
                                int squaresEqualPrecedenceCount = 0; // How many adjacent tiles of the same type

                                // Check for existence of these adjacent tiles and figure out what type they are
                                foreach (Tile adjacentTile in adjacentTiles)
                                {
                                    if (adjacentTile != null)
                                    {
                                        int adjacentTilePrecedence = _tilePrecedenceOrder.IndexOf(adjacentTile.ImageName);
                                        if (adjacentTilePrecedence > currentTilePrecedence)
                                        {
                                            squaresLowerPrecedenceCount++;
                                        }
                                        else if (adjacentTilePrecedence == currentTilePrecedence)
                                        {
                                            squaresEqualPrecedenceCount++;
                                        }
                                    }
                                }


                                // Draw inner corner only if the adjacent tiles are of equal precedence.
                                if (squaresEqualPrecedenceCount == 2)
                                {
                                    // Draw inner corner!
                                    TransitionPosition transOverlayPosition = TransitionPositionFromUnitVector(column, row);

                                    // Change the default of outer corner to inner corner
                                    if (transOverlayPosition == TransitionPosition.NorthEast)
                                        transitionTile.orientation = TransitionPosition.NorthEastInner;
                                    else if (transOverlayPosition == TransitionPosition.NorthWest)
                                        transitionTile.orientation = TransitionPosition.NorthWestInner;
                                    else if (transOverlayPosition == TransitionPosition.SouthEast)
                                        transitionTile.orientation = TransitionPosition.SouthEastInner;
                                    else
                                        transitionTile.orientation = TransitionPosition.SouthWestInner;
                                }
                                // Else if we're on high ground, draw outer corner.
                                else if (squaresLowerPrecedenceCount == 2)
                                {
                                    transitionTile.orientation = TransitionPositionFromUnitVector(column, row);
                                }
                                else
                                {
                                    // Else we have at least one adjacent tile with equal or greater precedence. Draw nothing!
                                    transitionTile.orientation = TransitionPosition.None;
                                }
                            }
                            else
                            {
                                // Edge overlay. Draw it no matter what.
                            }
                        }
                        // Else if this adjacent tile has greater or equal precedence.
                        else
                        {
                            // What if a diagonally opposite tile is equal in precedence but both adjacent tiles are lower in precedence?
                            // Draw an interior corner on one side (so the other tile will draw the other corner.)
                            if (surroundingTilePrecedence == currentTilePrecedence
                                && row != 0 && column != 0)
                            {

                                // Both adjacent tiles must be lower in precedence!
                                bool bothAdjacentTilesLowerInPrecedence = true;
                                foreach (Tile adjacentTile in adjacentTiles)
                                {
                                    if (adjacentTile != null)
                                    {
                                        int adjacentTilePrecedence = _tilePrecedenceOrder.IndexOf(adjacentTile.ImageName);
                                        if (adjacentTilePrecedence <= currentTilePrecedence)
                                        {
                                            bothAdjacentTilesLowerInPrecedence = false;
                                            break;
                                        }
                                    }
                                }


                                if (bothAdjacentTilesLowerInPrecedence)
                                {
                                    transitionTile.position = new Vector2(transitionTile.underlyingTile.Position.X, tile.Position.Y); // retain Y

                                    // We need to do the old switcheroo on the positions.
                                    if (transitionTile.orientation == TransitionPosition.NorthEast)
                                        transitionTile.orientation = TransitionPosition.SouthEastInner;
                                    else if (transitionTile.orientation == TransitionPosition.NorthWest)
                                        transitionTile.orientation = TransitionPosition.SouthWestInner;
                                    else if (transitionTile.orientation == TransitionPosition.SouthEast)
                                        transitionTile.orientation = TransitionPosition.NorthEastInner;
                                    else if (transitionTile.orientation == TransitionPosition.SouthWest)
                                        transitionTile.orientation = TransitionPosition.NorthWestInner;
                                }
                                else
                                {
                                    // Both adjacent tiles aren't lower in precedence, draw nothing!
                                    transitionTile.orientation = TransitionPosition.None;
                                }
                            }
                            else
                            {
                                // Diagonally opposite tile has greater precedence, draw nothing
                                transitionTile.orientation = TransitionPosition.None;
                            }
                        }

                        // Tell this nearby tile to recalculate it's transition tiles.
                        if (bounceCount < maxBounces)
                        {
                            CalculateTileTransitionsRecursive(transitionTile.underlyingTile, rootTile, bounceCount, maxBounces);
                        }



                        // Add to the list of tiles to be drawn!
                        // Insert the edges at the start of the list (some corners are overlays)
                        if (transitionTile.orientation == TransitionPosition.North ||
                            transitionTile.orientation == TransitionPosition.East ||
                            transitionTile.orientation == TransitionPosition.South ||
                            transitionTile.orientation == TransitionPosition.West)
                        {
                            // Add to the start!
                            transitionTileList.Insert(0, transitionTile);
                        }
                        else if (transitionTile.orientation != TransitionPosition.None)
                            transitionTileList.Add(transitionTile);
                    }
                    else
                    {
                        // Found a null underlying tile. It's the edge of the map.
                    }
                }
            }


            // Find this tile's template's transition overlay tiles that match the orientations (phew!)
            // Add to the tile to the scenegraph!
            foreach (TransitionTilePosition transitionTilePosition in transitionTileList)
            {
                foreach (TransitionOverlayTile tt in tileTemplate.TransitionOverlayTiles)
                {
                    if (tt.Orientation == transitionTilePosition.orientation)
                    {
                        // Give the current tile a transition tile on this edge/corner
                        TransitionOverlayTile newTransitionOverlayTile = tt.Clone();
                        newTransitionOverlayTile.Parent = tile;
                        newTransitionOverlayTile.Orientation = transitionTilePosition.orientation;
                        newTransitionOverlayTile.Position = transitionTilePosition.position;
                        newTransitionOverlayTile.Precedence = tileTemplate.Precedence;
                        newTransitionOverlayTile.Initialize();


                        _tileQuadTree.Insert(newTransitionOverlayTile);
                        _transitionTiles.Add(newTransitionOverlayTile);

                        if (newTransitionOverlayTile.Orientation == TransitionPosition.North ||
                            newTransitionOverlayTile.Orientation == TransitionPosition.East ||
                            newTransitionOverlayTile.Orientation == TransitionPosition.South ||
                            newTransitionOverlayTile.Orientation == TransitionPosition.West)
                        {
                            tile.TransitionOverlayTiles.Insert(0, newTransitionOverlayTile);
                        }
                        else
                        {
                            tile.TransitionOverlayTiles.Add(newTransitionOverlayTile);
                        }
                    }
                }
            }
        }
        #endregion



        #region ShowTileDebugGrid
        /// <summary>
        /// Show an alternating color grid on all the tiles in the current level.
        /// </summary>
        /// <param name="enabled"></param>
        public void ShowTileDebugGrid(bool enabled)
        {
            Color tileDebugGridColor = Color.Gainsboro;

            // Switch it off if specified
            if (enabled)
            {

                // Go through tile grid and set the debug color on every second tile.
                for (int x = 0; x < _currentLevel.Width; x++)
                {
                    for (int y = 0; y < _currentLevel.Width; y++)
                    {
                        // If Y is odd, mark odd X
                        if (y % 2 == 1)
                        {
                            if (x % 2 == 1)
                            {
                                if (_tileGrid[x, y].TintColor == Color.White)
                                    _tileGrid[x, y].TintColor = tileDebugGridColor;
                            }
                        }
                        else // Y is even, mark even X
                        {
                            if (x % 2 == 0)
                            {
                                if (_tileGrid[x, y].TintColor == Color.White)
                                    _tileGrid[x, y].TintColor = tileDebugGridColor;
                            }
                        }
                    }
                }
            }
        }
        #endregion

    }
}
