using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace RealisticSky
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
        /// The identifier key for this sky's shader.
        /// </summary>
        public const string ShaderKey = "RealisticSky:Shader";

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

            Matrix backgroundMatrix = Main.BackgroundViewMatrix.TransformationMatrix;
            Vector3 translationDirection = new(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically) ? -1f : 1f, 1f);

            backgroundMatrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * translationDirection;

            // Prepare for sky drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, backgroundMatrix);

            DrawSky();

            // Return to standard drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, backgroundMatrix);
        }

        public static void DrawSky()
        {
            // Prepare the sky shader.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            float worldYInterpolant = Main.LocalPlayer.Center.Y / Main.maxTilesY / 16f;
            float spaceInterpolant = Utils.GetLerpValue(0.074f, 0.01708f, worldYInterpolant, true);
            float surfaceInterpolant = Utils.GetLerpValue(0.071f, 0.11f, worldYInterpolant, true);
            float radius = MathHelper.Lerp(17000f, 4500f, spaceInterpolant);
            float yOffset = spaceInterpolant * 600f + 250f;
            float baseSkyBrightness = (Main.ColorOfTheSkies.R + Main.ColorOfTheSkies.G + Main.ColorOfTheSkies.B) / 765f;
            float specialSkyOpacity = Utils.GetLerpValue(0.08f, 0.2f, baseSkyBrightness + spaceInterpolant * 0.4f, true) * MathHelper.Lerp(1f, 0.5f, surfaceInterpolant);

            Effect shader = GameShaders.Misc[ShaderKey].Shader;
            Vector2 sunPosition = RealisticSkyManagerScene.SunPosition;
            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["atmosphereRadius"]?.SetValue(radius);
            shader.Parameters["planetRadius"]?.SetValue(radius * 0.8f);
            shader.Parameters["invertedGravity"]?.SetValue(Main.LocalPlayer.gravDir == -1f);
            shader.Parameters["performanceMode"]?.SetValue(RealisticSkyConfig.Instance.PerformanceMode);
            shader.Parameters["screenHeight"]?.SetValue(screenSize.Y);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(sunPosition, -50f));
            shader.Parameters["planetPosition"]?.SetValue(new Vector2(screenSize.X * 0.5f, radius + yOffset));
            shader.Parameters["rgbLightWavelengths"]?.SetValue(new Vector3(750f, 530f, 430f));
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw the sky.
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPosition = screenSize * 0.5f;
            Vector2 skyScale = screenSize / pixel.Size();
            Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White * specialSkyOpacity, 0f, pixel.Size() * 0.5f, skyScale, 0, 0f);
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
