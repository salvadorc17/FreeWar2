using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Microsoft.Xna.Framework.Storage;
using DGui;
using DEngine;
using FactionsGame.Actors;
using FileAccess = DEngine.FileAccess;
using XnaInput = Microsoft.Xna.Framework.Input;

using DSceneGraph;

namespace FactionsGame
{
    public delegate void UnitCommandModeEventHandler(bool commandModeStatus);

    public delegate void FactionsGameEventHandler();


    /// <summary>
    /// This class expands upon the DEngine to provide game-specific code.
    /// Factions is a real-time strategy game similar to Red Alert or Warcraft II.
    /// Uses the A* algorithm and occupancy grids to calculate unit pathfinding.
    /// Many actors descend from the RTSActor type in this project.
    /// </summary>
    public class FactionsGame : Engine
    {

        protected Collection<string> _levelNames;        // Names of levels loaded
        protected MiniMapPanel _miniMapPanel;     // HUD items
        protected UnitCommandPanel _unitCommandPanel;
        protected HQCommandPanel _hqCommandPanel;
        protected DroneCommandPanel _droneCommandPanel;
        protected SelectionBar _selectionBar;
        protected TopInfoBar _topInfoBar;
        protected ToolTip _toolTip;

        protected MainMenu _mainMenu;                    // Main menu and logo
        protected FactionsPlayer _localPlayer;                   // Current player
        protected FactionsGameSettings _gameSettings 
                        = new FactionsGameSettings();   // Game settings class
        protected InGameMenu _inGameMenu;

        // Camera movement variables
        protected bool _cameraBeingDragged = false;
        protected Vector2 _cameraMoveAnchor;
        protected float _cameraMoveScale = 1.0f;

        // Selection box and selected actor list
        protected StretchBox _stretchBox = null;
        protected Collection<RTSActor> _selectedActors = new Collection<RTSActor>();

        // State control variables
        bool _gameStarted = false;
        bool _moveCommandIssued = false;
        bool _menuButtonDown = false;        // Esc key menu, prevent rapid show/hide when held down
        bool _consoleButtonDown = false;     // Console key flag

        // Pathfinding object
        PathMarshal _pathMarshal;
        
        // Debug flags
        protected bool _tileGridEnabled = false;
        protected bool _pathDebugEnabled = false;

        ConsoleController _consoleController;

        protected bool _attackModeKeyPressed;
        protected bool _attackModeEnabled;
        protected bool _attackInitiated = false;

        protected bool _moveModeKeyPressed;
        protected bool _moveModeEnabled;
        protected bool _moveInitiated = false;

        int _doubleClickTime = 24;
        int _doubleClickCounter = 0;
        bool _doubleClickTimeExpired = true;
        bool _leftMouseDown = false;

        bool _selectAllPressed = false;

        bool _guiBeingUsed = false;


        // Tooltip
        Vector2 _lastMousePos;
        int _mouseHoverCounter = 0;
        int _mouseHoverTime = 80;


        /// <summary>
        /// Attack mode was changed by keyboard controls.
        /// </summary>
        public event UnitCommandModeEventHandler AttackModeChanged;
        
        /// <summary>
        /// Move mode was changed by keyboard controls.
        /// </summary>
        public event UnitCommandModeEventHandler MoveModeChanged;


        public event FactionsGameEventHandler OnMoveCommand;


        #region Public Properties
        public Collection<RTSActor> SelectedActors
        {
            get
            {
                return _selectedActors;
            }
        }

        /// <summary>
        /// Do not perform unit selection/move commands
        /// </summary>
        public bool GuiBeingUsed
        {
            get
            {
                return _guiBeingUsed; 
            }
            set
            {
                _guiBeingUsed = value;
            }
        }
        public bool MoveModeEnabled
        {
            get { return _moveModeEnabled; }
            set { _moveModeEnabled = value; }
        }
        public bool AttackModeEnabled
        {
            get { return _attackModeEnabled; }
            set { _attackModeEnabled = value; }
        }
        public bool CameraMoving
        {
            get
            {
                return _cameraBeingDragged;
            }
            set
            {
                _cameraBeingDragged = value;
            }
        }
        public bool TileGridEnabled
        {
            get
            {
                return _tileGridEnabled;
            }
            set
            {
                _tileGridEnabled = value;
            }
        }
        public bool PathDebugEnabled
        {
            get
            {
                return _pathDebugEnabled;
            }
            set
            {
                _pathDebugEnabled = value;

                // Reset colors if disabled!
                if (value == false)
                {
                    foreach (Tile t in Tiles)
                    {
                        t.TintColor = Color.White;
                    }
                }
            }
        }
        public PathMarshal PathMarshal
        {
            get
            {
                return _pathMarshal;
            }
        }
        public MiniMapPanel HeadsUpDisplay
        {
            get
            {
                return _miniMapPanel;
            }
        }
        public FactionsPlayer LocalPlayer
        {
            get
            {
                return _localPlayer;
            }
            set
            {
                _localPlayer = value;
            }
        }
        public TopInfoBar TopInfoBar
        {
            get { return _topInfoBar; }
        }
        public SelectionBar SelectionBar
        {
            get { return _selectionBar; }
        }
        #endregion



        #region Constructor
        public FactionsGame()
            : base()
        {
            this.Window.Title = "Warcraft Factions";
            _levelNames = new Collection<string>();

            this._graphics.PreferredBackBufferWidth = _gameSettings.ScreenWidth;
            this._graphics.PreferredBackBufferHeight = _gameSettings.ScreenHeight;
            this._graphics.IsFullScreen = _gameSettings.Fullscreen;
        }
        #endregion



        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            _mainMenu = new MainMenu(this);
            _mainMenu.ShowForm();
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

