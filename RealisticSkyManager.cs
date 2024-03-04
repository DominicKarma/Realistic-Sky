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

            // Prepare for sky drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.ZoomMatrix);

            DrawSky();

            // Return to standard drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }

        public static void DrawSky()
        {
            // Prepare the sky shader.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            float spaceInterpolant = Utils.GetLerpValue(3200f, 300f, Main.screenPosition.Y, true);
            float surfaceInterpolant = Utils.GetLerpValue(3000f, 5000f, Main.screenPosition.Y, true);
            float radius = MathHelper.Lerp(40000f, 400f, spaceInterpolant);
            float yOffset = spaceInterpolant * 600f + 250f;
            float baseSkyBrightness = (Main.ColorOfTheSkies.R + Main.ColorOfTheSkies.G + Main.ColorOfTheSkies.B) / 765f;
            float specialSkyOpacity = Utils.GetLerpValue(0.08f, 0.2f, baseSkyBrightness + spaceInterpolant * 0.287f, true) * MathHelper.Lerp(1f, 0.5f, surfaceInterpolant);

            Effect shader = GameShaders.Misc[ShaderKey].Shader;
            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["atmosphereRadius"]?.SetValue(radius);
            shader.Parameters["planetRadius"]?.SetValue(radius * 0.8f);
            shader.Parameters["invertedGravity"]?.SetValue(Main.LocalPlayer.gravDir == -1f);
            shader.Parameters["screenHeight"]?.SetValue(screenSize.Y);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(screenSize.X * 0.5f, 0f, 100f));
            shader.Parameters["planetPosition"]?.SetValue(new Vector2(screenSize.X * 0.5f, radius + yOffset));
            shader.Parameters["rgbLightWavelengths"]?.SetValue(new Vector3(650f, 530f, 430f));
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
