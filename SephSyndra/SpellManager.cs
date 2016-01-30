using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;

namespace SephSyndra
{
    class SpellManager
    {
        public static Spell Q, W, W2, E, R, QE, Ignite;



        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 750);
            W = new Spell(SpellSlot.W, 925);
            W2 = new Spell(SpellSlot.W, 925);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 675);
            QE = new Spell(SpellSlot.Q, 750 + 500);

            Q.SetSkillshot(0.6f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W2.SetSkillshot(0.25f, 140f, 1600f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);


            Ignite = new Spell(ObjectManager.Player.GetSpellSlot("SummonerDot"), 550f);
        }

    }
}
