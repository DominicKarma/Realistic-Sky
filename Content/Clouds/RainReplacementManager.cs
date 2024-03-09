using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content.Clouds
{
    public class RainReplacementManager : ModSystem
    {
        public override void OnModLoad()
        {
            On_Rain.GetRainFallVelocity += MakeRainFallFaster;
            IL_Main.DrawRain += MakeRainMoreTranslucent;
        }

        private Vector2 MakeRainFallFaster(On_Rain.orig_GetRainFallVelocity orig)
        {
            float rainSpeedFactor = Main.cloudAlpha * 2.32f + 1.8f;
            Vector2 rainVelocityFactor = new(1f - rainSpeedFactor * 0.06f, rainSpeedFactor);
            return orig() * rainVelocityFactor;
        }

        private void MakeRainMoreTranslucent(ILContext il)
        {
            ILCursor cursor = new(il);

            // Search for the Color color = 'Lighting.GetColor((int)(rain.position.X + 4f) >> 4, (int)(rain.position.Y + 4f) >> 4) * 0.85f' line.
            // This is responsible for deciding the color of the rain droplets.
            MethodInfo lightingGetColor = typeof(Lighting).GetMethod("GetColor", [typeof(int), typeof(int)]);
            if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt(lightingGetColor)))
            {
                Mod.Logger.Warn("The rain translucency IL edit could not load, due to the Lighting.GetColor call match failing.");
                return;
            }

            // Save the color local index for later.
            int colorLocalIndex = 0;
            if (!cursor.TryGotoNext(i => i.MatchStloc(out colorLocalIndex)))
            {
                Mod.Logger.Warn("The rain translucency IL edit could not load, due to the color local index storage match failing.");
                return;
            }

            // Save the rain instance's local index for later.
            int rainLocalIndex = 0;
            if (!cursor.TryGotoNext(i => i.MatchLdfld<Rain>("waterStyle")))
            {
                Mod.Logger.Warn("The rain translucency IL edit could not load, due to Rain.waterStyle load match failing.");
                return;
            }
            if (!cursor.TryGotoPrev(i => i.MatchLdloc(out rainLocalIndex)))
            {
                Mod.Logger.Warn("The rain translucency IL edit could not load, due to the rain index load match failing.");
                return;
            }

            // Change the color in the Draw method.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdloc(colorLocalIndex)))
            {
                Mod.Logger.Warn("The rain translucency IL edit could not load, due to the rain index load match in the Draw call failing.");
                return;
            }
            cursor.Emit(OpCodes.Ldloc, rainLocalIndex);
            cursor.EmitDelegate(CalculateRainColor);
        }

        public static Color CalculateRainColor(Color color, Rain rain)
        {
            return color * Utils.Remap(rain.velocity.Length(), 30f, 56f, 0.3f, 0.7f);
        }
    }
}
