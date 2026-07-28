using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace DEngine
{
    /// <summary>
    /// Object containing a grid of tiles of a width and height,
    /// transition tiles and actors within the level.
    /// </summary>
    public class Level
    {
        Engine engine;
        public int Width;  // Grid size
        public int Height;
        
        // Level entities
        protected Collection<Tile> tiles = null;
        protected Collection<Actor> actors = null;
        protected Collection<TransitionOverlayTile> transitionTiles = null;
        protected Collection<Actor> startPoints = null;
        protected string levelName = null;
        protected string levelTitle = null;


        #region Public Properties
        public string LevelTitle
        {
            get
            {
                return levelTitle;
            }
            set
            {
                levelTitle = value;
            }
        }
        public string LevelName
        {
            get
            {
                return levelName;
            }
            set
            {
                levelName = value;
            }
        }
        public Collection<Tile> Tiles
        {
            get
            {
                return tiles;
            }
            set
            {
                tiles = value;
            }
        }
        public Collection<Actor> Actors
        {
            get
            {
                return actors;
            }
            set
            {
                actors = value;
            }
        }
        public Collection<TransitionOverlayTile> TransitionTiles
        {
            get
            {
                return transitionTiles;
            }
            set
            {
                transitionTiles = value;
            }
        }
        public Collection<Actor> StartPoints
        {
            get
            {
                return startPoints;
            }
            set
            {
                startPoints = value;
            }
        }
        #endregion



        #region Constructor
        public Level(Engine e, string fileName)
        {
            levelName = fileName;
            engine = e;
            tiles = new Collection<Tile>();
            actors = new Collection<Actor>();
            startPoints = new Collection<Actor>();
            transitionTiles = new Collection<TransitionOverlayTile>();
        }
        #endregion






        #region Dispose
        public void Dispose()
        {
            foreach (Tile t in tiles)
            {
                engine.SceneGraph.RemoveNode(t);
            }
            foreach (Actor a in actors)
            {
                engine.SceneGraph.RemoveNode(a);
            }
            engine.Levels.Remove(this);
        }
        #endregion




        //public string ToXnaXml()
        //{
        //    MemoryStream ms = new MemoryStream();
        //    XmlTextWriter w = new XmlTextWriter(ms, Encoding.UTF8);
        //    w.Formatting = Formatting.Indented;
        //    IntermediateSerializer.Serialize(w, this, "simple");
        //    w.Flush();
        //    ms.Seek(0, SeekOrigin.Begin);
        //    TextReader r = new StreamReader(ms);
        //    return r.ReadToEnd();
        //}

    }
}
