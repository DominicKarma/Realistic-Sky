using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Common.DataStructures;

namespace RealisticSky.Assets
{
    /// <summary>
    ///     A centralized registry of all common textures within the mod.
    /// </summary>
    public static class TexturesRegistry
    {
        public const string ExtraTexturesPath = $"{nameof(RealisticSky)}/Assets/ExtraTextures";

        public static readonly LazyAsset<Texture2D> BloomCircle = LazyAsset<Texture2D>.RequestAsync($"{ExtraTexturesPath}/BloomCircle");

        public static readonly LazyAsset<Texture2D> BloomCircleBig = LazyAsset<Texture2D>.RequestAsync($"{ExtraTexturesPath}/BloomCircleBig");

        public static readonly LazyAsset<Texture2D> CloudDensityMap = LazyAsset<Texture2D>.RequestAsync($"{ExtraTexturesPath}/CloudDensityMap");

        public static readonly LazyAsset<Texture2D> EclipseMoon = LazyAsset<Texture2D>.RequestAsync($"{ExtraTexturesPath}/EclipseMoon");

        public static readonly LazyAsset<Texture2D> Galaxy = LazyAsset<Texture2D>.RequestAsync($"{ExtraTexturesPath}/Galaxy");

        public static readonly LazyAsset<Texture2D> LensFlare = LazyAsset<Texture2D>.RequestAsync($"{ExtraTexturesPath}/LensFlare");
    }
}
