using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PingSpammer
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static double lasttick;

        static void OnLoad(EventArgs args)
        {
            Game.OnUpdate += OnUpdate;
        }

        static void OnUpdate(EventArgs args)
        {
                foreach (var hero in HeroManager.AllHeroes)
                {
                    if (!(Game.Time - lasttick >= 1000)) return;
                    lasttick = Game.Time;
                    Vector3 kappa = hero.Position;
                    Game.SendPing(PingCategory.Normal, kappa);
                }
        }
    }
}