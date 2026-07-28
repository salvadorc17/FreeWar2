using System;
using System.Collections.Generic;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Configuration;

namespace FactionsGame
{
    /// <summary>
    /// Options menu system for the transformers game.
    /// Player, video, sound, keys
    /// </summary>
    public class OptionsMenu : DForm
    {
        // Geometry constants
        protected const int BUTTON_WIDTH = 130;
        protected const int BUTTON_HEIGHT = 35;
        protected const int PANEL_PADDING = 20;

        // Engine and settings
        protected FactionsGame game;
        protected FactionsGameSettings gameSettings;

        // Controls
        protected DButton okButton = null;
        protected DButton closeButton = null;
        protected DLayoutFlow layout;
        protected DText lblPlayerName;
        protected DTextBox txtPlayerName;
        protected DText lblTeam;
        protected DComboBox cmbTeam;
        protected DText lblColor;
        protected DComboBox cmbColor;
        protected DText lblResolution;
        protected DComboBox cmbResolution;
        protected DCheckbox chkFullscreen;


        #region Constructor
        public OptionsMenu(FactionsGame factionsGame, DForm parent)
            : base(factionsGame.GuiManager, "OptionsMenu", parent)
        {
            Name = "OptionsMenu";
            game = factionsGame;

            gameSettings = new FactionsGameSettings();
        }
        #endregion



