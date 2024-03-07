using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Common.DataStructures;
using ReLogic.Content;
using Terraria;
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
        /// The asset that holds the cloud texture.
        /// </summary>
        internal static Asset<Texture2D> CloudTextureAsset;

        /// <summary>
        /// The identifier key for the sky's cloud shader.
        /// </summary>
        public const string CloudShaderKey = "RealisticSky:CloudShader";

        public override void OnModLoad()
        {
            // Load the cloud texture.
            CloudTextureAsset = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/CloudDensity");

            // Store the atmosphere shader.
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
            float cloudOpacity = MathF.Min((Main.numCloudsTemp + 60) / (float)Main.maxClouds, Main.cloudAlpha);
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
            float sunZPosition = -4f - MathF.Pow(MathF.Sin(MathHelper.Pi * dayCycleCompletion), 0.51f) * 95f;
            Effect shader = GameShaders.Misc[CloudShaderKey].Shader;
            shader.Parameters["screenSize"]?.SetValue(screenSize);
            shader.Parameters["invertedGravity"]?.SetValue(player.InvertedGravity);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(Main.dayTime ? SunPositionSaver.SunPosition : SunPositionSaver.MoonPosition, sunZPosition));
            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["worldPosition"]?.SetValue(Main.screenPosition);
            shader.Parameters["cloudFadeHeightTop"]?.SetValue(3200f);
            shader.Parameters["cloudFadeHeightBottom"]?.SetValue(4000f);
            shader.Parameters["parallax"]?.SetValue(new Vector2(0.3f, 0.175f) * Main.caveParallax);
            shader.Parameters["cloudDensity"]?.SetValue(MathHelper.Clamp(cloudOpacity * 1.2f, 0f, 1f));
            shader.Parameters["horizontalOffset"]?.SetValue(CloudHorizontalOffset);
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw the clouds.
            Texture2D cloud = CloudTextureAsset.Value;
            Vector2 drawPosition = screenSize * 0.5f;
            Vector2 skyScale = screenSize / cloud.Size();
            Color cloudsColor = Color.Lerp(Main.ColorOfTheSkies, Color.White, 0.05f) * MathF.Pow(cloudOpacity, 0.67f) * 2.67f;
            cloudsColor.A = (byte)(cloudOpacity * 255f);
            cloudsColor = Color.White;
            Main.spriteBatch.Draw(cloud, drawPosition, null, cloudsColor, 0f, cloud.Size() * 0.5f, skyScale, 0, 0f);
        }
    }
}
