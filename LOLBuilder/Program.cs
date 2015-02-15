using LeagueSharp.Common;

namespace LolBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += EventProcessing.GameLoad;
        }
    }
}
