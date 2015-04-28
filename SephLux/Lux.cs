using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephLux
{
    class Lux
    {
        public static void OnLoad()
        {
            CustomEvents.Game.OnGameLoad += LuxMain;
        }

        static void LuxMain(EventArgs args)
        {
            Game.PrintChat("SephLux Loaded!");
        }


    }
}
