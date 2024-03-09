using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content.NightSky
{
    public class NightSkyBrightnessManager : ModSystem
    {
        /// <summary>
        /// The amount by which the brightness of the sky should be boosted.
        /// </summary>
        public static float NightSkyBrightnessBoost
        {
            get
            {
                float nightCompletion = (float)(Main.time / Main.nightLength);
                if (Main.dayTime)
                    nightCompletion = 0f;

                return Utils.GetLerpValue(0f, 0.15f, nightCompletion, true) * Utils.GetLerpValue(1f, 0.85f, nightCompletion, true) * 0.08f;
            }
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            backgroundColor = new Color(backgroundColor.ToVector3() + new Vector3(1.08f, 0.7f, 0.6f) * NightSkyBrightnessBoost);
        }
    }
}
