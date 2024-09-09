using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class SkyDisablingResetter : ModSystem
    {
        public override void PreUpdateWorld() => RealisticSkyManager.TemporarilyDisabled = false;
    }
}
