using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace RealisticSky.Assets
{
    /// <summary>
    ///     A centralized registry of all common textures within the mod.
    /// </summary>
    public static class TexturesRegistry
    {
        public const string ExtraTexturesPath = $"{nameof(RealisticSky)}/Assets/ExtraTextures";

        public static readonly Asset<Texture2D> BloomCircle = ModContent.Request<Texture2D>($"{ExtraTexturesPath}/BloomCircle");

        public static readonly Asset<Texture2D> BloomCircleBig = ModContent.Request<Texture2D>($"{ExtraTexturesPath}/BloomCircleBig");

        public static readonly Asset<Texture2D> CloudDensityMap = ModContent.Request<Texture2D>($"{ExtraTexturesPath}/CloudDensityMap");

        public static readonly Asset<Texture2D> EclipseMoon = ModContent.Request<Texture2D>($"{ExtraTexturesPath}/EclipseMoon");

        public static readonly Asset<Texture2D> Galaxy = ModContent.Request<Texture2D>($"{ExtraTexturesPath}/Galaxy");

        public static readonly Asset<Texture2D> LensFlare = ModContent.Request<Texture2D>($"{ExtraTexturesPath}/LensFlare");
    }
}
