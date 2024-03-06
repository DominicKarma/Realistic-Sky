using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;

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

        /// <summary>
        /// How long, in frames, that sunrises should last for the purposes of this sky's visuals.
        /// </summary>
        public const int DawnDuration = 6700;

        /// <summary>
        /// How long, in frames, that sunsets should last for the purposes of this sky's visuals.
        /// </summary>
        public const int DuskDuration = 6700;

        /// <summary>
        /// Where the space interpolant begins as a 0-1 ratio.
        /// </summary>
        /// <remarks>
        /// In this context, "ratio" refers to the world height. A value of 0.05 would, for example, correspond to the upper 5% of the world's height.
        /// </remarks>
        public const float SpaceYRatioStart = 0.074f;

        /// <summary>
        /// Where the space interpolant is considered at its maximum as a 0-1 ratio.
        /// </summary>
        /// <remarks>
        /// In this context, "ratio" refers to the world height. A value of 0.05 would, for example, correspond to the upper 5% of the world's height.
        /// </remarks>
        public const float SpaceYRatioEnd = 0.024f;

        /// <summary>
        /// The intensity of light based on dawn or dusk as a 0-1 ratio.
        /// </summary>
        public static float SunlightIntensityByTime
        {
            get
            {
                // Return 0 immediately if it's night time, since night time does not count towards dawn or dusk.
                if (!Main.dayTime)
                    return 0f;

                // If the time is less than the dawn duration, interpolate between it.
                // This will make the slope of this function go up from 0 to 1.
                float dawnDuskInterpolant = Utils.GetLerpValue(0f, DawnDuration, (float)Main.time, true);

                // If the time is greater than the dawn duration, account for the dusk duration instead.
                // Since this is a multiplication, it will be multiplying the previous result (which is 1, since again, this only happens after dawn is over) by
                // the dusk interpolant. This will make the value's slow go down as dusk's progression increases, until eventually it's 0 again by night time.
                if (Main.time > DawnDuration)
                    dawnDuskInterpolant *= Utils.GetLerpValue((float)Main.dayLength, (float)Main.dayLength - DuskDuration, (float)Main.time, true);

                return dawnDuskInterpolant;
            }
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
            float spaceInterpolant = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(SpaceYRatioStart, SpaceYRatioEnd, worldYInterpolant, true));

            // Draw stars.
            Vector2 transformedSunPosition = Vector2.Transform(SunPositionSaver.SunPosition, Matrix.Invert(backgroundMatrix));
            StarsRenderer.Render(spaceInterpolant, SunlightIntensityByTime, Opacity, transformedSunPosition);

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
                SunRenderer.Render(spaceInterpolant, 1f - SunlightIntensityByTime);
            }

            // Return to standard drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, backgroundMatrix);
        }

        public override void Update(GameTime gameTime)
        {
            if (Main.gameMenu)
                skyActive = false;

            // Increase or decrease the opacity of this sky based on whether it's active or not, stopping at 0-1 bounds.
            Opacity = MathHelper.Clamp(Opacity + skyActive.ToDirectionInt() * 0.1f, 0f, 1f);
        }

        #region Boilerplate
        public override void Deactivate(params object[] args) => skyActive = false;

        public override void Reset() => skyActive = false;

        public override bool IsActive() => skyActive || Opacity > 0f;

        public override void Activate(Vector2 position, params object[] args) => skyActive = true;

        // Ensure that cloud opacities are not disturbed by this sky effect.
        public override float GetCloudAlpha() => 1f;

        #endregion Boilerplate
    }
}
