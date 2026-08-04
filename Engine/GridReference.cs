using System;
using System.Collections.Generic;
using System.Text;

namespace DEngine
{
    /// <summary>
    /// Plain old grid reference using ints so as to avoid pulling in System.Drawing for the Size class.
    /// Used for array indexing instead of lots of tedious converts between Vector2 and Int.
    /// </summary>
    public struct GridReference
    {
        public int X;
        public int Y;

        public GridReference(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }

        public GridReference(float _x, float _y)
        {
            X = (int)_x;
            Y = (int)_y;
        }


        public static GridReference operator -(GridReference a, GridReference b)
        {
            GridReference result = new GridReference(a.X - b.X, a.Y - b.Y);
            return result;
        }

        public static GridReference operator *(GridReference a, float b)
        {
            GridReference result = new GridReference(a.X * b, a.Y * b);
            return result;
        }

        public static bool operator ==(GridReference value1, GridReference value2)
        {
            if (value1.X == value2.X)
            {
                return (value1.Y == value2.Y);
            }
            return false;
        }

        public static bool operator !=(GridReference value1, GridReference value2)
        {
            if (value1.X == value2.X)
            {
                return (value1.Y != value2.Y);
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
