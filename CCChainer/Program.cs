using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;


//INCOMPLETE PROJECT 
namespace CCChainer
{
    internal class Program
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            Game.PrintChat("CC Chainer Loaded");
            Game.OnGameUpdate += OnGameUpdate;
        }

        private static void OnGameUpdate(EventArgs args)
        {

        }

        private static Obj_AI_Hero CheckandReturn()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().ToList())
            {
                if (hero.Distance(Player) <= 300)
                {
                    foreach (var buff in hero.Buffs)
                    {
                        if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt || buff.Type == BuffType.Charm ||
                            buff.Type == BuffType.Fear)
                        {
                            return hero;
                        }
                    }
                }
            }
            return null;
        }
    }
}


