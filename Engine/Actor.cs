using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;
using System.Drawing;

using Color = Microsoft.Xna.Framework.Color;
using DGui;
using DSceneGraph;
 
namespace DEngine
{
    /// <summary>
    /// In-game actor with a physical body and a collection of animated sprites.
    /// </summary>
    public class Actor : GameSceneNode, ICloneable
    {
        public const int DEFAULT_WIDTH = 100;
        public const int DEFAULT_HEIGHT = 100;

        // Actor sprites, rendering, and geometry
        protected ContentManager _content;
        protected Engine _engine;
        protected string _actorName = null;              // Unique name of the actor
        protected Collection<Sprite> _sprites = null;    // Collection of animated sprites
        protected int _spriteIndex = -1;                 // Current sprite to render
        protected Body _physBody;                        // Physical body
        protected Vector2 _size;                         // Body size
        //protected Geom _geom;                            // On-screen geometry
        protected SpriteEffects _spriteEffects 
                        = SpriteEffects.None;           // Used to flip the geometry
        protected float _scale;                          // Body and render scale
        protected Vector2 _drawOrigin;                   // Draw origin

        // Audio
        protected Dictionary<string, string> _sounds;    // List of sounds
        
        // Color masking (replace all sprites' textures' pixels that match the hue to a color, if enabled)
        // Used for team coloration.
        protected Color _maskColor;
        protected int _maskHueValue = 205;
        protected bool _maskColorEnabled = true;

        // Debug tinting
        protected Color _tintColor = Color.White;

        // Actor flags
        protected bool _editorVisibleOnly;               // Visible only in the editor
        protected bool _physicallySimulated = true;         // Use FarseerPhysics
        protected int _team;                             // Team the actor is on
        protected string _subtype = null;                // Subtype of this actor (for inheritance purposes mainly (to avoid reflection)).

        protected GridReference _gridReference;


        protected bool _isEffect = false;   // Is the actor part of the quad tree? Reinsert upon move.


        int quadTreeUpdateCounter = 0;
        int quadTreeUpdateInterval = 40;


        #region Public Properties
        public GridReference GridReference
        {
            get
            {
                return _gridReference;
            }
        }
        public Vector2 DrawOrigin
        {
            get
            {
                return _drawOrigin;
            }
            set
            {
                _drawOrigin = value;
            }
        }
        public int SpriteIndex
        {
            get
            {
                return _spriteIndex;
            }
            set
            {
                _spriteIndex = value;
            }
        }
        
        public int Team
        {
            get
            {
                return _team;
            }
            set
            {
                _team = value;
            }
        }
        /// <summary>
        /// Subtype of this actor
        /// </summary>
        public string Subtype
        {
            get
            {
                return _subtype;
            }
            set
            {
                _subtype = value;
            }
        }

        public bool PhysicallySimulated
        {
            get
            {
                return _physicallySimulated;
            }
            set
            {
                _physicallySimulated = value;
            }
        }


        public bool IsEffect
        {
            get
            {
                return _isEffect;
            }
            set
            {
                _isEffect = value;
            }
        }

        /// <summary>
        /// Controls whether a color mask is applied.
        /// </summary>
        public bool MaskColorEnabled
        {
            get
            {
                return _maskColorEnabled;
            }
            set
            {
                _maskColorEnabled = value;
            }
        }
       
