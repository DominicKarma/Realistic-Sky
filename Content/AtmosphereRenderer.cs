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
        /// The render target that holds the contents of the atmosphere.
        /// </summary>
        internal static AtmosphereTargetContent AtmosphereTarget;

        /// <summary>
        /// The identifier key for the sky's atmosphere shader.
        /// </summary>
        public const string AtmosphereShaderKey = "RealisticSky:AtmosphereShader";

        public override void OnModLoad()
        {
            // Store the atmosphere shader.
            GameShaders.Misc[AtmosphereShaderKey] = new MiscShaderData(new(ModContent.Request<Effect>("RealisticSky/Assets/Effects/AtmosphereShader", AssetRequestMode.ImmediateLoad).Value), "AutoloadPass");

            // Initialize the atmosphere target.
            AtmosphereTarget = new();
            Main.ContentThatNeedsRenderTargets.Add(AtmosphereTarget);
        }

        public static void RenderToTarget()
        {
            float spaceInterpolant = RealisticSkyManager.SpaceHeightInterpolant;

            // Calculate the true screen size.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            // Calculate opacity and brightness values based on a combination of how far in space the player is and what the general sky brightness is.
            float upperSurfaceRatioStart = (float)(Main.worldSurface / Main.maxTilesY) * 0.5f;
            float worldYInterpolant = Main.LocalPlayer.Center.Y / Main.maxTilesY / 16f;
            float surfaceInterpolant = Utils.GetLerpValue(RealisticSkyManager.SpaceYRatioStart, upperSurfaceRatioStart, worldYInterpolant, true);

            float radius = MathHelper.Lerp(17000f, 6400f, spaceInterpolant);
            float yOffset = (spaceInterpolant * 600f + 250f) * screenSize.Y / 1440f;
            float baseSkyBrightness = (Main.ColorOfTheSkies.R + Main.ColorOfTheSkies.G + Main.ColorOfTheSkies.B) / 765f;
            float specialSkyOpacity = Utils.GetLerpValue(0.08f, 0.2f, baseSkyBrightness + spaceInterpolant * 0.4f, true) * MathHelper.Lerp(1f, 0.5f, surfaceInterpolant) * Utils.Remap(baseSkyBrightness, 0.078f, 0.16f, 0.9f, 1f);

            // Prepare the sky shader.
            RealisticSkyConfig config = RealisticSkyConfig.Instance;
            Vector3 lightWavelengths = new(config.RedWavelength, config.GreenWavelength, config.BlueWavelength);
            Effect shader = GameShaders.Misc[AtmosphereShaderKey].Shader;
            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["atmosphereRadius"]?.SetValue(radius);
            shader.Parameters["planetRadius"]?.SetValue(radius * 0.8f);
            shader.Parameters["invertedGravity"]?.SetValue(Main.LocalPlayer.gravDir == -1f);
            shader.Parameters["performanceMode"]?.SetValue(RealisticSkyConfig.Instance.PerformanceMode);
            shader.Parameters["screenHeight"]?.SetValue(screenSize.Y);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(SunPositionSaver.SunPosition, -50f));
            shader.Parameters["planetPosition"]?.SetValue(new Vector2(screenSize.X * 0.4f, radius + yOffset));
            shader.Parameters["rgbLightWavelengths"]?.SetValue(lightWavelengths);
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw the atmosphere.
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPosition = screenSize * 0.5f;
            Vector2 skyScale = screenSize / pixel.Size();
            Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White * specialSkyOpacity, 0f, pixel.Size() * 0.5f, skyScale, 0, 0f);
        }

        public static void RenderFromTarget()
        {
            AtmosphereTarget.Request();

            // If the drawer isn't ready, wait until it is.
            if (!AtmosphereTarget.IsReady)
                return;

            Main.spriteBatch.Draw(AtmosphereTarget.GetTarget(), Vector2.Zero, Color.White);
        }
    }
}