            // Setup controller for the console to be the FactionsGame controller
            _consoleController = new ConsoleController(this);
            _console.OnCommandEntered += new CommandEnteredHandler(_console_OnCommandEntered);


            // Initialize audio objects.
           
          
        }
        #endregion



        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Do the engine's scenegraph updates
            base.Update(gameTime);
            

            if (_gameStarted)
            {
                KeyboardState ks = Keyboard.GetState();

                ConsoleKeyUpdate();

                CameraMoveKeysUpdate();

                MenuEscapeKeyUpdate();


                // Do unit selection and mouse commands
                MouseRTSCommandsUpdate();

                // Update players
                if (_currentLevel != null)
                {
                    foreach (Player player in _players)
                    {
                        if (player != null)
                            player.Update(gameTime);
                    }
                }


                ToolTipHoverUpdate();


                // Update debug tile grid
                this.ShowTileDebugGrid(_tileGridEnabled);

                // Check for win condition
                CheckForWinOrLossCondition();
            }
        }
        #endregion



        void ToolTipHoverUpdate()
        {
            MouseState ms = Mouse.GetState();
            Vector2 mousePos = new Vector2(ms.X, ms.Y);
            if (_lastMousePos != mousePos)
            {
                _lastMousePos = mousePos;
                _mouseHoverCounter = 0;
            }
            _mouseHoverCounter++;
            if (_mouseHoverCounter >= _mouseHoverTime)
            {
                // Remain showing the tooltip
                _mouseHoverCounter = _mouseHoverTime;

                Vector2 absPos = AbsoluteCoordinates(mousePos);
                List<GameSceneNode> hoveredActors = ActorQuadTree.Query(new System.Drawing.RectangleF(absPos.X - 4, absPos.Y - 4, 8, 8));
                foreach (GameSceneNode node in hoveredActors)
                {
                    if (node is Actor)
                    {
                        Actor a = (Actor)node;

                        if (!_toolTip.Shown && !a.EditorVisibleOnly && a.Visible)
                        {
                            _toolTip.ActorTarget = a as RTSActor;
                            _toolTip.MainText = a.Name;

                            foreach (Player player in Players)
                            {
                                if (player != null && player.Team == a.Team && a.MaskColor == PlayerColors[player.Color - 1])
                                {
                                    _toolTip.SubText = player.Name + " (Team " + a.Team + ")";
                                    break;
                                }
                            }

                            //_toolTip.SubText = "Team " + a.Team.ToString();
                            _toolTip.ShowForm();
                        }
                    }
                }
            }
        }



        string _console_OnCommandEntered(string command)
        {
            return _consoleController.ParseCommand(command);
        }


        void SelectAllUnits()
        {
            DeselectAllUnits();

            bool unitPanelShown = false;

            foreach (Actor a in CurrentLevel.Actors)
            {
                if (a is RTSActor && IsOurs(a))
                {
                    // Select only movable units
                    RTSActor rtsActor = (RTSActor)a;
                    if (rtsActor.Movable && !rtsActor.IsDead)
                    {
                        SelectActor(a);
                        if (!unitPanelShown && !rtsActor.IsBuilding)
                        {
                            ShowCommandPanel(rtsActor);
                            unitPanelShown = true;
                        }
                    }
                }
            }
        }


        void SelectAllUnitsOfSameType(RTSActor target)
        {
            DeselectAllUnits();

            bool unitPanelShown = false;

            foreach (Actor a in CurrentLevel.Actors)
            {
                if (a is RTSActor && IsOurs(a) && a.Name == target.Name)
                {
                    // Select only movable units
                    RTSActor rtsActor = (RTSActor)a;
                    if (rtsActor.Movable && !rtsActor.IsDead)
                    {
                        SelectActor(a);
                        if (!unitPanelShown)
                        {
                            ShowCommandPanel(rtsActor);
                            unitPanelShown = true;
                        }
                    }
                }
            }
        }


        public void ShowCommandPanel(RTSActor actor)
        {
            if (actor.IsOurs)
            {
                if (actor is Headquarters && !_unitCommandPanel.Shown && !_droneCommandPanel.Shown)
                {
                    _hqCommandPanel.Headquarters = (Headquarters)actor;
                    _hqCommandPanel.ShowForm();
                    _selectionBar.Actor = actor;
                    _selectionBar.ShowForm();
                }
                else if (actor is Drone && !_unitCommandPanel.Shown && !_hqCommandPanel.Shown)
                {
                    _droneCommandPanel.Drone = (Drone)actor;
                    _droneCommandPanel.ShowForm();
                }
                else if (actor.Movable && !actor.IsDead && !actor.IsBuilding && !_hqCommandPanel.Shown && !_droneCommandPanel.Shown)
                {
                    _unitCommandPanel.ShowForm();
                }
            }
        }

        public void HideCommandPanel()
        {
            _unitCommandPanel.HideForm();
            _hqCommandPanel.HideForm();
            _droneCommandPanel.HideForm();
            _selectionBar.HideForm();
        }



        #region MouseRTSCommandsUpdate
        /// <summary>
        /// Unit selection by click, stretch selection box, and movement commands.
        /// </summary>
        protected void MouseRTSCommandsUpdate()
        {
            // Game controls - mouse
            if (_gameStarted)
            {
                KeyboardState ks = Keyboard.GetState();

                // Select all units (Ctrl+A)
                if (ks.IsKeyDown(Keys.LeftControl) && ks.IsKeyDown(Keys.A))
                {
                    if (!_selectAllPressed)
                    {
                        _selectAllPressed = true;
                        SelectAllUnits();
                    }
                }
                // Reset select all!
                else if (_selectAllPressed && ks.IsKeyUp(Keys.LeftControl) && ks.IsKeyUp(Keys.A))
                {
                    _selectAllPressed = false;
                }
                    // Attack move button press flag
                else if (ks.IsKeyDown(Keys.A) && !_selectAllPressed)
                {
                    _attackModeKeyPressed = true;
                }
                    // Attack move mode toggle
                else if (_attackModeKeyPressed && ks.IsKeyUp(Keys.A))
                {
                    _attackModeEnabled = !_attackModeEnabled;
                    _attackModeKeyPressed = false;

                    if (AttackModeChanged != null)
                        AttackModeChanged(_attackModeEnabled);
                }
                // Stop unit movements
                else if (ks.IsKeyDown(Keys.S))
                {
                    foreach (RTSActor actor in _selectedActors)
                    {
                        if (actor.Movable && !actor.IsDead)
                            actor.Stop();
                    }
                }
                    // Regular move button flag
                else if (ks.IsKeyDown(Keys.M))
                {
                    _moveModeKeyPressed = true;
                }
                    // Move mode toggle
                else if (_moveModeKeyPressed && ks.IsKeyUp(Keys.M))
                {
                    _moveModeEnabled = !_moveModeEnabled;
                    _moveModeKeyPressed = false;

                    if (MoveModeChanged != null)
                        MoveModeChanged(_moveModeEnabled);
                }

                

                if (_doubleClickCounter >= _doubleClickTime)
                    _doubleClickTimeExpired = true;
                else
                    _doubleClickCounter++;


                if (!_guiBeingUsed)
                {
                    // Unit move command
                    // Hideous unit selection & move code ahead!
                    MouseState ms = Mouse.GetState();
                    if (ms.RightButton == ButtonState.Pressed && !_cameraBeingDragged)
                        MoveUnits(ms);
                    else
                        _moveCommandIssued = false;


                    // Click selection or stretch selector box.
                    // Camera move with left shift.
                    if (ms.LeftButton == ButtonState.Pressed)
                    {
                        // Do drag move if shift held down
                        if (ks.IsKeyDown(Keys.LeftShift))
                            // Camera drag-move
                            MouseDragCamera(ms);
                        else if (_attackModeEnabled)
                        {
                            if (!_attackInitiated)
                            {
                                MoveUnits(ms);
                                _attackInitiated = true;
                            }
                        }
                        else if (_moveModeEnabled)
                        {
                            if (!_moveInitiated)
                            {
                                MoveUnits(ms);
                                _moveInitiated = true;
                            }
                        }
                        else // Else do click, and dragclick for unit selection
                        {
                            // Click selection only allowed if not dragging a selection stretch box
                            if (_stretchBox == null && _cameraBeingDragged == false)
                            {
                                bool actorSelected = false;
                                Actor a = ActorMouseSingleSelect(ms);
                                if (a != null && a is RTSActor)
                                {
                                    if (!_selectedActors.Contains(a as RTSActor))
                                    {
                                        if (ks.IsKeyUp(Keys.LeftControl))
                                            DeselectAllUnits();
                                        SelectActor(a);
                                        actorSelected = true;
                                    }

                                    // If we've had a mouse up since last mouse down
                                    if (!_leftMouseDown)
                                    {
                                        // If the double-click time hasn't expired and we're clicking the same actor we have selected
                                        if (!_doubleClickTimeExpired && _selectedActors.Count == 1 && _selectedActors[0] == a && a is RTSActor)
                                            SelectAllUnitsOfSameType(a as RTSActor);

                                        // Reset double click timer on mouse down - and only once
                                        _doubleClickCounter = 0;
                                        _doubleClickTimeExpired = false;
                                        _leftMouseDown = true;
                                    }

                                }
                                else
                                {
                                    // Deselect all
                                    if (ks.IsKeyUp(Keys.LeftControl))
                                        DeselectAllUnits();
                                }


                                if (!actorSelected) // We didn't hit an actor with a click. Make a stretch box
                                {
                                    // Stretch box selector!
                                    _stretchBox = new StretchBox(this, Mouse.GetState());
                                    _stretchBox.Initialize();
                                    this.EffectsSceneGraph.RootNode.Children.Add(_stretchBox);
                                    //this.SceneGraph.RootNode.Children.Add(_stretchBox);
                                    //this.ActorQuadTree.Insert(_stretchBox);
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        if (_leftMouseDown)
                            _leftMouseDown = false;

                        _cameraBeingDragged = false;

                        // Reset attack move on mouse left up to avoid deselection of just-moved group
                        if (_attackModeEnabled && _attackInitiated)
                        {
                            _attackModeEnabled = false;
                            _attackInitiated = false;

                            if (AttackModeChanged != null)
                                AttackModeChanged(_attackModeEnabled);
                        }

                        // ditto for Move
                        if (_moveModeEnabled && _moveInitiated)
                        {
                            _moveModeEnabled = false;
                            _moveInitiated = false;

                            if (MoveModeChanged != null)
                                MoveModeChanged(_moveModeEnabled);
                        }

                        if (_stretchBox != null)
                        {
                            // Stretch box released. Find all units that are selected!
                            Collection<Actor> newSelectedActors = new Collection<Actor>();
                            foreach (Actor a in _actors)
                            {
                                if (a.EditorVisibleOnly != true)
                                {
                                    // Create actor rectangle
                                    Rectangle actorRect = new Rectangle((int)a.AbsoluteTransform.X, (int)a.AbsoluteTransform.Y, (int)a.Size.X, (int)a.Size.Y);

                                    if (_stretchBox.DrawRect.Contains(actorRect))
                                    {
                                        // Only select movable RTS units with the drag!
                                        if (a is RTSActor)
                                        {
                                            RTSActor rtsActor = (RTSActor)a;
                                            // Only select our own, and movable
                                            if (IsOurs(a) && rtsActor.Movable)
                                                newSelectedActors.Add(a);
                                        }
                                    }
                                }
                            }


                            // If we have any, lose current selection
                            if (newSelectedActors.Count > 0)
                            {
                                DeselectAllUnits();

                                // Select all the units we found
                                foreach (Actor a in newSelectedActors)
                                {
                                    SelectActor(a);
                                }
                            }

                            // Remove it
                            this.EffectsSceneGraph.RemoveNode(_stretchBox);
                            _stretchBox.Dispose();
                            _stretchBox = null;
                        }
                    }
                }
            }
        }
        #endregion


        public Actor ActorMouseSingleSelect(MouseState ms)
        {
            // Find the actor and give him a selector box
            Vector2 absPoint = AbsoluteCoordinates(new Vector2(ms.X, ms.Y));
            List<GameSceneNode> actorHits = ActorQuadTree.Query(new System.Drawing.RectangleF(absPoint.X - 4, absPoint.Y - 4, 8, 8));
            Actor resultActor = null;
            foreach (GameSceneNode node in actorHits)
            {
                if (node is Actor)
                {
                    Actor a = (Actor)node;

                    if (a != null && a.EditorVisibleOnly != true) // disallow selecting editor actors!
                    {
                        resultActor = a;
                        break;
                    }
                }
            }

            return resultActor;
        }


        void MoveUnits(MouseState ms)
        {
            if (!_moveCommandIssued && _selectedActors.Count > 0)
            {
                if (OnMoveCommand != null)
                    OnMoveCommand();

                Collection<RTSActor> movedUnits = new Collection<RTSActor>();

                // Did we hit an enemy unit? If so, attack!
                Actor target = ActorMouseSingleSelect(ms);
                bool targetFound = false;
                if (target != null && target is RTSActor)
                {
                    RTSActor rtsActor = (RTSActor)target;
                    if (rtsActor.Team != LocalPlayer.Team && !rtsActor.IsDead)
                    {
                        targetFound = true;

                        // Make it flicker!

                        SelectionBox selBox = new SelectionBox(this);
                        selBox.Position = new Vector2(0, 0);
                        selBox.ParentActor = rtsActor;
                        selBox.OutlineColor = new Color(255, 20, 20, 150);
                        selBox.Initialize();
                        rtsActor.Children.Add(selBox);
                        selBox.Flicker();
                    }
                }


                // Move selected units
                Vector2 targetPos = new Vector2(ms.X, ms.Y);
                targetPos = GetTileGridPosition(targetPos);
                Tile targetTile = TileExistenceCheckByExactLocation(targetPos);
                GridReference targetTileGridRef = new GridReference(0, 0);
                if (targetTile != null)
                {
                    targetTileGridRef = targetTile.GridReference();
                    //targetTile.TintColor = Color.LightGreen;

                    // Get selected units
                    foreach (RTSActor unit in _selectedActors)
                    {
                        // If it's a controllable unit
                        if (IsOurs(unit) && unit.Movable == true && !unit.IsDead)
                        {
                            movedUnits.Add(unit);
                            if (targetFound)
                                unit.TargetActor = (RTSActor)target;
                        }
                    }

                    // Clear PathMarshal occupancy for our current grid square.
                    foreach (RTSActor rtsActor in movedUnits)
                    {
                        GridReference rtsActorPos = rtsActor.CurrentLocation;
                        _pathMarshal.OccupancyGrid[rtsActorPos.X, rtsActorPos.Y] = false;
                    }

                    // Get the required number of free adjacent nodes surrounding the target square.
                    if (movedUnits.Count > 0)
                    {
                        Collection<GridReference> validTargetGridRefs = GetFreeNodes(targetTileGridRef, movedUnits.Count);

                        // Apply move & pathing.
                        for (int i = 0; i < validTargetGridRefs.Count; i++)
                        {
                            movedUnits[i].MoveToGridLocation(validTargetGridRefs[i], _attackModeEnabled);

                            if (_pathDebugEnabled)
                            {
                                Tile tintTile = TileExistenceCheckByGridRef(validTargetGridRefs[i]);
                                tintTile.TintColor = Color.MediumBlue;
                            }
                        }
                        _moveCommandIssued = true;
                    }
                }
            }
        }


        /// <summary>
        /// Hackish handler for click-drag on minimap
        /// Destroy stretchbox if created by click-drag on minimap area
        /// </summary>
        /// <param name="dragging"></param>
        void MiniMap_OnMiniMapDrag(bool dragging)
        {
            _cameraBeingDragged = dragging;
            if (_stretchBox != null)
            {
                //this.SceneGraph.RemoveNode(_stretchBox);
                //ActorQuadTree.Remove(_stretchBox);
                this.EffectsSceneGraph.RemoveNode(_stretchBox);
                _stretchBox.Dispose();
                _stretchBox = null;
            }
        }


        /// <summary>
        /// Is an actor on our team and our color?
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        bool IsOurs(Actor a)
        {
            if (a.Team == LocalPlayer.Team && a.MaskColor == PlayerColors[LocalPlayer.Color - 1])
                return true;
            return false;
        }



        #region ConsoleKeyUpdate
        /// <summary>
        /// Tilde key to show/hide the console.
        /// </summary>
        protected void ConsoleKeyUpdate()
        {
            // Show/hide console
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.OemTilde) && !_consoleButtonDown)
            {
                _consoleButtonDown = true;
                if (_console.Visible == false)
                {
                    //_gameState = GameState.Paused;
                    _console.Show();
                }
                else if (_console.Visible == true)
                {
                    //_gameState = GameState.Running;
                    _console.Hide();
                }
            }
            else if (ks.IsKeyUp(Keys.OemTilde))
            {
                _consoleButtonDown = false;
            }
        }
        #endregion



        #region CameraMoveKeysUpdate
        /// <summary>
        /// Camera control with arrow keys.
        /// </summary>
        protected void CameraMoveKeysUpdate()
        {
            KeyboardState ks = Keyboard.GetState();
            if (_gameState == GameState.Running)
            {
                // Camera movement via keys
                float cameraKeyMoveValue = 6f;
                if (ks.IsKeyDown(Keys.Left))
                {
                    _cameraBeingDragged = true;
                    this.MoveCameraBy(cameraKeyMoveValue, 0);
                }
                else if (ks.IsKeyDown(Keys.Right))
                {
                    _cameraBeingDragged = true;
                    this.MoveCameraBy(-cameraKeyMoveValue, 0);
                }
                else if (ks.IsKeyDown(Keys.Up))
                {
                    _cameraBeingDragged = true;
                    this.MoveCameraBy(0, cameraKeyMoveValue);
                }
                else if (ks.IsKeyDown(Keys.Down))
                {
                    _cameraBeingDragged = true;
                    this.MoveCameraBy(0, -cameraKeyMoveValue);
                }
            }
        }
        #endregion



        #region MenuEscapeKeyUpdate
        /// <summary>
        /// Access main menu from in game using escape key.
        /// </summary>
        protected void MenuEscapeKeyUpdate()
        {
            // Access menu from in-game
            if (_gameStarted)
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.Escape) && !_menuButtonDown)
                {
                    _menuButtonDown = true;
                    if (_gameState == GameState.Running)
                    {
                        _gameState = GameState.Paused;
                        _inGameMenu.ShowForm();
                    }
                    else if (_gameState == GameState.Paused)
                    {
                        _gameState = GameState.Running;
                        _inGameMenu.HideForm();
                    }
                }
                else if (ks.IsKeyUp(Keys.Escape))
                {
                    _menuButtonDown = false;
                }
            }
        }
        #endregion



        #region AddTemplateActor
        /// <summary>
        /// Override of the engine's template actor list add method.
        /// Initialize specialized versions of actors as the template objects so dynamically-created actors
        /// (cloned from the template) are of the proper type and execute inherited behavior accordingly.
        /// </summary>
        /// <param name="actor"></param>
        public override void AddTemplateActor(Actor actor)
        {
            if (actor.Name == "Barracks")
            {
                Barracks newActor = new Barracks(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "Drone")
            {
                Drone newActor = new Drone(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "DroneTurret")
            {
                DroneTurret newActor = new DroneTurret(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "BloodSplat1")
            {
                BloodSplat1 newActor = new BloodSplat1(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "BulletImpact")
            {
                BulletImpact newActor = new BulletImpact(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "TurretGun")
            {
                TurretGun newActor = new TurretGun(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "Turret")
            {
                Turret newActor = new Turret(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "SmallTank")
            {
                SmallTank newActor = new SmallTank(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "SmallTankTurret")
            {
                SmallTankTurret newActor = new SmallTankTurret(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "PlayerStart")
            {
                PlayerStart newActor = new PlayerStart(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                //newActor.Team = PlayerColors[actor.Team - 1];
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "Headquarters")
            {
                Headquarters newActor = new Headquarters(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                //newActor.Team = PlayerColors[actor.Team - 1];
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "Pillbox")
            {
                Pillbox newActor = new Pillbox(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                //newActor.Team = PlayerColors[actor.Team - 1];
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "EnergyExplosion")
            {
                EnergyExplosion newActor = new EnergyExplosion(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                //newActor.Team = PlayerColors[actor.Team - 1];
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "Peasant")
            {
                Peasant newActor = new Peasant(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }

            if (actor.Name == "Projectile")
            {
                Projectile newActor = new Projectile(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                //newActor.Team = PlayerColors[actor.Team - 1];
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "Soldier")
            {
                Soldier newActor = new Soldier(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "GemRockHuge")
            {
                GemRockHuge newActor = new GemRockHuge(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "GemRockLarge")
            {
                GemRockLarge newActor = new GemRockLarge(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }
            if (actor.Name == "GemRockSmall")
            {
                GemRockSmall newActor = new GemRockSmall(this);
                newActor.Scale = actor.Scale;
                newActor.Size = actor.Size;
                newActor.Sprites = actor.Sprites;
                _actorTemplates.Add(newActor);
            }

            
            //if (actor.Name == "MiniMapStartPoint")
            //{
            //    MiniMapStartPoint newActor = new MiniMapStartPoint(this);
            //    newActor.Scale = actor.Scale;
            //    newActor.Size = actor.Size;
            //    newActor.Sprites = actor.Sprites;
            //    actorTemplates.Add(newActor);
            //}
            else
            {
                base.AddTemplateActor(actor);
            }
        }
        #endregion



        #region MouseDragCamera
        /// <summary>
        /// Dragging on the xna window - move the camera accordingly.
        /// Enable/disable mouseDragging on a key press (Mouse2 for example) and call this method in Update to perform mouse camera dragging.
        /// </summary>
        /// <param name="ms"></param>
        void MouseDragCamera(XnaInput.MouseState ms)
        {
            Vector2 pos = new Vector2(ms.X, ms.Y);
            // Establish a start point and move by relative amounts.
            if (!_cameraBeingDragged)
            {
                _cameraMoveAnchor = new Vector2(pos.X, pos.Y);
                _cameraBeingDragged = true;
            }

            // Get the distance the mouse has moved from the anchor
            Vector2 moveOffset = new Vector2(_cameraMoveAnchor.X - pos.X, _cameraMoveAnchor.Y - pos.Y);

            // Set this as the new anchor
            _cameraMoveAnchor = pos;

            // Scale the camera move
            Vector2 scaledOffset = new Vector2(moveOffset.X * _cameraMoveScale, moveOffset.Y * _cameraMoveScale);
            MoveCameraBy(-scaledOffset.X, -scaledOffset.Y);
        }
        #endregion



        #region DeselectAllUnits
        /// <summary>
        /// Remove all selection boxes.
        /// </summary>
        public void DeselectAllUnits()
        {
            foreach (RTSActor rtsActor in _selectedActors)
            {
                if (rtsActor.SelectionBox != null)
                {
                    this.SceneGraph.RemoveNode(rtsActor.SelectionBox);
                    rtsActor.Children.Remove(rtsActor.SelectionBox);
                    //rtsActor.SelectionBox.Dispose();
                    rtsActor.SelectionBox = null;
                }

                if (rtsActor.HealthBar != null)
                {
                    this.SceneGraph.RemoveNode(rtsActor.HealthBar);
                    rtsActor.Children.Remove(rtsActor.HealthBar);
                    //rtsActor.HealthBar.Dispose();
                    rtsActor.HealthBar = null;
                }
            }
            _selectedActors.Clear();

            HideCommandPanel();
        }
        #endregion



        #region NewSkirmishGame
        /// <summary>
        /// Create a new skirmish game from the supplied Match object.
        /// Load the level and perform starting point/unit allocation.
        /// Initialize the PathMarshal for actor pathfinding.
        /// Create a new heads-up display.
        /// </summary>
        /// <param name="match"></param>
        public void NewSkirmishGame(Match match)
        {
            // Load the level
            _levels.Clear();
            string mapDirectory = Path.Combine(Content.RootDirectory, "levels");
            string mapFile = Path.Combine(mapDirectory, match.LevelFile);
            _currentLevel = EngineIO.LoadLevelFromXml(mapFile);


            // Add our player list to the current game
            _players = match.Players;

            // Order of players determines starting point allocation
            int startPoint = 0;
            foreach (Player pl in _players)
            {
                if (pl != null)
                {
                    // Match with starting point list.
                    // There are at least as many start points as players.
                    if (startPoint < _currentLevel.StartPoints.Count)
                    {
                        Actor startPointActor = _currentLevel.StartPoints[startPoint];
                        int team = startPointActor.Team;

                        foreach (Actor a in _actors)
                        {
                            if (a.Team == team)
                            {
                                if (a is RTSActor)
                                {
                                    RTSActor rtsActor = (RTSActor)a;
                                    rtsActor.Team = pl.Team;
                                    int teamColorIndex = pl.Color - 1;
                                    rtsActor.MaskColor = PlayerColors[teamColorIndex];
                                    rtsActor.MaskHueValue = PlayerColorHueMask;
                                    rtsActor.MaskColorEnabled = true;
                                    rtsActor.ApplyColorMaskToSprites();
                                }
                                else if (a.Name == "PlayerStart")
                                {
                                    _currentLevel.StartPoints[startPoint].Team = pl.Team;
                                    a.MaskColor = PlayerColors[pl.Color - 1];
                                }
                            }
                        }
                    }
                }
                startPoint++;


            }


            // Find all actors not assigned
            Collection<int> invalidPlayers = new Collection<int>();
            for (int i = 0; i < 12; i++)
            {
                bool playerFound = false;
                foreach (Player pl in _players)
                {
                    if (pl != null && pl.Team == i + 1)
                    {
                        playerFound = true;
                        break;

                    }
                }
                if (!playerFound)
                    invalidPlayers.Add(i + 1);
            }

            // Remove all start points not assigned
            foreach (int invalidPlayer in invalidPlayers)
            {
                for (int startPointIndex = 0; startPointIndex < _currentLevel.StartPoints.Count; startPointIndex++)
                {
                    if (_currentLevel.StartPoints[startPointIndex].Team == invalidPlayer)
                    {
                        _currentLevel.StartPoints.Remove(_currentLevel.StartPoints[startPointIndex]);
                        startPointIndex--;
                    }
                }
            }

            // Remove all actors not assigned to a team
            for (int i = 0; i < _currentLevel.Actors.Count; i++)
            {
                if (invalidPlayers.Contains(_currentLevel.Actors[i].Team))
                {
                    this.SceneGraph.RemoveNode(_currentLevel.Actors[i]);
                    Actors.Remove(_currentLevel.Actors[i]);

                    if (_currentLevel.Actors[i].PhysicallySimulated)
                        _currentLevel.Actors[i].Body.Dispose();
                    
                    _currentLevel.Actors[i].Dispose();
                    _currentLevel.Actors.Remove(_currentLevel.Actors[i]);
                    i--;
                }
            }


            if (_inGameMenu == null)
            {
                _inGameMenu = new InGameMenu(this);
            }


            // Start it up!
            RunLevel(_currentLevel);

            // Create our path marshal for the currently loaded tile grid
            _pathMarshal = new PathMarshal(this);
            _pathMarshal.Initialize();

            // Initialize the players
            foreach (Player player in _players)
            {
                if (player != null)
                {
                    player.Initialize();
                }
            }

            // Enable alternate tile colorings
            this.ShowTileDebugGrid(_tileGridEnabled);


            // Create GUI items
            _miniMapPanel = new MiniMapPanel(this);
            _miniMapPanel.Initialize();
            _miniMapPanel.ShowForm();
            _miniMapPanel.Map.OnMiniMapDrag += new OnMiniMapDragHandler(MiniMap_OnMiniMapDrag);

            _topInfoBar = new TopInfoBar(this);
            _topInfoBar.Initialize();
            _topInfoBar.ShowForm();

            _unitCommandPanel = new UnitCommandPanel(this);
            _unitCommandPanel.Initialize();

            _hqCommandPanel = new HQCommandPanel(this, null);
            _hqCommandPanel.Initialize();

            _droneCommandPanel = new DroneCommandPanel(this, null);
            _droneCommandPanel.Initialize();

            _toolTip = new ToolTip(this);
            _toolTip.Initialize();

            _selectionBar = new SelectionBar(this);
            _selectionBar.Initialize();

            _gameStarted = true;
        }
        #endregion



        #region GetFreeAdjacentNodes
        /// <summary>
        /// Get all the free adjacent nodes (i.e. non-solid and not occupied) surrounding this target grid ref.
        /// </summary>
        /// <param name="targetTileGridRef">The grid ref to search from</param>
        /// <param name="nodeCount">The number of free grid squares required</param>
        /// <returns>Collection of free adjacent nodes.</returns>
        public Collection<GridReference> GetFreeAdjacentNodes(GridReference targetTileGridRef, int nodeCount)
        {
            int occupiedCount = 0;
            return GetFreeAdjacentNodes(targetTileGridRef, nodeCount, ref occupiedCount);
        }

        /// <summary>
        /// Get all the free adjacent nodes (i.e. non-solid and not occupied) surrounding this target grid ref.
        /// </summary>
        /// <param name="targetTileGridRef">The grid ref to search from</param>
        /// <param name="nodeCount">The number of free grid squares required</param>
        /// <param name="searchDepth">Reference value to return number of nodes that are occupied.</param>
        /// <returns>Collection of free adjacent nodes.</returns>
        public Collection<GridReference> GetFreeAdjacentNodes(GridReference targetTileGridRef, int nodeCount, ref int occupiedCount)
        {
            occupiedCount = 0;
            Collection<GridReference> validTargetGridRefs = new Collection<GridReference>();
            Tile targetTile = TileExistenceCheckByGridRef(targetTileGridRef);
            if (targetTile != null)
            {
                if (targetTile.Solid == false && PathMarshal.OccupancyGrid[targetTileGridRef.X, targetTileGridRef.Y] == false)
                    validTargetGridRefs.Add(targetTileGridRef);
                else
                    occupiedCount++;

                if (nodeCount > 1 || occupiedCount == 1)
                {
                    // Calculate target tile grid!
                    // We must convert a single specified tile into a set of target tiles as separate
                    // targets for each selected actor.
                    // Start by asking for adjacent nodes

                    // Get the initial batch of valid squares
                    Collection<GridReference> adjacentTargetGridRefs = targetTile.AdjacentGridReferences();


                    foreach (GridReference adjacentGridRef in adjacentTargetGridRefs)
                    {
                        // Check for solidity
                        Tile adjacentTile = TileExistenceCheckByGridRef(adjacentGridRef);
                        if (adjacentTile != null && adjacentTile.Solid == false &&
                            PathMarshal.OccupancyGrid[adjacentGridRef.X, adjacentGridRef.Y] == false)
                        {
                            validTargetGridRefs.Add(adjacentGridRef);

                            if (validTargetGridRefs.Count >= nodeCount)
                                break;

                            //if (PathDebugEnabled)
                            //    adjacentTile.TintColor = Color.MediumBlue;
                        }
                        else
                            occupiedCount++;
                    }


                    adjacentTargetGridRefs.Clear();


                    // Keep getting valid adjacent squares until we have enough to move all our units.
                    int i = 0;
                    while (validTargetGridRefs.Count < nodeCount && i < validTargetGridRefs.Count)
                    {
                        GridReference currentValidRef = validTargetGridRefs[i];
                        //validTargetGridRefs.Remove(currentValidRef);

                        Tile validTile = TileExistenceCheckByGridRef(currentValidRef);





                        adjacentTargetGridRefs = validTile.AdjacentGridReferences();
                        //int adjacentIndex = 0;
                        foreach (GridReference adjacentGridRef in adjacentTargetGridRefs)
                        {
                            // Exclude grid references in valid tile list
                            bool alreadyUsed = false;
                            foreach (GridReference validRef in validTargetGridRefs)
                            {
                                if (validRef.X == adjacentGridRef.X && validRef.Y == adjacentGridRef.Y)
                                {
                                    alreadyUsed = true;
                                    break;
                                }
                            }

                            Tile adjacentTile = TileExistenceCheckByGridRef(adjacentGridRef);
                            if (adjacentTile != null && adjacentTile.Solid == false &&
                                !alreadyUsed &&
                                PathMarshal.OccupancyGrid[adjacentGridRef.X, adjacentGridRef.Y] == false)
                            {
                                validTargetGridRefs.Insert(validTargetGridRefs.Count - 1, adjacentGridRef);

                                //if (PathDebugEnabled)
                                //    adjacentTile.TintColor = Color.MediumBlue;
                                //adjacentIndex++;

                                if (validTargetGridRefs.Count >= nodeCount)
                                    break;
                            }
                            else if (adjacentTile.Solid == true ||
                                PathMarshal.OccupancyGrid[adjacentGridRef.X, adjacentGridRef.Y] == true)
                                occupiedCount++;
                        }
                        i++;
                    }
                }
            }
            return validTargetGridRefs;
        }
        #endregion



        public Collection<GridReference> GetFreeNodes(GridReference gridRef, int nodeCount)
        {
            int occupied = 0;
            Collection<GridReference> nodes = GetFreeAdjacentNodesRecursive(gridRef, new Collection<GridReference>(), new Collection<GridReference>(), 0, nodeCount, ref occupied);
            return nodes;
        }

        public Collection<GridReference> GetFreeNodes(GridReference gridRef, int nodeCount, out int occupiedCount)
        {
            int occupied = 0;
            Collection<GridReference> nodes = GetFreeAdjacentNodesRecursive(gridRef, new Collection<GridReference>(), new Collection<GridReference>(), 0, nodeCount, ref occupied);
            occupiedCount = occupied;
            return nodes;
        }


        protected Collection<GridReference> GetFreeAdjacentNodesRecursive(GridReference reference, Collection<GridReference> validGridRefs, Collection<GridReference> visitedGridRefs, int visitedIndex, int nodeCount, ref int occupied)
        {
            Tile targetTile = TileExistenceCheckByGridRef(reference);
            if (targetTile != null)
            {
                //// Initial reference gets a special check (if it's not already in the visited list)
                if (!visitedGridRefs.Contains(reference))
                {
                    if (targetTile != null && targetTile.Solid == false &&
                        PathMarshal.OccupancyGrid[reference.X, reference.Y] == false)
                    {
                        validGridRefs.Add(reference);
                        visitedGridRefs.Add(reference);

                        if (validGridRefs.Count == nodeCount)
                        {
                            return validGridRefs;
                        }
                    }
                    else
                        occupied++;
                }


                // Check our adjacent tiles for occupancy
                Collection<GridReference> adjacentTargetGridRefs = targetTile.AdjacentGridReferences();
                //adjacentTargetGridRefs.Add(reference);
                foreach (GridReference gridRef in adjacentTargetGridRefs)
                {
                    if (!visitedGridRefs.Contains(gridRef))
                    {
                        visitedGridRefs.Add(gridRef);

                        Tile adjacentTile = TileExistenceCheckByGridRef(gridRef);
                        if (adjacentTile != null && adjacentTile.Solid == false &&
                            PathMarshal.OccupancyGrid[gridRef.X, gridRef.Y] == false)
                        {
                            validGridRefs.Add(gridRef);

                            if (validGridRefs.Count == nodeCount)
                                break;
                        }
                        else
                            occupied++;
                    }
                }


                if (validGridRefs.Count < nodeCount)
                {
                    GridReference queuedGridRef = visitedGridRefs[visitedIndex];
                    visitedIndex++;
                    //visitedGridRefs.Remove(queuedGridRef);

                    validGridRefs = GetFreeAdjacentNodesRecursive(queuedGridRef, validGridRefs, visitedGridRefs, visitedIndex, nodeCount, ref occupied);
                }
            }
            return validGridRefs;
        }







        #region SelectActor
        /// <summary>
        /// Select the current RTS actor.
        /// Give it a selection box and a health bar.
        /// Add it to the selected list.
        /// </summary>
        /// <param name="actor"></param>
        public void SelectActor(Actor actor)
        {
            if (actor is RTSActor)
            {
                RTSActor rtsActor = (RTSActor)actor;
                if (rtsActor.Selectable && !_selectedActors.Contains(rtsActor))
                {
                    SelectionBox selBox = new SelectionBox(this);
                    selBox.Position = new Vector2(0, 0);
                    selBox.ParentActor = actor;
                    if (!IsOurs(rtsActor))
                        selBox.OutlineColor = new Color(255, 20, 20, 150);
                    selBox.Initialize();
                    actor.Children.Add(selBox);

                    // Give it a health bar!
                    HealthBar healthBar = new HealthBar(this);
                    healthBar.ParentActor = (RTSActor)actor;
                    if (!IsOurs(rtsActor))
                        healthBar.OutlineColor = Color.White;
                    healthBar.Initialize();
                    actor.Children.Add(healthBar);

                    rtsActor.HealthBar = healthBar;
                    rtsActor.SelectionBox = selBox;

                    _selectedActors.Add(rtsActor);

                    ShowCommandPanel(rtsActor);
                }
            }
        }
        #endregion




        private void CheckForWinOrLossCondition()
        {
            // Perform a count of team unit numbers
            // If our own team has no units, show loss screen
            // If all enemy units dead, show win

            // Get a list of all team numbers
            List<int> playerTeams = new List<int>();
            foreach (Player p in Players)
            {
                if (p != null)
                {
                    if (!playerTeams.Contains(p.Team))
                    {
                        playerTeams.Add(p.Team);
                    }
                }
            }

            int[] teamUnitCounts = new int[playerTeams.Count];


            // Count units that aren't dead or invisible, for each team
            foreach (Actor actor in Actors)
            {
                if (actor is RTSActor)
                {        
                    RTSActor rtsActor = (RTSActor)actor;
                    if (!rtsActor.IsDead && !rtsActor.EditorVisibleOnly)
                    {
                        int actorTeamIndex = playerTeams.IndexOf(actor.Team);
                        teamUnitCounts[actorTeamIndex]++;
                    }
                    
                }
            }


            // If our own team has no units/buildings, show game over
            int localPlayerTeamIndex = playerTeams.IndexOf(_localPlayer.Team);
            if (teamUnitCounts[localPlayerTeamIndex] == 0)
            {
                // Loss!
                _gameStarted = false;
                GameEndDialog gameEndDialog = new GameEndDialog(this, false);
                gameEndDialog.OnFormHide += new OnFormHideHandler(gameEndDialog_OnFormHide);
                gameEndDialog.ShowForm();
            }
            else
            {
                // Check if all other teams have no units
                int totalEnemyUnitCount = 0;
                for (int i = 0; i < playerTeams.Count; i++)
                {
                    if (i != localPlayerTeamIndex)
                    {
                        totalEnemyUnitCount += teamUnitCounts[i];
                    }
                }

                if (totalEnemyUnitCount == 0)
                {
                    // Win!
                    _gameStarted = false;
                    GameEndDialog gameEndDialog = new GameEndDialog(this, true);
                    gameEndDialog.OnFormHide += new OnFormHideHandler(gameEndDialog_OnFormHide);
                    gameEndDialog.ShowForm();
                }
            }
        }

        void gameEndDialog_OnFormHide(object sender)
        {
            ReturnToMenu();
        }



        /// <summary>
        /// Tell all currently selected units to stop.
        /// </summary>
        public void StopAllSelectedUnits()
        {
            foreach (RTSActor a in _selectedActors)
            {
                if (IsOurs(a))
                {
                    a.Stop();
                }
            }
        }



        public void ReturnToMenu()
        {
            _gameStarted = false;
            EndLevel();
            _mainMenu.ShowForm();
        }


    }
}
