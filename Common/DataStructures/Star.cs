using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace RealisticSky.Common.DataStructures
{
    public readonly struct Star
    {
        /// <summary>
        /// How far, in a 0-1 ratio, this star should be horizontally positioned. 0 Means to the left edge of the screen, 1 means to the right edge.
        /// </summary>
        public float ScreenXPositionRatio { get; }

        /// <summary>
        /// How far, in a 0-1 ratio, this star should be vertically positioned. 0 Means to the top of the screen, 1 means to the bottom.
        /// </summary>
        public float ScreenYPositionRatio { get; }

        /// <summary>
        /// The radius of this star, in pixels.
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// The twinkle phase shift of this star.
        /// </summary>
        public float TwinklePhaseShift { get; }

        /// <summary>
        /// The color of this star.
        /// </summary>
        public Color Color { get; }

        [SuppressMessage("Style", "IDE0290:Use primary constructor")]
        public Star(float xRatio, float yRatio, Color color, float radius, float twinklePhaseShift)
        {
            ScreenXPositionRatio = xRatio;
            ScreenYPositionRatio = yRatio;
            TwinklePhaseShift = twinklePhaseShift;
            Color = color;
            Radius = radius;
        }

        internal void GenerateVertices(float scale, out VertexPositionColorTexture topLeft, out VertexPositionColorTexture topRight, out VertexPositionColorTexture bottomLeft, out VertexPositionColorTexture bottomRight)
        {
            // Calculate screen-relative position values.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            Vector2 radiusVector = Vector2.One * Radius / screenSize * scale;
            Vector2 screenPosition = new(ScreenXPositionRatio, ScreenYPositionRatio);

            // Generate vertex positions.
            Vector2 topLeftPosition = screenPosition - radiusVector;
            Vector2 topRightPosition = screenPosition + new Vector2(1f, -1f) * radiusVector;
            Vector2 bottomLeftPosition = screenPosition + new Vector2(-1f, 1f) * radiusVector;
            Vector2 bottomRightPosition = screenPosition + radiusVector;

            // Generate vertices.
            topLeft = new(new(topLeftPosition, TwinklePhaseShift), Color, Vector2.Zero);
            topRight = new(new(topRightPosition, TwinklePhaseShift), Color, Vector2.UnitX);
            bottomLeft = new(new(bottomLeftPosition, TwinklePhaseShift), Color, Vector2.UnitY);
            bottomRight = new(new(bottomRightPosition, TwinklePhaseShift), Color, Vector2.One);
        }
    }
}
