using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing; // for converting PNG to Texture2D - must be a better way for dynamically generated content.
using Color = Microsoft.Xna.Framework.Color;

namespace FactionsGame
{
    /// <summary>
    /// New skirmish match.
    /// Select level, team, and other map options.
    /// Add computer opponents.
    /// </summary>
    public class SkirmishMenu : DForm
    {
        // Engine and settings
        protected FactionsGame engine;
        protected FactionsGameSettings gameSettings;

        // OK and Cancel buttons
        protected DButton okButton = null;
        protected DButton closeButton = null;

        // Maps panel, label and listbox
        protected DPanel pnlMaps;
        protected DText lblMaps;
        protected DListBox lstMapList;

        // Players panel, label, and column labels
        protected DPanel pnlPlayers;
        protected DText lblPlayerList;
        protected DText lblPlayerNameHeader;
        protected DText lblPlayerTeamHeader;
        protected DText lblPlayerColorHeader;

        // Player list row objects
        protected Collection<SkirmishMenuPlayerRow> playerRows = new Collection<SkirmishMenuPlayerRow>();

        // Minimap panel, label and image
        protected DPanel pnlMiniMap;
        protected DText lblMiniMap;
        protected DImage mapImage;
        protected DText lblMapTitle;

        // Minimap start point labels and info
        protected Collection<PlayerStartPoint> playerStartPoints = new Collection<PlayerStartPoint>();
        protected Collection<DText> playerStartTexts = new Collection<DText>();

        // Map options panel and label
        protected DText lblMapOptions;
        protected DPanel pnlMapOptions;
        protected DLayoutFlow mapOptionsLayout;
        protected DText lblStartPointAllocation;
        protected DComboBox cmbStartPointAllocation;
        protected DText lblStartingResources;
        protected DComboBox cmbStartingResources;
        protected DText lblWinCondition;
        protected DComboBox cmbWinCondition;
        

        // Status bar for skirmish screen
        protected DText lblStatus;

        // Current level name and loading screen
        protected string levelFile = null;
        protected LoadingScreen loadScreen;

        // Layout control objects
        protected DLayoutFlow leftColumnLayout;
        protected DLayoutFlow rightColumnLayout;

        // GUI colors
        protected DGuiColorTheme _colorTheme = new DGuiColorTheme();
        //protected Color fillColor = new Color(255, 252, 214);
        //protected Color clickedFillColor = new Color(79, 146, 255);
        //protected Color borderColor = new Color(20, 20, 20);
        //protected Color hoverFillColor = new Color(255, 240, 168);
        //protected Color panelFillColor = Color.White;

        // Geometry
        protected float panelYPosition = 0f;
        protected int panelHeight = 0;
        protected int panelWidthMax = 0;
        protected int buttonWidth = 130;
        protected int buttonHeight = 35;
        protected int panelPadding = 10;


        #region Public Properties
        public Collection<SkirmishMenuPlayerRow> PlayerRows
        {
            get
            {
                return playerRows;
            }
        }
        #endregion



        #region Constructor
        public SkirmishMenu(FactionsGame game, DForm parent)
            : base(game.GuiManager, "SkirmishMenu", parent)
        {
            engine = game;
            Name = "SkirmishMenu";
        }
        #endregion



