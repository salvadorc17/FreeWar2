using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using DSceneGraph;

namespace FactionsGame
{
    /// <summary>
    /// Unit commands panel
    /// </summary>
    public class UnitCommandPanel : DForm
    {
        protected FactionsGame engine;

        protected int alpha = 100;    // Overall alpha

        protected DLayoutFlow commandButtonLayout;
        protected Collection<DButton> commandButtons = new Collection<DButton>();
        protected DGuiColorTheme colorTheme;// = DGuiColorThemePresets.BlueTheme;

        protected DButton _stopButton;
        protected DToggleButton _attackButton;
        protected DToggleButton _moveButton;


        // Geometry
        



        #region Public Properties

        #endregion



        #region Constructor
        /// <summary>
        /// Unit command panel ala starcraft/warcraft
        /// Stop, Move, Attack
        /// </summary>
        /// <param name="game"></param>
        public UnitCommandPanel(FactionsGame game)
            : base(game.GuiManager, "UnitCommand", null)
        {
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


                _moveButton = new DToggleButton(engine.GuiManager);
                _moveButton.FontName = "MiramonteTiny";
                _moveButton.Size = commandButtonSize;
                commandButtonLayout.Add(_moveButton);
                //_moveButton.Position += this.Position;
                _moveButton.Position += new Vector2(commandButtonPadding, commandButtonPadding);
                _moveButton.Text = "Move";
                _moveButton.ColorTheme = ColorTheme;
                //commandButtons.Add(_moveButton);
                _moveButton.Initialize();
                this.AddPanel(_moveButton);
                _moveButton.OnToggle += new ToggleButtonEventHandler(_moveButton_OnToggle);


                _stopButton = new DButton(engine.GuiManager);
                _stopButton.FontName = "MiramonteTiny";
                _stopButton.Size = commandButtonSize;
                commandButtonLayout.Add(_stopButton);
                //_stopButton.Position += this.Position;
                _stopButton.Position += new Vector2(commandButtonPadding, commandButtonPadding);
                _stopButton.Text = "Stop";
                commandButtons.Add(_stopButton);
                _stopButton.Initialize();
                this.AddPanel(_stopButton);
                _stopButton.OnClick += new DButtonEventHandler(_stopButton_OnClick);

                _attackButton = new DToggleButton(engine.GuiManager);
                _attackButton.FontName = "MiramonteTiny";
                _attackButton.Size = commandButtonSize;
                commandButtonLayout.Add(_attackButton);
                //_attackButton.Position += this.Position;
                _attackButton.Position += new Vector2(commandButtonPadding, commandButtonPadding);
                _attackButton.Text = "Attack";
                _attackButton.ColorTheme = ColorTheme;
                //commandButtons.Add(_attackButton);
                _stopButton.Initialize();
                this.AddPanel(_attackButton);
                _attackButton.OnToggle += new ToggleButtonEventHandler(_attackButton_OnToggle);

                engine.AttackModeChanged += new UnitCommandModeEventHandler(engine_AttackModeChanged);
                engine.MoveModeChanged += new UnitCommandModeEventHandler(engine_MoveModeChanged);

                this.Visible = true;
            }

            base.ShowForm();
        }

        void UnitCommandPanel_OnHoverExit()
        {
            engine.GuiBeingUsed = false;
        }

        void UnitCommandPanel_OnHoverEnter()
        {
            engine.GuiBeingUsed = true;
        }

        void engine_MoveModeChanged(bool commandModeStatus)
        {
            if (commandModeStatus)
            {
                _moveButton.Toggle(DButton.DButtonState.On);
                _attackButton.Toggle(DButton.DButtonState.Off);
            }
            else
            {
                _moveButton.Toggle(DButton.DButtonState.Off);
            }
        }

        void engine_AttackModeChanged(bool commandModeStatus)
        {
            if (commandModeStatus)
            {
                _attackButton.Toggle(DButton.DButtonState.On);
                _moveButton.Toggle(DButton.DButtonState.Off);
            }
            else
            {
                _attackButton.Toggle(DButton.DButtonState.Off);
            }
        }

        void _stopButton_OnClick(GameTime gameTime)
        {
            engine.StopAllSelectedUnits();
        }

        void _attackButton_OnToggle(DButton.DButtonState state)
        {
            if (state == DButton.DButtonState.On)
            {
                _attackButton.State = DButton.DButtonState.Off;
                engine.AttackModeEnabled = true;
                engine.MoveModeEnabled = false;
            }
            else
            {
                engine.AttackModeEnabled = false;
                engine.MoveModeEnabled = false;
            }
        }

        void _moveButton_OnToggle(DButton.DButtonState state)
        {
            if (state == DButton.DButtonState.On)
            {
                _attackButton.State = DButton.DButtonState.Off;
                engine.MoveModeEnabled = true;
                engine.AttackModeEnabled = false;
            }
            else
            {
                engine.MoveModeEnabled = false;
                engine.AttackModeEnabled = false;
            }
        }





        public override void HideForm()
        {
            if (shown)
            {
                this.OnHoverExit -= UnitCommandPanel_OnHoverExit;
                this.OnHoverEnter -= UnitCommandPanel_OnHoverEnter;
                engine.AttackModeChanged -= engine_AttackModeChanged;
                engine.MoveModeChanged -= engine_MoveModeChanged;
                _moveButton.OnToggle -= _moveButton_OnToggle;
                _attackButton.OnToggle -= _attackButton_OnToggle;
                _stopButton.OnClick -= _stopButton_OnClick;

                engine.StaticSceneGraph.RemoveNode(this);
                this.Children.Remove(_moveButton);
                this.Children.Remove(_stopButton);
                this.Children.Remove(_attackButton);
                _moveButton = null;
                _stopButton = null;
                _attackButton = null;
                //this.Dispose();
            }

            base.HideForm();
        }



    }



}
