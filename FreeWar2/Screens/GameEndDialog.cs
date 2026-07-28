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
/// Dialog box shown in-game when game has ended.
/// Has Win or Defeat flag.
/// </summary>
    public class GameEndDialog : DForm
    {
        // Geometry constants
        protected const int BUTTON_WIDTH = 130;
        protected const int BUTTON_HEIGHT = 35;
        protected const int PANEL_PADDING = 20;

        // Engine and settings
        protected FactionsGame game;

        // Controls
        protected DButton okButton = null;
        protected DText lblMessage;
        protected bool _victory;


        #region Constructor
        public GameEndDialog(FactionsGame factionsGame, bool victory)
            : base(factionsGame.GuiManager, "GameEndDialog", null)
        {
            Name = "GameEndDialog";
            game = factionsGame;
            _victory = victory;
        }
        #endregion



        #region ShowForm
        /// <summary>
        /// Create menu items and attach them to the scenegraph.
        /// </summary>
        public override void ShowForm()
        {
            this.Visible = false;

            // Get center screen coords
            Vector2 centerScreen = new Vector2(game.Window.ClientBounds.Width / 2f,
                game.Window.ClientBounds.Height / 2f);

            // Setup form
            this.Size = new Vector2(game.Window.ClientBounds.Width * 0.2f, game.Window.ClientBounds.Height * 0.12f);
            this.Position = centerScreen - (this.Size / 2);
            this.FillColor = Color.GhostWhite;
            this.BorderColor = Color.Black;
            this.BorderWidth = 1;
            this.Alpha = 200;
            this.Initialize();
            game.StaticSceneGraph.RootNode.Children.Add(this);

            okButton = new DButton(game.GuiManager,
                (this.Size.X / 2) - (BUTTON_WIDTH / 2),
                (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "OK", BUTTON_WIDTH, BUTTON_HEIGHT);

            okButton.OnClick += new DButtonEventHandler(okButton_OnClick);
            this.AddPanel(okButton);
            okButton.Initialize();


            // Screen res
            string message = string.Empty;
            if (_victory)
                message = "You won!";
            else
                message = "You lost!";

            lblMessage = new DText(game.GuiManager, PANEL_PADDING, PANEL_PADDING, message);
            lblMessage.Position = new Vector2(this.Size.X / 2, PANEL_PADDING);
            this.AddPanel(lblMessage);
            lblMessage.Initialize();


            this.Visible = true;
            

            //base.ShowForm();
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
            game.StaticSceneGraph.RemoveNode(lblMessage);
            game.StaticSceneGraph.RemoveNode(this);

            okButton.Dispose();
            lblMessage.Dispose();
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

            HideForm();
            this.Dispose();
        }
        #endregion

    }
}
