using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace RealisticSky.Common.DataStructures
{
    public readonly struct Star
    {
        /// <summary>
        /// The orientation of this star in 3D space.
        /// </summary>
        public Vector3 Orientation { get; }

        /// <summary>
        /// The radius of this star, in pixels.
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// The color of this star.
        /// </summary>
        public Color Color { get; }

        public Star(float latitude, float longitude, Color color, float radius)
        {
            float latitudeCosine = MathF.Cos(latitude);
            float latitudeSine = MathF.Sin(latitude);
            float longitudeCosine = MathF.Cos(longitude);
            float longitudeSine = MathF.Sin(longitude);

            Orientation = new(latitudeCosine * longitudeCosine, latitudeCosine * longitudeSine, latitudeSine);
            Color = color;
            Radius = radius;
        }

        internal void GenerateVertices(float scale, out VertexPositionColorTexture topLeft, out VertexPositionColorTexture topRight, out VertexPositionColorTexture bottomLeft, out VertexPositionColorTexture bottomRight)
        {
            // Calculate screen-relative position values.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            Vector3 radiusVector = new Vector3(1f / screenSize.X, 1f / screenSize.Y, 0f) * Radius * scale;
            Vector3 screenPosition = Orientation;

            // Generate vertex positions.
            Vector3 topLeftPosition = screenPosition - radiusVector;
            Vector3 topRightPosition = screenPosition + new Vector3(1f, -1f, 0f) * radiusVector;
            Vector3 bottomLeftPosition = screenPosition + new Vector3(-1f, 1f, 0f) * radiusVector;
            Vector3 bottomRightPosition = screenPosition + radiusVector;

            // Generate vertices.
            topLeft = new(topLeftPosition, Color, Vector2.Zero);
            topRight = new(topRightPosition, Color, Vector2.UnitX);
            bottomLeft = new(bottomLeftPosition, Color, Vector2.UnitY);
            bottomRight = new(bottomRightPosition, Color, Vector2.One);
        }
    }
}
