using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DEngine;
using DGui;


namespace FactionsGame
{
    public delegate void PlayerNameChangeEventHandler(SkirmishMenuPlayerRow sender, string value);
    public delegate void PlayerColorChangeEventHandler(SkirmishMenuPlayerRow sender, int value);

    /// <summary>
    /// A GUI item for a player row.
    /// Contains a selection box for Open, Closed or AI,
    /// and if a player has been added the combo box shows a Kick option.
    /// Specify a player name to show the latter combo box.
    /// Selecting the AI Player enables the team and color selection boxes.
    /// </summary>
    public class SkirmishMenuPlayerRow : DPanel
    {
        protected FactionsGame engine;
        
        // Parent menu
        protected SkirmishMenu skirmishMenu;
        protected DPanel parentPanel;

        // Controls
        protected DText lblName;
        protected DComboBox cmbColor;
        protected DComboBox cmbTeam;
        protected DComboBox cmbPlayerSlot;
        protected DPanel parent = null;
        protected DButton btnAiOptions = null;

        // Control Event handlers
        public event PlayerNameChangeEventHandler OnPlayerNameChange;
        public event PlayerColorChangeEventHandler OnPlayerColorChange;

        // Static attributes for all rows
        public static int StartingColorIndex;
        public static int StartingTeamIndex;
        public static int PlayerNum = 2;

        // Player attributes
        protected FactionsPlayer player = null;
        //protected string playerName = null;
        //protected int team = -1;
        //protected int playerColor = -1;
        protected bool unkickable = false;
        


