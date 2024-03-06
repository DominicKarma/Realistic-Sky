using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RealisticSky
{
    [BackgroundColor(86, 109, 154, 216)]
    public class RealisticSkyConfig : ModConfig
    {
        public static RealisticSkyConfig Instance => ModContent.GetInstance<RealisticSkyConfig>();

        /// <summary>
        /// The minimum wavelength a color channel can have,
        /// </summary>
        public const float MinWavelength = 100f;

        /// <summary>
        /// The maximum wavelength a color channel can have,
        /// </summary>
        public const float MaxWavelength = 1100f;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(44, 54, 128, 192)]
        [DefaultValue(false)]
        public bool PerformanceMode
        {
            get;
            set;
        }

        [BackgroundColor(44, 54, 128, 192)]
        [DefaultValue(750f)]
        [Range(MinWavelength, MaxWavelength)]
        public float RedWavelength
        {
            get;
            set;
        }

        [BackgroundColor(44, 54, 128, 192)]
        [DefaultValue(530f)]
        [Range(MinWavelength, MaxWavelength)]
        public float GreenWavelength
        {
            get;
            set;
        }

        [BackgroundColor(44, 54, 128, 192)]
        [DefaultValue(420f)]
        [Range(MinWavelength, MaxWavelength)]
        public float BlueWavelength
        {
            get;
            set;
        }
    }
}
