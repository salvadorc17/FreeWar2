//Copyright (c) 2007 GfxStorm.com
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

// Modifications by David Wilson 2008

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
//using System.Drawing;



namespace DSceneGraph
{
    /// <summary>
    /// Defines a scene graph.
    /// </summary>
    public class SceneGraph : DrawableGameComponent
    {

        #region Private Fields
        
        protected SceneNode rootNode = null;
        //GameTime gameTime;
        bool cullingDisabled = false;
        Vector3 centerScreen = Vector3.Zero;
        double cullingDistance = 1200;

        long _updateIndex = 0;

        #endregion



        #region Public Properties

        /// <summary>
        /// The <see cref="T:SceneNode"/> that is the root of the graph.
        /// </summary>
        public SceneNode RootNode
        {
            get { return rootNode; }
            set { rootNode = value; }
        }

        /// <summary>
        /// Determines whether the scene graph will cull objects.  The default is enabled (false).
        /// </summary>
        public bool CullingDisabled
        {
            get { return cullingDisabled; }
            set { cullingDisabled = value; }
        }


        public double CullingDistance
        {
            get { return cullingDistance; }
            set { cullingDistance = value; }
        }


        public int NodeCount
        {
            get
            {
                return NodeCountRecursive(rootNode, 0);
            }
        }

        #endregion



        #region Constructor

        /// <summary>
        /// Intializes an instance of the <see cref="SceneGraph"/> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> that will be used for rendering.</param>
        public SceneGraph(Game game)
            : base(game)
        {
            rootNode = new SceneNode(game);
        }

        #endregion



        #region Private Methods

        int NodeCountRecursive(SceneNode node, int count)
        {
            count += node.Children.Count;
            foreach (SceneNode childNode in node.Children)
            {
                count = NodeCountRecursive(childNode, count);
            }
            return count;
        }

        protected void UpdateRecursive(GameTime time, SceneNode node)
        {
            //Update node
            node.Update(time);

            // Set update index, for height calculations
            _updateIndex++;
            node.UpdateIndex = _updateIndex;

            //Update children recursively
            for (int i = 0; i < node.Children.Count; i++) // (SceneNode childNode in node.Children)
            {
                UpdateRecursive(time,node.Children[i]);
            }
        }

        protected void CalculateTransformsRecursive(GameTime time, SceneNode node)
        {
            node.AbsoluteTransform += new Vector3(node.Position.X, node.Position.Y, node.Rotation);

            for (int i = 0; i < node.Children.Count; i++) // (SceneNode childNode in node.Children)
            {
                node.Children[i].AbsoluteTransform = node.AbsoluteTransform;
                CalculateTransformsRecursive(time, node.Children[i]);
            }
        }

        void DrawRecursive(GameTime gameTime, SceneNode node)
        {
            //Draw
            if (node.Visible || node.AlwaysVisible)
            {
                node.Draw(gameTime);

                for (int i = 0; i < node.Children.Count; i++)
                {
                    DrawRecursive(gameTime, node.Children[i]);
                }
            }
        }


        void RemoveNodeRecursive(SceneNode node, SceneNode removeNode)
        {
            // Depth-first remove
            for (int i = 0; i < node.Children.Count; i++)
            {
                SceneNode childNode = node.Children[i];
                if (childNode == removeNode)
                {
                    node.Children.Remove(childNode);
                    break;
                }
                else
                    RemoveNodeRecursive(childNode, removeNode);
            }
        }
        #endregion



        #region Public Methods

        /// <summary>
        /// Allows the scene graph to update its state.
        /// </summary>
        /// <param name="time"></param>
        public override void Update(GameTime gameTime)
        {
            

            //rootNode.AbsoluteTransform = Vector3.Zero;
            CalculateTransformsRecursive(gameTime, rootNode);
            UpdateRecursive(gameTime, rootNode);

            _updateIndex = 0;

            //base.Update(time);
        }

        /// <summary>
        /// Allows the scene graph to render.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            DrawRecursive(gameTime, rootNode);
        }


        public void InsertNode(SceneNode node, SceneNode parentNode)
        {
            if (parentNode != null)
            {
                parentNode.Children.Add(node);
            }
            else
            {
                rootNode.Children.Add(node);
            }
        }


        public void RemoveNode(SceneNode node)
        {
            RemoveNodeRecursive(rootNode, node);
        }
        #endregion

    }

}
