using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class SunRenderer : ModSystem
    {
        public static void Render(float spaceInterpolant, float sunriseAndSetInterpolant)
        {
            // Make things stronger when in space, and weaker during sunrises and sunsets.
            float bloomOpacity = MathHelper.Lerp(1f, 0.5f, sunriseAndSetInterpolant);
            float scaleFactor = (spaceInterpolant * 0.21f + 1f) * bloomOpacity;
            float pureWhiteInterpolant = MathF.Pow(spaceInterpolant, 2f);
            Vector2 sunPosition = SunPositionSaver.SunPosition;
            Texture2D bloom = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/BloomCircle").Value;

            // Draw the innermost, bright yellow bloom.
            Main.spriteBatch.Draw(bloom, sunPosition, null, new Color(1f, 1f, 0.92f, 0f) * bloomOpacity, 0f, bloom.Size() * 0.5f, (MathF.Pow(scaleFactor, 1.5f) + pureWhiteInterpolant * 1.5f) * 0.9f, 0, 0f);

            // Make successive bloom draws weaker in accordance with the white interpolant.
            bloomOpacity *= MathHelper.Lerp(1f, 0.25f, pureWhiteInterpolant);

            // Draw the mid-bloom. This is still mostly bright, but is biased a bit towards bright maroons during sunrises and sunsets.
            Color midBloomColor = Color.Lerp(new Color(1f, 1f - sunriseAndSetInterpolant, 0.5f, 0f), Color.White with { A = 0 }, pureWhiteInterpolant);
            Main.spriteBatch.Draw(bloom, sunPosition, null, midBloomColor * bloomOpacity * 0.48f, 0f, bloom.Size() * 0.5f, scaleFactor * 1.6f, 0, 0f);

            // Draw the outermost bloom. This is mostly red, with a tinge of blue.
            Color outerBloomColor = Color.Lerp(new Color(1f - pureWhiteInterpolant, 0f, pureWhiteInterpolant * 0.96f + 0.3f, 0f), Color.White with { A = 0 }, pureWhiteInterpolant);
            Main.spriteBatch.Draw(bloom, sunPosition, null, outerBloomColor * bloomOpacity * 0.3f, 0f, bloom.Size() * 0.5f, scaleFactor * (2.1f - sunriseAndSetInterpolant + pureWhiteInterpolant), 0, 0f);

            // Draw a lens flare over everything when in space.
            if (spaceInterpolant > 0f)
            {
                int lensFlareCount = 1;
                ulong seed = 11589uL;

                Texture2D lensFlare = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/LensFlare").Value;
                for (int i = 0; i < lensFlareCount; i++)
                {
                    float lensFlareScaleInterpolant = MathF.Cos(Utils.RandomFloat(ref seed) * MathHelper.TwoPi + Main.GlobalTimeWrappedHourly * 0.3f) * 0.5f + 0.5f;
                    float lensFlareScale = MathF.Pow(spaceInterpolant, 0.7f) * MathHelper.Lerp(0.8f, 1.1f, lensFlareScaleInterpolant);
                    Color lensFlareColor = Color.LightGoldenrodYellow;
                    lensFlareColor.R -= 10;
                    lensFlareColor.B += 12;
                    lensFlareColor *= spaceInterpolant * lensFlareScale;
                    lensFlareColor.A = 0;

                    float lensFlareRotation = MathHelper.TwoPi * i / lensFlareCount + Main.GlobalTimeWrappedHourly * 0.045f;
                    Main.spriteBatch.Draw(lensFlare, sunPosition, null, lensFlareColor, lensFlareRotation + lensFlareScaleInterpolant * 0.1f, lensFlare.Size() * 0.5f, lensFlareScale, 0, 0f);
                }
            }
        }
    }
}
