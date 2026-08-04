using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

using DSceneGraph;

namespace DEngine
{
    public class GameSceneGraph : SceneGraph
    {
        protected Camera _camera;

        public Camera Camera
        {
            get { return _camera; }
            set { _camera = value; }
        }


        public GameSceneGraph(Game game)
            : base(game)
        {
            _camera = new Camera();
            _camera.Position = new Vector3(0, 0, 400);
        }



        public override void Update(GameTime gameTime)
        {
            if (_camera != null)
                _camera.Update(this);

            rootNode.AbsoluteTransform = _camera.Position;

            base.Update(gameTime);
        }
    }
}
