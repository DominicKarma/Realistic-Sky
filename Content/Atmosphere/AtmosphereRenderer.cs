using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Common.DataStructures;
using RealisticSky.Content.Sun;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RealisticSky.Content.Atmosphere
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
            GameShaders.Misc[AtmosphereShaderKey] = new MiscShaderData(ModContent.Request<Effect>("RealisticSky/Assets/Effects/AtmosphereShader"), "AutoloadPass");

            // Initialize the atmosphere target.
            AtmosphereTarget = new();
            Main.ContentThatNeedsRenderTargets.Add(AtmosphereTarget);
        }

        public static void RenderToTarget()
        {
            // Since this can render on the mod screen it's important that the shader be checked for if it's disposed or not.
            if (!GameShaders.Misc.TryGetValue(AtmosphereShaderKey, out MiscShaderData s) || RealisticSkyConfig.Instance is null)
                return;
            Effect shader = s.Shader;
            if (shader?.IsDisposed ?? true)
                return;

            SkyPlayerSnapshot player = SkyPlayerSnapshot.TakeSnapshot();
            float spaceInterpolant = RealisticSkyManager.SpaceHeightInterpolant;

            // Calculate the true screen size.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            // Calculate opacity and brightness values based on a combination of how far in space the player is and what the general sky brightness is.
            float worldYInterpolant = player.Center.Y / player.MaxTilesY / 16f;
            float upperSurfaceRatioStart = (float)(player.WorldSurface / player.MaxTilesY) * 0.5f;
            float surfaceInterpolant = Utils.GetLerpValue(RealisticSkyManager.SpaceYRatioStart, upperSurfaceRatioStart, worldYInterpolant, true);

            float radius = MathHelper.Lerp(17000f, 6400f, spaceInterpolant);
            float yOffset = (spaceInterpolant * 600f + 250f) * screenSize.Y / 1440f;
            float baseSkyBrightness = RealisticSkyManager.SkyBrightness;
            float atmosphereOpacity = Utils.GetLerpValue(0.08f, 0.2f, baseSkyBrightness + spaceInterpolant * 0.4f, true) * MathHelper.Lerp(1f, 0.5f, surfaceInterpolant) * Utils.Remap(baseSkyBrightness, 0.078f, 0.16f, 0.9f, 1f);

            // Calculate the exponential sunlight exposure coefficient.
            float sunlightExposure = Utils.Remap(RealisticSkyConfig.Instance.SunlightExposure, RealisticSkyConfig.MinSunlightExposure, RealisticSkyConfig.MaxSunlightExposure, 0.4f, 1.6f);

            // Prepare the sky shader.
            RealisticSkyConfig config = RealisticSkyConfig.Instance;
            Vector3 lightWavelengths = new(config.RedWavelength, config.GreenWavelength, config.BlueWavelength);

            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["atmosphereRadius"]?.SetValue(radius);
            shader.Parameters["planetRadius"]?.SetValue(radius * 0.8f);
            shader.Parameters["invertedGravity"]?.SetValue(player.InvertedGravity);
            shader.Parameters["performanceMode"]?.SetValue(RealisticSkyConfig.Instance.PerformanceMode);
            shader.Parameters["screenHeight"]?.SetValue(screenSize.Y);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(SunPositionSaver.SunPosition - Vector2.UnitY * Main.sunModY * RealisticSkyManager.SpaceHeightInterpolant, -500f));
            shader.Parameters["planetPosition"]?.SetValue(new Vector3(screenSize.X * 0.4f, radius + yOffset, 0f));
            shader.Parameters["rgbLightWavelengths"]?.SetValue(lightWavelengths);
            shader.Parameters["sunlightExposure"]?.SetValue(sunlightExposure);
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw the atmosphere.
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPosition = screenSize * 0.5f;
            Vector2 skyScale = screenSize / pixel.Size();
            Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White * atmosphereOpacity, 0f, pixel.Size() * 0.5f, skyScale, 0, 0f);
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
