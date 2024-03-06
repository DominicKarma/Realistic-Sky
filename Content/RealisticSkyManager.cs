﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class RealisticSkyManager : CustomSky
    {
        private bool skyActive;

        internal static new float Opacity;

        /// <summary>
        /// The identifier key for this sky.
        /// </summary>
        public const string SkyKey = "RealisticSky:Sky";

        public override void Deactivate(params object[] args)
        {
            skyActive = false;
        }

        public override void Reset()
        {
            skyActive = false;
        }

        public override bool IsActive()
        {
            return skyActive || Opacity > 0f;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            skyActive = true;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth < float.MaxValue || minDepth >= float.MaxValue)
                return;

            // Calculate the background draw matrix in advance.
            Matrix backgroundMatrix = Main.BackgroundViewMatrix.TransformationMatrix;
            Vector3 translationDirection = new(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically) ? -1f : 1f, 1f);
            backgroundMatrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * translationDirection;

            // Calculate various useful interpolants in advance.
            float worldYInterpolant = Main.LocalPlayer.Center.Y / Main.maxTilesY / 16f;
            float spaceInterpolant = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.074f, 0.024f, worldYInterpolant, true));
            float sunriseAndSetInterpolant = Utils.GetLerpValue(0f, 6700f, (float)Main.time, true);
            if (Main.time > 6700f)
                sunriseAndSetInterpolant *= Utils.GetLerpValue((float)Main.dayLength, (float)Main.dayLength - 6700f, (float)Main.time, true);
            if (!Main.dayTime)
                sunriseAndSetInterpolant = 0f;

            // Draw stars.
            DrawStars(spaceInterpolant, sunriseAndSetInterpolant, Vector2.Transform(SunPositionSaver.SunPosition, Matrix.Invert(backgroundMatrix)));

            // Prepare for atmosphere drawing by allowing shaders.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, backgroundMatrix);

            // Draw the atmosphere.
            AtmosphereRenderer.Render(worldYInterpolant, spaceInterpolant);

            // Draw bloom over the sun.
            if (!Main.eclipse && Main.dayTime)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
                SunRenderer.Render(spaceInterpolant, 1f - sunriseAndSetInterpolant);
            }

            // Return to standard drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, backgroundMatrix);
        }

        public static void DrawStars(float spaceInterpolant, float sunriseAndSetInterpolant, Vector2 sunPosition)
        {
            // Make vanilla's stars disappear. They are not needed.
            for (int i = 0; i < Main.numStars; i++)
                Main.star[i] = new();

            // Draw custom stars.
            float skyBrightness = (Main.ColorOfTheSkies.R + Main.ColorOfTheSkies.G + Main.ColorOfTheSkies.B) / 765f;
            float starOpacity = MathHelper.Clamp(MathF.Pow(1f - Main.atmo, 3f) + MathF.Pow(1f - skyBrightness, 5f), 0f, 1f) * Opacity;
            if (starOpacity <= 0f)
                return;

            int starCount = RealisticSkyConfig.Instance.PerformanceMode ? 1050 : 2000;
            float yOffset = spaceInterpolant * 600f + 250f;
            ulong starSeed = 71493uL;
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            Texture2D bloom = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/BloomCircle").Value;
            for (int i = 0; i < starCount; i++)
            {
                // Randomly calculate the star's draw position.
                float starX = MathHelper.Lerp(-350f, screenSize.X + 350f, Utils.RandomFloat(ref starSeed));
                float starY = MathHelper.Lerp(-10f, screenSize.Y * 0.5f, MathF.Pow(Utils.RandomFloat(ref starSeed), 1.5f));

                // Randomly calculate the size of the star. This is heavily biased towards the minimum value, to ensure that large stars are a rarity.
                float starSize = MathHelper.Lerp(0.12f, 0.42f, MathF.Pow(Utils.RandomFloat(ref starSeed), 5.5f));

                // Randomly calculate the star's color. Most are white-ish yellow, but some can have a blue-ish white color
                float starColorInterpolant = Utils.RandomFloat(ref starSeed);
                float starFlarePulse = MathHelper.Lerp(0.85f, 1.2f, MathF.Cos(Main.GlobalTimeWrappedHourly * 4.4f + i * 2.3f) * 0.5f + 0.5f);
                Color starColor = Color.Lerp(Color.Wheat, Color.LightGoldenrodYellow, starColorInterpolant);
                starColor = Color.Lerp(starColor, Color.Cyan, Utils.GetLerpValue(0.7f, 1f, starColorInterpolant) * 0.6f);
                starColor.A = 0;

                // Make stars that go into the atmosphere far, far less visible.
                float atmospherePieceInterpolant = MathHelper.Lerp(Utils.GetLerpValue(-120f, -170f, starY - yOffset, true), 1f, MathF.Pow(1f - sunriseAndSetInterpolant, 2f));
                float localStarOpacity = starOpacity * atmospherePieceInterpolant;

                // Make stars super close to the sun far, far less visible, since its light should overpower that of the super distant stars.
                float distanceToSun = new Vector2(starX, starY).Distance(sunPosition);
                localStarOpacity *= MathF.Pow(Utils.GetLerpValue(70f, 160f, distanceToSun - spaceInterpolant * 50f, true), 2.5f);

                // Draw the star.
                Color brightStarColor = Color.Lerp(starColor, Color.White with { A = 0 }, 0.5f);
                Vector2 starDrawPosition = new(starX, starY);
                Main.spriteBatch.Draw(bloom, starDrawPosition, null, starColor * localStarOpacity * 0.9f, 0f, bloom.Size() * 0.5f, starSize / starFlarePulse * 0.2f, 0, 0f);
                if (starSize >= 0.14f)
                    Main.spriteBatch.Draw(bloom, starDrawPosition, null, brightStarColor * localStarOpacity, 0f, bloom.Size() * 0.5f, starSize / starFlarePulse * 0.1f, 0, 0f);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Main.gameMenu)
                skyActive = false;

            Opacity = MathHelper.Clamp(Opacity + skyActive.ToDirectionInt() * 0.1f, 0f, 1f);
        }

        public override float GetCloudAlpha() => 1f;
    }
}
