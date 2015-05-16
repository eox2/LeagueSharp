using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace AutoZhonya
{
    class DangerousBuffs
    {
        public static Dictionary<String, float> ScaryBuffs = new Dictionary<String, float>(); 

        static DangerousBuffs()
        {
            ScaryBuffs.Add("KarthusFallenOne", 2200);
            ScaryBuffs.Add("monkeykingspinknockup", 0);
            ScaryBuffs.Add("zedultexecute", 0);
            ScaryBuffs.Add("fizzmarinerdoombomb", 0);
            ScaryBuffs.Add("missfortunebulletsound", 0);
        }
    }

}
