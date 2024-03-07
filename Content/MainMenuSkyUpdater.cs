using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    /// <summary>
    ///     Updates the sky in the main menu.
    /// </summary>
    public sealed class MainMenuSkyUpdater : ModSystem
    {
        public override void Load()
        {
            On_Main.DoUpdate += UpdateSky;
        }

        private static void UpdateSky(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
        {
            orig(self, ref gameTime);

            if (!Main.gameMenu)
                return;

            const string sky_key = RealisticSkyManager.SkyKey;
            if (SkyManager.Instance[sky_key] is null)
                return;

            SkyManager.Instance[sky_key].Update(gameTime);
        }
    }
}
