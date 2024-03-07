using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Common.DataStructures;
using Terraria;
using Terraria.Graphics.Effects;

namespace RealisticSky.Content
{
    public class RealisticSkyManager : CustomSky
    {
        private bool skyActive;

        /// <summary>
        /// The general opacity of this sky.
        /// </summary>
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

        /// <summary>
        /// How far up in space the player is, on a 0-1 interpolant.
        /// </summary>
        public static float SpaceHeightInterpolant
        {
            get
            {
                SkyPlayerSnapshot player = SkyPlayerSnapshot.TakeSnapshot();
                float worldYInterpolant = player.Center.Y / player.MaxTilesY / 16f;
                float spaceInterpolant = Utils.GetLerpValue(SpaceYRatioStart, SpaceYRatioEnd, worldYInterpolant, true);

                // Apply a smoothstep function to the space interpolant, since that helps make the transitions more natural.
                return MathHelper.SmoothStep(0f, 1f, spaceInterpolant);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            // Prevent drawing beyond the back layer.
            if (maxDepth < float.MaxValue || minDepth >= float.MaxValue)
                return;

            // Calculate the background draw matrix in advance.
            Matrix backgroundMatrix = Main.BackgroundViewMatrix.TransformationMatrix;
            Vector3 translationDirection = new(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically) ? -1f : 1f, 1f);
            backgroundMatrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * translationDirection;

            // Draw stars.
            StarsRenderer.Render(Opacity, backgroundMatrix);

            // Draw the atmosphere.
            AtmosphereRenderer.RenderFromTarget();

            // Draw bloom over the sun.
            if (!Main.eclipse && Main.dayTime)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
                SunRenderer.Render(1f - SunlightIntensityByTime);
            }

            // Draw clouds.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, backgroundMatrix);
            CloudsRenderer.Render();

            // Return to standard drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, backgroundMatrix);
        }

        public override void Update(GameTime gameTime)
        {
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
