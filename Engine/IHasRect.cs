using System;
using System.Drawing;

namespace DSceneGraph
{
    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IHasRect
    {
        RectangleF Rectangle { get; }
    }
}
