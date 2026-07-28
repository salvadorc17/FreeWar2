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
    /// Main menu for the FactionsGame.
    /// </summary>
    public class MainMenu : DForm
    {
        protected FactionsGame engine;

        // Child forms
        protected OptionsMenu optionsMenu;
        protected SkirmishMenu skirmishMenu;

        // Controls
        protected DButton newSkirmishGameButton = null;
        //protected DButton newSingleGameButton = null;
        //protected DButton newMultiplayerGameButton = null;
        protected DButton optionsButton = null;
        protected DButton exitGameButton = null;
        protected DImage mainLogo = null;
        protected DLayoutFlow layout;
        protected Background bg;

        // Geometry and coloring
        protected int buttonWidth = 280;
        protected int buttonHeight = 40;
        protected DGuiColorTheme _colorTheme = new DGuiColorTheme();
        //protected Color fillColor = new Color(255, 252, 214);
        //protected Color clickedFillColor = new Color(79, 146, 255);
        //protected Color borderColor = new Color(20, 20, 20);
        //protected Color hoverFillColor = new Color(255, 240, 168);

        


        #region Constructor
        public MainMenu(FactionsGame game)
            : base(game.GuiManager, "MainMenu", null)
        {
            engine = game;
            Name = "MainMenu";
            optionsMenu = new OptionsMenu(engine, this);
            ChildForms.Add(optionsMenu.Name, optionsMenu);

            skirmishMenu = new SkirmishMenu(engine, this);
            ChildForms.Add(skirmishMenu.Name, skirmishMenu);
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

            // Add a background
            bg = new Background(engine);
            bg.ImageName = "galvanized-steel";
            bg.Velocity = new Vector2(0f, 0.2f);
            bg.Initialize();
            engine.BackgroundSceneNode1.Children.Add(bg);
            

            layout = new DLayoutFlow(1, 10);
            layout.Position = centerScreen - (new Vector2(buttonWidth / 2, (buttonHeight * 3)));

            // Logo
            mainLogo = new DImage(engine.GuiManager, centerScreen.X - 268, centerScreen.Y - 300, "logo");
            //layout.Add(mainLogo);
            mainLogo.Size = new Vector2(537, 152);
            mainLogo.Initialize();
            this.AddPanel(mainLogo);

            // New Single Player Game button
            newSkirmishGameButton = new DButton(engine.GuiManager, 0, 0, "New Skirmish Match", buttonWidth, buttonHeight);
            layout.Add(newSkirmishGameButton);
            newSkirmishGameButton.ColorTheme = _colorTheme;
            newSkirmishGameButton.Initialize();
            newSkirmishGameButton.OnClick += new DButtonEventHandler(newSkirmishGameButton_OnClick);
            this.AddPanel(newSkirmishGameButton);

            // Options button
            optionsButton = new DButton(engine.GuiManager, 0, 0, "Options", buttonWidth, buttonHeight);
            layout.Add(optionsButton);
            optionsButton.ColorTheme = _colorTheme;
            optionsButton.Initialize();
            optionsButton.OnClick += new DButtonEventHandler(optionsButton_OnClick);
            this.AddPanel(optionsButton);

            // Exit game button
            exitGameButton = new DButton(engine.GuiManager, 0, 0, "Exit", buttonWidth, buttonHeight);
            layout.Add(exitGameButton);
            exitGameButton.ColorTheme = _colorTheme;
            exitGameButton.Initialize();
            exitGameButton.OnClick += new DButtonEventHandler(exitGameButton_OnClick);
            this.AddPanel(exitGameButton);


            //engine.StaticSceneGraph.RootNode.Children.Add(this);
            engine.GuiManager.AddControl(this);

            this.Visible = true;
        }
        #endregion



        #region HideForm
        /// <summary>
        /// Remove all menu objects from the scenegraph.
        /// </summary>
        public override void HideForm()
        {
            engine.SceneGraph.RemoveNode(bg);

            engine.StaticSceneGraph.RemoveNode(mainLogo);
            engine.StaticSceneGraph.RemoveNode(newSkirmishGameButton);
            engine.StaticSceneGraph.RemoveNode(exitGameButton);
            engine.StaticSceneGraph.RemoveNode(optionsButton);
            //engine.StaticSceneGraph.RemoveNode(this);

            engine.GuiManager.RemoveControl(this);

            this.Children.Remove(exitGameButton);
            this.Children.Remove(mainLogo);
            this.Children.Remove(newSkirmishGameButton);
            this.Children.Remove(exitGameButton);
            this.Children.Remove(optionsButton);

            bg.Dispose();
            mainLogo.Dispose();
            newSkirmishGameButton.Dispose();
            exitGameButton.Dispose();
            optionsButton.Dispose();
            this.Dispose();
        }
        #endregion



        #region Button event handlers
        // New skirmish game!
        void newSkirmishGameButton_OnClick(GameTime gameTime)
        {
            engine.PlaySound("ButtonClick");

            HideForm();
            ShowChildForm("SkirmishMenu");

            //engine.LoadFirstLevel();

        }

        void optionsButton_OnClick(GameTime gameTime)
        {
            engine.PlaySound("ButtonClick");

            HideForm();
            ShowChildForm("OptionsMenu");
        }

        void newMultiplayerGameButton_OnClick(GameTime gameTime)
        {
            //throw new NotImplementedException();
        }

        void exitGameButton_OnClick(GameTime gameTime)
        {
            engine.PlaySound("ButtonClick");

            engine.Exit();
        }
        #endregion

    }
}