        #region ShowForm
        /// <summary>
        /// Create the form objects and add them to the scenegraph.
        /// </summary>
        public override void ShowForm()
        {
            this.Visible = false;

            // Get center screen coords
            Vector2 centerScreen = new Vector2(engine.Window.ClientBounds.Width / 2f,
                engine.Window.ClientBounds.Height / 2f);

            // Make the form span the whole screen and make it partially invisible
            this.Size = new Vector2(engine.Window.ClientBounds.Width * 0.95f, engine.Window.ClientBounds.Height * 0.85f);
            this.Position = new Vector2((engine.Window.ClientBounds.Width - this.Size.X) / 2f,
                (engine.Window.ClientBounds.Height - this.Size.Y) / 2f);
            this.FillColor = Color.GhostWhite;
            this.BorderColor = Color.Black;
            this.BorderWidth = 1;
            this.Alpha = 200;
            this.Initialize();

            // Figure out max panel width from this
            panelWidthMax = (int)this.Size.Y - (2 * panelPadding);


            // Status label
            lblStatus = new DText(engine.GuiManager);
            lblStatus.Position = new Vector2(panelPadding, this.Size.Y - (2 * panelPadding));
            lblStatus.FontColor = Color.Crimson;
            lblStatus.Initialize();
            this.AddPanel(lblStatus);


            // Close without saving
            closeButton = new DButton(engine.GuiManager,
                this.Size.X - (buttonWidth + panelPadding),
                this.Size.Y - (buttonHeight + panelPadding), "Close", buttonWidth, buttonHeight);
            closeButton.ColorTheme = _colorTheme;
            closeButton.Initialize();
            closeButton.OnClick += new DButtonEventHandler(closeButton_OnClick);
            this.AddPanel(closeButton);

            // OK button
            okButton = new DButton(engine.GuiManager,
                this.Size.X - (2 * (buttonWidth + panelPadding)),
                this.Size.Y - (buttonHeight + panelPadding), "OK", buttonWidth, buttonHeight);
            okButton.ColorTheme = _colorTheme;
            okButton.Initialize();
            okButton.OnClick += new DButtonEventHandler(okButton_OnClick);
            this.AddPanel(okButton);

            // Establish position and height values for panels
            //panelYPosition = this.Position.Y + (2 * panelPadding) + buttonHeight;
            

            // Load the panels and their sub-components!
            LoadMapList();
            LoadMiniMapPanel();
            LoadMapOptions();
            LoadPlayerList();


            engine.StaticSceneGraph.RootNode.Children.Add(this);

            SetupPlayerList();

            UpdateMiniMapStartLabels();

            this.Visible = true;
        }
        #endregion



        #region HideForm
        /// <summary>
        /// Remove all menu items from the scenegraph.
        /// </summary>
        public override void HideForm()
        {
            foreach (SkirmishMenuPlayerRow playerRow in playerRows)
            {
                this.Children.Remove(playerRow);
                //engine.StaticSceneGraph.RemoveNode(playerRow);
                playerRow.Dispose();
            }
            playerRows.Clear();

            //engine.StaticSceneGraph.RemoveNode(okButton);
            //engine.StaticSceneGraph.RemoveNode(closeButton);

            this.Children.Remove(okButton);
            this.Children.Remove(closeButton);


            this.Children.Remove(pnlMaps);
            this.Children.Remove(pnlPlayers);
            this.Children.Remove(pnlMiniMap);
            this.Children.Remove(pnlMapOptions);

            this.Children.Remove(lblMaps);
            this.Children.Remove(lblPlayerList);
            this.Children.Remove(lblMiniMap);
            this.Children.Remove(lblMapOptions);

            this.Children.Remove(lstMapList);
            this.Children.Remove(mapImage);
            foreach (DText text in playerStartTexts)
            {
                this.Children.Remove(text);
                text.Dispose();
            }
            playerStartTexts.Clear();

            this.Children.Remove(lblMapTitle);
            this.Children.Remove(lblPlayerNameHeader);
            this.Children.Remove(lblPlayerTeamHeader);
            this.Children.Remove(lblPlayerColorHeader);

            this.Children.Remove(lblStartPointAllocation);
            this.Children.Remove(cmbStartPointAllocation);
            this.Children.Remove(lblStartingResources);
            this.Children.Remove(cmbStartingResources);
            this.Children.Remove(lblWinCondition);
            this.Children.Remove(cmbWinCondition);

            engine.StaticSceneGraph.RemoveNode(this);


            okButton.Dispose();
            closeButton.Dispose();


            pnlMaps.Dispose();
            pnlPlayers.Dispose();
            pnlMiniMap.Dispose();
            pnlMapOptions.Dispose();

            lblMaps.Dispose();
            lblPlayerList.Dispose();
            lblMiniMap.Dispose();
            lblMapOptions.Dispose();


            lstMapList.Dispose();

            mapImage.Dispose();

            lblMapTitle.Dispose();
            lblPlayerNameHeader.Dispose();
            lblPlayerTeamHeader.Dispose();
            lblPlayerColorHeader.Dispose();


            lblStartPointAllocation.Dispose();
            cmbStartPointAllocation.Dispose();
            lblStartingResources.Dispose();
            cmbStartingResources.Dispose();
            lblWinCondition.Dispose();
            cmbWinCondition.Dispose();

            //this.Dispose();
        }
        #endregion