        #region ShowForm
        /// <summary>
        /// Create menu items and attach them to the scenegraph.
        /// </summary>
        public override void ShowForm()
        {
            this.Visible = false;

            base.ShowForm();

            // Get center screen coords
            Vector2 centerScreen = new Vector2(game.Window.ClientBounds.Width / 2f,
                game.Window.ClientBounds.Height / 2f);

            // Setup form
            this.Size = new Vector2(game.Window.ClientBounds.Width * 0.8f, game.Window.ClientBounds.Height * 0.8f);
            this.Position = new Vector2((game.Window.ClientBounds.Width * 0.1f),
                                        (game.Window.ClientBounds.Height * 0.1f));
            this.FillColor = Color.GhostWhite;
            this.BorderColor = Color.Black;
            this.BorderWidth = 1;
            this.Alpha = 200;
            this.Initialize();
            this.RecreateTexture();
            

            // Close without saving
            closeButton = new DButton(game.GuiManager,
                this.Size.X - (BUTTON_WIDTH + PANEL_PADDING),
                this.Size.Y - (BUTTON_HEIGHT + PANEL_PADDING), "Close", BUTTON_WIDTH, BUTTON_HEIGHT);
            closeButton.Initialize();
            closeButton.OnClick += new DButtonEventHandler(closeButton_OnClick);
            this.AddPanel(closeButton);

            // Save and close
            okButton = new DButton(game.GuiManager,
                this.Size.X - (2 * (BUTTON_WIDTH + PANEL_PADDING)),
                this.Size.Y - (BUTTON_HEIGHT + PANEL_PADDING), "OK", BUTTON_WIDTH, BUTTON_HEIGHT);
            okButton.Initialize();
            okButton.OnClick += new DButtonEventHandler(okButton_OnClick);
            this.AddPanel(okButton);

           
            // Laid-out controls
            layout = new DLayoutFlow(1, 10);
            layout.Position = new Vector2(PANEL_PADDING, PANEL_PADDING);
            layout.CellPadding = 15;


            lblPlayerName = new DText(game.GuiManager, 0, 0, "Player Name:");
            lblPlayerName.Initialize();
            layout.Add(lblPlayerName);
            this.AddPanel(lblPlayerName);

            txtPlayerName = new DTextBox(game.GuiManager);
            txtPlayerName.Initialize();
            txtPlayerName.Text = gameSettings.PlayerName;
            layout.Add(txtPlayerName);
            this.AddPanel(txtPlayerName);

            lblTeam = new DText(game.GuiManager, 0, 0, "Default Team:");
            lblTeam.Initialize();
            layout.Add(lblTeam);
            this.AddPanel(lblTeam);

            cmbTeam = new DComboBox(game.GuiManager, 0, 0); // this
            cmbTeam.Size = new Vector2(120, 28);
            cmbTeam.Initialize();
            layout.Add(cmbTeam);
            this.AddPanel(cmbTeam);

            // Give it teams!
            cmbTeam.AddItem("1", null);
            cmbTeam.AddItem("2", null);
            cmbTeam.AddItem("3", null);
            cmbTeam.AddItem("4", null);
            cmbTeam.AddItem("5", null);
            cmbTeam.AddItem("6", null);
            cmbTeam.AddItem("7", null);
            cmbTeam.AddItem("8", null);
            cmbTeam.AddItem("9", null);
            cmbTeam.AddItem("10", null);
            cmbTeam.AddItem("11", null);
            cmbTeam.AddItem("12", null);

            // Select our team
            cmbTeam.SelectedIndex = gameSettings.Team - 1;
            cmbTeam.Text = Convert.ToString(gameSettings.Team);
            cmbTeam.OnShowHide += new ComboBoxToggleHandler(cmbTeam_OnShowHide);


            lblColor = new DText(game.GuiManager, 0, 0, "Default Color:");
            lblColor.Initialize();
            layout.Add(lblColor);
            this.AddPanel(lblColor);

            // Player color
            cmbColor = new DComboBox(game.GuiManager); // this
            cmbColor.Initialize();
            layout.Add(cmbColor);
            this.Children.Add(cmbColor);

            // Give it colors!
            cmbColor.AddItem("Blue", "gui\\teamflag1");
            cmbColor.AddItem("Red", "gui\\teamflag2");
            cmbColor.AddItem("Purple", "gui\\teamflag3");
            cmbColor.AddItem("Yellow", "gui\\teamflag4");
            cmbColor.AddItem("Green", "gui\\teamflag5");
            cmbColor.AddItem("Orange", "gui\\teamflag6");
            cmbColor.AddItem("White", "gui\\teamflag7");
            cmbColor.AddItem("Brown", "gui\\teamflag8");
            cmbColor.AddItem("Gray", "gui\\teamflag9");
            cmbColor.AddItem("Aqua", "gui\\teamflag10");
            cmbColor.AddItem("Tan", "gui\\teamflag11");
            cmbColor.AddItem("Pink", "gui\\teamflag12");

            cmbColor.Text = game.PlayerColorNames[gameSettings.PlayerColor - 1];
            cmbColor.ImageName = cmbColor.Items[cmbColor.SelectedIndex].ImageName;
            cmbColor.OnShowHide += new ComboBoxToggleHandler(cmbColor_OnShowHide);


            // Screen res
            lblResolution = new DText(game.GuiManager, 0, 0, "Screen Resolution:");
            lblResolution.Initialize();
            layout.Add(lblResolution);
            this.AddPanel(lblResolution);


            cmbResolution = new DComboBox(game.GuiManager);
            cmbResolution.Initialize();
            layout.Add(cmbResolution);
            this.Children.Add(cmbResolution);

            // Give it resolutions!
            cmbResolution.AddItem("640x480", null);
            cmbResolution.AddItem("800x600", null);
            cmbResolution.AddItem("1024x768", null);
            cmbResolution.AddItem("1152x864", null);
            cmbResolution.AddItem("1280x960", null);
            cmbResolution.AddItem("1280x1024", null);
            cmbResolution.AddItem("1600x900", null);
            cmbResolution.AddItem("1680x1050", null);
            cmbResolution.OnShowHide += new ComboBoxToggleHandler(cmbResolution_OnShowHide);

            string resString = gameSettings.ScreenWidth.ToString() + "x" + gameSettings.ScreenHeight.ToString();
            cmbResolution.Text = resString;


            chkFullscreen = new DCheckbox(game.GuiManager);
            chkFullscreen.Text = "Fullscreen";
            chkFullscreen.Initialize();
            chkFullscreen.Checked = gameSettings.Fullscreen;
            chkFullscreen.OnToggle += new CheckboxEventHandler(chkFullscreen_OnToggle);
            layout.Add(chkFullscreen);
            this.Children.Add(chkFullscreen);


            //game.StaticSceneGraph.RootNode.Children.Add(this);
            game.GuiManager.AddControl(this);

            this.Visible = true;
        }

