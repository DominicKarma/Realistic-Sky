using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class SunPositionSaver : ModSystem
    {
        /// <summary>
        /// The sun's position in the sky.
        /// </summary>
        /// <remarks>
        /// This does not have any in-built transformations. Be sure to use <see cref="Vector2.Transform(Vector2, Matrix)"/> where applicable when using this.
        /// </remarks>
        public static Vector2 SunPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// The moon's position in the sky.
        /// </summary>
        /// <remarks>
        /// This does not have any in-built transformations. Be sure to use <see cref="Vector2.Transform(Vector2, Matrix)"/> where applicable when using this.
        /// </remarks>
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

            // Bias the sun and moon.
            cursor.EmitDelegate(VerticallyBiasSunAndMoon);

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

        public static void VerticallyBiasSunAndMoon()
        {
            // Let the sun and moon positions return to normal on the title screen.
            if (Main.gameMenu)
                return;

            // Make the sunset and sunrise positions more natural.
            float dayCompletion = (float)(Main.time / Main.dayLength);
            float nightCompletion = (float)(Main.time / Main.nightLength);
            Main.sunModY = (short)((1f - MathF.Sin(dayCompletion * MathHelper.Pi)) * 640f);
            Main.moonModY = (short)(MathF.Sin(nightCompletion * MathHelper.Pi) * -220f + 200f);
        }
    }
}
