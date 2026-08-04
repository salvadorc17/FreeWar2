using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FactionsGame.Actors;

namespace FactionsGame
{
    /// <summary>
    /// Info bar at the top of the screen.
    /// Resources, unit count, other simple stats
    /// </summary>
    public class TopInfoBar : DForm
    {
        protected FactionsGame _engine;

        protected int alpha = 100;    // Overall alpha

        protected DText resourcesLabel;
        protected DText resourcesValueLabel;
        protected DGuiColorTheme colorTheme = new DGuiColorTheme();





        #region Public Properties
        public string ResourcesAmount
        {
            get { return resourcesValueLabel.Text; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    resourcesValueLabel.Text = value;
                }
                else
                    resourcesValueLabel.Text = "";
            }
        }
        #endregion



        #region Constructor
        /// <summary>
        /// Unit command panel ala starcraft/warcraft
        /// Stop, Move, Attack
        /// </summary>
        /// <param name="game"></param>
        public TopInfoBar(FactionsGame game)
            : base(game.GuiManager, "TopInfoBar", null)
        {
            _engine = game;

            this.Position = Vector2.Zero;
            this.Size = new Vector2(_engine.Window.ClientBounds.Width, 35);
            this.Alpha = alpha;
            //this.ColorTheme = colorTheme;
        }
        #endregion





        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }


        public override void ShowForm()
        {
            if (!shown)
            {
                this.Visible = false;

                resourcesLabel = new DText(_engine.GuiManager, 20, 18, "Resources:");
                resourcesLabel.HorizontalAlignment = DText.DHorizontalAlignment.Left;
                resourcesLabel.FontName = "Arial";
                resourcesLabel.FontColor = Color.Blue;
                //resourcesLabel.Initialize();
                this.AddPanel(resourcesLabel);

                resourcesValueLabel = new DText(_engine.GuiManager, resourcesLabel.Position.X + resourcesLabel.Size.X + 10, 18, "0");
                resourcesValueLabel.HorizontalAlignment = DText.DHorizontalAlignment.Left;
                resourcesValueLabel.FontName = "Arial";
                resourcesValueLabel.FontColor = Color.Blue;
                this.AddPanel(resourcesValueLabel);

                _engine.StaticSceneGraph.RootNode.Children.Add(this);

                this.Visible = true;
            }

            base.ShowForm();
        }



        void UnitCommandPanel_OnHoverExit()
        {
            _engine.GuiBeingUsed = false;
        }

        void UnitCommandPanel_OnHoverEnter()
        {
            _engine.GuiBeingUsed = true;
        }



        public override void HideForm()
        {
            if (shown)
            {
                this.OnHoverExit -= UnitCommandPanel_OnHoverExit;
                this.OnHoverEnter -= UnitCommandPanel_OnHoverEnter;

                _engine.StaticSceneGraph.RemoveNode(this);
                this.Children.Remove(resourcesLabel);
                this.Children.Remove(resourcesValueLabel);
                resourcesLabel = null;
                resourcesValueLabel = null;
                //this.Dispose();
            }

            base.HideForm();
        }



    }



}
