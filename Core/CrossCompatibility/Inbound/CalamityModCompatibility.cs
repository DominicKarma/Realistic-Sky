using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Core.CrossCompatibility.Inbound
{
    public class CalamityModCompatibility : ModSystem
    {
        public static Mod CalamityMod
        {
            get;
            private set;
        }

        public static bool InAstralBiome(Player player)
        {
            if (CalamityMod is null || Main.gameMenu)
                return false;

            return (bool)CalamityMod.Call("GetInZone", player, "AstralBiome");
        }

        public override void PostSetupContent()
        {
            // Attempt to load Calamity.
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
                CalamityMod = cal;
        }
    }
}
