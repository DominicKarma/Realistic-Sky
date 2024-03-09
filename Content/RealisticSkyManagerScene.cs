using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class RealisticSkyManagerScene : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player)
        {
            // Make the effect not appear during boss fights if the config says so.
            if (!RealisticSkyConfig.Instance.DisableEffectsDuringBossFights)
                return true;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i] is null || !Main.npc[i].active)
                    continue;

                NPC npc = Main.npc[i];
                bool isEaterOfWorlds = npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsTail;
                if (npc.boss || isEaterOfWorlds)
                    return false;
            }

            return true;
        }

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