        #region UpdateMiniMapStartLabels
        /// <summary>
        /// Update the color of the minimap start point position labels.
        /// </summary>
        protected void UpdateMiniMapStartLabels()
        {
            // Update minimap's position colors
            for (int i = 0; i < playerRows.Count; i++)
            {
                if (playerRows[i].Player.Name != null && playerRows[i].Player.Name != "Open")
                {
                    if (i < playerStartTexts.Count)
                        playerStartTexts[i].FontColor = engine.PlayerColors[playerRows[i].Player.Color - 1];
                }
            }
        }
        #endregion



        #region SetupPlayerList
        /// <summary>
        /// Setup a list of player slots for this map.
        /// </summary>
        protected void SetupPlayerList()
        {
            // Clear first
            foreach (SkirmishMenuPlayerRow pRow in playerRows)
            {
                this.Children.Remove(pRow);
                pRow.Dispose();
            }
            playerRows.Clear();
            //playerStartPoints.Clear();
            //playerStartTexts.Clear();
            SkirmishMenuPlayerRow.PlayerNum = 2;
            rightColumnLayout.Clear();

            // Add ourselves to the player list first.
            gameSettings = new FactionsGameSettings();
            SkirmishMenuPlayerRow playerRow = new SkirmishMenuPlayerRow(engine);
            playerRow.Player.Name = gameSettings.PlayerName;
            playerRow.FillColor = Color.CornflowerBlue;
            playerRow.Player.Team = gameSettings.Team;
            playerRow.Player.Color = gameSettings.PlayerColor;
            playerRow.SkirmishMenu = this;
            playerRow.Unkickable = true;
            playerRow.ParentPanel = pnlPlayers;
            rightColumnLayout.Add(playerRow);
            playerRows.Add(playerRow);
            this.AddPanel(playerRow);
            playerRow.OnPlayerNameChange += new PlayerNameChangeEventHandler(playerRow_OnPlayerNameChange);
            playerRow.OnPlayerColorChange += new PlayerColorChangeEventHandler(playerRow_OnPlayerColorChange);

            bool defaultAIAdded = false;
            if (levelFile != null)
            {
                for (int i = 1; i < playerStartPoints.Count; i++)
                {
                    playerRow = new SkirmishMenuPlayerRow(engine);
                    if (!defaultAIAdded)
                    {
                        // Add a single AI to this skirmish game.
                        playerRow.Player.Name = "AI Player";
                        defaultAIAdded = true;
                    }
                    playerRow.SkirmishMenu = this;
                    playerRow.ParentPanel = pnlPlayers;
                    rightColumnLayout.Add(playerRow);
                    playerRows.Add(playerRow);
                    playerRow.OnPlayerNameChange += new PlayerNameChangeEventHandler(playerRow_OnPlayerNameChange);
                    playerRow.OnPlayerColorChange += new PlayerColorChangeEventHandler(playerRow_OnPlayerColorChange);
                    this.AddPanel(playerRow);
                }
            }
        }
        #endregion



        #region Player Row Event Handlers
        void playerRow_OnPlayerColorChange(SkirmishMenuPlayerRow sender, int value)
        {
            // Get index of this player
            int playerRowIndex = -1;
            for (int i = 0; i < playerRows.Count; i++)
            {
                if (playerRows[i] == sender)
                {
                    playerRowIndex = i;
                    break;
                }
            }


            // Set player's team color
            if (playerRowIndex >= 0 && value >= 0 && playerRows[playerRowIndex].Player.Name != null)
            {
                playerStartTexts[playerRowIndex].FontColor = engine.PlayerColors[value - 1];
                playerStartTexts[playerRowIndex].RecreateTexture();
            }
        }


        void playerRow_OnPlayerNameChange(SkirmishMenuPlayerRow sender, string value)
        {
            // Get index of this player
            int playerRowIndex = -1;
            for (int i = 0; i < playerRows.Count; i++)
            {
                if (playerRows[i] == sender)
                {
                    playerRowIndex = i;
                    break;
                }
            }


            if (playerRowIndex >= 0)
            {
                if (value != "Open" && value != "Closed")
                {
                    playerStartTexts[playerRowIndex].FontColor = engine.PlayerColors[playerRows[playerRowIndex].Player.Color - 1];
                }
                else
                {
                    playerStartTexts[playerRowIndex].FontColor = Color.White;
                }
            }
        }
        #endregion



