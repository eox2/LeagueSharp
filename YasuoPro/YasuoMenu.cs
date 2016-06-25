using System.Reflection;
using System.Linq;
using LeagueSharp.Common;

namespace YasuoPro
{
    internal static class YasuoMenu
    {
        internal static Menu Config;
        internal static Orbwalking.Orbwalker Orbwalker;
        internal static Yasuo Yas;

        public static void Init(Yasuo yas)
        {
            Yas = yas;

            Config = new Menu("YasuoPro", "YasuoPro", true);

            Menu OWMenu = Config.AddSubMenu("Orbwalking");
            Orbwalker = new Orbwalking.Orbwalker(OWMenu);


            Menu Combo = Config.AddSubMenu("Combo");
            YasuoMenu.Combo.Attach(Combo);

            Menu Harass = Config.AddSubMenu("Harass");
            YasuoMenu.Harass.Attach(Harass);

            Menu Evade = Config.AddSubMenu("Evade");
            YasuoMenu.Evade.Attach(Evade);


            Menu Killsteal = Config.AddSubMenu("Killsteal");
            YasuoMenu.Killsteal.Attach(Killsteal);

            Menu Farming = Config.AddSubMenu("LastHitting");
            YasuoMenu.Farm.Attach(Farming);

            Menu Waveclear = Config.AddSubMenu("Waveclear");
            YasuoMenu.Waveclear.Attach(Waveclear);

            Menu Misc = Config.AddSubMenu("Misc");
            YasuoMenu.Misc.Attach(Misc);

            Menu Drawings = Config.AddSubMenu("Drawings");
            YasuoMenu.Drawings.Attach(Drawings);

            Config.AddToMainMenu();
        }

        internal static Menu GetEvadeMenu()
        {
            return Config.SubMenu(Assembly.GetExecutingAssembly().GetName() + ".Evade");
        }

        struct Combo
        {
            internal static void Attach(Menu menu)
            {
                var items = menu.AddSubMenu("Items");
                items.AddBool("Items.Enabled", "Use Items");
                items.AddBool("Items.UseTitanic", "Use Titanic");
                items.AddBool("Items.UseTIA", "Use Tiamat");
                items.AddBool("Items.UseHDR", "Use Hydra");
                items.AddBool("Items.UseBRK", "Use BORK");
                items.AddBool("Items.UseBLG", "Use Bilgewater");
                items.AddBool("Items.UseYMU", "Use Youmu");

                menu.AddBool("Combo.UseQ", "Use Q");
                menu.AddBool("Combo.UseQ2", "Use Q2");
                menu.AddBool("Combo.StackQ", "Stack Q if not in Range");
                menu.AddBool("Combo.UseW", "Use W");
                menu.AddBool("Combo.UseE", "Use E");
                menu.AddBool("Combo.UseEQ", "Use EQ");
                menu.AddBool("Combo.ETower", "Use E under Tower", false);
                menu.AddBool("Combo.EAdvanced", "Predict E position with Waypoints");
                menu.AddBool("Combo.EToSafety", "E towards base if unhealthy", false);

                var ultmenu = menu.AddSubMenu("Ult Settings");
                var blacklist = new Menu("Ult Targets", "BlackList");

                foreach (var hero in HeroManager.Enemies)
                {
                    blacklist.AddBool("ult" + hero.ChampionName, "Ult " + hero.ChampionName);
                }

                ultmenu.AddSubMenu(blacklist);

                ultmenu.AddSList("Combo.UltMode", "Ult Prioritization", new string[] { "Lowest Health", "TS Priority", "Most enemies" }, 0);
                ultmenu.AddSlider("Combo.knockupremainingpct", "Knockup Remaining % for Ult", 95, 40, 100);
                ultmenu.AddBool("Combo.UseR", "Use R");
                ultmenu.AddBool("Combo.UltTower", "Ult under Tower", false);
                ultmenu.AddBool("Combo.UltOnlyKillable", "Ult only Killable", false);
                ultmenu.AddBool("Combo.RPriority", "Ult if priority 5 target is knocked up", true);
                ultmenu.AddSlider("Combo.RMinHit", "Min Enemies for Ult", 1, 1, 5);
                ultmenu.AddBool("Combo.OnlyifMin", "Only Ult if minimum enemies met", false);
                ultmenu.AddSlider("Combo.MinHealthUlt", "Minimum health to Ult %", 0, 0 , 100);


                menu.AddBool("Combo.UseIgnite", "Use Ignite");
            }
        }

