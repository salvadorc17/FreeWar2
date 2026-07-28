using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using DEngine;

namespace FactionsGame
{
    /// <summary>
    /// A generic game match (i.e. a boxing match). Acts like a referee.
    /// Monitors gamestate for game happenings like a flag being captured or kill being made.
    /// Updates the game HUD.
    /// Can control the gamestate (i.e. trigger a level load, disallow shooting)
    /// </summary>
    public class Match : GameComponent
    {
        protected string _levelFile;
        protected List<Player> _players = new List<Player>();

        #region Public Properties
        public string LevelFile
        {
            get
            {
                return _levelFile;
            }
            set
            {
                _levelFile = value;
            }
        }
        public List<Player> Players
        {
            get
            {
                return _players;
            }
            set
            {
                _players = value;
            }
        }
        #endregion


        public Match(Game game)
            : base(game)
        {

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
        }
    }
}
