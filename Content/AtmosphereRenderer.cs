using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class AtmosphereRenderer : ModSystem
    {
        /// <summary>
        /// The identifier key for the sky's atmosphere shader.
        /// </summary>
        public const string AtmosphereShaderKey = "RealisticSky:AtmosphereShader";

        public override void OnModLoad()
        {
            GameShaders.Misc[AtmosphereShaderKey] = new MiscShaderData(new(ModContent.Request<Effect>("RealisticSky/Assets/Effects/AtmosphereShader", AssetRequestMode.ImmediateLoad).Value), "AutoloadPass");
        }

        public static void Render(float worldYInterpolant, float spaceInterpolant)
        {
            // Calculate the true screen size.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            // Calculate opacity and brightness values based on a combination of how far in space the player is and what the general sky brightness is.
            float surfaceInterpolant = Utils.GetLerpValue(0.071f, 0.11f, worldYInterpolant, true);
            float radius = MathHelper.Lerp(17000f, 7000f, spaceInterpolant);
            float yOffset = (spaceInterpolant * 600f + 250f) * screenSize.Y / 1440f;
            float baseSkyBrightness = (Main.ColorOfTheSkies.R + Main.ColorOfTheSkies.G + Main.ColorOfTheSkies.B) / 765f;
            float specialSkyOpacity = Utils.GetLerpValue(0.08f, 0.2f, baseSkyBrightness + spaceInterpolant * 0.4f, true) * MathHelper.Lerp(1f, 0.5f, surfaceInterpolant) * Utils.Remap(baseSkyBrightness, 0.078f, 0.16f, 0.9f, 1f);

            // Prepare the sky shader.
            Effect shader = GameShaders.Misc[AtmosphereShaderKey].Shader;
            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["atmosphereRadius"]?.SetValue(radius);
            shader.Parameters["planetRadius"]?.SetValue(radius * 0.8f);
            shader.Parameters["invertedGravity"]?.SetValue(Main.LocalPlayer.gravDir == -1f);
            shader.Parameters["performanceMode"]?.SetValue(RealisticSkyConfig.Instance.PerformanceMode);
            shader.Parameters["screenHeight"]?.SetValue(screenSize.Y);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(SunPositionSaver.SunPosition, -50f));
            shader.Parameters["planetPosition"]?.SetValue(new Vector2(screenSize.X * 0.5f, radius + yOffset));
            shader.Parameters["rgbLightWavelengths"]?.SetValue(new Vector3(750f, 530f, 430f));
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw the sky.
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPosition = screenSize * 0.5f;
            Vector2 skyScale = screenSize / pixel.Size();
            Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White * specialSkyOpacity, 0f, pixel.Size() * 0.5f, skyScale, 0, 0f);
        }
    }
}
