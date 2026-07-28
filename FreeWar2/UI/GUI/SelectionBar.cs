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
    /// Shows building/unit status and queues
    /// </summary>
    public class SelectionBar : DForm
    {
        protected FactionsGame _engine;

        protected int alpha = 100;    // Overall alpha

        protected DLayoutFlow _buttonQueueLayout;
        protected Collection<DButton> _queueButtons = new Collection<DButton>();
        protected Collection<DPanel> _queueButtonPanels = new Collection<DPanel>();
        protected DGuiColorTheme _colorTheme;// = DGuiColorThemePresets.BlueTheme;
        protected DProgressBar _progressBar;
        protected List<RTSActor> _actorBuildQueue = new List<RTSActor>();

        protected DPanel _unitPanel;
        protected DText _unitText;
        protected DText _unitHealthText;

        Vector2 _queueOffset = new Vector2(160, 20);

        protected int _queueButtonCount = 12;

        protected RTSActor _actor;

        int _buildCounter = 0;
        int _buildTime;


        #region Public Properties
        public RTSActor Actor
        {
            get { return _actor; }
            set { _actor = value; }
        }
        #endregion



        #region Constructor
        /// <summary>
        /// Unit command panel ala starcraft/warcraft
        /// Stop, Move, Attack
        /// </summary>
        /// <param name="game"></param>
        public SelectionBar(FactionsGame game)
            : base(game.GuiManager, "SelectionBar", null)
        {
            _engine = game;

            Vector2 hudBoxSize;
            Vector2 hudBoxPosition;
            float screenWidth;
            float screenHeight;

            // Get the window dimensions
            screenWidth = _engine.Window.ClientBounds.Width;
            screenHeight = _engine.Window.ClientBounds.Height;
            hudBoxSize = new Vector2(256, 200);
            hudBoxPosition = new Vector2(0, (screenHeight - hudBoxSize.Y));

            float middleHudHeightScale = 0.8f;

            this.Position = new Vector2(hudBoxSize.X, (screenHeight - (hudBoxSize.Y * middleHudHeightScale)));
            this.Size = new Vector2((screenWidth - (2f * hudBoxSize.X)), (hudBoxSize.Y * middleHudHeightScale));
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


                this.Alpha = alpha;
                //this.ColorTheme = _colorTheme;
                _engine.StaticSceneGraph.RootNode.Children.Add(this);
                this.OnHoverEnter += new DPanelHoverHandler(UnitCommandPanel_OnHoverEnter);
                this.OnHoverExit += new DPanelHoverHandler(UnitCommandPanel_OnHoverExit);


                _buttonQueueLayout = new DLayoutFlow(_queueButtonCount, 1, DLayoutFlow.DLayoutFlowStyle.Horizontally);
                Vector2 commandButtonSize = new Vector2(40, 40);
                _buttonQueueLayout.CellPadding = 5;
                _buttonQueueLayout.CellWidth = (int)commandButtonSize.X;
                _buttonQueueLayout.CellHeight = (int)commandButtonSize.Y;

                

                for (int i = 0; i < _queueButtonCount; i++)
                {
                    DPanel panel = new DPanel(_engine.GuiManager);
                    _buttonQueueLayout.Add(panel);
                    panel.Position += _queueOffset;
                    //panel.ColorTheme = DGuiColorThemePresets.GreyTheme;
                    panel.Size = commandButtonSize;
                    panel.Initialize();
                    _queueButtonPanels.Add(panel);
                    this.AddPanel(panel);
                }


                _progressBar = new DProgressBar(_engine.GuiManager);
                _progressBar.Position = _queueOffset + new Vector2(0, commandButtonSize.Y + 10);
                _progressBar.Size = new Vector2(_queueButtonCount * (commandButtonSize.X + _buttonQueueLayout.CellPadding), 40);
                _progressBar.Initialize();
                _progressBar.Value = 0;
                this.AddPanel(_progressBar);

                _unitPanel = new DPanel(_engine.GuiManager);
                _unitPanel.Position = new Vector2(20, 20);
                //_unitPanel.ColorTheme = DGuiColorThemePresets.GreyTheme;
                float combinedHeight = (_progressBar.Position.Y + _progressBar.Size.Y) - _queueOffset.Y;
                _unitPanel.Size = new Vector2(combinedHeight, combinedHeight);
                _unitPanel.Initialize();
                this.AddPanel(_unitPanel);

                if (_actor != null)
                {
                    _unitText = new DText(_engine.GuiManager);
                    _unitText.Position = new Vector2(_unitPanel.Position.X + (_unitPanel.Size.X / 2), _unitPanel.Position.Y + _unitPanel.Size.Y + 10);
                    _unitText.Text = _actor.Name;
                    _unitText.FontName = "Miramonte";
                    _unitText.Initialize();
                    this.AddPanel(_unitText);

                    _unitHealthText = new DText(_engine.GuiManager);
                    _unitHealthText.Position = new Vector2(_unitPanel.Position.X + (_unitPanel.Size.X / 2), _unitText.Position.Y + _unitText.Size.Y + 10);
                    _unitHealthText.Text = _actor.Health.ToString() + " / " + _actor.MaxHealth.ToString();
                    _unitHealthText.FontName = "Miramonte";
                    _unitHealthText.FontColor = Color.Lime;
                    _unitHealthText.Initialize();
                    this.AddPanel(_unitHealthText);
                }

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

                for (int i = 0; i < _queueButtonCount; i++)
                {
                    this.Children.Remove(_queueButtonPanels[i]);
                    //this.Children.Remove(_queueButtons[i]);
                }
                _queueButtons.Clear();
                _queueButtonPanels.Clear();

                _engine.StaticSceneGraph.RemoveNode(this);
                //this.Dispose();


            }

            base.HideForm();
        }



        public void Enqueue(RTSActor actor, int timer)
        {
            if (_actorBuildQueue.Count < _queueButtonCount)
            {
                _actorBuildQueue.Add(actor);

                DPanel panel = _queueButtonPanels[_actorBuildQueue.Count - 1];
                DButton button = new DButton(_engine.GuiManager);
                button.FontName = "MiramonteTiny";
                button.Position = panel.Position;
                button.Size = panel.Size;
                button.Text = actor.Name;
                button.Initialize();
                _queueButtons.Add(button);
                this.AddPanel(button);
            }
        }

    }



}
