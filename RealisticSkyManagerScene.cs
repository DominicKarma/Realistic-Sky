using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RealisticSky
{
    public class RealisticSkyManagerScene : ModSceneEffect
    {
        public static Vector2 SunPosition
        {
            get;
            private set;
        }

        public static Vector2 MoonPosition
        {
            get;
            private set;
        }

        public override void Load()
        {
            IL_Main.DrawSunAndMoon += RecordSunAndMoonPositions;
        }

        private void RecordSunAndMoonPositions(ILContext context)
        {
            int sunPositionIndex = 0;
            int moonPositionIndex = 0;
            ILCursor cursor = new(context);
            if (!cursor.TryGotoNext(i => i.MatchLdsfld<Main>("sunModY")))
            {
                Mod.Logger.Error("The Main.sunModY load could not be found.");
                return;
            }
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStloc(out sunPositionIndex)))
            {
                Mod.Logger.Error("The sun position local variable storage could not be found.");
                return;
            }

            // Store the sun's draw position.
            cursor.Emit(OpCodes.Ldloc, sunPositionIndex);
            cursor.EmitDelegate<Action<Vector2>>(sunPosition => SunPosition = sunPosition);

            if (!cursor.TryGotoNext(i => i.MatchLdsfld<Main>("moonModY")))
            {
                Mod.Logger.Error("The Main.moonModY load could not be found.");
                return;
            }
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStloc(out moonPositionIndex)))
            {
                Mod.Logger.Error("The moon position local variable storage could not be found.");
                return;
            }

            cursor.Emit(OpCodes.Ldloc, moonPositionIndex);
            cursor.EmitDelegate<Action<Vector2>>(moonPosition => MoonPosition = moonPosition);
        }

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