        #region OK Button Handler
        void okButton_OnClick(GameTime gameTime)
        {
            //engine.PlaySound("ButtonClick");

            // Get selected item
            if (lstMapList.Items.Count > 0 && lstMapList.SelectedItems().Count > 0)
            {
                Match newSkirmishMatch = new Match(engine);


                // Load this level name
                DListBoxItem item = (DListBoxItem)lstMapList.Items[lstMapList.SelectedIndex];
                string fileName = Path.Combine(DEngine.FileAccess.GetLevelsDir(), item.Text);
                SkirmishMenuPlayerRow.PlayerNum = 1; // static player num reset, bad place to put it

                newSkirmishMatch.LevelFile = item.Text;



                // Setup computer players
                bool validPlayers = false;
                foreach (SkirmishMenuPlayerRow playerRow in playerRows)
                {
                    if (playerRow.Player.Name == gameSettings.PlayerName)
                    {
                        // Set up a player for ourselves
                        FactionsPlayer localPlayer = new FactionsPlayer(engine);
                        localPlayer.Team = playerRow.Player.Team;
                        localPlayer.Name = gameSettings.PlayerName;
                        localPlayer.Color = playerRow.Player.Color;
                        newSkirmishMatch.Players.Add(localPlayer);
                        engine.LocalPlayer = localPlayer;
                    }
                    else if (playerRow.Player.Name == "AI Player")
                    {
                        AIPlayer aiPlayer = new AIPlayer(engine);
                        aiPlayer.Team = playerRow.Player.Team;
                        aiPlayer.Color = playerRow.Player.Color;
                        aiPlayer.Name = aiPlayer.Color + " Computer";
                        newSkirmishMatch.Players.Add(aiPlayer);
                        validPlayers = true; // We have one AI player so it's a valid skirmish game.
                    }
                    else
                        newSkirmishMatch.Players.Add(null);
                }


                if (validPlayers)
                {
                    HideForm();
                    engine.Tick();
                    engine.EndLevel();

                    loadScreen = new LoadingScreen(engine);
                    loadScreen.InfoLabel.Text = newSkirmishMatch.LevelFile;
                    loadScreen.ShowForm();
                    engine.Tick();

                    engine.EngineIO.OnLoadTick += new LoadFileTickHandler(loadScreen.LoadTickHandler);
                    engine.NewSkirmishGame(newSkirmishMatch);
                    loadScreen.HideForm();
                    loadScreen.Dispose();
                }
                else
                {
                    lblStatus.Text = "Please select at least one opponent!";
                }
            }
        }
        #endregion



        #region Close Button Handler
        void closeButton_OnClick(GameTime gameTime)
        {
            //engine.PlaySound("ButtonClick");

            //HideForm();
            ShowParentForm();
            SkirmishMenuPlayerRow.PlayerNum = 1;
        }
        #endregion



        #region Map Select Handler
        void lstMapList_OnItemSelect()
        {
            //engine.PlaySound("ButtonClick");

            // Throw away current level and load this one
            if (mapImage != null)
            {
                engine.StaticSceneGraph.RemoveNode(mapImage);
                mapImage.Dispose();
            }

            // Clear minimap icons and reload for this map
            foreach (DText minimapPlayerNumText in playerStartTexts)
            {
                engine.StaticSceneGraph.RemoveNode(minimapPlayerNumText);
                minimapPlayerNumText.Dispose();
            }
            playerStartPoints.Clear();
            playerStartTexts.Clear();
            levelFile = lstMapList.Items[lstMapList.SelectedIndex].Text;
            playerStartPoints = engine.EngineIO.LoadPlayerStartPointsFromLevel(levelFile);

            // Load the minimap texture
            LoadMiniMap();

            // Load the player list
            SetupPlayerList();

            UpdateMiniMapStartLabels();
        }
        #endregion



