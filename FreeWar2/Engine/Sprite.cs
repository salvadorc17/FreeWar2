using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;
using DGui;
using DSceneGraph;

namespace DEngine
{
    /// <summary>
    /// Animated game object.
    /// Has a collection of textures to render.
    /// Set animation speed, alignment, loop flag, and current index.
    /// </summary>
    public class Sprite : SceneNode, ICloneable
    {
        const int DEFAULT_SPEED = 10;

        // Public variables
        public enum DSpriteHorizontalAlign { Left, Center, Right };
        public enum DSpriteVerticalAlign { Top, Center, Bottom };

        // Protected variables
        protected ContentManager content;
        protected Engine engine;

        protected string name;                          // Unique name of the sprite
        protected Actor actor;                          // Parent actor
        //protected Collection<Texture2D> textures = new Collection<Texture2D>();      // Collection of textures to render
        //protected Collection<Vector2> drawOrigins = new Collection<Vector2>();
        protected Collection<SpriteFrame> spriteFrames = new Collection<SpriteFrame>();
        protected bool loopAnimation = true;            // Loop animation flag
        protected int animationSpeed = DEFAULT_SPEED;   // Animation speed
        protected int frameIndex = -1;                  // Current animation index

        protected DSpriteHorizontalAlign horizontalAlign    // Render alignments
            = DSpriteHorizontalAlign.Center;
        protected DSpriteVerticalAlign verticalAlign
            = DSpriteVerticalAlign.Center;
        
        // Private variables
        int animCounter = 0;                    // Internal animation speed counter
        bool animFinished;                      // Flag for when animation has completed and loop is false

        protected Color tintColor = Color.RosyBrown;
        protected bool animating = true;
        protected SpriteEffects spriteEffects = SpriteEffects.None;



        #region Public Properties
        public SpriteEffects SpriteEffects
        {
            get
            {
                return spriteEffects;
            }
            set
            {
                spriteEffects = value;
            }
        }
        public bool Animating
        {
            get
            {
                return animating;
            }
            set
            {
                animating = value;
            }
        }
        public int FrameIndex
        {
            get
            {
                return frameIndex;
            }
            set
            {
                frameIndex = value;
            }
        }
        public int FrameCount
        {
            get
            {
                return spriteFrames.Count;
            }
        }
        public SpriteFrame Frame
        {
            get
            {
                return spriteFrames[frameIndex];
            }
        }
        public Color TintColor
        {
            get
            {
                return tintColor;
            }
            set
            {
                tintColor = value;
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
        public int TextureIndex
        {
            get
            {
                return frameIndex;
            }
            set
            {
                frameIndex = value;
            }
        }
        public DSpriteHorizontalAlign HorizontalAlign
        {
            get
            {
                return horizontalAlign;
            }
            set
            {
                horizontalAlign = value;
            }
        }
        public DSpriteVerticalAlign VerticalAlign
        {
            get
            {
                return verticalAlign;
            }
            set
            {
                verticalAlign = value;
            }
        }
        /*public int AnimCounter
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }*/
        public int AnimationSpeed
        {
            get
            {
                return animationSpeed;
            }
            set
            {
                animationSpeed = value;
            }
        }
        public bool LoopAnimation
        {
            get
            {
                return loopAnimation;
            }
            set
            {
                loopAnimation = value;
            }
        }
        public bool AnimationFinished
        {
            get
            {
                return animFinished;
            }
            set
            {
                animFinished = value;
            }
        }
        public Actor Actor
        {
            get
            {
                return actor;
            }
            set
            {
                actor = value;
            }
        }
        public Collection<SpriteFrame> Frames
        {
            get
            {
                return spriteFrames;
            }
            set
            {
                spriteFrames = value;
            }
        }
        public ContentManager Content
        {
            get
            {
                return content;
            }
        }
        #endregion


        // ICloneable
        #region Clone
        public Object Clone()
        {
            Sprite s = (Sprite)this.MemberwiseClone();
            // Also make new textures
            s.Frames = new Collection<SpriteFrame>();
            foreach (SpriteFrame frame in this.spriteFrames)
            {
                SpriteFrame newFrame = (SpriteFrame)frame.Clone();
                s.Frames.Add(newFrame);
            }
            return (Object)s;
        }
        #endregion
        

        // Xna
        #region Constructor
        public Sprite(Game game, string actorName)
            : base(game)
        {
            animCounter = animationSpeed;
            content = new ContentManager(Game.Services);
            content.RootDirectory = FileAccess.GetActorImagesDir(actorName);

            //content = new ContentManager(Game.Services);
            engine = (Engine)game;
            
        }
        #endregion



        #region Initialize
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            foreach (SpriteFrame spriteFrame in spriteFrames)
            {
                spriteFrame.Sprite = this;
            }
        }
        #endregion



        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
        }
        #endregion



