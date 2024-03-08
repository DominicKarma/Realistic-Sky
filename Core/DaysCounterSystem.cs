using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RealisticSky
{
    public class DaysCounterSystem : ModSystem
    {
        /// <summary>
        /// How many days, including fractional values, have passed so far for the given world.
        /// </summary>
        public static float DayCounter
        {
            get;
            set;
        }

        public override void PostUpdateWorld()
        {
            DayCounter += (float)(Main.dayRate / (Main.dayLength + Main.nightLength));
        }

        public override void OnWorldLoad() => DayCounter = 0f;

        public override void OnWorldUnload() => DayCounter = 0f;

        public override void SaveWorldData(TagCompound tag) => tag[nameof(DayCounter)] = DayCounter;

        public override void LoadWorldData(TagCompound tag) => DayCounter = tag.GetFloat(nameof(DayCounter));
    }
}
