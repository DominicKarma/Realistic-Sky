using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content
{
    public class GalaxyRenderer : ModSystem
    {
        internal static Asset<Texture2D> GalaxyAsset;

        /// <summary>
        ///     The moving opacity of the galaxy.
        /// </summary>
        /// <remarks>
        ///     This exists so that the galaxy's opacity can smoothly move around, rather than having the entire thing make choppy visual changes due to sharp <see cref="Utils.GetLerpValue(float, float, float, bool)"/> calls.
        /// </remarks>
        public static float MovingGalaxyOpacity
        {
            get;
            set;
        }

        /// <summary>
        ///     The intensity at which the galaxy changes its opacity every frame.
        /// </summary>
        public const float GalaxyOpacityMoveSpeedInterpolant = 0.14f;

        public override void OnModLoad()
        {
            GalaxyAsset = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/Galaxy");
        }

        internal static void UpdateOpacity()
        {
            float idealGlaxayOpacityInterpolant = Utils.GetLerpValue(2500f, 8000f, RealisticSkyConfig.Instance.NightSkyStarCount, true) * Utils.GetLerpValue(0.1f, 0.05f, RealisticSkyManager.SkyBrightness, true);
            float idealGalaxyOpacity = MathHelper.SmoothStep(0f, 0.7f, idealGlaxayOpacityInterpolant);

            // Make the galaxy harder to see in space, since in the space the atmosphere noticeably creates light.
            idealGalaxyOpacity *= MathHelper.SmoothStep(1f, 0.25f, RealisticSkyManager.SpaceHeightInterpolant * Utils.GetLerpValue(0.95f, 0.81f, RealisticSkyManager.SpaceHeightInterpolant, true));

            // Update the moving opacity.
            MovingGalaxyOpacity = MathHelper.Lerp(MovingGalaxyOpacity, idealGalaxyOpacity, GalaxyOpacityMoveSpeedInterpolant);
        }

        public static void Render()
        {
            if (GalaxyAsset.IsDisposed)
                return;

            // Update the galaxy's opacity.
            UpdateOpacity();

            // Draw the galaxy.
            Texture2D galaxy = GalaxyAsset.Value;
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            float galaxyScale = screenSize.X / galaxy.Width * 0.95f;
            Color galaxyColor = new Color(1f, 1f, 1f) * MovingGalaxyOpacity;
            Vector2 galaxyDrawPosition = screenSize * 0.5f;
            Main.spriteBatch.Draw(galaxy, galaxyDrawPosition, null, galaxyColor, RealisticSkyManager.StarViewRotation + 0.23f, galaxy.Size() * 0.5f, galaxyScale, 0, 0f);
        }
    }
}
