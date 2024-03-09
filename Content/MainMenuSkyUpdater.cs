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
        private bool readyToRender;

        public override void Load()
        {
            On_Main.DoUpdate += UpdateSky;
        }

        public override void PostSetupContent()
        {
            readyToRender = true;
        }

        private void UpdateSky(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
        {
            orig(self, ref gameTime);

            // Don't bother if not in the game menu or "not ready to render"
            // (not fully initialized).
            if (!Main.gameMenu || !readyToRender)
                return;

            const string sky_key = RealisticSkyManager.SkyKey;
            if (SkyManager.Instance[sky_key] is null)
                return;

            if (RealisticSkyConfig.Instance.ShowInMainMenu)
                SkyManager.Instance.Activate(sky_key);
            else
                SkyManager.Instance.Deactivate(sky_key);

            SkyManager.Instance[sky_key].Update(gameTime);
        }
    }
}
