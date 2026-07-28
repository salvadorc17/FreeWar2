using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DEngine
{
    /// <summary>
    /// Drawable frame.
    /// Owned by a Sprite.
    /// The Sprite will update the draw position as necessary.
    /// </summary>
    public class SpriteFrame //: ICloneable
    {
        Engine engine;

        public Texture2D Texture;
        public Vector2 DrawOrigin;
        public Vector2 DrawPosition;
        public Sprite Sprite;
        public Color TintColor = Color.White;


        public SpriteFrame(Engine game)
        {
            engine = game;
        }


        // ICloneable
        #region Clone
        public Object Clone()
        {
            //SpriteFrame s = (SpriteFrame)this.MemberwiseClone();
            SpriteFrame s = new SpriteFrame(engine);
            s.DrawOrigin = this.DrawOrigin;
            s.DrawPosition = this.DrawPosition;
            s.Texture = this.Texture;
            return (Object)s;
        }
        #endregion



        public void Draw(GameTime gameTime)
        {
            if (Sprite.Actor != null)
            {
                //draw the box using the position and rotation of the body
                engine.SpriteBatch.Draw(Texture,
                                    DrawPosition,
                                    null,
                                    TintColor,
                                    Sprite.Actor.Rotation,
                                    DrawOrigin,
                                    Sprite.Actor.Scale,
                                    Sprite.SpriteEffects,
                                    0);
            }
        }


    }
}
