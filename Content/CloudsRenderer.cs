using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            // Disable normal clouds.
            for (int i = 0; i < Main.maxClouds; i++)
                Main.cloud[i].active = false;
            float cloudOpacity = MathF.Min((Main.numCloudsTemp + 60) / (float)Main.maxClouds, Main.cloudAlpha);
            if (cloudOpacity > 1f)
                cloudOpacity = 1f;
            if (cloudOpacity <= 0f)
                return;

            Main.cloudBGAlpha = 0f;

            // Calculate the true screen size.
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);

            // Move clouds.
            if (!Main.gamePaused)
                CloudHorizontalOffset -= Main.windSpeedCurrent * (float)Main.dayRate * 0.0017f;

            // Prepare the sky shader.
            float dayCycleCompletion = (float)(Main.time / (Main.dayTime ? Main.dayLength : Main.nightLength));
            float sunZPosition = -4f - MathF.Pow(MathF.Sin(MathHelper.Pi * dayCycleCompletion), 0.51f) * 95f;
            Effect shader = GameShaders.Misc[CloudShaderKey].Shader;
            shader.Parameters["screenSize"]?.SetValue(screenSize);
            shader.Parameters["invertedGravity"]?.SetValue(Main.LocalPlayer.gravDir == -1f);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(Main.dayTime ? SunPositionSaver.SunPosition : SunPositionSaver.MoonPosition, sunZPosition));
            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["worldPosition"]?.SetValue(Main.screenPosition);
            shader.Parameters["parallax"]?.SetValue(new Vector2(0.3f, 0.175f) * Main.caveParallax);
            shader.Parameters["cloudDensity"]?.SetValue(MathHelper.Clamp(cloudOpacity * 1.2f, 0f, 1f));
            shader.Parameters["horizontalOffset"]?.SetValue(CloudHorizontalOffset);
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw the atmosphere.
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
