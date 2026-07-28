using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Drawing;

using DSceneGraph;

namespace DEngine
{
    public class GameSceneNode : SceneNode, IHasRect
    {
        //protected Vector3 absoluteTransform = Vector3.Zero;
        Engine _engine;
        protected RectangleF _rectangle;

        /// <summary>
        /// RectangleF for use in QuadTree
        /// </summary>
        public RectangleF Rectangle
        {
            get { return _rectangle; }
            set { _rectangle = value; }
        }

        /*
        /// <summary>
        /// The scene node's calculated world transform.
        /// </summary>
        public Vector3 AbsoluteTransform
        {
            get { return absoluteTransform; }
            set { absoluteTransform = value; }
        }
        */
        /// <summary>
        /// The scene node's calculated position (does not include camera displacement)
        /// </summary>
        public Vector2 AbsolutePosition
        {
            get
            {
                return new Vector2(absoluteTransform.X - _engine.SceneGraph.Camera.Position.X,
                                   absoluteTransform.Y - _engine.SceneGraph.Camera.Position.Y);
            }
        }

        public GameSceneNode(Engine engine)
            : base(engine)
        {
            _engine = engine;
        }
    }
}
