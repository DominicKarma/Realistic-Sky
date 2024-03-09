using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Assets;
using RealisticSky.Common.DataStructures;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class CloudsRenderer : ModSystem
    {
        /// <summary>
        /// The horizontal offset of clouds.
        /// </summary>
        public static float CloudHorizontalOffset
        {
            get;
            set;
        }

        /// <summary>
        /// The identifier key for the sky's cloud shader.
        /// </summary>
        public const string CloudShaderKey = "RealisticSky:CloudShader";

        public override void OnModLoad()
        {
            // Store the cloud shader.
            GameShaders.Misc[CloudShaderKey] = new MiscShaderData(new(ModContent.Request<Effect>("RealisticSky/Assets/Effects/CloudShader", AssetRequestMode.ImmediateLoad).Value), "AutoloadPass");
        }

        public static void Render()
        {
            // Don't do anything if the realistic clouds config is disabled.
            if (!RealisticSkyConfig.Instance.RealisticClouds)
                return;

            // Disable normal clouds.
            Main.cloudBGAlpha = 0f;
            for (int i = 0; i < Main.maxClouds; i++)
                Main.cloud[i].active = false;

            // Calculate the cloud opacity, capping it at 1.
            float cloudOpacity = MathF.Max((Main.numCloudsTemp - 20) / (float)Main.maxClouds, Main.cloudAlpha);
            if (cloudOpacity > 1f)
                cloudOpacity = 1f;

            // If the cloud opacity is 0 or less, that means that there's nothing to draw and this method should terminate immediately for performance reasons.
            if (cloudOpacity <= 0f)
                return;

            // Calculate the true screen size.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            // Move clouds.
            if (!Main.gamePaused)
                CloudHorizontalOffset -= Main.windSpeedCurrent * (float)Main.dayRate * 0.0017f;

            // Prepare the cloud shader.
            SkyPlayerSnapshot player = SkyPlayerSnapshot.TakeSnapshot();
            float dayCycleCompletion = (float)(Main.time / (Main.dayTime ? Main.dayLength : Main.nightLength));
            float sunZPosition = -10f - MathF.Pow(MathF.Sin(MathHelper.Pi * dayCycleCompletion), 0.51f) * 495f;
            float cloudExposure = Utils.Remap(RealisticSkyConfig.Instance.CloudExposure, RealisticSkyConfig.MinCloudExposure, RealisticSkyConfig.MaxCloudExposure, 0.5f, 1.5f) * 1.3f;
            Effect shader = GameShaders.Misc[CloudShaderKey].Shader;
            shader.Parameters["screenSize"]?.SetValue(screenSize);
            shader.Parameters["invertedGravity"]?.SetValue(player.InvertedGravity);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(Main.dayTime ? SunPositionSaver.SunPosition : SunPositionSaver.MoonPosition, sunZPosition));
            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["worldPosition"]?.SetValue(Main.screenPosition);
            shader.Parameters["cloudFadeHeightTop"]?.SetValue(3300f);
            shader.Parameters["cloudFadeHeightBottom"]?.SetValue(4400f);
            shader.Parameters["cloudSurfaceFadeHeightTop"]?.SetValue((float)player.WorldSurface * 16f - player.MaxTilesY * 0.25f);
            shader.Parameters["cloudSurfaceFadeHeightBottom"]?.SetValue((float)player.WorldSurface * 16f);
            shader.Parameters["parallax"]?.SetValue(new Vector2(0.3f, 0.175f) * Main.caveParallax);
            shader.Parameters["cloudDensity"]?.SetValue(MathHelper.Clamp(cloudOpacity * 1.2f, 0f, 1f));
            shader.Parameters["horizontalOffset"]?.SetValue(CloudHorizontalOffset);
            shader.Parameters["cloudExposure"]?.SetValue(cloudExposure);
            shader.Parameters["pixelationFactor"]?.SetValue(4f);
            shader.CurrentTechnique.Passes[0].Apply();

            // Supply the atmosphere data to the clouds shader.
            Main.instance.GraphicsDevice.Textures[1] = !AtmosphereRenderer.AtmosphereTarget.IsReady ? TextureAssets.MagicPixel.Value : AtmosphereRenderer.AtmosphereTarget.GetTarget();
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            // Calculate the sunset glow interpolant.
            // This will give colors an orange tint.
            float sunsetGlowInterpolant = MathF.Sqrt(1f - RealisticSkyManager.SunlightIntensityByTime);
            sunsetGlowInterpolant *= Utils.GetLerpValue(1f, 0.7f, sunsetGlowInterpolant, true);
            if (!Main.dayTime)
                sunsetGlowInterpolant = 0f;

            // Draw the clouds.
            Texture2D cloud = TexturesRegistry.CloudDensityMap.Value;
            Vector2 drawPosition = screenSize * 0.5f;
            Vector2 skyScale = screenSize / cloud.Size();
            Color cloudsColor = Color.Lerp(Main.ColorOfTheSkies, Color.White, 0.05f) * MathF.Pow(cloudOpacity, 0.67f) * 2.67f;
            cloudsColor = Color.Lerp(cloudsColor, Color.OrangeRed, sunsetGlowInterpolant);
            cloudsColor.A = (byte)(cloudOpacity * 255f);
            Main.spriteBatch.Draw(cloud, drawPosition, null, cloudsColor, 0f, cloud.Size() * 0.5f, skyScale, 0, 0f);
        }
    }
}
