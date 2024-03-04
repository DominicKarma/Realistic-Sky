using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RealisticSky
{
    [BackgroundColor(86, 109, 154, 216)]
    public class RealisticSkyConfig : ModConfig
    {
        public static RealisticSkyConfig Instance => ModContent.GetInstance<RealisticSkyConfig>();

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(44, 54, 128, 192)]
        [DefaultValue(false)]
        public bool PerformanceMode
        {
            get;
            set;
        }
    }
}
