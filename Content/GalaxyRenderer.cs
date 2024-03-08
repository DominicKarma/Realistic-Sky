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
            // Calculate interpolants that go into the overall desired galaxy opacity.
            // These interpolants and their reasonings are the following:
            // 1. The amount of stars in the scene. Basically all panoramic photography of the Milky Way that I could find was littered with tiny stars. It would be really strange for all those stars to appear at full intensity
            // when there are few stars in the sky otherwise.
            // 2. The darkness of the sky. In the real world, the Milky Way is only visible on the darkest, most light-pollution-free nights. This is modeled here by requiring a super dark sky brightness to appear.
            // 3. The Y position of the camera. This results in the galaxy's opacity being weak near the edge of the planet's atmosphere, since that counts as a form of light.
            float starInterpolant = Utils.GetLerpValue(1000f, 5000f, RealisticSkyConfig.Instance.NightSkyStarCount, true);
            float skyDarknessInterpolant = Utils.GetLerpValue(0.1f, 0.05f, RealisticSkyManager.SkyBrightness, true);
            float spaceHeightInterpolant = RealisticSkyManager.SpaceHeightInterpolant;
            float proximityToAtmosphereEdgeInterpolant = spaceHeightInterpolant * Utils.GetLerpValue(0.95f, 0.81f, spaceHeightInterpolant, true);

            // Combine the aforementioned interpolants together into a single desired opacity value.
            float idealGlaxayOpacityInterpolant = starInterpolant * skyDarknessInterpolant;
            float idealGalaxyOpacity = MathHelper.SmoothStep(0f, 0.7f, idealGlaxayOpacityInterpolant);

            // Make the galaxy harder to see in space, since in the space the atmosphere noticeably creates light.
            idealGalaxyOpacity *= MathHelper.SmoothStep(1f, 0.25f, proximityToAtmosphereEdgeInterpolant);

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
            Vector2 galaxyDrawPosition = screenSize * new Vector2(0.6f, 0.6f);
            Main.spriteBatch.Draw(galaxy, galaxyDrawPosition, null, galaxyColor, RealisticSkyManager.StarViewRotation * 0.84f + 0.23f, galaxy.Size() * 0.5f, galaxyScale, 0, 0f);
        }
    }
}
