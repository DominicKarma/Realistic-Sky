using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;

namespace RealisticSky
{
    public class RealisticSkyManager : CustomSky
    {
        private bool skyActive;

        internal static new float Opacity;

        /// <summary>
        /// The identifier key for this sky.
        /// </summary>
        public const string SkyKey = "RealisticSky:TerraBlade";

        public override void Deactivate(params object[] args)
        {
            skyActive = false;
        }

        public override void Reset()
        {
            skyActive = false;
        }

        public override bool IsActive()
        {
            return skyActive || Opacity > 0f;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            skyActive = true;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth < float.MaxValue || minDepth >= float.MaxValue)
                return;


        }
        public override void Update(GameTime gameTime)
        {
            if (Main.gameMenu)
                skyActive = false;

            Opacity = MathHelper.Clamp(Opacity + skyActive.ToDirectionInt() * 0.1f, 0f, 1f);
        }

        public override float GetCloudAlpha() => 1f - Opacity;
    }
}
