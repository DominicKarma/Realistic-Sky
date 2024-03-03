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
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.BackgroundViewMatrix.TransformationMatrix);

            DrawSky();

            // Return to standard drawing.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.BackgroundViewMatrix.EffectMatrix);
        }

        public static void DrawSky()
        {
            // Prepare the sky shader.
            Effect shader = GameShaders.Misc[ShaderKey].Shader;
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw the sky.
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPosition = Vector2.Zero;
            Vector2 skyScale = new Vector2(Main.screenWidth, Main.screenHeight) / pixel.Size();
            Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White, 0f, Vector2.Zero, skyScale, 0, 0f);
        }

        public override void Update(GameTime gameTime)
        {
            if (Main.gameMenu)
                skyActive = false;

            Opacity = MathHelper.Clamp(Opacity + skyActive.ToDirectionInt() * 0.1f, 0f, 1f);
        }

        public override float GetCloudAlpha() => 1f - Opacity;
    }
}
