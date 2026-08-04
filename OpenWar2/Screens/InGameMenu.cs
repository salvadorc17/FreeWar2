using System;
using System.Collections.Generic;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FactionsGame
{
    /// <summary>
    /// In-Game menu. Surrender, Exit Program, Options
    /// </summary>
    public class InGameMenu : DForm
    {
        protected FactionsGame engine;

        protected OptionsMenu optionsMenu;

        // Controls
        protected DButton optionsButton = null;
        protected DButton quitMissionButton = null;
        protected DButton exitGameButton = null;
        protected DImage mainLogo = null;
        protected DLayoutFlow layout;

        // Geometry and coloring
        protected int buttonWidth = 200;
        protected int buttonHeight = 40;
        protected Color fillColor = new Color(255, 252, 214);
        protected Color clickedFillColor = new Color(79, 146, 255);
        protected Color borderColor = new Color(20, 20, 20);
        protected Color hoverFillColor = new Color(255, 240, 168);

        


        #region Constructor
        public InGameMenu(FactionsGame game)
            : base(game.GuiManager, "InGameMenu", null)
        {
            engine = game;
            Name = "InGameMenu";

            //optionsMenu = new OptionsMenu(engine, this);
            //ChildForms.Add(optionsMenu.Name, optionsMenu);
        }
        #endregion



        #region ShowForm
        /// <summary>
        /// Create menu objects and add them to the scenegraph.
        /// </summary>
        public override void ShowForm()
        {
            this.Visible = false;

            // Get center screen coords
            Vector2 centerScreen = new Vector2(engine.Window.ClientBounds.Width / 2f,
                engine.Window.ClientBounds.Height / 2f);

            // Make the form span the whole screen and make it partially invisible
            this.Size = new Vector2(engine.Graphics.PreferredBackBufferWidth, engine.Graphics.PreferredBackBufferHeight);
            this.Position = Vector2.Zero;
            this.FillColor = Color.Black;
            this.BorderWidth = 0;
            this.Alpha = 100;
            this.AlwaysVisible = true;
            this.Initialize();
            this.RecreateTexture();
            

            layout = new DLayoutFlow(1, 10);
            layout.Position = centerScreen - (new Vector2(buttonWidth / 2, (buttonHeight * 3)));

            // Logo
            mainLogo = new DImage(engine.GuiManager, centerScreen.X - 268, centerScreen.Y - 300, "logo");
            //layout.Add(mainLogo);
            mainLogo.Size = new Vector2(537, 152);
            mainLogo.Initialize();
            this.AddPanel(mainLogo);

            // Options button
            optionsButton = new DButton(engine.GuiManager, 0, 0, "Options", buttonWidth, buttonHeight);
            layout.Add(optionsButton);
            optionsButton.Initialize();
            optionsButton.OnClick += new DButtonEventHandler(optionsButton_OnClick);
            this.AddPanel(optionsButton);

            // Quit mission button
            quitMissionButton = new DButton(engine.GuiManager, 0, 0, "Surrender", buttonWidth, buttonHeight);
            layout.Add(quitMissionButton);
            quitMissionButton.Initialize();
            quitMissionButton.OnClick += new DButtonEventHandler(quitMissionButton_OnClick);
            this.AddPanel(quitMissionButton);

            // Exit game button
            exitGameButton = new DButton(engine.GuiManager, 0, 0, "Exit Game", buttonWidth, buttonHeight);
            layout.Add(exitGameButton);
            exitGameButton.Initialize();
            exitGameButton.OnClick += new DButtonEventHandler(exitGameButton_OnClick);
            this.AddPanel(exitGameButton);

            engine.StaticSceneGraph.RootNode.Children.Add(this);

            this.Visible = true;
        }


        #endregion

        #region HideForm
        /// <summary>
        /// Remove all menu objects from the scenegraph.
        /// </summary>
        public override void HideForm()
        {
            engine.StaticSceneGraph.RemoveNode(mainLogo);
            engine.StaticSceneGraph.RemoveNode(quitMissionButton);
            engine.StaticSceneGraph.RemoveNode(exitGameButton);
            engine.StaticSceneGraph.RemoveNode(optionsButton);
            engine.StaticSceneGraph.RemoveNode(this);
            this.Children.Remove(exitGameButton);
            this.Children.Remove(mainLogo);
            this.Children.Remove(quitMissionButton);
            this.Children.Remove(exitGameButton);
            this.Children.Remove(optionsButton);

            mainLogo.Dispose();
            quitMissionButton.Dispose();
            exitGameButton.Dispose();
            optionsButton.Dispose();
            this.Dispose();
        }
        #endregion



        #region Button event handlers
        void optionsButton_OnClick(GameTime gameTime)
        {
           // engine.PlaySound("ButtonClick");

            //HideForm();
            ShowChildForm("OptionsMenu");
        }

        void exitGameButton_OnClick(GameTime gameTime)
        {
           // engine.PlaySound("ButtonClick");

            MessageDialog exitConfirm
                = new MessageDialog(engine, "Sure you want to exit?", MessageDialog.MessageDialogButtons.OKCancel, "Exit Game");
            exitConfirm.DialogClosed += new MessageDialog.MessageDialogCloseHandler(exitConfirm_DialogClosed);
            exitConfirm.ShowForm();
        }

        void exitConfirm_DialogClosed(object sender, MessageDialog.MessageDialogResult result)
        {
            if (result == MessageDialog.MessageDialogResult.OK)
            {
                engine.Exit();
            }
        }

        void quitMissionButton_OnClick(GameTime gameTime)
        {
            //engine.PlaySound("ButtonClick");
            MessageDialog areYouSureDialog
                = new MessageDialog(engine, "Sure you want to surrender?", MessageDialog.MessageDialogButtons.OKCancel, "Surrender");
            areYouSureDialog.DialogClosed += new MessageDialog.MessageDialogCloseHandler(areYouSureDialog_DialogClosed);
            areYouSureDialog.ShowForm();
        }


        void areYouSureDialog_DialogClosed(object sender, MessageDialog.MessageDialogResult result)
        {
            if (result == MessageDialog.MessageDialogResult.OK)
            {
                HideForm();
                engine.ReturnToMenu();
            }
        }
        #endregion

    }
}
