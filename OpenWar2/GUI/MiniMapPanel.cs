using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FactionsGame
{
    /// <summary>
    /// Minimap!
    /// </summary>
    public class MiniMapPanel : DForm
    {
        protected FactionsGame _engine;

        protected int alpha = 100;    // Overall alpha

        // Gui objects!
        protected MiniMap _miniMap;

        protected DPanel minimapPanel;
        protected DButton minimapButton1;
        protected DButton minimapButton2;
        protected DButton minimapButton3;
        protected DLayoutFlow commandButtonLayout;
        protected DGuiColorTheme colorTheme = new DGuiColorTheme();

        // Geometry
        protected Vector2 hudBoxSize;
        protected Vector2 hudBoxPosition;
        protected float screenWidth;
        protected float screenHeight;



        #region Public Properties
        public MiniMap Map
        {
            get
            {
                return _miniMap;
            }
        }

        #endregion



        #region Constructor
        /// <summary>
        /// Create a starcraft-esque heads-up-display.
        /// </summary>
        /// <param name="game"></param>
        public MiniMapPanel(FactionsGame game) : base(game.GuiManager, "MiniMapPanel", null)
        {
            _engine = game;

            // Get the window dimensions
            screenWidth = _engine.Window.ClientBounds.Width;
            screenHeight = _engine.Window.ClientBounds.Height;
            hudBoxSize = new Vector2(156, 120);
            hudBoxPosition = new Vector2(0, (screenHeight - hudBoxSize.Y));


            this.Position = hudBoxPosition;
            this.Size = hudBoxSize;
            this.Alpha = alpha;
            //this.ColorTheme = colorTheme;
        }
        #endregion








        public override void ShowForm()
        {
            if (!shown)
            {
                this.Visible = false;

                _engine.StaticSceneGraph.RootNode.Children.Add(this);

                // Make a minimap!
                _miniMap = new MiniMap(_engine, _engine.CurrentLevel);

                int miniMapPadding = 5;
                Vector2 minimapSize = new Vector2(this.Size.Y - (2 * miniMapPadding),
                                                  this.Size.Y - (2 * miniMapPadding)); // doubling of Y is intentional
                Vector2 minimapPosition = new Vector2(hudBoxPosition.X + miniMapPadding,
                                                      hudBoxPosition.Y + miniMapPadding);
                _miniMap.Size = minimapSize;
                _miniMap.Position = minimapPosition;
                _miniMap.Initialize();
                _engine.StaticSceneGraph.RootNode.Children.Add(_miniMap);

                // Make minimap buttons
                Vector2 buttonPosition = new Vector2(minimapSize.X + (2 * miniMapPadding), miniMapPadding);
                int minimapButtonWidth = (int)((this.Position.X + this.Size.X - miniMapPadding) - buttonPosition.X);

                minimapButton1 = new DButton(_engine.GuiManager, buttonPosition.X, buttonPosition.Y, "1", minimapButtonWidth, minimapButtonWidth);
                minimapButton1.Initialize();
                this.AddPanel(minimapButton1);

                minimapButton2 = new DButton(_engine.GuiManager, buttonPosition.X, buttonPosition.Y + (miniMapPadding + minimapButtonWidth), "2", minimapButtonWidth, minimapButtonWidth);
                minimapButton2.Initialize();
                this.AddPanel(minimapButton2);

                minimapButton3 = new DButton(_engine.GuiManager, buttonPosition.X, buttonPosition.Y + (2 * (miniMapPadding + minimapButtonWidth)), "3", minimapButtonWidth, minimapButtonWidth);
                minimapButton3.Initialize();
                this.AddPanel(minimapButton3);

                

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
                this.Children.Remove(minimapButton1);
                this.Children.Remove(minimapButton2);
                this.Children.Remove(minimapButton3);
                _engine.StaticSceneGraph.RemoveNode(_miniMap);
                minimapButton1 = null;
                minimapButton2 = null;
                minimapButton3 = null;
                _miniMap = null;
                //this.Dispose();
            }

            base.HideForm();
        }



    }



}
