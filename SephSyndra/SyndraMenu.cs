using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace SephSyndra
{
    class SyndraMenu
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;

        public static void Init()
        {
            Config = new Menu("SephSyndra", "SephSyndra", true);
            var ow = Config.AddSubMenu("Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(ow);

           
            SPrediction.Prediction.Initialize(Config);

            var combo = Config.AddSubMenu("Combo");
            combo.AddBool("c.q", "Use Q");
            combo.AddBool("c.w", "Use W");
            combo.AddBool("c.e", "Use E");
            combo.AddBool("c.r", "Use R");
            combo.AddBool("c.useq4e", "Use Q for E (Q won't hit)");
            combo.AddSlider("c.rminh", "Min Target Health % for Ult", 10, 1, 100);

            var blacklist = Config.AddSubMenu("Ult BlackList");
            foreach (var h in HeroManager.Enemies)
            {
                blacklist.AddBool("noult." + h.ChampionName, h.ChampionName, false);
            }

            var harass = Config.AddSubMenu("Harass");
            harass.AddKeyBind("h.enabled", "Harass", 'H', KeyBindType.Toggle).Permashow();
            harass.AddBool("h.inmixed", "In Mixed");
            harass.AddSlider("h.mana", "Mana Percent %", 40, 0, 100);
            harass.AddBool("h.q", "Use Q");
            harass.AddBool("h.w", "Use W");
            harass.AddBool("h.e", "Auto Stun");

            var lh = Config.AddSubMenu("LastHitting");
            lh.AddSlider("lh.mana", "Mana Percent %", 40, 0, 100);
            lh.AddBool("lh.q", "Use Q");
            lh.AddBool("lh.w", "Use W");
            lh.AddBool("lh.e", "Use E");

            var lc = Config.AddSubMenu("Waveclear");
            var jung = lc.AddSubMenu("Jungle");
            lc.AddSlider("lc.mana", "Mana Percent %", 40, 0, 100);
            jung.AddBool("jc.q", "Use Q");
            jung.AddBool("jc.w", "Use W");
            jung.AddBool("jc.e", "Use E");
            lc.AddBool("lc.q", "Use Q");
            lc.AddBool("lc.w", "Use W");
            lc.AddBool("lc.e", "Use E", false);

            var ks = Config.AddSubMenu("Killsteal");
            ks.AddBool("ks.enabled", "Killsteal");
            ks.AddBool("ks.q", "Use Q");
            ks.AddBool("ks.w", "Use W");
            ks.AddBool("ks.e", "Use E");
            ks.AddBool("ks.r", "Use R");
            ks.AddBool("ks.ignite", "Use Ignite");

            var drawings = Config.AddSubMenu("Drawings");
            drawings.AddBool("d.enabled", "Enabled", false);
            drawings.AddCircle("d.q", "Draw Q", SpellManager.Q.Range, System.Drawing.Color.Blue);
            drawings.AddCircle("d.w", "Draw W", SpellManager.Q.Range, System.Drawing.Color.Crimson);
            drawings.AddCircle("d.e", "Draw E", SpellManager.Q.Range, System.Drawing.Color.Aqua);
            drawings.AddCircle("d.r", "Draw R", SpellManager.Q.Range, System.Drawing.Color.Red);

            var misc = Config.AddSubMenu("Misc");
            misc.AddKeyBind("qekey", "Q -> E Key", 'T', KeyBindType.Press);
            misc.AddSList("qemode", "QE Mode", new string[] { "Normal", "Fast" }, 0);
            misc.AddBool("m.interrupter", "Interrupter");
            misc.AddBool("m.ag", "Anti Gapcloser");
            misc.AddBool("m.utilw", "Use W2 if noone in range");
            misc.AddBool("m.dbg", "Debug", false);
            Config.AddToMainMenu();
        }
    }
}