        #region LoadMapList
        /// <summary>
        /// Left side column listbox with level names
        /// </summary>
        protected void LoadMapList()
        {
            //int mapsListX = 5;

            // Maps label
            lblMaps = new DText(engine.GuiManager, 0, 0, "Maps:");
            lblMaps.Text = "Maps:";
            lblMaps.Initialize();
            //leftColumnLayout.Add(lblMaps);
            this.AddPanel(lblMaps);
            lblMaps.Position = new Vector2(panelPadding, lblMaps.Size.Y / 2);

            // Load default Y position and height of content panels!
            panelYPosition = (2 * panelPadding) + lblMaps.Size.Y;
            panelHeight = (int)(Math.Abs(okButton.Position.Y - panelPadding) - ((2 * panelPadding) + lblMaps.Size.Y));

            // Map list panel (30% width)
            pnlMaps = new DPanel(engine.GuiManager, 
                                 panelPadding, 
                                 panelYPosition, 
                                 (int)(panelWidthMax * 0.4), 
                                 panelHeight);
            //pnlMaps.FillColor = panelFillColor;
            pnlMaps.ColorTheme = _colorTheme;
            pnlMaps.Initialize();
            this.AddPanel(pnlMaps);

            // Maps listbox
            lstMapList = new DListBox(engine.GuiManager, 
                                      0, 
                                      0, 
                                      (int)(pnlMaps.Width - (2 * panelPadding)), 
                                      (int)(pnlMaps.Height - (2 * panelPadding))
                                     );
            lstMapList.Initialize();
            //leftColumnLayout.Add(lstMapList);
            pnlMaps.AddPanel(lstMapList);
            lstMapList.Position = new Vector2(panelPadding, panelPadding);
            
            // Load the map names
            string mapsDir = DEngine.FileAccess.GetLevelsDir();
            DirectoryInfo di = new DirectoryInfo(mapsDir);
            FileInfo[] files = di.GetFiles("*.xml");
            foreach (FileInfo file in files)
            {
                // Should really do some format checking here.
                lstMapList.AddListItem(new DListBoxItem(engine.GuiManager, file.Name));
            }
            if (lstMapList.Items.Count > 0)
            {
                lstMapList.SelectedIndex = 0;
                lstMapList.Items[0].Selected = true;
                levelFile = lstMapList.Items[lstMapList.SelectedIndex].Text;
                playerStartPoints = engine.EngineIO.LoadPlayerStartPointsFromLevel(levelFile);
            }
            lstMapList.OnItemSelect += new ListBoxChangeEventHandler(lstMapList_OnItemSelect);
        }
        #endregion



        #region LoadMiniMap
        protected void LoadMiniMap()
        {
            // Load minimap
            string levelFile = Path.Combine(DEngine.FileAccess.GetLevelsDir(), lstMapList.Items[lstMapList.SelectedIndex].Text);
            string mapImageFile = lstMapList.Items[lstMapList.SelectedIndex].Text;
            mapImageFile = mapImageFile.Replace(".xml", ".png");
            string mapDirectory = Path.Combine(engine.Content.RootDirectory, "levels");
            mapImageFile = Path.Combine(mapDirectory, mapImageFile);

            

            // Make an image from the map image file
            mapImage = new DImage(engine.GuiManager);
            Vector2 minimapSize = new Vector2(pnlMiniMap.Size.X - (2 * panelPadding),
                                              pnlMiniMap.Size.X - (2 * panelPadding)); // doubling of Y is intentional
            Vector2 minimapPosition = new Vector2(panelPadding,
                                                  panelPadding);
            mapImage.Position = minimapPosition;
            mapImage.Size = minimapSize;

            FileStream fileStream = new FileStream(mapImageFile, FileMode.Open);
            mapImage.Image = Texture2D.FromStream(engine.GraphicsDevice, fileStream);
            fileStream.Close();

            mapImage.Initialize();
            pnlMiniMap.Children.Add(mapImage);

            // Setup start points
            foreach (PlayerStartPoint psp in playerStartPoints)
            {
                Vector2 scaledStartPoint = new Vector2((psp.Position.X / engine.TileWidth) * mapImage.Scale,
                                                        (psp.Position.Y / engine.TileHeight) * mapImage.Scale);
                scaledStartPoint += pnlMiniMap.Position;
                scaledStartPoint += new Vector2(panelPadding, panelPadding);
                DText startPointText = new DText(engine.GuiManager,
                                                scaledStartPoint.X,
                                                scaledStartPoint.Y,
                                                psp.Team.ToString());
                startPointText.FontColor = Color.White;//engine.PlayerColors[a.Team - 1];
                startPointText.Initialize();
                this.Children.Add(startPointText);
                //startPointText.Parent = pnlMiniMap;

                //engine.StaticSceneGraph.RootNode.Children.Add(startPointText);
                playerStartTexts.Add(startPointText);
            }
        }
        #endregion



