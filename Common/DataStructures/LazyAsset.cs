using System;
using ReLogic.Content;
using Terraria.ModLoader;

namespace RealisticSky.Common.DataStructures
{
    /// <summary>
    ///     <see cref="Asset{T}"/> wrapper that facilitates lazy-loading.
    /// </summary>
    /// <typeparam name="T">The asset type.</typeparam>
    public readonly struct LazyAsset<T> where T : class
    {
        private readonly Lazy<Asset<T>> asset;

        public Asset<T> Asset => asset.Value;

        public T Value => asset.Value.Value;

        public LazyAsset(Func<Asset<T>> func)
        {
            asset = new Lazy<Asset<T>>(func);
        }

        public LazyAsset<T> ImmediatelyGet()
        {
            Asset.Wait();
            return this;
        }

        public static LazyAsset<T> RequestAsync(string path)
        {
            return new LazyAsset<T>(() => ModContent.Request<T>(path));
        }

        public static LazyAsset<T> RequestImmediate(string path)
        {
            return new LazyAsset<T>(() => ModContent.Request<T>(path, AssetRequestMode.ImmediateLoad));
        }
    }
}