        #region Update
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Handle animation
            if (animating)
            {
                if (!animFinished)
                {
                    animCounter--;
                    if (animCounter < 0)
                    {
                        animCounter = animationSpeed;

                        // Animation delay finished
                        // Update frame
                        if (frameIndex < spriteFrames.Count)
                        {
                            frameIndex++;
                        }
                        if (frameIndex == spriteFrames.Count)
                        {
                            if (loopAnimation)
                                frameIndex = 0;
                            else
                            {
                                frameIndex--;
                                animFinished = true;
                            }
                        }
                    }
                }
            }
            else
            {
                //frameIndex = 0;
                animFinished = false;
                animCounter = animationSpeed;
            }

            //base.Update(gameTime);
        }
        #endregion



        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            if (spriteFrames.Count > 0)
            {
                SpriteFrame spriteFrame = spriteFrames[frameIndex];
                
                // Correct for no draw origin
                Vector2 drawOrigin = spriteFrame.DrawOrigin;
                if (drawOrigin == Vector2.Zero)
                {
                    // Set centered origin (default)
                    //if (horizontalAlign == DSpriteHorizontalAlign.Center &&
                    //    verticalAlign == DSpriteVerticalAlign.Center)
                    //{
                    //    drawOrigin = new Vector2(spriteFrame.Texture.Width / 2, spriteFrame.Texture.Height / 2);
                    //}
                    //else
                    drawOrigin = actor.Origin;
                    spriteFrame.DrawOrigin = drawOrigin;
                }

                // Take into account camera
                Vector2 drawPos = new Vector2(actor.AbsoluteTransform.X, actor.AbsoluteTransform.Y);
                spriteFrame.DrawPosition = drawPos;
                spriteFrame.TintColor = TintColor;
                spriteFrame.Draw(gameTime);
            }


        }
        #endregion


        // Public functions
        #region AddFrame
        /// <summary>
        /// Add a new frame for this sprite's animation.
        /// </summary>
        /// <param name="actorName"></param>
        /// <param name="filename"></param>
        public void AddFrame(string actorName, string filename, Vector2 origin)
        {
            //content.RootDirectory = FileAccess.GetActorImagesDir(actorName);

            // Load texture from file (XNA filename cleanup)
            FileInfo file = new FileInfo(filename);
            filename = filename.Replace(file.Extension, ""); // clip file extension

            Texture2D texture = content.Load<Texture2D>(filename);
            texture.Name = filename;

            SpriteFrame spriteFrame = new SpriteFrame(engine);
            spriteFrame.Sprite = this;
            spriteFrame.Texture = texture;
            spriteFrame.DrawOrigin = origin;
            spriteFrames.Add(spriteFrame);

            if (frameIndex == -1)
                frameIndex = 0;
        }
        #endregion



        #region ApplyColorMaskByHue
        /// <summary>
        /// Apply a mask color by hue value to all frames in this sprite.
        /// </summary>
        /// <param name="maskColor"></param>
        /// <param name="maskHueValue"></param>
        public void ApplyColorMaskByHue(Color maskColor, int maskHueValue)
        {
            for (int i = 0; i < spriteFrames.Count; i++)
            {
                // Get this tile's texture color data
                int count = spriteFrames[i].Texture.Width * spriteFrames[i].Texture.Height;
                Color[] colorArray = new Color[count];
                spriteFrames[i].Texture.GetData<Color>(colorArray, 0, count);

                // Go through the pixels and convert the team hue pixels to the team color.
                int x;
                int y = -1;
                for (int pixIndex = 0; pixIndex < count; pixIndex++)
                {
                    if (pixIndex % spriteFrames[i].Texture.Width == 0) { y += 1; }
                    x = pixIndex % spriteFrames[i].Texture.Width;
                    {
                        // Convert Xna color to System.Drawing.Color to access hue values
                        System.Drawing.Color pixelColor = System.Drawing.Color.FromArgb(colorArray[pixIndex].A, colorArray[pixIndex].R, colorArray[pixIndex].G, colorArray[pixIndex].B);
                        //System.Drawing.Color pixelColorTeam = System.Drawing.Color.FromArgb(teamColor.A, teamColor.R, teamColor.G, teamColor.B);

                        if ((int)pixelColor.GetHue() == maskHueValue)
                        {
                            //float saturation = pixelColor.GetSaturation();
                            //float brightness = pixelColor.GetBrightness();

                            // Apply team colors and restore saturation and brightness.
                            colorArray[pixIndex] = maskColor;
                        }
                    }
                }

                spriteFrames[i].Texture = new Texture2D(engine.GraphicsDevice, spriteFrames[i].Texture.Width, spriteFrames[i].Texture.Height);
                spriteFrames[i].Texture.SetData<Color>(colorArray);
            }
        }
        #endregion

    }
}
