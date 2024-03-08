using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class SunRenderer : ModSystem
    {
        internal static Asset<Texture2D> BloomAsset;

        internal static Asset<Texture2D> EclipseMoonAsset;

        public override void OnModLoad()
        {
            BloomAsset = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/BloomCircle");
            EclipseMoonAsset = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/EclipseMoon");
        }

        public static void Render(float sunriseAndSetInterpolant)
        {
            if (BloomAsset.IsDisposed)
                return;

            // Make things stronger when in space, and weaker during sunrises and sunsets.
            float bloomOpacity = MathHelper.Lerp(1f, 0.5f, sunriseAndSetInterpolant);
            float spaceInterpolant = RealisticSkyManager.SpaceHeightInterpolant;
            float scaleFactor = (spaceInterpolant * 0.21f + 1f) * bloomOpacity;
            float pureWhiteInterpolant = MathF.Pow(spaceInterpolant, 2f);
            float lensFlareOpacity = spaceInterpolant;
            Vector2 sunPosition = SunPositionSaver.SunPosition;
            Texture2D bloom = BloomAsset.Value;

            if (Main.eclipse)
            {
                pureWhiteInterpolant = 0.3f;
                lensFlareOpacity = 0.5f;
                sunriseAndSetInterpolant = 0f;
            }

            // Draw the innermost, bright yellow bloom.
            Main.spriteBatch.Draw(bloom, sunPosition, null, new Color(1f, 1f, 0.92f, 0f) * bloomOpacity, 0f, bloom.Size() * 0.5f, (MathF.Pow(scaleFactor, 1.5f) + pureWhiteInterpolant * 1.5f) * 0.9f, 0, 0f);

            // Make successive bloom draws weaker in accordance with the white interpolant.
            bloomOpacity *= MathHelper.Lerp(1f, 0.25f, pureWhiteInterpolant);

            // Draw the mid-bloom. This is still mostly bright, but is biased a bit towards bright maroons during sunrises and sunsets.
            Color midBloomColor = Color.Lerp(new Color(1f, 1f - sunriseAndSetInterpolant, 0.5f, 0f), Color.White with { A = 0 }, pureWhiteInterpolant);
            Main.spriteBatch.Draw(bloom, sunPosition, null, midBloomColor * bloomOpacity * 0.48f, 0f, bloom.Size() * 0.5f, scaleFactor * 1.6f, 0, 0f);

            // Draw the outermost bloom. This is mostly red, with a tinge of blue.
            Color outerBloomColor = Color.Lerp(new Color(1f - pureWhiteInterpolant, 0f, pureWhiteInterpolant * 0.96f + 0.3f, 0f), Color.White with { A = 0 }, pureWhiteInterpolant);
            if (Main.eclipse)
                outerBloomColor = new(1f, 0.3f, 0f, 0f);

            Main.spriteBatch.Draw(bloom, sunPosition, null, outerBloomColor * bloomOpacity * 0.3f, 0f, bloom.Size() * 0.5f, scaleFactor * (2.1f - sunriseAndSetInterpolant + pureWhiteInterpolant), 0, 0f);

            // Draw a lens flare everything when in space.
            // This can also occur during eclipses.
            if (lensFlareOpacity > 0f)
            {
                Texture2D lensFlare = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/LensFlare").Value;
                float lensFlareScale = MathF.Pow(lensFlareOpacity, 0.7f) * 1.3f;
                Color lensFlareColor = Main.eclipse ? Color.LightGoldenrodYellow : Color.LightCyan;
                lensFlareColor *= lensFlareOpacity * lensFlareScale * 0.35f;
                lensFlareColor.A = 0;
                Main.spriteBatch.Draw(lensFlare, sunPosition, null, lensFlareColor, Main.GlobalTimeWrappedHourly * -0.02f, new(352f, 405f), lensFlareScale, 0, 0f);
                Main.spriteBatch.Draw(lensFlare, sunPosition, null, lensFlareColor, Main.GlobalTimeWrappedHourly * 0.03f, new(352f, 405f), lensFlareScale, 0, 0f);
            }

            // Draw the moon over the sun during an eclipse.
            if (Main.eclipse)
                DrawEclipseOverlay();
        }

        public static void DrawEclipseOverlay()
        {
            Texture2D moon = EclipseMoonAsset.Value;
            Texture2D bloom = BloomAsset.Value;
            Vector2 sunPosition = SunPositionSaver.SunPosition;
            for (int i = 0; i < 2; i++)
                Main.spriteBatch.Draw(bloom, sunPosition, null, new(1f, 1f, 1f, 0f), 0f, bloom.Size() * 0.5f, 1.3f, 0, 0f);
            Main.spriteBatch.Draw(moon, sunPosition, null, Color.White, 0f, moon.Size() * 0.5f, 0.44f, 0, 0f);
        }
    }
}
