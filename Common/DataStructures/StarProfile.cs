using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Utilities;

namespace RealisticSky.Common.DataStructures
{
    public readonly struct StarProfile
    {
        public const int TemperatureMin = 3000; // 2000;
        public const int TemperatureMax = 40000;
        public readonly int[] Segments = { TemperatureMin, 4000, 6000, 7000, 10000, 20000, 30000, TemperatureMax };
        public static readonly SegmentedGradient TemperatureToColorGradient = new(new GradientSegment[]
        {
            new(TemperatureMin, new Color(237, 26, 35)),
            new(4000, new Color(237, 55, 34)),
            new(6000, new Color(247, 182, 18)),
            new(7000, new Color(255, 250, 182)),
            new(10000, Color.White),
            new(20000, new Color(131, 216, 247)),
            new(30000, new Color(12, 140, 215)),
            new(TemperatureMax, new Color(44, 53, 148)),
        });

        public int Temperature { get; }

        public float Scale { get; }

        public StarProfile(UnifiedRandom random)
        {
            float normal = random.NextFloat(0.99999f);
            int segment = (int)(normal * (Segments.Length - 1));
            float t = (normal - segment / (float)(Segments.Length - 1)) * (Segments.Length - 1);
            Temperature = (int)MathHelper.Lerp(Segments[segment], Segments[segment + 1], t);
            Scale = normal + MathHelper.Lerp(0.5f, 1.2f, MathF.Pow(random.NextFloat(), 10.5f));
        }

        public static Color TemperatureToColor(int temperature)
        {
            if (temperature < TemperatureMin)
                temperature = TemperatureMin;

            if (temperature > TemperatureMax)
                temperature = TemperatureMax;

            return TemperatureToColorGradient.GetColor(temperature);
        }
    }
}
