using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace RealisticSky.Common.DataStructures
{
    /// <summary>
    ///     A snapshot of a player with relevant data for rendering the
    ///     realistic sky. Context-aware, providing dummy data when no player
    ///     is available (e.g. in the main menu).
    /// </summary>
    public readonly struct SkyPlayerSnapshot
    {
        public Vector2 Center { get; }

        public bool InvertedGravity { get; }

        public double WorldSurface { get; }

        public int MaxTilesY { get; }

        public bool InEternalGardenSubworld { get; }

        public SkyPlayerSnapshot(Player player)
        {
            Center = player.Center;
            InvertedGravity = player.gravDir <= -1f;
            WorldSurface = Main.worldSurface;
            MaxTilesY = Main.maxTilesY;

            if (ModContent.TryFind("NoxusBoss", "EternalGardenBiome", out ModBiome garden))
                InEternalGardenSubworld = player.InModBiome(garden);
        }

        public SkyPlayerSnapshot()
        {
            // Magic numbers from testing in-game. Feel free to adjust.
            Center = new Vector2(33500f, 1500f);
            InvertedGravity = false;
            WorldSurface = 337;
            MaxTilesY = 1200;
            InEternalGardenSubworld = false;
        }

        public static SkyPlayerSnapshot TakeSnapshot()
        {
            return Main.gameMenu ? new SkyPlayerSnapshot() : new SkyPlayerSnapshot(Main.LocalPlayer);
        }
    }
}
