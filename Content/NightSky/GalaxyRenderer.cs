using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Assets;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Content.NightSky
{
    public class GalaxyRenderer : ModSystem
    {
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

        internal static void UpdateOpacity()
        {
            // Calculate interpolants that go into the overall desired galaxy opacity.
            // These interpolants and their reasonings are the following:
            // 1. The amount of stars in the scene. Basically all panoramic photography of the Milky Way that I could find was littered with tiny stars. It would be really strange for all those stars to appear at full intensity
            // when there are few stars in the sky otherwise.
            // 2. The darkness of the sky. In the real world, the Milky Way is only visible on the darkest, most light-pollution-free nights. This is modeled here by requiring a super dark sky brightness to appear.
            // 3. The Y position of the camera. This results in the galaxy's opacity being weak near the edge of the planet's atmosphere, since that counts as a form of light.
            float starInterpolant = Utils.GetLerpValue(1000f, 5000f, RealisticSkyConfig.Instance.NightSkyStarCount, true);
            float skyDarknessInterpolant = Utils.GetLerpValue(0.093f, 0.03f, RealisticSkyManager.SkyBrightness, true);
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
            if (TexturesRegistry.Galaxy.Asset.IsDisposed)
                return;

            // Update the galaxy's opacity.
            UpdateOpacity();

            // Calculate draw variables.
            Texture2D galaxy = TexturesRegistry.Galaxy.Value;
            Vector2 screenSize = new(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
            float galaxyScale = screenSize.X / galaxy.Width * 0.95f;
            Color galaxyColor = new Color(1.2f, 0.9f, 1f) * MovingGalaxyOpacity;
            Vector2 galaxyDrawPosition = screenSize * new Vector2(0.6f, 0.6f);

            // Draw a glow behind the galaxy.
            Texture2D bloom = TexturesRegistry.BloomCircleBig.Value;
            Main.spriteBatch.Draw(bloom, galaxyDrawPosition, null, galaxyColor * MathF.Sqrt(MovingGalaxyOpacity) * 0.85f, 0f, bloom.Size() * 0.5f, galaxyScale * 0.29f, 0, 0f);
            Main.spriteBatch.Draw(bloom, galaxyDrawPosition, null, galaxyColor * MovingGalaxyOpacity * 0.5f, 0f, bloom.Size() * 0.5f, galaxyScale * 3f, 0, 0f);

            // Draw the galaxy.
            Main.spriteBatch.Draw(galaxy, galaxyDrawPosition, null, galaxyColor * 0.64f, RealisticSkyManager.StarViewRotation * 0.84f + 0.23f, galaxy.Size() * 0.5f, galaxyScale, 0, 0f);
        }
    }
}
