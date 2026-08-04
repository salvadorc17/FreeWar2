using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using DEngine;

namespace DEngine
{
    public delegate void LoadFileTickHandler(int current, int max);

    /// <summary>
    /// An effort to centralize the save/load functionality of the engine in a single class.
    /// Mostly XML files.
    /// </summary>
    public class EngineIO
    {
        protected Engine _engine;
        protected string _allowedImageTypes = "*.xnb";

        public event LoadFileTickHandler OnLoadTick;


        #region Constructor
        public EngineIO(Engine engine)
        {
            _engine = engine;
        }
        #endregion



        #region Load Actor Templates
        /// <summary>
        /// Load a single instance of each actor into the template list.
        /// Template actors are not added to the scene graph or the physics simulation.
        /// The purpose of template actors is to have a "ready to go" actor to add to the game on the fly.
        /// Upon adding an actor to the game field, the engine will clone the appropriate template actor and initialize it.
        /// </summary>
        public void LoadActorTemplates()
        {
            // This loads all actor xml files.
            // Get actors, sprites, textures and sounds and adds them to template actor index
            _engine.ActorTemplates.Clear();
            string[] actorFolderNames = FileAccess.GetAllActorNames();
            if (actorFolderNames != null)
            {
                foreach (string actorName in actorFolderNames)
                {
                    // Look for an xml file
                    string xmlPath = FileAccess.GetActorXmlFile(actorName);
                    if (File.Exists(xmlPath))
                    {
                        //ActorContentData actorData = engine.Content.Load<ActorContentData>(xmlPath);
                        //Actor actor = new Actor(engine, actorName);
                        //actor.EditorVisibleOnly = actorData.EditorObject;
                        //actor.Size = actorData.Size;
                        //actor.Scale = actorData.Scale;

                        //engine.AddTemplateActor(actor);

                        Actor actor = new Actor(_engine, actorName);
                        Sprite sprite = null;
                        XmlTextReader textReader = null;

                        try
                        {
                            textReader = new XmlTextReader(xmlPath);


                            // Read until end of file
                            while (textReader.Read())
                            {
                                XmlNodeType nType = textReader.NodeType;
                                // if node type is an attribute
                                switch (nType)
                                {
                                    case XmlNodeType.Element:

                                        if (textReader.Name == "Actor")
                                        {
                                            // Pretty unsafe code, all of this!
                                            float width = Convert.ToSingle(textReader.GetAttribute("width"));
                                            float height = Convert.ToSingle(textReader.GetAttribute("height"));
                                            float scale = Convert.ToSingle(textReader.GetAttribute("scale"));

                                            bool editorVisible = true;
                                            if (textReader.GetAttribute("editorobject") != null)
                                                editorVisible = Convert.ToBoolean(textReader.GetAttribute("editorobject"));

                                            actor.Size = new Vector2(width, height);
                                            actor.Scale = scale;
                                            actor.EditorVisibleOnly = editorVisible;
                                        }
                                        // Start of a Sprite element
                                        if (textReader.Name == "Sprite")
                                        {
                                            if (sprite == null)
                                                sprite = new Sprite(_engine, actorName);

                                            // Get the sprite properties
                                            string spriteName = textReader.GetAttribute("name");
                                            int animSpeed = Convert.ToInt32(textReader.GetAttribute("speed"));
                                            bool loop = Convert.ToBoolean(textReader.GetAttribute("loop"));
                                            bool flipX = Convert.ToBoolean(textReader.GetAttribute("flipX"));
                                            bool flipY = Convert.ToBoolean(textReader.GetAttribute("flipY"));

                                            //string vAlign = textReader.GetAttribute("valign");
                                            //string hAlign = textReader.GetAttribute("halign");

                                            if (spriteName != null)
                                            {
                                                // Add the sprite name to the local stack
                                                sprite.Name = spriteName;
                                                sprite.AnimationSpeed = animSpeed;
                                                sprite.LoopAnimation = loop;
                                                if (flipX)
                                                    sprite.SpriteEffects |= Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                                                if (flipY)
                                                    sprite.SpriteEffects |= Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;

                                                sprite.HorizontalAlign = Sprite.DSpriteHorizontalAlign.Center;
                                                sprite.VerticalAlign = Sprite.DSpriteVerticalAlign.Bottom;
                                            }
                                        }

                                            // Start of an image element
                                        else if (textReader.Name == "Image")
                                        {
                                            // Get image name and current sprite name
                                            string imageName = textReader.GetAttribute("name");
                                            int originX = Convert.ToInt32(textReader.GetAttribute("originX"));
                                            int originY = Convert.ToInt32(textReader.GetAttribute("originY"));

                                            // Add to the sprite/image names lookup and load the image
                                            if (sprite != null && imageName != null)
                                            {
                                                sprite.AddFrame(actorName, imageName, new Vector2(originX, originY));
                                            }
                                        }
                                        else if (textReader.Name == "Sound")
                                        {
                                            string soundName = textReader.GetAttribute("name");
                                            string soundPath = textReader.GetAttribute("path");

                                            actor.Sounds.Add(soundName, soundPath);
                                        }


                                        break;
                                    case XmlNodeType.EndElement:
                                        // Pop the sprite name off the local stack
                                        if (textReader.Name == "Sprite")
                                        {
                                            if (sprite != null)
                                                actor.Sprites.Add(sprite);
                                            sprite = null;
                                        }
                                        break;
                                    case XmlNodeType.Attribute:
                                        break;
                                    case XmlNodeType.Whitespace:
                                        break;
                                    case XmlNodeType.Text:
                                        break;
                                    default:
                                        break;
                                }
                            }

                            // Add the actor to the template list!
                            //actorTemplates.Add(actor);
                            _engine.AddTemplateActor(actor);
                        }
                        catch (Exception e)
                        {
                            Log.Message("Error loading actor xml file " + FileAccess.GetFileNameFromPath(xmlPath) + ": " + e.Message);
                        }
                        finally
                        {
                            if (textReader != null)
                                textReader.Close();
                        }
                    }
                    else // XML file doesn't exist
                    {
                        Log.Message("Couldn't find XML file for actor: " + actorName);
                    }
                }

                Log.Message(_engine.ActorTemplates.Count + " actor templates loaded.");
            }
        }
        #endregion


