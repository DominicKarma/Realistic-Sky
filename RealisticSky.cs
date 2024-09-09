using System;
using RealisticSky.Content;
using RealisticSky.Content.Sun;
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

        public override object Call(params object[] args)
        {
            string command = ((string)args[0]).ToLower();
            if (command == "setsunbloomopacity")
            {
                float sunBloomOpacity = Convert.ToSingle(args[1]);
                SunRenderer.SunBloomOpacity = sunBloomOpacity;
            }
            if (command == "temporarilydisable")
                RealisticSkyManager.TemporarilyDisabled = true;

            return new();
        }
    }
}
