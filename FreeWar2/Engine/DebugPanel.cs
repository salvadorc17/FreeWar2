using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DEngine;
using DGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaInput = Microsoft.Xna.Framework.Input;

using System.Diagnostics;

namespace DEngine
{
    public class DebugPanel : DForm
    {
        DText _framerateText;
        DText _sceneGraphText;
        DText _staticSceneGraphText;
        DText _effectsSceneGraphText;
        DText _actorQuadTreeText;
        DText _tileQuadTreeText;
        DText _memoryUsedText;
        DText _tilesText;
        DText _actorsText;
        DText _transitionTilesText;
        DLayoutFlow _layout;

        Engine _engine;
        //DGuiManager _guiManager;


        int _frameRate = 0;
        int _frameCounter = 0;
        TimeSpan _elapsedTime = TimeSpan.Zero;

        Process _currentProcess;
        long _memoryUsed;


        public DebugPanel(Engine engine)
            : base(engine.GuiManager, "DebugForm", null)
        {
            //_guiManager = guiManager;
            _engine = engine;


            _currentProcess = Process.GetCurrentProcess();
        }

        #region ShowForm
        /// <summary>
        /// Create menu items and attach them to the scenegraph.
        /// </summary>
        public override void ShowForm()
        {
            base.ShowForm();

            this.Alpha = 0;
            this.Position = new Vector2(0, 32);
            this.Size = new Vector2(320, 640);
            this.Initialize();
            this.RecreateTexture();

            // Laid-out controls
            _layout = new DLayoutFlow(1, 10);
            _layout.Position = new Vector2(15, 15);
            _layout.CellPadding = 4;


            _framerateText = new DText(_guiManager, 0, 0, "FPS: 0");
            _framerateText.FontName = "Miramonte";
            _framerateText.FontColor = Color.Aqua;
            _framerateText.Initialize();
            _layout.Add(_framerateText);
            this.AddPanel(_framerateText);

            _memoryUsedText = new DText(_guiManager, 0, 0, "Memory Used: 0");
            _memoryUsedText.FontName = "Miramonte";
            _memoryUsedText.FontColor = Color.Aqua;
            _memoryUsedText.Initialize();
            _layout.Add(_memoryUsedText);
            this.AddPanel(_memoryUsedText);

            _actorQuadTreeText = new DText(_guiManager, 0, 0, "Actor QuadTree Count: 0");
            _actorQuadTreeText.FontName = "Miramonte";
            _actorQuadTreeText.FontColor = Color.Aqua;
            _actorQuadTreeText.Initialize();
            _layout.Add(_actorQuadTreeText);
            this.AddPanel(_actorQuadTreeText);

            _tileQuadTreeText = new DText(_guiManager, 0, 0, "Tile QuadTree Count: 0");
            _tileQuadTreeText.FontName = "Miramonte";
            _tileQuadTreeText.FontColor = Color.Aqua;
            _tileQuadTreeText.Initialize();
            _layout.Add(_tileQuadTreeText);
            this.AddPanel(_tileQuadTreeText);

            _sceneGraphText = new DText(_guiManager, 0, 0, "SceneGraph Count: 0");
            _sceneGraphText.FontName = "Miramonte";
            _sceneGraphText.FontColor = Color.Aqua;
            _sceneGraphText.Initialize();
            _layout.Add(_sceneGraphText);
            this.AddPanel(_sceneGraphText);

            _staticSceneGraphText = new DText(_guiManager, 0, 0, "Static SceneGraph Count: 0");
            _staticSceneGraphText.FontName = "Miramonte";
            _staticSceneGraphText.FontColor = Color.Aqua;
            _staticSceneGraphText.Initialize();
            _layout.Add(_staticSceneGraphText);
            this.AddPanel(_staticSceneGraphText);

            _effectsSceneGraphText = new DText(_guiManager, 0, 0, "Effects SceneGraph Count: 0");
            _effectsSceneGraphText.FontName = "Miramonte";
            _effectsSceneGraphText.FontColor = Color.Aqua;
            _effectsSceneGraphText.Initialize();
            _layout.Add(_effectsSceneGraphText);
            this.AddPanel(_effectsSceneGraphText);


            _tilesText = new DText(_guiManager, 0, 0, "Tiles: 0");
            _tilesText.FontName = "Miramonte";
            _tilesText.FontColor = Color.Aqua;
            _tilesText.Initialize();
            _layout.Add(_tilesText);
            this.AddPanel(_tilesText);

            _actorsText = new DText(_guiManager, 0, 0, "Actors: 0");
            _actorsText.FontName = "Miramonte";
            _actorsText.FontColor = Color.Aqua;
            _actorsText.Initialize();
            _layout.Add(_actorsText);
            this.AddPanel(_actorsText);

            _transitionTilesText = new DText(_guiManager, 0, 0, "Transition Tiles: 0");
            _transitionTilesText.FontName = "Miramonte";
            _transitionTilesText.FontColor = Color.Aqua;
            _transitionTilesText.Initialize();
            _layout.Add(_transitionTilesText);
            this.AddPanel(_transitionTilesText);

            _engine.StaticSceneGraph.RootNode.Children.Add(this);
        }
        #endregion





