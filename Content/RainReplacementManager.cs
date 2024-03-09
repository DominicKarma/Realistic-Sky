using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class RainReplacementManager : ModSystem
    {
        /// <summary>
        ///     The opacity factor of all rain droplets.
        /// </summary>
        /// <remarks>
        ///     This exists to offset the effect of using <see cref="RainVelocityFactor"/>, so that the speed of rain doesn't incur issues pertaining to visual noise.
        /// </remarks>
        public static float Opacity => 0.6f;

        /// <summary>
        ///     The factor by which all rain droplet velocities are multiplied.
        /// </summary>
        public static readonly Vector2 RainVelocityFactor = new(0.8f, 2.3f);

        public override void OnModLoad()
        {
            On_Rain.GetRainFallVelocity += MakeRainFallFaster;
            IL_Main.DrawRain += MakeRainMoreTranslucent;
        }

        private Vector2 MakeRainFallFaster(On_Rain.orig_GetRainFallVelocity orig)
        {
            return orig() * RainVelocityFactor;
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

            // Go right before the storage of the color variable.
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStloc(out _)))
            {
                Mod.Logger.Warn("The rain translucency IL edit could not load, due to the Stloc match failing.");
                return;
            }

            // Multiply the color by a given opacity value.
            // Since this is right before the stloc instruction the value above is the already completed color, and it's possible to freely modify its value further before
            // it gets properly stored.
            MethodInfo opacityGetter = typeof(RainReplacementManager).GetMethod("get_Opacity");
            MethodInfo colorFloatMultiply = typeof(Color).GetMethod("op_Multiply", [typeof(Color), typeof(float)]);
            cursor.Emit(OpCodes.Call, opacityGetter);
            cursor.Emit(OpCodes.Call, colorFloatMultiply);
        }
    }
}
