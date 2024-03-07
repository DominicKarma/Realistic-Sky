using System;
using Microsoft.Xna.Framework;

namespace RealisticSky.Common.DataStructures
{
    public readonly struct GradientSegment
    {
        public float Position { get; }

        public Color Color { get; }

        public GradientSegment(float position, Color color)
        {
            Position = position;
            Color = color;
        }
    }

    public readonly struct SegmentedGradient
    {
        public GradientSegment[] Segments { get; }

        public SegmentedGradient(GradientSegment[] segments)
        {
            Segments = segments;
        }

        public Color GetColor(float position)
        {
            if (Segments.Length == 0)
                throw new InvalidOperationException("Cannot get color from an empty gradient.");

            if (Segments.Length == 1)
                return Segments[0].Color;

            if (position <= Segments[0].Position)
                return Segments[0].Color;

            if (position >= Segments[^1].Position)
                return Segments[^1].Color;

            for (int i = 0; i < Segments.Length - 1; i++)
            {
                if (!(position >= Segments[i].Position) || !(position <= Segments[i + 1].Position))
                    continue;

                float t = (position - Segments[i].Position) / (Segments[i + 1].Position - Segments[i].Position);
                return Color.Lerp(Segments[i].Color, Segments[i + 1].Color, t);
            }

            throw new InvalidOperationException("Failed to find a color for the given position.");
        }
    }
}