        #region HideForm
        /// <summary>
        /// Remove all the menu objects from the scenegraph.
        /// </summary>
        public override void HideForm()
        {
            this.Children.Remove(_framerateText);
            this.Children.Remove(_sceneGraphText);
            this.Children.Remove(_staticSceneGraphText);
            this.Children.Remove(_effectsSceneGraphText);
            this.Children.Remove(_tileQuadTreeText);
            this.Children.Remove(_actorQuadTreeText);
            this.Children.Remove(_memoryUsedText);
            this.Children.Remove(_tilesText);
            this.Children.Remove(_actorsText);
            this.Children.Remove(_transitionTilesText);
            _engine.StaticSceneGraph.RootNode.Children.Remove(this);

            //_framerateText.Dispose();
            //_sceneGraphText.Dispose();
            //_staticSceneGraphText.Dispose();
            //_memoryUsedText.Dispose();
            //_tilesText.Dispose();
            //_actorsText.Dispose();
            //_transitionTilesText.Dispose();

            base.HideForm();
        }
        #endregion




        public override void Update(GameTime gameTime)
        {
            // Count of all items in scene graph
            _sceneGraphText.Text = "SceneGraph Count: " + _engine.SceneGraph.NodeCount.ToString();
            //_sceneGraphText.RecreateTexture();

            // Count of all items in static scene graph
            _staticSceneGraphText.Text = "Static SceneGraph Count: " + _engine.StaticSceneGraph.NodeCount.ToString();
            //_staticSceneGraphText.RecreateTexture();

            _effectsSceneGraphText.Text = "Effects SceneGraph Count: " + _engine.EffectsSceneGraph.NodeCount.ToString();

            //_tileQuadTreeText.Text = "Tile QuadTree Count: " + _engine.TileQuadTree.Count;

            //_actorQuadTreeText.Text = "Actor QuadTree Count: " + _engine.ActorQuadTree.Count;


            // FPS timer maths
            _elapsedTime += gameTime.ElapsedGameTime;
            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);
                _frameRate = _frameCounter;
                _frameCounter = 0;
            }
            _framerateText.Text = "FPS: " + _frameRate.ToString();
            //_framerateText.RecreateTexture();

            // Memory in use by this process
            _memoryUsed = _currentProcess.WorkingSet64;
            _memoryUsed /= 1048576; // megabytes
            _memoryUsedText.Text = "Memory Used: " + _memoryUsed.ToString("0.00") + " MB";
            //_memoryUsedText.RecreateTexture();

            // Tiles loaded
            _tilesText.Text = "Tiles: " + _engine.Tiles.Count.ToString();

            // Actors
            _actorsText.Text = "Actors: " + _engine.Actors.Count.ToString();

            // Transition tiles
            _transitionTilesText.Text = "Transition Tiles: " + _engine.TransitionTiles.Count.ToString();



            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            _frameCounter++;

            base.Draw(gameTime);
        }
    }
}
