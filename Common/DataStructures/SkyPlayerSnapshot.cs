using Microsoft.Xna.Framework;
using Terraria;

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

        public SkyPlayerSnapshot(Player player)
        {
            Center = player.Center;
            InvertedGravity = player.gravDir <= -1f;
        }

        public SkyPlayerSnapshot()
        {
            Center = new Vector2(0f, 0f);
            InvertedGravity = false;
        }

        public static SkyPlayerSnapshot TakeSnapshot()
        {
            return Main.gameMenu ? new SkyPlayerSnapshot() : new SkyPlayerSnapshot(Main.LocalPlayer);
        }
    }
}
