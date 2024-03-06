using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Content;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RealisticSky
{
    public class RealisticSky : Mod
    {
        public override void Load()
        {
            GameShaders.Misc[RealisticSkyManager.ShaderKey] = new MiscShaderData(new(Assets.Request<Effect>("Assets/Effects/RealisticSkyShader", AssetRequestMode.ImmediateLoad).Value), "AutoloadPass");

            SkyManager.Instance[RealisticSkyManager.SkyKey] = new RealisticSkyManager();
            SkyManager.Instance[RealisticSkyManager.SkyKey].Load();
        }
    }
}
