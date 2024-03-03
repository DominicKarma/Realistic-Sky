using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RealisticSky
{
    public class TerraBladeSkyScene : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => true;

        public override SceneEffectPriority Priority => SceneEffectPriority.None;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            string skyKey = RealisticSkyManager.SkyKey;
            if (SkyManager.Instance[skyKey] is not null)
            {
                if (isActive)
                    SkyManager.Instance.Activate(skyKey);
                else
                    SkyManager.Instance.Deactivate(skyKey);
            }
        }
    }
}