        #region LoadMiniMapPanel
        /// <summary>
        /// Load a minimap panel, label, and the minimap from the level we have loaded
        /// </summary>
        protected void LoadMiniMapPanel()
        {
            // Minimap label
            int miniMapX = (int)pnlMaps.Width + (2 * panelPadding);
            lblMiniMap = new DText(engine.GuiManager, 0, 0, "MiniMap:");
            lblMiniMap.Initialize();
            this.AddPanel(lblMiniMap);
            lblMiniMap.Position = new Vector2(miniMapX, lblMiniMap.Size.Y / 2);

            // Minimap panel
            int miniMapPanelWidth = (int)(panelWidthMax * 0.3);
            pnlMiniMap = new DPanel(engine.GuiManager, 
                                    miniMapX,
                                    panelYPosition, 
                                    miniMapPanelWidth, 
                                    (int)(miniMapPanelWidth * 1.4)
                                    );
            //pnlMiniMap.FillColor = panelFillColor;
            pnlMiniMap.ColorTheme = _colorTheme;
            pnlMiniMap.Initialize();
            this.AddPanel(pnlMiniMap);

            LoadMiniMap();

            lblMapTitle = new DText(engine.GuiManager, mapImage.Size.X / 2, mapImage.Size.Y + 35, "-Map Title-");
            lblMapTitle.FontName = "Arial";
            lblMapTitle.Initialize();
            pnlMiniMap.AddPanel(lblMapTitle);
        }
        #endregion



        #region LoadMapOptions
        /// <summary>
        /// Various map settings panel (player starts, resources, rules, etc)
        /// </summary>
        protected void LoadMapOptions()
        {
            // Map options label
            int mapOptionsX = (int)pnlMiniMap.Position.X;
            lblMapOptions = new DText(engine.GuiManager, 0, 0, "Options:");
            lblMapOptions.Initialize();
            this.AddPanel(lblMapOptions);
            lblMapOptions.Position = new Vector2(mapOptionsX + panelPadding, pnlMiniMap.Position.Y + pnlMiniMap.Size.Y + panelPadding);

            int miniMapOptionsWidth = (int)(panelWidthMax * 0.3);
            float miniMapOptionsY = lblMapOptions.Position.Y + lblMapOptions.Size.Y + panelPadding;

            // Map options panel
            pnlMapOptions = new DPanel(engine.GuiManager, 
                                        mapOptionsX, 
                                        miniMapOptionsY,
                                        miniMapOptionsWidth, 
                                        (int)(okButton.Position.Y - (miniMapOptionsY + panelPadding))
                                       );
            //pnlMapOptions.FillColor = panelFillColor;
            pnlMapOptions.ColorTheme = _colorTheme;
            pnlMapOptions.Initialize();
            

            mapOptionsLayout = new DLayoutFlow(1, 10, DLayoutFlow.DLayoutFlowStyle.Vertically);
            mapOptionsLayout.CellPadding = 2;
            mapOptionsLayout.Position = new Vector2(5, 5);

            // Start point allocation
            lblStartPointAllocation = new DText(engine.GuiManager, 0, 0, "Start Order:");
            lblStartPointAllocation.FontName = "Arial";
            lblStartPointAllocation.Initialize();
            mapOptionsLayout.Add(lblStartPointAllocation);
            pnlMapOptions.AddPanel(lblStartPointAllocation);

            cmbStartPointAllocation = new DComboBox(engine.GuiManager, 0, 0); // pnlMapOptions, 
            mapOptionsLayout.Add(cmbStartPointAllocation);
            cmbStartPointAllocation.Initialize();
            pnlMapOptions.AddPanel(cmbStartPointAllocation);
            cmbStartPointAllocation.AddItem("Fixed", null);
            cmbStartPointAllocation.AddItem("Random", null);
            cmbStartPointAllocation.Text = "Fixed";


            // Starting resources
            lblStartingResources = new DText(engine.GuiManager, 0, 0, "Starting Resources:");
            lblStartingResources.FontName = "Arial";
            lblStartingResources.Initialize();
            mapOptionsLayout.Add(lblStartingResources);
            pnlMapOptions.AddPanel(lblStartingResources);

            cmbStartingResources = new DComboBox(engine.GuiManager, 0, 0); // pnlMapOptions
            mapOptionsLayout.Add(cmbStartingResources);
            cmbStartingResources.Initialize();
            pnlMapOptions.AddPanel(cmbStartingResources);

            cmbStartingResources.AddItem("None", null);
            cmbStartingResources.AddItem("Low", null);
            cmbStartingResources.AddItem("Medium", null);
            cmbStartingResources.AddItem("High", null);
            cmbStartingResources.Text = "Medium";


            // Win condition
            lblWinCondition = new DText(engine.GuiManager, 0, 0, "Win Condition:");
            lblWinCondition.FontName = "Arial";
            lblWinCondition.Initialize();
            mapOptionsLayout.Add(lblWinCondition);
            pnlMapOptions.AddPanel(lblWinCondition);

            cmbWinCondition = new DComboBox(engine.GuiManager, 0, 0); // pnlMapOptions,
            mapOptionsLayout.Add(cmbWinCondition);
            cmbWinCondition.Initialize();
            pnlMapOptions.AddPanel(cmbWinCondition);
            
            cmbWinCondition.AddItem("Infinite Game", null);
            cmbWinCondition.AddItem("Annihilation", null);
            cmbWinCondition.AddItem("Buildings Razed", null);
            cmbWinCondition.AddItem("Time Limit", null);
            cmbWinCondition.Text = "Annihilation";
            

            this.AddPanel(pnlMapOptions);
        }
        #endregion



