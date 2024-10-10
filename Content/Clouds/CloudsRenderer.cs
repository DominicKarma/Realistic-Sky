using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Assets;
using RealisticSky.Common.DataStructures;
using RealisticSky.Common.Utilities;
using RealisticSky.Content.Sun;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RealisticSky.Content.Clouds
{
    public class CloudsRenderer : ModSystem
    {
        /// <summary>
        /// The render target that holds the contents of the clouds.
        /// </summary>
        internal static CloudsTargetContent CloudTarget;

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
            GameShaders.Misc[CloudShaderKey] = new MiscShaderData(ModContent.Request<Effect>("RealisticSky/Assets/Effects/CloudShader"), "AutoloadPass");

            CloudTarget = new();
            Main.ContentThatNeedsRenderTargets.Add(CloudTarget);
        }

        public static void RenderToTarget()
        {
            if (!GameShaders.Misc.TryGetValue(CloudShaderKey, out MiscShaderData s) || RealisticSkyConfig.Instance is null)
                return;
            Effect shader = s.Shader;
            if (shader?.IsDisposed ?? true)
                return;

            SkyPlayerSnapshot player = SkyPlayerSnapshot.TakeSnapshot();
            GraphicsDevice gd = Main.instance.GraphicsDevice;
            Vector2 screenSize = new(gd.Viewport.Width, gd.Viewport.Height);

            Matrix backgroundMatrix = Main.BackgroundViewMatrix.TransformationMatrix;
            Vector3 translationDirection = new(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically) ? -1f : 1f, 1f);
            backgroundMatrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * translationDirection;

            Vector2 sunPosition = Main.dayTime ? SunPositionSaver.SunPosition : SunPositionSaver.MoonPosition;
            sunPosition *= 0.5f;
            sunPosition = Vector2.Transform(sunPosition, Matrix.Invert(backgroundMatrix));

            float windDensityInterpolant = MathUtils.Saturate(Main.cloudAlpha + MathF.Abs(Main.windSpeedCurrent) * 0.84f);

            shader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["invertedGravity"]?.SetValue(player.InvertedGravity);
            shader.Parameters["screenSize"]?.SetValue(screenSize);
            shader.Parameters["worldPosition"]?.SetValue(Main.screenPosition);
            shader.Parameters["sunPosition"]?.SetValue(new Vector3(sunPosition, 5f));
            shader.Parameters["sunColor"]?.SetValue(Main.ColorOfTheSkies.ToVector4());
            shader.Parameters["cloudColor"]?.SetValue(Color.Lerp(Color.Wheat, Color.LightGray, 0.85f).ToVector4());
            shader.Parameters["densityFactor"]?.SetValue(MathHelper.Lerp(10f, 0.3f, MathF.Pow(windDensityInterpolant, 0.48f)));
            shader.Parameters["cloudHorizontalOffset"]?.SetValue(CloudHorizontalOffset);
            shader.CurrentTechnique.Passes[0].Apply();

            Texture2D cloud = TexturesRegistry.CloudDensityMap.Value;
            Main.spriteBatch.Draw(cloud, new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.White);
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

            CloudHorizontalOffset -= Main.windSpeedCurrent * 0.3f;

            CloudTarget.Request();
            if (!CloudTarget.IsReady)
                return;

            GraphicsDevice gd = Main.instance.GraphicsDevice;
            Vector2 screenSize = new(gd.Viewport.Width, gd.Viewport.Height);
            Main.spriteBatch.Draw(CloudTarget.GetTarget(), new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.White);
        }
    }
}
