using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Assets;
using RealisticSky.Common.DataStructures;
using RealisticSky.Common.Utilities;
using RealisticSky.Content.Atmosphere;
using RealisticSky.Content.Sun;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RealisticSky.Content.Clouds
{
    public class CloudsRenderer : ModSystem
    {
        /// <summary>
        ///     The horizontal offset of clouds.
        /// </summary>
        public static float CloudHorizontalOffset
        {
            get;
            set;
        }

        /// <summary>
        ///     The moving opacity of the clouds.
        /// </summary>
        /// <remarks>
        ///     This exists so that the clouds's opacity can smoothly move around, rather than having the entire thing make choppy visual changes due to the potential for cloud configurations to suddenly and randomly change.
        /// </remarks>
        public static float MovingCloudOpacity
        {
            get;
            set;
        }

        /// <summary>
        ///     The default movement speed that clouds should adhere to, ignoring <see cref="Main.dayRate"/> and <see cref="Main.windSpeedCurrent"/>.
        /// </summary>
        public const float StandardCloudMovementSpeed = 0.0017f;

        /// <summary>
        ///     The closest possible Z position of the sun from the perspective of the clouds.
        /// </summary>
        public const float NearSunZPosition = -10f;

        /// <summary>
        ///     The furthest possible Z position of the sun from the perspective of the clouds.
        /// </summary>
        public const float FarSunZPosition = -500f;

        /// <summary>
        ///     The identifier key for the sky's cloud shader.
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
            float idealCloudOpacity = MathF.Max((Main.numCloudsTemp - 20) / (float)Main.maxClouds, Main.cloudAlpha);
            if (idealCloudOpacity > 1f)
                idealCloudOpacity = 1f;

            // Update the cloud opacity.
            MovingCloudOpacity = MathHelper.Lerp(MovingCloudOpacity, idealCloudOpacity, 0.08f);

            // If the cloud opacity is 0.002 or less, that means that there's nothing to draw and this method should terminate immediately for performance reasons.
            if (MovingCloudOpacity <= 0.002f)
                return;

            // Calculate the true screen size.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            // Move clouds.
            if (!Main.gamePaused)
                CloudHorizontalOffset -= Main.windSpeedCurrent * (float)Main.dayRate * StandardCloudMovementSpeed;

            // Prepare the cloud shader.
            SkyPlayerSnapshot player = SkyPlayerSnapshot.TakeSnapshot();
            float dayCycleCompletion = (float)(Main.time / (Main.dayTime ? Main.dayLength : Main.nightLength));
            float noonMidnightInterpolant = MathF.Sin(MathHelper.Pi * dayCycleCompletion);

            // Reports have arisen of sunZPosition in occasional circumstances having NaN values. I am strongly suspicious that the above sine
            // calculation is in some circumstances outputting tiny negative values, causing the power function to fail and give back NaN.
            // In order to get around this, a sanity check will be performed to ensure that the interpolant will never be below zero.
            noonMidnightInterpolant = MathUtils.Saturate(noonMidnightInterpolant);

            float sunDistanceInterpolant = MathF.Pow(noonMidnightInterpolant, 0.51f);
            float sunZPosition = MathHelper.Lerp(NearSunZPosition, FarSunZPosition, sunDistanceInterpolant);
            float cloudExposure = Utils.Remap(RealisticSkyConfig.Instance.CloudExposure, RealisticSkyConfig.MinCloudExposure, RealisticSkyConfig.MaxCloudExposure, 0.5f, 1.5f) * 1.3f;
            Effect shader = GameShaders.Misc[CloudShaderKey].Shader;
            if (shader.IsDisposed)
                return;

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
            shader.Parameters["cloudDensity"]?.SetValue(MathUtils.Saturate(MovingCloudOpacity * 1.2f));
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
            Color cloudsColor = Color.Lerp(Main.ColorOfTheSkies, Color.White, 0.05f) * MathF.Pow(idealCloudOpacity, 0.67f) * 2.67f;
            cloudsColor = Color.Lerp(cloudsColor, Color.OrangeRed, sunsetGlowInterpolant);
            cloudsColor.A = (byte)(idealCloudOpacity * 255f);
            Main.spriteBatch.Draw(cloud, drawPosition, null, cloudsColor, 0f, cloud.Size() * 0.5f, skyScale, 0, 0f);
        }
    }
}
