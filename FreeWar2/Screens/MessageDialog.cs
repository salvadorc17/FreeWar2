using System;
using System.Collections.Generic;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Configuration;

namespace FactionsGame
{
    /// <summary>
    /// Dialog box for notices and OK/Cancel  /   Yes/No
    /// </summary>
    public class MessageDialog : DForm
    {
        public enum MessageDialogButtons
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel
        };

        public enum MessageDialogResult
        {
            OK,
            Cancel,
            Yes,
            No
        };

        public delegate void MessageDialogCloseHandler(object sender, MessageDialogResult result);


        // Geometry constants
        protected const int BUTTON_WIDTH = 100;
        protected const int BUTTON_HEIGHT = 35;
        protected const int PANEL_PADDING = 20;

        // Engine and settings
        protected FactionsGame game;

        // Controls
        protected DLayoutFlow _layout;
        protected List<DButton> _buttons;
        protected DText _lblMessage;
        protected DText _lblCaption;
        protected MessageDialogButtons _buttonsType = MessageDialogButtons.OK;
        protected MessageDialogResult _dialogResult = MessageDialogResult.Cancel;

        protected string _caption = " ";
        protected string _message = " ";

        public event MessageDialogCloseHandler DialogClosed;


        #region Public Properties
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
            }
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        public MessageDialogResult DialogResult
        {
            get
            {
                return _dialogResult;
            }
            set
            {
                _dialogResult = value;
            }
        }
        #endregion


        #region Constructors
        public MessageDialog(FactionsGame factionsGame, string messageText, MessageDialogButtons dialogButtons, string caption)
            : this(factionsGame, messageText, dialogButtons)
        {
            _caption = caption;
        }

        public MessageDialog(FactionsGame factionsGame, string messageText, MessageDialogButtons dialogButtons)
            : this(factionsGame, messageText)
        {
            _buttonsType = dialogButtons;
        }

        public MessageDialog(FactionsGame factionsGame, string messageText)
            : base(factionsGame.GuiManager, "MessageDialog", null)
        {
            _message = messageText;
            Name = "MessageDialog";
            game = factionsGame;
        }
        #endregion



        #region ShowForm
        /// <summary>
        /// Create menu items and attach them to the scenegraph.
        /// </summary>
        public override void ShowForm()
        {
            this.Visible = false;

            base.ShowForm();

            // Get center screen coords
            Vector2 centerScreen = new Vector2(game.Window.ClientBounds.Width / 2f,
                game.Window.ClientBounds.Height / 2f);

            // Setup form
            this.Size = new Vector2(game.Window.ClientBounds.Width * 0.3f, game.Window.ClientBounds.Height * 0.16f);
            this.Position = centerScreen - (this.Size / 2);
            this.FillColor = Color.GhostWhite;
            this.BorderColor = Color.Black;
            this.BorderWidth = 1;




            // Setup buttons
            switch (_buttonsType)
            {
                case MessageDialogButtons.OK:
                    DButton okButton = new DButton(game.GuiManager,
                        (this.Size.X / 2) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "OK", BUTTON_WIDTH, BUTTON_HEIGHT);

                    okButton.OnClick += new DButtonEventHandler(okButton_OnClick);
                    this.AddPanel(okButton);
                    okButton.Initialize();
                    break;
                case MessageDialogButtons.OKCancel:
                    okButton = new DButton(game.GuiManager,
                        (this.Size.X / 3) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "OK", BUTTON_WIDTH, BUTTON_HEIGHT);

                    okButton.OnClick += new DButtonEventHandler(okButton_OnClick);
                    this.AddPanel(okButton);
                    okButton.Initialize();

                    DButton closeButton = new DButton(game.GuiManager,
                        (2 * (this.Size.X / 3)) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "Cancel", BUTTON_WIDTH, BUTTON_HEIGHT);

                    closeButton.OnClick += new DButtonEventHandler(closeButton_OnClick);
                    this.AddPanel(closeButton);
                    closeButton.Initialize();
                    break;
                case MessageDialogButtons.YesNo:
                    okButton = new DButton(game.GuiManager,
                        (this.Size.X / 3) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "Yes", BUTTON_WIDTH, BUTTON_HEIGHT);

                    okButton.OnClick += new DButtonEventHandler(okButton_OnClick);
                    this.AddPanel(okButton);
                    okButton.Initialize();

                    closeButton = new DButton(game.GuiManager,
                        (2 * (this.Size.X / 3)) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "No", BUTTON_WIDTH, BUTTON_HEIGHT);

                    closeButton.OnClick += new DButtonEventHandler(closeButton_OnClick);
                    this.AddPanel(closeButton);
                    closeButton.Initialize();
                    break;
                case MessageDialogButtons.YesNoCancel:
                    okButton = new DButton(game.GuiManager,
                        (this.Size.X / 3) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "Yes", BUTTON_WIDTH, BUTTON_HEIGHT);

                    okButton.OnClick += new DButtonEventHandler(okButton_OnClick);
                    this.AddPanel(okButton);
                    okButton.Initialize();

                    closeButton = new DButton(game.GuiManager,
                        (2 * (this.Size.X / 3)) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "No", BUTTON_WIDTH, BUTTON_HEIGHT);

                    closeButton.OnClick += new DButtonEventHandler(closeButton_OnClick);
                    this.AddPanel(closeButton);
                    closeButton.Initialize();

                    DButton cancelButton = new DButton(game.GuiManager,
                        (3 * (this.Size.X / 3)) - (BUTTON_WIDTH / 2),
                        (this.Size.Y) - (BUTTON_HEIGHT + PANEL_PADDING), "Cancel", BUTTON_WIDTH, BUTTON_HEIGHT);

                    cancelButton.OnClick += new DButtonEventHandler(cancelButton_OnClick);
                    this.AddPanel(cancelButton);
                    cancelButton.Initialize();
                    break;
                default:
                    break;
            }






            _lblCaption = new DText(game.GuiManager, PANEL_PADDING, PANEL_PADDING, _caption);
            _lblCaption.FontColor = Color.Blue;
            _lblCaption.Position = new Vector2(PANEL_PADDING + (_lblCaption.Size.X / 2), PANEL_PADDING);
            this.AddPanel(_lblCaption);
            _lblCaption.Initialize();


            _lblMessage = new DText(game.GuiManager, PANEL_PADDING, PANEL_PADDING, _message);
            _lblMessage.FontName = "Miramonte";
            _lblMessage.Position = new Vector2(PANEL_PADDING + (_lblMessage.Size.X), (PANEL_PADDING * 3));
            this.AddPanel(_lblMessage);
            _lblMessage.Initialize();


            this.Initialize();
            this.RecreateTexture();
            game.StaticSceneGraph.RootNode.Children.Add(this);


            this.Visible = true;
        }

        

        
        #endregion



        #region HideForm
        /// <summary>
        /// Remove all the menu objects from the scenegraph.
        /// </summary>
        public override void HideForm()
        {
            if (DialogClosed != null)
                DialogClosed(this, _dialogResult);

            base.HideForm();

            game.StaticSceneGraph.RemoveNode(_lblCaption);
            game.StaticSceneGraph.RemoveNode(_lblMessage);
            game.StaticSceneGraph.RemoveNode(this);

            _lblCaption.Dispose();
            _lblMessage.Dispose();
        }
        #endregion







        #region Button Event Handlers
        void okButton_OnClick(GameTime gameTime)
        {
            game.PlaySound("ButtonClick");

            if (_buttonsType == MessageDialogButtons.OK || _buttonsType == MessageDialogButtons.OKCancel)
                _dialogResult = MessageDialogResult.OK;
            else if (_buttonsType == MessageDialogButtons.YesNo || _buttonsType == MessageDialogButtons.YesNoCancel)
                _dialogResult = MessageDialogResult.Yes;


            HideForm();
            this.Dispose();
        }

        void closeButton_OnClick(GameTime gameTime)
        {
            game.PlaySound("ButtonClick");

            if (_buttonsType == MessageDialogButtons.OKCancel)
                _dialogResult = MessageDialogResult.Cancel;
            else if (_buttonsType == MessageDialogButtons.YesNo)
                _dialogResult = MessageDialogResult.No;

            HideForm();
            this.Dispose();
        }

        void cancelButton_OnClick(GameTime gameTime)
        {

            if (_buttonsType == MessageDialogButtons.YesNoCancel)
                _dialogResult = MessageDialogResult.Cancel;

            HideForm();
            this.Dispose();
        }
        #endregion

    }
}
