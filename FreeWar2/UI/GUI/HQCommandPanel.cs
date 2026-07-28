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
    /// Headquarters panel.
    /// Build drone units
    /// </summary>
    public class HQCommandPanel : DForm
    {
        protected FactionsGame engine;

        protected int alpha = 100;    // Overall alpha

        protected DLayoutFlow commandButtonLayout;
        protected Collection<DButton> commandButtons = new Collection<DButton>();
        protected DGuiColorTheme colorTheme;// = DGuiColorThemePresets.BlueTheme;



        protected DButton _droneButton;


        protected Headquarters _headquarters;

        // Geometry
        



        #region Public Properties
        public Headquarters Headquarters
        {
            get { return _headquarters; }
            set { _headquarters = value; }
        }
        #endregion



        #region Constructor
        /// <summary>
        /// Unit command panel ala starcraft/warcraft
        /// Stop, Move, Attack
        /// </summary>
        /// <param name="game"></param>
        public HQCommandPanel(FactionsGame game, Headquarters headquarters)
            : base(game.GuiManager, "HQCommand", null)
        {
            _headquarters = headquarters;
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
                hudBoxSize = new Vector2(480, 140);
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



                _droneButton = new DButton(engine.GuiManager);
                _droneButton.FontName = "MiramonteTiny";
                _droneButton.Size = commandButtonSize;
                commandButtonLayout.Add(_droneButton);
                //_stopButton.Position += this.Position;
                _droneButton.Position += new Vector2(commandButtonPadding, commandButtonPadding);
                _droneButton.Text = "Drone";
                commandButtons.Add(_droneButton);
                _droneButton.Initialize();
                this.AddPanel(_droneButton);
                _droneButton.OnClick += new DButtonEventHandler(_droneButton_OnClick);


                this.Visible = true;
            }

            base.ShowForm();
        }



        void _droneButton_OnClick(GameTime gameTime)
        {
            if (_headquarters != null)
            {
                engine.SelectionBar.Enqueue((RTSActor)engine.GetTemplateActorByName("Drone"), 60);


                // Gimme a drone!
                Drone drone = (Drone)engine.GetTemplateActorByName("Drone").Clone();
                drone.Team = engine.LocalPlayer.Team;
                drone.MaskColor = engine.PlayerColors[engine.LocalPlayer.Color - 1];
                drone.PlayerColor = engine.LocalPlayer.Color;
                drone.Position = new Vector2(_headquarters.Position.X, _headquarters.Position.Y + ((_headquarters.Size.Y * _headquarters.Scale) / 2) + (drone.Size.Y / 2));
                drone.Initialize();


                GridReference targetReference = new GridReference(_headquarters.GridReference.X, _headquarters.GridReference.Y + 4);
                int occupiedCount = 0;
                Collection<GridReference> validTargetGridRefs = engine.GetFreeNodes(targetReference, 1, out occupiedCount);

                if (occupiedCount <= 21)
                {
                    foreach (GridReference gridRef in validTargetGridRefs)
                    {
                        drone.MoveToGridLocation(gridRef, false);
                        break;
                    }

                    engine.ActorQuadTree.Insert(drone);
                    engine.ActorsSceneNode.Children.Add(drone);
                    engine.Actors.Add(drone);
                    engine.CurrentLevel.Actors.Add(drone);
                }
                else
                {
                    drone.Dispose();
                }
            }
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
                _droneButton.OnClick -= _droneButton_OnClick;

                engine.StaticSceneGraph.RemoveNode(this);
                this.Children.Remove(_droneButton);
                _droneButton = null;
                //this.Dispose();
            }

            base.HideForm();
        }



    }



}