        #region Public Properties
        public DPanel ParentPanel
        {
            get
            {
                return parentPanel;
            }
            set
            {
                parentPanel = value;
            }
        }
        public bool Unkickable
        {
            get
            {
                return unkickable;
            }
            set
            {
                unkickable = value;
            }
        }
        public SkirmishMenu SkirmishMenu
        {
            get
            {
                return skirmishMenu;
            }
            set
            {
                skirmishMenu = value;
            }
        }
        public FactionsPlayer Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
            }
        }
        #endregion



        #region Constructors
        public SkirmishMenuPlayerRow(FactionsGame game)
            : base(game.GuiManager)
        {
            engine = game;
            player = new FactionsPlayer(game);
            this.FillColor = Color.White;
        }

        public SkirmishMenuPlayerRow(FactionsGame game, DPanel _parent)
            : this(game)
        {
            parent = _parent;
        }
        #endregion



        #region Initialize
        public override void Initialize()
        {
            cmbPlayerSlot = new DComboBox(engine.GuiManager);
            cmbPlayerSlot.Position = new Vector2(5, 5);
            cmbPlayerSlot.Size = new Vector2(150, 28);
            cmbPlayerSlot.Initialize();
            // add to skirmish menu

            if (player.Name == null)
            {
                // Combo box for Open, Closed, Computer
                cmbPlayerSlot.AddItem("Open", null);
                cmbPlayerSlot.AddItem("Closed", null);
                cmbPlayerSlot.AddItem("AI Player", null);
                cmbPlayerSlot.Text = "Open";
            }
            else
            {
                cmbPlayerSlot.AddItem(player.Name, null);

                if (!Unkickable)
                    cmbPlayerSlot.AddItem("Kick", null);

                cmbPlayerSlot.Text = player.Name;
            }
            cmbPlayerSlot.OnChange += new ComboBoxEventHandler(cmbPlayerSlot_OnChange);
            this.Children.Add(cmbPlayerSlot);



            // Team number (alliance is based on this)
            cmbTeam = new DComboBox(engine.GuiManager);//, skirmishMenu);
            cmbTeam.Position = new Vector2(170, 4);
            cmbTeam.Size = new Vector2(70, 28);
            cmbTeam.Text = Convert.ToString(player.Team);
            cmbTeam.Initialize();
            cmbTeam.OnChange += new ComboBoxEventHandler(cmbTeam_OnChange);
            this.Children.Add(cmbTeam);

            // Give it teams!
            cmbTeam.AddItem("1", null);
            cmbTeam.AddItem("2", null);
            cmbTeam.AddItem("3", null);
            cmbTeam.AddItem("4", null);
            cmbTeam.AddItem("5", null);
            cmbTeam.AddItem("6", null);
            cmbTeam.AddItem("7", null);
            cmbTeam.AddItem("8", null);
            cmbTeam.AddItem("9", null);
            cmbTeam.AddItem("10", null);
            cmbTeam.AddItem("11", null);
            cmbTeam.AddItem("12", null);

            if (player.Team > 0)
            {
                cmbTeam.SelectedIndex = player.Team - 1;
                StartingTeamIndex = player.Team;
            }
            else
            {
                StartingTeamIndex++;
                if (StartingTeamIndex > 12)
                    StartingTeamIndex = 1;
                cmbTeam.SelectedIndex = StartingTeamIndex - 1;
            }
            player.Team = cmbTeam.SelectedIndex + 1;

            // Player color (no repeats should be allowed)
            cmbColor = new DComboBox(engine.GuiManager);//, skirmishMenu);
            cmbColor.Position = new Vector2(250, 4);
            cmbColor.Size = new Vector2(120, 28);
            cmbColor.Initialize();
            cmbColor.OnChange += new ComboBoxEventHandler(cmbColor_OnChange);
            this.Children.Add(cmbColor);

            // Give it colors!
            cmbColor.AddItem("Blue", "gui\\teamflag1");
            cmbColor.AddItem("Red", "gui\\teamflag2");
            cmbColor.AddItem("Purple", "gui\\teamflag3");
            cmbColor.AddItem("Yellow", "gui\\teamflag4");
            cmbColor.AddItem("Green", "gui\\teamflag5");
            cmbColor.AddItem("Orange", "gui\\teamflag6");
            cmbColor.AddItem("White", "gui\\teamflag7");
            cmbColor.AddItem("Brown", "gui\\teamflag8");
            cmbColor.AddItem("Gray", "gui\\teamflag9");
            cmbColor.AddItem("Aqua", "gui\\teamflag10");
            cmbColor.AddItem("Tan", "gui\\teamflag11");
            cmbColor.AddItem("Pink", "gui\\teamflag12");

            if (player.Color > 0)
            {
                cmbColor.Text = engine.PlayerColorNames[player.Color - 1];
                cmbColor.ImageName = cmbColor.Items[cmbColor.SelectedIndex].ImageName;
                StartingColorIndex = cmbColor.SelectedIndex;
            }
            else
            {
                // Randomly assign colors
                StartingColorIndex++;
                if (StartingColorIndex >= cmbColor.Items.Count)
                    StartingColorIndex = 0;
                cmbColor.SelectedIndex = StartingColorIndex;
                cmbColor.Text = cmbColor.Items[cmbColor.SelectedIndex].Text;
                cmbColor.ImageName = cmbColor.Items[cmbColor.SelectedIndex].ImageName;
            }
            player.Color = Array.IndexOf(engine.PlayerColorNames, cmbColor.Text) + 1;


            // AI options button
            btnAiOptions = new DButton(engine.GuiManager);
            btnAiOptions.Position = new Vector2(cmbColor.Position.X + cmbColor.Size.X + 10, cmbColor.Position.Y);
            btnAiOptions.Size = new Vector2(92, cmbColor.Size.Y);
            btnAiOptions.Text = "AI Options";
            btnAiOptions.FontName = "Arial";
            btnAiOptions.Initialize();
            if (player.Name == "AI Player")
                this.Children.Add(btnAiOptions);


            if (player.Name == null)
            {
                cmbColor.GreyedOut = true;
                cmbTeam.GreyedOut = true;
            }

            this.Size = new Vector2(parentPanel.Size.X - 20, 36);


            base.Initialize();
        }
        #endregion



        #region UnloadContent
        protected override void UnloadContent()
        {
            cmbTeam.Dispose();
            cmbColor.Dispose();
            if (cmbPlayerSlot != null)
                cmbPlayerSlot.Dispose();
            if (btnAiOptions != null)
                btnAiOptions.Dispose();
            base.UnloadContent();
        }
        #endregion



        #region Combo Box handlers
        void cmbColor_OnChange(string value)
        {
            player.Color = cmbColor.SelectedIndex + 1;

            if (OnPlayerColorChange != null)
                OnPlayerColorChange(this, player.Color);

            // Grey this option out for all items
            //foreach (SkirmishMenuPlayerRow item in skirmishMenu.PlayerRows)
            //{
            //    item.cmbColor.Items[cmbColor.SelectedIndex].GreyedOut = true;
            //}
        }

        void cmbTeam_OnChange(string value)
        {
            player.Team = cmbTeam.SelectedIndex + 1;
        }


        void cmbPlayerSlot_OnChange(string value)
        {
            if (OnPlayerNameChange != null)
                OnPlayerNameChange(this, value);

            if (player.Name == null)
            {
                if (value == "AI Player")
                {
                    this.Children.Add(btnAiOptions);
                    cmbColor.GreyedOut = false;
                    cmbTeam.GreyedOut = false;
                    cmbPlayerSlot.DropDownList.ClearItems();
                    // Combo box for Open, Closed, Computer
                    cmbPlayerSlot.AddItem(value, null);
                    cmbPlayerSlot.AddItem("Kick", null);
                    btnAiOptions.Visible = true;
                    player.Name = value;
                    cmbPlayerSlot.Text = value;
                    
                }
                else
                {
                    cmbColor.GreyedOut = true;
                    cmbTeam.GreyedOut = true;
                }
            }
            else
            {
                if (value == "Kick")
                {
                    player.Name = null;
                    cmbPlayerSlot.DropDownList.ClearItems();
                    if (this.Children.Contains(btnAiOptions))
                        this.Children.Remove(btnAiOptions);
                    // Combo box for Open, Closed, Computer
                    cmbPlayerSlot.AddItem("Open", null);
                    cmbPlayerSlot.AddItem("Closed", null);
                    cmbPlayerSlot.AddItem("AI Player", null);
                    cmbPlayerSlot.Text = "Open";
                }
            }
        }
        #endregion


    }
}
