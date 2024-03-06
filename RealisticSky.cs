using RealisticSky.Content;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RealisticSky
{
    public class RealisticSky : Mod
    {
        public override void Load()
        {
            SkyManager.Instance[RealisticSkyManager.SkyKey] = new RealisticSkyManager();
            SkyManager.Instance[RealisticSkyManager.SkyKey].Load();
        }
    }
}