        struct Harass
        {
            internal static void Attach(Menu menu)
            {
                menu.AddKeyBind("Harass.KB", "Harass Key", KeyCode("H"), KeyBindType.Toggle).Permashow(true, "Harass");
                menu.AddBool("Harass.InMixed", "Harass in Mixed Mode", false);
                menu.AddBool("Harass.UseQ", "Use Q");
                menu.AddBool("Harass.UseQ2", "Use Q2");
                menu.AddBool("Harass.UseE", "Use E", false);
                menu.AddBool("Harass.UseEMinion", "Use E Minions", false);
            }
        }

        struct Farm
        {
            internal static void Attach(Menu menu)
            {
                menu.AddBool("Farm.UseQ", "Use Q");
                menu.AddBool("Farm.UseQ2", "Use Q - Tornado");
                menu.AddSlider("Farm.Qcount", "Minions for Q (Tornado)", 1, 1, 10);
                menu.AddBool("Farm.UseE", "Use E");
            }
        }


        struct Waveclear
        {
            internal static void Attach(Menu menu)
            {
                var items = menu.AddSubMenu("Items");
                items.AddBool("Waveclear.UseItems", "Use Items");
                items.AddSlider("Waveclear.MinCountHDR", "Minion count for Cleave", 2, 1, 10);
                items.AddSlider("Waveclear.MinCountYOU", "Minion count for Youmu", 2, 1, 10);
                items.AddBool("Waveclear.UseTitanic", "Use Titanic");
                items.AddBool("Waveclear.UseTIA", "Use Tiamat");
                items.AddBool("Waveclear.UseHDR", "Use Hydra");
                items.AddBool("Waveclear.UseYMU", "Use Youmu", false);

                menu.AddBool("Waveclear.UseQ", "Use Q");
                menu.AddBool("Waveclear.UseQ2", "Use Q - Tornado");
                menu.AddSlider("Waveclear.Qcount", "Minions for Q (Tornado)", 1, 1, 10);
                menu.AddBool("Waveclear.UseE", "Use E");
                menu.AddSlider("Waveclear.Edelay", "Delay for E", 0, 0, 400);
                menu.AddBool("Waveclear.ETower", "Use E under Tower", false);
                menu.AddBool("Waveclear.UseENK", "Use E even if not killable");
                menu.AddBool("Waveclear.Smart", "Smart Waveclear");
            }
        }