        #region Load Tile Templates
        /// <summary>
        /// Load a single instance of each tile into the template list.
        /// Template tiles are not added to the scene graph or the physics simulation.
        /// The purpose of template tiles is to have a "ready to go" tile to add to the game on the fly.
        /// Upon adding a tile to the game field, the engine will clone the appropriate template tile and initialize it.
        /// </summary>
        public void LoadTileTemplates()
        {
            string tilesDir = FileAccess.GetTilesDir();
            try
            {
                string[] extensions = _allowedImageTypes.Split(';');
                foreach (string ext in extensions)
                {
                    string[] tileFiles = Directory.GetFiles(tilesDir, ext);
                    foreach (string tileName in tileFiles)
                    {
                        Tile t = new Tile(_engine);

                        // Clip extension
                        FileInfo file = new FileInfo(tileName);
                        t.ImageName = file.Name.Replace(file.Extension, "");
                        t.Precedence = _engine.TilePrecedenceOrder.IndexOf(t.ImageName);  // lookup precedence
                        _engine.TileTemplates.Add(t);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Message("Could not load tile templates: " + e.Message);
            }
            finally
            {
                Log.Message(_engine.TileTemplates.Count + " tile templates loaded.");
            }
        }
        #endregion


        #region Load Overlay Tiles and Precedence Order
        /// <summary>
        /// Load an XML precedence map of terrain types to allow drawing of smooth terrain transitions.
        /// See http://www.gamedev.net/reference/articles/article934.asp
        /// Or http://disposableheroes.dyndns.org:8084/wiki/index.php/2D_Game_Engines#Tile_Terrain_Transitions_-_proposed_XML_format
        /// 
        /// 
        /// The easiest way would be to have the editor create/destroy and render the transition tiles during level creation.
        /// This would be similar to the Starcraft or Warcraft II map editors which enforced proper terrain transitions.
        /// The necessary transition tiles would have to be embedded in the XML level definition.
        /// The editor would also save the tiles in the proper precedence order so once loaded by the engine, they will render
        /// in the correct order without any extra work being done by the engine.
        /// 
        /// The editor itself will need access to this precedence order so that when terrain is added or removed, adjacent tiles
        /// will have transition tiles added or removed accordingly. This will probably add up to a lot of calculations.
        /// 
        /// Possibly this function should load the overlay tiles first as TransitionOverlayTile template objects then load all other
        /// images in the tiles directory as Tile objects.
        /// (what about if we want collision off these transition tiles? don't we basically want just another Tile then?)
        /// A Tile must contain a set of TransitionOverlayTiles if it is adjacent to different terrain. The Tile will render its own
        /// transition tiles when it it called upon to be drawn. This way we can simply render the terrain types in reverse order of precedence
        /// and the terrain will render correctly.
        /// </summary>
        public void LoadOverlayTilesAndPrecedenceOrder()
        {
            string tileTransitionsDir = FileAccess.GetTileTransitionsDir();
            string precedenceFile = Path.Combine(tileTransitionsDir, "Transitions.xml");
            bool tileExists = false; // a little extra checking
            int transitionTileTemplateCount = 0;

            if (File.Exists(precedenceFile))
            {
                XmlTextReader textReader = null;
                try
                {
                    textReader = new XmlTextReader(precedenceFile);
                    Tile tileTemplate = null;

                    // Read until end of file
                    while (textReader.Read())
                    {
                        XmlNodeType nType = textReader.NodeType;


                        // if node type us an attribute
                        switch (nType)
                        {
                            case XmlNodeType.Element:

                                if (textReader.Name == "DTileTransitions")
                                {
                                    // Lets us know we've got a valid XML file
                                }
                                if (textReader.Name == "Tile")
                                {
                                    // Establish precedence order

                                    string tileName = textReader.GetAttribute("name").ToString();

                                    // Ensure we have a valid tile (LoadTileTemplates precedes this)
                                    // This is just a bit of extra checking
                                    foreach (Tile t in _engine.TileTemplates)
                                    {
                                        if (t.ImageName == tileName)
                                        {
                                            tileTemplate = t;
                                            tileExists = true;
                                            break;
                                        }
                                    }

                                    // Add to the ordered list of tile precedence
                                    if (tileExists)
                                    {
                                        _engine.TilePrecedenceOrder.Add(tileName);

                                        // Give the tile template its new precedence order
                                        tileTemplate.Precedence = _engine.TilePrecedenceOrder.IndexOf(tileName);

                                        // Reset tile existence check
                                        tileExists = false;
                                    }
                                    else
                                    {
                                        Log.Message("Unknown tile in Transitions.xml: " + tileName);
                                    }
                                }
                                if (textReader.Name == "Overlay")
                                {
                                    // Create template transition tile!
                                    string overlayTileName = textReader.GetAttribute("name").ToString();
                                    string overlayTilePosition = textReader.GetAttribute("position").ToString();
                                    string overlayTileFile = Path.Combine(tileTransitionsDir, overlayTileName + ".xnb"); // XNA format
                                    if (File.Exists(overlayTileFile))
                                    {
                                        FileInfo file = new FileInfo(overlayTileFile);
                                        TransitionOverlayTile transitionOverlayTile = new TransitionOverlayTile(_engine);
                                        transitionOverlayTile.ImageName = file.Name.Replace(file.Extension, ""); // Clip extension (XNA)
                                        transitionOverlayTile.Precedence = _engine.TilePrecedenceOrder.IndexOf(tileTemplate.ImageName); // Give it the precedence of its parent!

                                        // The orientation of this transition tile compared to it's owner tile.
                                        switch (overlayTilePosition)
                                        {
                                            case "n":
                                                transitionOverlayTile.Orientation = TransitionPosition.North;
                                                break;
                                            case "e":
                                                transitionOverlayTile.Orientation = TransitionPosition.East;
                                                break;
                                            case "s":
                                                transitionOverlayTile.Orientation = TransitionPosition.South;
                                                break;
                                            case "w":
                                                transitionOverlayTile.Orientation = TransitionPosition.West;
                                                break;
                                            case "ne":
                                                transitionOverlayTile.Orientation = TransitionPosition.NorthEast;
                                                break;
                                            case "se":
                                                transitionOverlayTile.Orientation = TransitionPosition.SouthEast;
                                                break;
                                            case "sw":
                                                transitionOverlayTile.Orientation = TransitionPosition.SouthWest;
                                                break;
                                            case "nw":
                                                transitionOverlayTile.Orientation = TransitionPosition.NorthWest;
                                                break;
                                            case "ne-inner":
                                                transitionOverlayTile.Orientation = TransitionPosition.NorthEastInner;
                                                break;
                                            case "se-inner":
                                                transitionOverlayTile.Orientation = TransitionPosition.SouthEastInner;
                                                break;
                                            case "sw-inner":
                                                transitionOverlayTile.Orientation = TransitionPosition.SouthWestInner;
                                                break;
                                            case "nw-inner":
                                                transitionOverlayTile.Orientation = TransitionPosition.NorthWestInner;
                                                break;
                                            default:
                                                break;
                                        }
                                        _engine.TransitionOverlayTileTemplates.Add(transitionOverlayTile);

                                        // Also add this transition tile template to its tile template
                                        tileTemplate.TransitionOverlayTiles.Add(transitionOverlayTile);

                                        transitionTileTemplateCount++;
                                    }
                                }
                                break;
                            case XmlNodeType.EndElement:
                                break;
                            case XmlNodeType.Attribute:
                                break;
                            case XmlNodeType.Whitespace:
                                break;
                            case XmlNodeType.Text:
                                break;
                            default:
                                break;
                        }

                    }
                }
                catch (Exception e)
                {
                    Log.Message("Error loading tile transitions file " + FileAccess.GetFileNameFromPath(precedenceFile) + ": " + e.Message);
                }
                finally
                {
                    if (textReader != null)
                        textReader.Close();

                    Log.Message(transitionTileTemplateCount + " transition tile templates loaded.");
                    Log.Message("Tile precedence list contains " + _engine.TilePrecedenceOrder.Count + " items. ");
                }
            }
        }
        #endregion


        #region LoadTileProperties
        /// <summary>
        /// Load the xml file Tiles.xml in the tiles folder.
        /// Contains default properties of tiles.
        /// Currently only solidity.
        /// </summary>
        public void LoadTileProperties()
        {
            string filePath = Path.Combine(FileAccess.GetTilesDir(), "Tiles.xml");
            XmlTextReader textReader = null;
            int tilePropsCount = 0;
            try
            {
                // Read a document
                textReader = new XmlTextReader(filePath);

                // Read until end of file
                while (textReader.Read())
                {
                    XmlNodeType nType = textReader.NodeType;
                    // if node type us an attribute
                    switch (nType)
                    {
                        case XmlNodeType.Element:
                            if (textReader.Name == "Tile")
                            {
                                string tileName = textReader.GetAttribute("name");
                                bool tileSolidity = Convert.ToBoolean(textReader.GetAttribute("solid"));
                                Tile templateTile = _engine.GetTileTemplateByName(tileName);
                                if (templateTile != null)
                                {
                                    templateTile.Solid = tileSolidity;
                                }
                                tilePropsCount++;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Message("Error loading tile defs file " + FileAccess.GetFileNameFromPath(filePath) + ": " + e.Message);
            }
            finally
            {
                if (textReader != null)
                    textReader.Close();

                Log.Message("Tile properties file loaded. " + tilePropsCount + " tile property definitions found.");
            }
        }
        #endregion


        #region Load Background Templates
        /// <summary>
        /// Load plain images for use as backgrounds.
        /// </summary>
        public void LoadBackgroundTemplates()
        {
            string backgroundsDir = FileAccess.GetBackgroundsDir();
            try
            {
                foreach (string backgroundName in Directory.GetFiles(backgroundsDir))
                {
                    Background bg = new Background(_engine);

                    // Clip extension
                    FileInfo file = new FileInfo(backgroundName);
                    bg.ImageName = file.Name.Replace(file.Extension, "");
                    _engine.BackgroundTemplates.Add(bg);
                }
            }
            catch (Exception e)
            {
                Log.Message("Could not load background templates: " + e.Message);
            }
            finally
            {
                Log.Message(_engine.BackgroundTemplates.Count + " background templates loaded.");
            }
        }
        #endregion





        #region LoadLevelFromXml
        /// <summary>
        /// Loads an XML level into a Level object and adds it to the engine's Level list.
        /// Objects are not added to the scenegraph upon level load. Levels must be run before they are displayed.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public Level LoadLevelFromXml(string filepath)
        {
            // XML reader
            XmlTextReader textReader = null;

            // Debug counts
            int entityCount = 0; // total of the three below
            int tileCount = 0;
            int actorCount = 0;
            int transitionTileCount = 0;

            // Engine Tick() value
            int tickEvery = 350; // make the engine tick after each count hits a multiple of this number

            // Perform two passes.
            // Load the tiles, load the tile navigation grid, then load the actors (who use the grid when initializing).
            Level newLevel = new Level(_engine, FileAccess.GetFileNameFromPath(filepath));
            if (File.Exists(filepath))
            {
                // Reset tiles and actors
                _engine.Tiles.Clear();
                _engine.TransitionTiles.Clear();
                _engine.Actors.Clear();
                _engine.Levels.Clear();

                // Establish number of elements in the level
                int levelFileElementCount = -1; // start at negative 1 to exclude header
                textReader = new XmlTextReader(filepath);
                while (textReader.Read())
                {
                    XmlNodeType nType = textReader.NodeType;
                        // if node type us an attribute
                    switch (nType)
                    {
                        case XmlNodeType.Element:
                            levelFileElementCount++;
                            break;
                        default:
                            break;
                    }
                }
                textReader.Close();




                // Load tiles and transition tiles
                float tx = 0.0f;
                float ty = 0.0f;
                float trotation = 0f;
                string textureName = "";
                int tileId = 0;
                Tile newTile = null;
                try
                {
                    textReader = new XmlTextReader(filepath);
                    while (textReader.Read())
                    {
                        XmlNodeType nType = textReader.NodeType;
                        // if node type us an attribute
                        switch (nType)
                        {
                            case XmlNodeType.Element:

                                if (textReader.Name == "DLevel")
                                {
                                    // Load the level definition
                                    int width = Convert.ToInt32(textReader.GetAttribute("width"));
                                    int height = Convert.ToInt32(textReader.GetAttribute("height"));
                                    newLevel.Width = width;
                                    newLevel.Height = height;
                                }

                                if (textReader.Name == "Actor")
                                {
                                    string name = textReader.GetAttribute("name");
                                    int team = Convert.ToInt32(textReader.GetAttribute("team"));
                                    tx = Convert.ToSingle(textReader.GetAttribute("x"));
                                    ty = Convert.ToSingle(textReader.GetAttribute("y"));

                                    // Figure out which actor it is
                                    for (int i = 0; i < _engine.ActorTemplates.Count; i++)
                                    {
                                        Actor actor = _engine.ActorTemplates[i];
                                        if (actor.Name == name)
                                        {
                                            entityCount++;
                                            actorCount++;
                                            Actor newActor = (Actor)_engine.ActorTemplates[i].Clone();
                                            newActor.Position = new Vector2(tx, ty);
                                            newActor.Team = team;
                                            // Lookup default team color and assign it to this actor
                                            newActor.MaskColor = _engine.PlayerColors[team - 1];
                                            newActor.MaskHueValue = _engine.PlayerColorHueMask;
                                            newActor.MaskColorEnabled = true;
                                            if (_engine.EditorMode)
                                                newActor.ApplyColorMaskToSprites();

                                            newLevel.Actors.Add(newActor);

                                            // Also check if it's a start point
                                            if (name == "PlayerStart")
                                                newLevel.StartPoints.Add(newActor);

                                            newActor.Initialize();
                                            _engine.Actors.Add(newActor);
                                            break;
                                        }
                                    }
                                }

                                if (textReader.Name == "Tile")
                                {
                                    // Load a tile
                                    tileId = Convert.ToInt32(textReader.GetAttribute("id"));
                                    textureName = textReader.GetAttribute("name");
                                    tx = Convert.ToSingle(textReader.GetAttribute("x"));
                                    ty = Convert.ToSingle(textReader.GetAttribute("y"));
                                    trotation = Convert.ToSingle(textReader.GetAttribute("rotation"));

                                    // Check if tile exists and discard existing if it does
                                    //Tile existingTile = _engine.TileExistenceCheckByExactLocation(new Vector2(tx, ty));
                                    //if (existingTile != null)
                                    //{
                                    //    //_engine.SceneGraph.RemoveNode(existingTile);
                                    //    _engine.QuadTree.Remove(existingTile);
                                    //    _engine.Tiles.Remove(existingTile);
                                    //    newLevel.Tiles.Remove(existingTile);
                                    //}

                                    Tile tileTemplate = _engine.GetTileTemplateByName(textureName);
                                    if (tileTemplate != null)
                                    {
                                        entityCount++;
                                        tileCount++;
                                        newTile = tileTemplate.Clone();
                                        newTile.ID = tileId;
                                        newTile.Position = new Vector2(tx, ty);
                                        newTile.Rotation = trotation;

                                        newTile.Initialize();
                                        _engine.Tiles.Add(newTile);
                                        newLevel.Tiles.Add(newTile);
                                    }
                                }
                                if (textReader.Name == "TransitionTile")
                                {
                                    tileId = Convert.ToInt32(textReader.GetAttribute("parentId"));
                                    textureName = textReader.GetAttribute("name");
                                    tx = Convert.ToSingle(textReader.GetAttribute("x"));
                                    ty = Convert.ToSingle(textReader.GetAttribute("y"));
                                    string orientationStr = textReader.GetAttribute("orientation").ToString();
                                    TransitionPosition orientation = (TransitionPosition)Enum.Parse(typeof(TransitionPosition), orientationStr);


                                    TransitionOverlayTile overlayTile = null;

                                    // Find the transition tile template
                                    foreach (TransitionOverlayTile tt in _engine.TransitionOverlayTileTemplates)
                                    {
                                        if (tt.ImageName == textureName)
                                        {
                                            overlayTile = tt.Clone();
                                            break;
                                        }
                                    }
                                    overlayTile.Position = new Vector2(tx, ty);
                                    overlayTile.Orientation = orientation;
                                    overlayTile.Initialize();

                                    // Add it to the tile
                                    foreach (Tile t in newLevel.Tiles)
                                    {
                                        if (t.ID == tileId)
                                        {
                                            overlayTile.Parent = t;
                                            t.TransitionOverlayTiles.Add(overlayTile);
                                            transitionTileCount++;
                                            entityCount++;
                                            break;
                                        }
                                    }

                                    newLevel.TransitionTiles.Add(overlayTile);
                                }
                                break;
                            default:
                                break;
                        }

                        if (entityCount % tickEvery == 0)
                        {
                            if (OnLoadTick != null)
                                OnLoadTick(entityCount, levelFileElementCount);
                            _engine.Tick();
                        }
                    }


                    textReader.Close();
                    


                    // Add the new level to the engine's level list
                    _engine.Levels.Add(newLevel);

                    // Show level load info.
                    Log.Message("--------------------------" + Environment.NewLine +
                                "Loaded level ::: " + FileAccess.GetFileNameFromPath(filepath) + ":::" + Environment.NewLine +
                                "Level Dimensions: Width " + newLevel.Width + ", Height " + newLevel.Height + "." + Environment.NewLine +
                                "Level Entities: Tiles " + tileCount + ", Actors " + actorCount + 
                                ", TransitionTiles " + transitionTileCount + Environment.NewLine +
                                 "--------------------------" + Environment.NewLine);
                }
                catch (Exception e)
                {
                    Log.Message("Error loading level " + FileAccess.GetFileNameFromPath(filepath) + ": " + e.Message);
                }
                finally
                {
                    if (textReader != null)
                        textReader.Close();
                }
            }
            else
                Log.Message("Couldn't find level " + filepath);


            // Refesh the console log after loading
            _engine.Console.RefreshLog();

            return newLevel;
        }
        #endregion


        #region SaveLevelToXml
        public void SaveLevelToXml(string filepath)
        {
            // Choose the first level for now
            Level currentLevel = _engine.Levels[0];

            // Dump tiles/actors to XML!
            XmlTextWriter textWriter = new XmlTextWriter(filepath, null);
            // Opens the document
            textWriter.Formatting = Formatting.Indented;

            textWriter.WriteStartDocument();
            // Write comments
            textWriter.WriteComment("DEngine Level Definition");
            textWriter.WriteStartElement("DLevel");
            textWriter.WriteAttributeString("width", Convert.ToString(currentLevel.Width));
            textWriter.WriteAttributeString("height", Convert.ToString(currentLevel.Height));


            // Go through tile list
            for (int i = 0; i < _engine.Tiles.Count; i++)
            {
                Tile t = (Tile)_engine.Tiles[i];

                // Give it an ID now
                t.ID = i;

                textWriter.WriteStartElement("Tile");
                textWriter.WriteAttributeString("id", Convert.ToString(i));
                textWriter.WriteAttributeString("name", FileAccess.GetFileNameFromPath(t.ImageName));
                textWriter.WriteAttributeString("x", t.Position.X.ToString());
                textWriter.WriteAttributeString("y", t.Position.Y.ToString());
                textWriter.WriteAttributeString("rotation", t.Rotation.ToString());

                // Call our event to write any extra attributes!
                //if (OnTileAttributeWrite != null)
                //    OnTileAttributeWrite(textWriter, t);

                textWriter.WriteEndElement();
            }


            // Transition tiles 1
            for (int i = 0; i < _engine.TransitionTiles.Count; i++)
            {
                TransitionOverlayTile tt = (TransitionOverlayTile)_engine.TransitionTiles[i];
                textWriter.WriteStartElement("TransitionTile");
                textWriter.WriteAttributeString("parentId", Convert.ToString(tt.Parent.ID));
                textWriter.WriteAttributeString("name", tt.ImageName);
                textWriter.WriteAttributeString("orientation", tt.Orientation.ToString());
                textWriter.WriteAttributeString("x", tt.Position.X.ToString());
                textWriter.WriteAttributeString("y", tt.Position.Y.ToString());
                textWriter.WriteEndElement();
            }


            // Go through actor list
            foreach (Actor a in _engine.Actors)
            {
                textWriter.WriteStartElement("Actor");
                textWriter.WriteAttributeString("name", a.Name);
                textWriter.WriteAttributeString("x", a.Position.X.ToString());
                textWriter.WriteAttributeString("y", a.Position.Y.ToString());
                textWriter.WriteAttributeString("team", a.Team.ToString());

                // Call our event to write any extra attributes!
                //if (OnActorAttributeWrite != null)
                //    OnActorAttributeWrite(textWriter, a);

                // textWriter.WriteAttributeString("team", Convert.ToString(a.Team));

                textWriter.WriteEndElement();
            }

            textWriter.WriteEndElement();

            // Ends the document.
            textWriter.WriteEndDocument();

            // close writer
            textWriter.Close();
        }
        #endregion


        #region SaveActorTemplates
        /// <summary>
        /// Save actor templates to their appropriate content folders.
        /// </summary>
        public void SaveActorTemplates()
        {
            string[] actorFolderNames = FileAccess.GetAllActorNames();
            if (actorFolderNames != null)
            {
                foreach (string actorName in actorFolderNames)
                {
                    Actor templateActor = _engine.GetTemplateActorByName(actorName);
                    if (templateActor != null)
                    {
                        // Look for an xml file
                        string xmlPath = FileAccess.GetActorXmlFile(actorName);
                        if (File.Exists(xmlPath))
                        {
                            File.Copy(xmlPath, xmlPath.Replace(".xml", "-backup.xml"), true);
                        }

                        XmlTextWriter textWriter = new XmlTextWriter(xmlPath, null);
                        // Opens the document
                        textWriter.Formatting = Formatting.Indented;

                        textWriter.WriteStartDocument();
                        // Write comments
                        textWriter.WriteComment("DEngine Actor Definition");
                        textWriter.WriteStartElement("Actor");
                        textWriter.WriteAttributeString("name", templateActor.Name);
                        textWriter.WriteAttributeString("width", Convert.ToString(templateActor.Size.X));
                        textWriter.WriteAttributeString("height", Convert.ToString(templateActor.Size.Y));
                        textWriter.WriteAttributeString("scale", Convert.ToString(templateActor.Scale));

                        // Write sprites
                        foreach (Sprite sprite in templateActor.Sprites)
                        {
                            textWriter.WriteStartElement("Sprite");
                            textWriter.WriteAttributeString("name", sprite.Name);
                            textWriter.WriteAttributeString("loop", sprite.LoopAnimation.ToString());
                            textWriter.WriteAttributeString("speed", sprite.AnimationSpeed.ToString());
                            if (sprite.SpriteEffects == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally)
                                textWriter.WriteAttributeString("flipX", "true");
                            if (sprite.SpriteEffects == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically)
                                textWriter.WriteAttributeString("flipY", "true");

                            // Write sprite frames
                            for (int i = 0; i < sprite.FrameCount; i++)
                            {
                                textWriter.WriteStartElement("Image");
                                textWriter.WriteAttributeString("name", sprite.Frames[i].Texture.Name + ".png");
                                textWriter.WriteAttributeString("originX", ((int)sprite.Frames[i].DrawOrigin.X).ToString());
                                textWriter.WriteAttributeString("originY", ((int)sprite.Frames[i].DrawOrigin.Y).ToString());
                                textWriter.WriteEndElement();
                            }

                            textWriter.WriteEndElement();
                        }

                        textWriter.WriteEndElement();

                        // Ends the document.
                        textWriter.WriteEndDocument();

                        // close writer
                        textWriter.Close();
                    }
                }
            }
        }
        #endregion


        // Save tile properties
        // Save precedence order?


        // Load level player start points
        #region LoadPlayerStartPointsFromLevel
        /// <summary>
        /// Get a collection of start points from a level.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public Collection<PlayerStartPoint> LoadPlayerStartPointsFromLevel(string filepath)
        {
            string mapDirectory = Path.Combine(_engine.Content.RootDirectory, "levels");
            filepath = Path.Combine(mapDirectory, filepath);
            Collection<PlayerStartPoint> playerStartPoints = new Collection<PlayerStartPoint>();
            if (File.Exists(filepath))
            {
                XmlTextReader textReader = null;

                // Load the start points!
                try
                {
                    textReader = new XmlTextReader(filepath);
                    while (textReader.Read())
                    {
                        XmlNodeType nType = textReader.NodeType;
                        // if node type us an attribute
                        switch (nType)
                        {
                            case XmlNodeType.Element:
                                if (textReader.Name == "Actor")
                                {
                                    string name = textReader.GetAttribute("name");
                                    if (name == "PlayerStart")
                                    {
                                        int team = Convert.ToInt32(textReader.GetAttribute("team"));
                                        float tx = Convert.ToSingle(textReader.GetAttribute("x"));
                                        float ty = Convert.ToSingle(textReader.GetAttribute("y"));

                                        playerStartPoints.Add(new PlayerStartPoint(team, new Vector2(tx, ty)));
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Message("Error loading player start points from level: " + FileAccess.GetFileNameFromPath(filepath) + ": " + e.Message);
                }
                finally
                {
                    if (textReader != null)
                        textReader.Close();
                }
            }
            else
                Log.Message("Couldn't find level from LoadPlayerStartPointsFromLevel: " + filepath);

            return playerStartPoints;
        }
        #endregion
    }


    #region PlayerStartPoint
    /// <summary>
    /// The bare minimum data for a player start point. Team and position.
    /// Used by player start point load function.
    /// </summary>
    public class PlayerStartPoint
    {
        public int Team = 0;
        public Vector2 Position = Vector2.Zero;

        public PlayerStartPoint()
        {
        }

        public PlayerStartPoint(int team, Vector2 position)
        {
            Team = team;
            Position = position;
        }
    }
    #endregion

}