        void cmbTeam_OnShowHide(bool open)
        {
            game.PlaySound("ButtonClick");
        }

        void cmbColor_OnShowHide(bool open)
        {
            game.PlaySound("ButtonClick");
        }

        void cmbResolution_OnShowHide(bool open)
        {
            game.PlaySound("ButtonClick");
        }

        void chkFullscreen_OnToggle()
        {
            game.PlaySound("ButtonClick");
        }
        #endregion



        #region HideForm
        /// <summary>
        /// Remove all the menu objects from the scenegraph.
        /// </summary>
        public override void HideForm()
        {
            base.HideForm();

            game.StaticSceneGraph.RemoveNode(okButton);
            game.StaticSceneGraph.RemoveNode(closeButton);
            game.StaticSceneGraph.RemoveNode(cmbResolution);
            game.StaticSceneGraph.RemoveNode(cmbColor);
            game.StaticSceneGraph.RemoveNode(cmbTeam);
            game.StaticSceneGraph.RemoveNode(chkFullscreen);
            game.StaticSceneGraph.RemoveNode(lblPlayerName);
            game.StaticSceneGraph.RemoveNode(lblTeam);
            game.StaticSceneGraph.RemoveNode(lblColor);
            game.StaticSceneGraph.RemoveNode(lblResolution);
            game.StaticSceneGraph.RemoveNode(txtPlayerName);
            //game.StaticSceneGraph.RemoveNode(this);
            game.GuiManager.RemoveControl(this);

            okButton.Dispose();
            closeButton.Dispose();
            cmbResolution.Dispose();
            cmbColor.Dispose();
            cmbTeam.Dispose();
            chkFullscreen.Dispose();
            lblPlayerName.Dispose();
            lblTeam.Dispose();
            lblColor.Dispose();
            lblResolution.Dispose();
            txtPlayerName.Dispose();
            this.Dispose();
        }
        #endregion







        #region Button Event Handlers
        /// <summary>
        /// OK/Save button.
        /// Save options to app.config.
        /// </summary>
        void okButton_OnClick(GameTime gameTime)
        {
            game.PlaySound("ButtonClick");

            gameSettings.PlayerName = txtPlayerName.Text;
            gameSettings.Team = cmbTeam.SelectedIndex + 1;

            // Lookup color
            gameSettings.PlayerColor = cmbColor.SelectedIndex + 1;

            string resString = cmbResolution.Text;
            string[] screenDimensions = resString.Split('x');
            int newWidth, newHeight;
            newWidth = Convert.ToInt32(screenDimensions[0]);
            newHeight = Convert.ToInt32(screenDimensions[1]);

            bool videoSettingsChanged = false;
            if (gameSettings.ScreenWidth != newWidth ||
                gameSettings.ScreenHeight != newHeight ||
                gameSettings.Fullscreen != chkFullscreen.Checked)
            {
                videoSettingsChanged = true;
            }

            gameSettings.ScreenWidth = newWidth;
            gameSettings.ScreenHeight = newHeight;
            gameSettings.Fullscreen = chkFullscreen.Checked;
            gameSettings.Save();

            if (videoSettingsChanged)
            {
                game.Graphics.PreferredBackBufferWidth = newWidth;
                game.Graphics.PreferredBackBufferHeight = newHeight;
                game.Graphics.IsFullScreen = chkFullscreen.Checked;
                game.Graphics.ApplyChanges();
            }

            ShowParentForm();
        }

        void closeButton_OnClick(GameTime gameTime)
        {
            game.PlaySound("ButtonClick");

            ShowParentForm();
        }
        #endregion

    }
}