        struct Killsteal
        {
            internal static void Attach(Menu menu)
            {
                menu.AddBool("Killsteal.Enabled", "KillSteal");
                menu.AddBool("Killsteal.UseQ", "Use Q");
                menu.AddBool("Killsteal.UseE", "Use E");
                menu.AddBool("Killsteal.UseR", "Use R");
                menu.AddBool("Killsteal.UseIgnite", "Use Ignite");
                menu.AddBool("Killsteal.UseItems", "Use Items");
            }
        }

      
        struct Evade
        {
            internal static void Attach(Menu menu)
            {
                var targettedmenu = new Menu("Targetted Spells", "Targetted");

                foreach (var spell in TargettedDanger.spellList.Where(x => HeroManager.Enemies.Any(e => e.CharData.BaseSkinName == x.championName)))
                {
                    Menu champmenu = targettedmenu.SubMenu(spell.championName);
                    champmenu.AddBool("enabled." + spell.spellName, spell.spellName, true);
                    champmenu.AddSlider("enabled." + spell.spellName + ".delay", spell.spellName + " Delay", 0, 0, 1000);
                }

                foreach (var spell in TargettedDanger.spellList.Where(x => HeroManager.Enemies.Any(e => x.championName == "Baron")))
                {
                    Menu champmenu = targettedmenu.SubMenu(spell.championName + " [Experimental] ");
                    champmenu.AddBool("enabled." + spell.spellName, spell.spellName, true);
                }

                menu.AddSubMenu(targettedmenu);

                Menu Flee = new Menu("Flee Settings", "Flee");
                Flee.AddSList("Flee.Mode", "Flee Mode", new[] { "To Nexus", "To Allies", "To Cursor" }, 2);
                Flee.AddBool("Flee.Smart", "Smart Flee", true);
                Flee.AddBool("Flee.StackQ", "Stack Q during Flee");
                Flee.AddBool("Flee.UseQ2", "Use Tornado", false);

                menu.AddSubMenu(Flee);

                menu.AddBool("Evade.Enabled", "Evade Enabled").Permashow(true, "Yasuo| Evade");
                menu.AddBool("Evade.OnlyDangerous", "Evade only Dangerous", false).Permashow(true, "Yasuo| Only Dangerous");
                menu.AddBool("Evade.FOW", "Dodge FOW Skills");
                menu.AddBool("Evade.WFilter", "Filter Circular Spells");
                // menu.AddKeyBind("Evade.OnlyDangerous", "Dodge only dangerous", 32, KeyBindType.Press).Permashow(true, "Yasuo| Only Dangerous");
                menu.AddSlider("Evade.MinDangerLevelWW", "Min Danger Level WindWall", 1, 1 , 5);
                menu.AddSlider("Evade.MinDangerLevelE", "Min Danger Level Dash", 1, 1, 5);
                menu.AddBool("Evade.WTS", "Windwall Targetted");
                menu.AddBool("Evade.WSS", "Windwall Skillshots");
                menu.AddBool("Evade.UseW", "Evade with Windwall");
                menu.AddBool("Evade.UseE", "Evade with E");
                menu.AddSlider("Evade.Delay", "Windwall Base Delay", 0, 0, 1000);
            }
        }


        struct Misc
        {
            internal static void Attach(Menu menu)
            {
                menu.AddBool("Misc.SafeE", "Safety Check for E");
                menu.AddBool("Misc.AutoStackQ", "Auto Stack Q", false);
                menu.AddBool("Misc.AutoR", "Auto Ultimate").Permashow();
                menu.AddSlider("Misc.RMinHit", "Min Enemies for Autoult", 1, 1, 5);
                menu.AddKeyBind("Misc.TowerDive", "Tower Dive Key", KeyCode("T"), KeyBindType.Press).Permashow(true, "Tower Diving Override");
                menu.AddSList("Hitchance.Q", "Q Hitchance", new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 2);
                menu.AddSlider("Misc.Healthy", "Healthy Amount HP", 5, 0, 100);
                menu.AddBool("Misc.AG", "Use Q (Tornado) on Gapcloser");
                menu.AddBool("Misc.Interrupter", "Use Q (Tornado) to Interrupt");
                menu.AddBool("Misc.Walljump", "Use Walljump", false);
                menu.AddKeyBind("Misc.DashMode", "Move to Mouse", '9', KeyBindType.Press);
                menu.AddBool("Misc.saveQ4QE", "Save Q3 for QE") ;
;               menu.AddBool("Misc.Debug", "Debug", false);
            }
        }

        struct Drawings
        {
            internal static void Attach(Menu menu)
            {
                menu.AddBool("Drawing.Disable", "Disable Drawings", true);
                menu.AddCircle("Drawing.DrawQ", "Draw Q", Yas.Qrange, System.Drawing.Color.Red);
                menu.AddCircle("Drawing.DrawE", "Draw E", Yas.Erange, System.Drawing.Color.CornflowerBlue);
                menu.AddCircle("Drawing.DrawR", "Draw R", Yas.Rrange, System.Drawing.Color.DarkOrange);
                menu.AddBool("Drawing.SS", "Draw Skillshot Drawings", false);
            }
        }

        internal static uint KeyCode(string s)
        {
            return s.ToCharArray()[0];
        }
    }
}
