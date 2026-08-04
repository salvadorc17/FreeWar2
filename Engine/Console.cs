using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaInput = Microsoft.Xna.Framework.Input;

namespace DEngine
{
    public delegate string CommandEnteredHandler(string command);

    public class DConsole : DPanel
    {
        Engine _engine;
        
        DText _consoleLabel;
        DTextBox _consoleTextLog;
        DTextBox _consoleTextEntry;
        bool _shown = false;


        public event CommandEnteredHandler OnCommandEntered;


        #region Public Properties
        public bool Shown
        {
            get
            {
                return _shown;
            }
        }
        #endregion


        public DConsole(Engine engine)
            : base(engine.GuiManager)
        {
            _engine = engine;
            //this.Position = new Vector2(240, 32);
            this.Size = new Vector2(_engine.Window.ClientBounds.Width / 2, 3 * (_engine.Window.ClientBounds.Height / 4));
            this.FillColor = Color.SlateGray;
            this.BorderWidth = 0;
            this.Alpha = 200;
        }

        protected override void LoadContent()
        {
            int padding = 5;
            //int buttonSize = 30;

            _consoleLabel = new DText(_guiManager);
            _consoleLabel.Position = new Vector2(padding + 32, padding + 14);
            _consoleLabel.Text = "Console";
            //_consoleLabel.FontName = "LucidaConsole";
            _consoleLabel.FontColor = Color.White;
            _consoleLabel.Initialize();
            this.AddPanel(_consoleLabel);

            _consoleTextLog = new DTextBox(_guiManager);
            _consoleTextLog.Position = new Vector2(padding, _consoleLabel.Size.Y + (2 * padding));
            _consoleTextLog.Size = new Vector2(this.Size.X - (2 * padding), this.Size.Y - ((3 * padding) + 40 + _consoleLabel.Size.Y));
            _consoleTextLog.ReadOnly = true;
            _consoleTextLog.ApplyChildClipping = false;
            _consoleTextLog.FontName = "LucidaConsole";
            //_consoleTextLog.MultiLine = true;
            //_consoleTextLog.FontColor = Color.Khaki;
            _consoleTextLog.FillColor = new Color(40, 40, 50);
            _consoleTextLog.Initialize();
            this.AddPanel(_consoleTextLog);
            _consoleTextLog.Text = Log.GetLog();

            _consoleTextEntry = new DTextBox(_guiManager);
            _consoleTextEntry.Position = new Vector2(padding, this.Size.Y - (padding + 35));
            _consoleTextEntry.Size = new Vector2(this.Size.X - (2 *padding), 30);
            //_consoleTextEntry.FontColor = Color.Khaki;
            _consoleTextEntry.FillColor = new Color(40, 40, 50);
            _consoleTextEntry.FontName = "Arial";
            _consoleTextEntry.Initialize();
            _consoleTextEntry.EnterPressed += new DTextBoxEventHandler(OnEnter);
            this.AddPanel(_consoleTextEntry);

            base.LoadContent();
            this.Visible = false;
        }






        
        void OnEnter()
        {
            if (_consoleTextEntry.Text != "")
            {
                // Enter the command
                if (OnCommandEntered != null)
                {
                    string parseResponse = OnCommandEntered(_consoleTextEntry.Text);

                    // Add it to the console log
                    _consoleTextLog.Text += Environment.NewLine + parseResponse;

                    // Clear text
                    _consoleTextEntry.Text = "";
                }
            }
        }


        public void ClearLog()
        {
            _consoleTextLog.Text = "";
        }

        public void RefreshLog()
        {
            ClearLog();
            _consoleTextLog.Text = Log.GetLog();
        }


        void closeBox_OnClick()
        {
            Hide();
        }


        public void Show()
        {
            this.Visible = true;
            _engine.StaticSceneGraph.RootNode.Children.Add(this);
            _guiManager.FocusedControl = _consoleTextEntry;
            _shown = true;
        }

        public void Hide()
        {
            this.Visible = false;
            _engine.StaticSceneGraph.RemoveNode(this);
            _shown = false;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
