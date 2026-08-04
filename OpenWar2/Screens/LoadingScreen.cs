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
    /// Factions loading screen.
    /// </summary>
    public class LoadingScreen : DForm
    {
        protected FactionsGame engine;
        protected Background bg;
        protected DText loadingLabel;
        protected DText infoLabel;
        protected DProgressBar progressBar;



        #region Public Properties
        public DProgressBar ProgressBar
        {
            get
            {
                return progressBar;
            }
        }
        public DText InfoLabel
        {
            get
            {
                return infoLabel;
            }
        }
        #endregion



        #region Constructor
        public LoadingScreen(FactionsGame game)
            : base(game.GuiManager, "LoadingScreen", null)
        {
            engine = game;
            Name = "LoadingScreen";
            infoLabel = new DText(engine.GuiManager);
        }
        #endregion



        #region ShowForm
        /// <summary>
        /// Create screen objects and add them to the scenegraph.
        /// </summary>
        public override void ShowForm()
        {
            this.Visible = false;

            // Get center screen coords
            Vector2 centerScreen = new Vector2(engine.Window.ClientBounds.Width / 2f,
                engine.Window.ClientBounds.Height / 2f);
            float loadingBarWidth = centerScreen.X - 100;

            // Make the form span the whole screen and make it partially invisible
            this.Position = Vector2.Zero;
            this.Size = new Vector2(engine.Window.ClientBounds.Width, engine.Window.ClientBounds.Height);
            this.FillColor = Color.Black;
            this.BorderWidth = 0;
            this.Alpha = 100;
            this.Initialize();
            engine.StaticSceneGraph.RootNode.Children.Add(this);

            // Add a background
            bg = new Background(engine);
            bg.ImageName = "metal-plate";
            bg.Velocity = new Vector2(0f, 0.2f);
            bg.Initialize();
            engine.BackgroundSceneNode1.Children.Add(bg);


            loadingLabel = new DText(engine.GuiManager);
            loadingLabel.Position = new Vector2(centerScreen.X, centerScreen.Y + 200);
            loadingLabel.Text = "Loading Level...";
            loadingLabel.FontColor = Color.White;
            loadingLabel.Initialize();
            engine.StaticSceneGraph.RootNode.Children.Add(loadingLabel);

            progressBar = new DProgressBar(engine.GuiManager);
            progressBar.Position = new Vector2(centerScreen.X - (loadingBarWidth / 2), centerScreen.Y + 233);
            progressBar.Size = new Vector2(loadingBarWidth, 30);
            progressBar.BarColor = Color.SteelBlue;
            progressBar.Initialize();
            progressBar.Value = 0;
            engine.StaticSceneGraph.RootNode.Children.Add(progressBar);

            //infoLabel = new DText(engine);
            infoLabel.Position = new Vector2(centerScreen.X, centerScreen.Y + 300);
            infoLabel.FontColor = Color.White;
            infoLabel.Initialize();
            engine.StaticSceneGraph.RootNode.Children.Add(infoLabel);

            this.Visible = true;
        }
        #endregion



        #region HideForm
        /// <summary>
        /// Remove all objects from the scenegraph.
        /// </summary>
        public override void HideForm()
        {
            engine.SceneGraph.RemoveNode(bg);
            engine.StaticSceneGraph.RemoveNode(loadingLabel);
            engine.StaticSceneGraph.RemoveNode(progressBar);
            engine.StaticSceneGraph.RemoveNode(infoLabel);
            engine.StaticSceneGraph.RemoveNode(this);
        }
        #endregion



        #region LoadTickHandler
        /// <summary>
        /// Level load tick handler to update the progress bar.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        public void LoadTickHandler(int current, int max)
        {
            progressBar.ValueMax = max;
            progressBar.Value = current;
        }
        #endregion

    }
}
