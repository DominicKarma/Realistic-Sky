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

        /// <summary>
        /// The minimum exposure coefficient for sunlight.
        /// </summary>
        public const float MinSunlightExposure = -3f;

        /// <summary>
        /// The maximum exposure coefficient for sunlight.
        /// </summary>
        public const float MaxSunlightExposure = 3f;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(44, 54, 128, 192)]
        [DefaultValue(true)]
        public bool ShowInMainMenu
        {
            get;
            set;
        }

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

        [BackgroundColor(44, 54, 128, 192)]
        [DefaultValue(0f)]
        [Range(MinSunlightExposure, MaxSunlightExposure)]
        public float SunlightExposure
        {
            get;
            set;
        }

        public override void OnChanged()
        {
            base.OnChanged();

            ModContent.GetInstance<RealisticSky>().UpdateInMainMenu(ShowInMainMenu);
        }
    }
}
