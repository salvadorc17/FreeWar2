using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FactionsGame.Actors;

namespace FactionsGame
{
    /// <summary>
    /// Simple tooltip with two labels
    /// </summary>
    public class ToolTip : DForm
    {
        protected FactionsGame engine;

        protected int alpha = 140;    // Overall alpha

        protected DText _mainText;
        protected DText _subText;
        protected string _mainTextString;
        protected string _subTextString;
        protected DGuiColorTheme colorTheme;// = DGuiColorThemePresets.YellowTheme;

        bool _hideOnMouseMove = true;
        Vector2 _lastMousePos;

        RTSActor _actorTarget;




        #region Public Properties
        public string MainText
        {
            get { return _mainTextString; }
            set { _mainTextString = value; }
        }
        public string SubText
        {
            get { return _subTextString; }
            set { _subTextString = value; }
        }
        public RTSActor ActorTarget
        {
            get { return _actorTarget; }
            set { _actorTarget = value; }
        }
        #endregion



        #region Constructor
        /// <summary>
        /// Unit command panel ala starcraft/warcraft
        /// Stop, Move, Attack
        /// </summary>
        /// <param name="game"></param>
        public ToolTip(FactionsGame game)
            : base(game.GuiManager, "ToolTip", null)
        {
            engine = game;
            Size = new Vector2(175, 40);
            BorderWidth = 0;
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

                MouseState ms = Mouse.GetState();
                //Vector2 absPos = engine.AbsoluteCoordinates(ms.X, ms.Y);
                this.Position = new Vector2(ms.X + 15, ms.Y);
                //this.Size = new Vector2(70, 30);
                this.Alpha = alpha;
                //this.NeedsUpdating = true;
                engine.StaticSceneGraph.RootNode.Children.Add(this);



                _mainText = new DText(engine.GuiManager);
                _mainText.FontName = "MiramonteMedium";
                //_mainText.Size = new Vector2(this.Size.X, (this.Size.Y / 2));
                _mainText.HorizontalAlignment = DText.DHorizontalAlignment.Left;
                _mainText.Position = new Vector2(5, 10);
                _mainText.Text = _mainTextString != null ? _mainTextString : "";

                if (_actorTarget != null)
                {
                    _mainText.FontColor = _actorTarget.MaskColor;
                    _mainText.RecreateTexture();
                }
                else
                    _mainText.FontColor = Color.Black;
                _mainText.Initialize();
                this.AddPanel(_mainText);


                _subText = new DText(engine.GuiManager);
                _subText.FontName = "MiramonteTiny";
                _subText.Size = new Vector2(this.Size.X, this.Size.Y / 2);
                _subText.HorizontalAlignment = DText.DHorizontalAlignment.Left;
                _subText.Position = new Vector2(5, (this.Size.Y / 2) + 10);
                _subText.Text = _subTextString != null ? _subTextString : "";
                _subText.FontColor = Color.Black;
                _subText.Initialize();
                this.AddPanel(_subText);

                _lastMousePos = new Vector2(ms.X, ms.Y);

                if (_mainText.Size.X > _subText.Size.X)
                    this.Size = new Vector2(_mainText.Size.X + 10, this.Size.Y);
                else
                    this.Size = new Vector2(_subText.Size.X + 10, this.Size.Y);
                this.RecreateTexture();

                this.Visible = true;
            }

            base.ShowForm();
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_hideOnMouseMove)
            {
                MouseState ms = Mouse.GetState();
                Vector2 currentPos = new Vector2(ms.X, ms.Y);
                Vector2 difference = _lastMousePos - currentPos;
                //_lastMousePos = currentPos;
                if (Math.Abs(difference.Length()) > 5)
                {
                    HideForm();
                }
            }
        }


        public override void HideForm()
        {
            if (shown)
            {
                engine.StaticSceneGraph.RemoveNode(this);
                this.Children.Remove(_mainText);
                this.Children.Remove(_subText);
                _mainText = null;
                _subText = null;
                //this.Dispose();
            }

            base.HideForm();
        }



    }



}