        /// <summary>
        /// Color to be used when recoloring textures by pixel hue color.
        /// </summary>
        public Color MaskColor
        {
            get
            {
                return _maskColor;
            }
            set
            {
                _maskColor = value;
            }
        }
        /// <summary>
        /// Pixel hue value to use in color masking.
        /// </summary>
        public int MaskHueValue
        {
            get
            {
                return _maskHueValue;
            }
            set
            {
                _maskHueValue = value;
            }
        }
        public Color TintColor
        {
            get
            {
                return _tintColor;
            }
            set
            {
                _tintColor = value;
                // Set all sprites to this too
                foreach (Sprite s in _sprites)
                {
                    s.TintColor = value;
                }
            }
        }
        public string Name
        {
            get
            {
                return _actorName;
            }
            set
            {
                _actorName = value;
            }
        }
        public SpriteEffects SpriteEffects
        {
            get
            {
                return _spriteEffects;
            }
            set
            {
                _spriteEffects = value;
            }
        }
        public Dictionary<string,string> Sounds
        {
            get
            {
                return _sounds;
            }
        }
        public Collection<Sprite> Sprites
        {
            get
            {
                return _sprites;
            }
            set
            {
                _sprites = value;
            }
        }
        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }
        public Vector2 Origin
        {
            get
            {
                return _drawOrigin;
            }
            set
            {
                _drawOrigin = value;
            }
        }
        public Sprite Sprite
        {
            get
            {
                return _sprites[_spriteIndex];
            }
        }
        public Body Body
        {
            get
            {
                return _physBody;
            }
            set
            {
                _physBody = value;
            }
        }
        /*
        public Geom Geom
        {
            get
            {
                return _geom;
            }
            set
            {
                _geom = value;
            }
        }
        */
        public float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }
        public bool EditorVisibleOnly
        {
            get
            {
                return _editorVisibleOnly;
            }
            set
            {
                _editorVisibleOnly = value;
            }
        }
        
        #endregion



        // ICloneable
        #region Clone
        public Object Clone()
        {
            Actor a = (Actor)this.MemberwiseClone();
            //new Actor(Game, name, position.X, position.Y);

            a.children = new SceneNodeCollection();

            // Copy the sprites
            a.Sprites = new Collection<Sprite>();
            foreach (Sprite sprite in _sprites)
            {
                //sprite.Actor = a;
                Sprite newSprite = (Sprite)sprite.Clone();
                newSprite.Actor = this;
                newSprite.TintColor = this.TintColor;
                a.Sprites.Add(newSprite);
            }
            return (Object)a;
        }
        #endregion



        // Xna
        #region Constructors
        public Actor(Engine game, string _name) : base(game)
        {
            _engine = (Engine)game;

            _actorName = _name;

            _size = new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT);

            _content = new ContentManager(game.Services);
            string actorDir = FileAccess.GetActorDir(_actorName);
            if (actorDir != null)
                _content.RootDirectory = actorDir;
            else
                _content.RootDirectory = FileAccess.GetActorsDir();

            _sprites = new Collection<Sprite>();
            _sounds = new Dictionary<string, string>();

            _physicallySimulated = _engine.ActorsPhysicallySimulated;
        }

        public Actor(Engine game, string _name, float x, float y)
            : this(game, _name)
        {
            Position = new Vector2(x, y);
        }
        #endregion



        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }
        #endregion



        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _engine = (Engine)Game;

            if (_sprites.Count > 0)
                _spriteIndex = 0;


            Random random = new Random();
            quadTreeUpdateCounter = random.Next(quadTreeUpdateInterval - 1);

            Vector2 scaledSize = new Vector2(_size.X * _scale, _size.Y * _scale);

            // Origin bug workaround:
            // Origin must be set to size * ((1 - scale) + 1)
            if (_scale != 1.0)
                _drawOrigin = new Vector2((_size.X / 2),
                                         (_size.Y / 2));
            else
                _drawOrigin = new Vector2(scaledSize.X / 2, scaledSize.Y / 2);

            if (_engine.ActorsPhysicallySimulated)
            {
                // Create physical body
                _physBody = BodyFactory.CreateBody(_engine.PhysicsSimulator, new Vector2(Position.X, Position.Y));//BodyFactory.Instance.CreateRectangleBody(_engine.PhysicsSimulator, scaledSize.X, scaledSize.Y, 1);
                _physBody.Enabled = true; // engine.ActorsPhysicallySimulated;

                // Create body geometry
                //_geom = GeomFactory.Instance.CreateRectangleGeom(_engine.PhysicsSimulator, _physBody, scaledSize.X, scaledSize.Y);
                //_geom.CollisionResponseEnabled = _engine.ActorsPhysicallySimulated;
            }


            foreach (Sprite sprite in _sprites)
            {
                sprite.Actor = this;
                sprite.Initialize();
            }


            // Get closest tile pos
            if (!IsEffect)
            {
                Vector2 tilePos = _engine.GetTileGridPosition(this.Position);
                Tile closestTile = _engine.TileExistenceCheckByExactLocation(tilePos);
                if (closestTile != null)
                {
                    _gridReference = closestTile.GridReference();
                }
            }


            _rectangle = new RectangleF(Position.X - (scaledSize.X / 2), Position.Y - (scaledSize.Y / 2), scaledSize.X, scaledSize.Y);
        }
        #endregion


        protected override void UnloadContent()
        {
            if (!_isEffect)
            {
                if (_engine.ActorQuadTree != null)
                    _engine.ActorQuadTree.Remove(this);
            }


            base.UnloadContent();
        }


        // Animate the current sprite
        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Animate only the current sprite
            if (_spriteIndex > -1)
                _sprites[_spriteIndex].Update(gameTime);

            // Update scene node position
            if (_physBody != null)
            {
                Position = _physBody.Position;
                Rotation = _physBody.Rotation;
            }

            Vector2 scaledSize = _size * _scale;
            _rectangle = new RectangleF(Position.X - (scaledSize.X / 2), Position.Y - (scaledSize.Y / 2), scaledSize.X, scaledSize.Y);


            quadTreeUpdateCounter++;

            // Reindex this actor in the quad tree!
            if (!_isEffect && quadTreeUpdateCounter == quadTreeUpdateInterval)
            {
                _engine.ActorQuadTree.Remove(this);
                _engine.ActorQuadTree.Insert(this);
                quadTreeUpdateCounter = 0;
            }
        }
        #endregion



        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            if ((!_editorVisibleOnly || _engine.EditorMode) && _spriteIndex > -1)
            {
                _sprites[_spriteIndex].Draw(gameTime);
            }


            //base.Draw(gameTime);
        }
        #endregion



        // Actor public methods

        #region PointHitCheck
        /// <summary>
        /// Check if a point touches this actor.
        /// Uses FarseerPhysics Geom.Collide() - inexact!
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Actor PointHitCheck(Vector2 point)
        {
            //if (_geom != null && _geom.Collide(point))
            //    return this;
            return null;
        }
        #endregion



        #region ApplyColorMaskToSprites
        /// <summary>
        /// Apply pixel color change by hue to all frames in all sprites of this actor.
        /// </summary>
        /// <param name="maskColor"></param>
        /// <param name="maskHueValue"></param>
        public void ApplyColorMaskToSprites()
        {
            foreach (Sprite sp in _sprites)
            {
                sp.ApplyColorMaskByHue(_maskColor, _maskHueValue);
            }
        }
        #endregion

    }
}