        #region LoadPlayerList
        protected void LoadPlayerList()
        {
            // Player list label
            int playerListX = (int)(pnlMiniMap.Position.X + pnlMiniMap.Size.X + panelPadding);
            lblPlayerList = new DText(engine.GuiManager, 0, 0, "Players:");
            lblPlayerList.Initialize();
            this.AddPanel(lblPlayerList);
            lblPlayerList.Position = new Vector2(playerListX + panelPadding, lblMaps.Position.Y);

            int playerListWidth = (int)((this.Position.X + this.Size.X - (4 * panelPadding)) - playerListX);

            // Player list panel
            pnlPlayers = new DPanel(engine.GuiManager, 
                                    playerListX, 
                                    panelYPosition,
                                    playerListWidth, 
                                    panelHeight);
            //pnlPlayers.FillColor = panelFillColor;
            pnlPlayers.ColorTheme = _colorTheme;
            pnlPlayers.Initialize();
            this.AddPanel(pnlPlayers);

            // Player Name column header
            lblPlayerNameHeader = new DText(engine.GuiManager, 5, 0, "Name");
            lblPlayerNameHeader.Initialize();
            pnlPlayers.AddPanel(lblPlayerNameHeader);
            lblPlayerNameHeader.Position = new Vector2(12, (lblPlayerNameHeader.Size.Y / 2));

            // Player team column header
            lblPlayerTeamHeader = new DText(engine.GuiManager, 0, 0, "Team");
            lblPlayerTeamHeader.Initialize();
            pnlPlayers.AddPanel(lblPlayerTeamHeader);
            lblPlayerTeamHeader.Position = new Vector2(lblPlayerNameHeader.Position.X + lblPlayerNameHeader.Size.X + 115, lblPlayerNameHeader.Y);

            // Player color column header
            lblPlayerColorHeader = new DText(engine.GuiManager, 0, 0, "Color");
            lblPlayerColorHeader.Initialize();
            pnlPlayers.AddPanel(lblPlayerColorHeader);
            lblPlayerColorHeader.Position = new Vector2(lblPlayerTeamHeader.Position.X + lblPlayerTeamHeader.Size.X + 30, lblPlayerNameHeader.Y);

            // Player list layout
            rightColumnLayout = new DLayoutFlow(1, 12);
            rightColumnLayout.Position = new Vector2(playerListX + panelPadding, lblPlayerNameHeader.Position.Y + lblPlayerNameHeader.Size.Y + 45);
            rightColumnLayout.CellHeight = 20;
            rightColumnLayout.CellPadding = 2;
        }
        #endregion


    }
}
