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
    /// Drone command panel.
    /// Build structures!
    /// </summary>
    public class DroneCommandPanel : DForm
    {
        protected FactionsGame engine;

        protected int alpha = 100;    // Overall alpha

        protected DLayoutFlow commandButtonLayout;
        protected Collection<DButton> commandButtons = new Collection<DButton>();
        protected DGuiColorTheme colorTheme;// = DGuiColorThemePresets.BlueTheme;



        protected DButton _hqButton;
        protected DButton _barracksButton;


        protected Drone _drone;

        // Geometry
        



        #region Public Properties
        public Drone Drone
        {
            get { return _drone; }
            set { _drone = value; }
        }
        #endregion



        #region Constructor
        /// <summary>
        /// Unit command panel ala starcraft/warcraft
        /// Stop, Move, Attack
        /// </summary>
        /// <param name="game"></param>
        public DroneCommandPanel(FactionsGame game, Drone drone)
            : base(game.GuiManager, "DroneCommandPanel", null)
        {
            _drone = drone;
            engine = game;
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

                Vector2 hudBoxSize;
                Vector2 hudBoxPosition;
                float screenWidth;
                float screenHeight;

                // Get the window dimensions
                screenWidth = engine.Window.ClientBounds.Width;
                screenHeight = engine.Window.ClientBounds.Height;
                hudBoxSize = new Vector2(256, 200);
                hudBoxPosition = new Vector2(0, (screenHeight - hudBoxSize.Y));

                int rows = 3;
                int columns = 4;
                int commandButtonPadding = 3;

                // Right panel (commands)
                this.Position = new Vector2((screenWidth - hudBoxSize.X), (screenHeight - hudBoxSize.Y));
                this.Size = new Vector2(hudBoxSize.X, hudBoxSize.Y);
                this.Alpha = alpha;
                //this.ColorTheme = colorTheme;
                engine.StaticSceneGraph.RootNode.Children.Add(this);
                this.OnHoverEnter += new DPanelHoverHandler(UnitCommandPanel_OnHoverEnter);
                this.OnHoverExit += new DPanelHoverHandler(UnitCommandPanel_OnHoverExit);


                commandButtonLayout = new DLayoutFlow(columns, rows, DLayoutFlow.DLayoutFlowStyle.Horizontally);
                Vector2 commandButtonSize = new Vector2((this.Size.X - ((columns + 1) * commandButtonPadding)) / columns,
                                                        (this.Size.Y - ((rows + 1) * commandButtonPadding)) / rows);
                commandButtonLayout.CellPadding = commandButtonPadding;
                commandButtonLayout.CellWidth = (int)commandButtonSize.X;
                commandButtonLayout.CellHeight = (int)commandButtonSize.Y;



                _hqButton = new DButton(engine.GuiManager);
                _hqButton.FontName = "Arial";
                _hqButton.Size = commandButtonSize;
                commandButtonLayout.Add(_hqButton);
                //_stopButton.Position += this.Position;
                _hqButton.Position += new Vector2(commandButtonPadding, commandButtonPadding);
                _hqButton.Text = "HQ";
                commandButtons.Add(_hqButton);
                _hqButton.Initialize();
                this.AddPanel(_hqButton);
                _hqButton.OnClick += new DButtonEventHandler(_hqButton_OnClick);


                _barracksButton = new DButton(engine.GuiManager);
                _barracksButton.FontName = "Arial";
                _barracksButton.Size = commandButtonSize;
                commandButtonLayout.Add(_barracksButton);
                //_stopButton.Position += this.Position;
                _barracksButton.Position += new Vector2(commandButtonPadding, commandButtonPadding);
                _barracksButton.Text = "Barracks";
                commandButtons.Add(_barracksButton);
                _barracksButton.Initialize();
                this.AddPanel(_barracksButton);
                _barracksButton.OnClick += new DButtonEventHandler(_barracksButton_OnClick);

                this.Visible = true;
            }

            base.ShowForm();
        }

        void _barracksButton_OnClick(GameTime gameTime)
        {
 
        }



        void _hqButton_OnClick(GameTime gameTime)
        {
            
        }



        void UnitCommandPanel_OnHoverExit()
        {
            engine.GuiBeingUsed = false;
        }

        void UnitCommandPanel_OnHoverEnter()
        {
            engine.GuiBeingUsed = true;
        }



        public override void HideForm()
        {
            if (shown)
            {
                this.OnHoverExit -= UnitCommandPanel_OnHoverExit;
                this.OnHoverEnter -= UnitCommandPanel_OnHoverEnter;
                _hqButton.OnClick -= _hqButton_OnClick;
                _barracksButton.OnClick -= _barracksButton_OnClick;

                engine.StaticSceneGraph.RemoveNode(this);
                this.Children.Remove(_hqButton);
                this.Children.Remove(_barracksButton);
                _hqButton = null;
                _barracksButton = null;
                //this.Dispose();
            }

            base.HideForm();
        }



    }



}
