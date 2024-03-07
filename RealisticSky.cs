using RealisticSky.Content;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RealisticSky
{
    public class RealisticSky : Mod
    {
        private bool showingInMainMenu;

        public override void Load()
        {
            SkyManager.Instance[RealisticSkyManager.SkyKey] = new RealisticSkyManager();
            SkyManager.Instance[RealisticSkyManager.SkyKey].Load();
        }

        public override void PostSetupContent()
        {
            UpdateInMainMenu(RealisticSkyConfig.Instance.ShowInMainMenu);
        }

        public void UpdateInMainMenu(bool showInMainMenu)
        {
            // Early return here because this can be called before loading is
            // completed. We don't want to update the showing variable prior to
            // actually loading the sky.
            const string sky_key = RealisticSkyManager.SkyKey;
            if (SkyManager.Instance[sky_key] is null)
                return;

            if (showingInMainMenu == showInMainMenu)
                return;

            showingInMainMenu = showInMainMenu;

            if (showingInMainMenu)
                SkyManager.Instance.Activate(sky_key);
            else
                SkyManager.Instance.Deactivate(sky_key);
        }
    }
}
