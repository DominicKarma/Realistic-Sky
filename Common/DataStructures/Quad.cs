using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;

namespace RealisticSky.Common.DataStructures
{
    /// <summary>
    ///     Represents a collection of four vertices oriented in a quadrilateral formation.
    /// </summary>
    /// <typeparam name="T">The type of vertex to use.</typeparam>
    public readonly struct Quad<T> where T : IVertexType
    {
        /// <summary>
        ///     The top left vertex.
        /// </summary>
        public T TopLeft
        {
            get;
        }

        /// <summary>
        ///     The top right vertex.
        /// </summary>
        public T TopRight
        {
            get;
        }

        /// <summary>
        ///     The bottom left vertex.
        /// </summary>
        public T BottomLeft
        {
            get;
        }

        /// <summary>
        ///     The bottom right vertex.
        /// </summary>
        public T BottomRight
        {
            get;
        }

        [SuppressMessage("Style", "IDE0290:Use primary constructor")]
        public Quad(T topLeft, T topRight, T bottomLeft, T bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }
    }
}
