using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;

namespace DEngine
{
    /// <summary>
    /// Non-visible entity to represent the player.
    /// Contains lives, score, player name
    /// Works with a Match (i.e. a match has players) to trigger player spawns and
    /// to disallow input when dead or when the game state is suspended.
    /// In the future this class will be expanded to be a remoted object on the server.
    /// A client can attempt to join the server and will pass on it's own player object.
    /// All upstream communication should be done by this object and transparently.
    /// </summary>
    public class Player : GameComponent
    {
        protected int team;
        protected string name;
        protected int color;

        protected Collection<Actor> actors;

        #region Public Properties
        public int Team
        {
            get
            {
                return team;
            }
            set
            {
                team = value;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public int Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
        #endregion


        public Player(Engine game)
            : base(game)
        {

        }
    }
}
