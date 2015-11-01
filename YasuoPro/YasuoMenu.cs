using System.Reflection;
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
                items.AddBool("Items.UseTIA", "Use Tiamat");
                items.AddBool("Items.UseHDR", "Use Hydra");
                items.AddBool("Items.UseBRK", "Use BORK");
                items.AddBool("Items.UseBLG", "Use Bilgewater");
                items.AddBool("Items.UseYMU", "Use Youmu");

                menu.AddBool("Combo.UseQ", "Use Q");
                menu.AddBool("Combo.UseW", "Use W");
                menu.AddBool("Combo.UseE", "Use E");
                menu.AddBool("Combo.ETower", "Use E under Tower", false);

                var ultmenu = menu.AddSubMenu("Ult Settings");
                ultmenu.AddBool("Combo.UseR", "Use R");
                ultmenu.AddBool("Combo.UltTower", "Ult under Tower", false);
                ultmenu.AddSlider("Combo.RMinHit", "Min Enemies for Ult", 1, 1, 5);
                ultmenu.AddBool("Combo.UltLogic", "Some Ult Logic");


                menu.AddBool("Combo.UseIgnite", "Use Ignite");
            }
        }

        struct Harass
        {
            internal static void Attach(Menu menu)
            {
                menu.AddKeyBind("Harass.KB", "Harass Key", KeyCode("H"), KeyBindType.Toggle).Permashow(true, "Harass");
                menu.AddBool("Harass.InMixed", "Harass in Mixed Mode");
                menu.AddBool("Harass.UseQ", "Use Q");
                menu.AddBool("Harass.UseE", "Use E");
                menu.AddBool("Harass.UseEMinion", "Use E Minions");
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
                items.AddBool("Waveclear.UseTIA", "Use Tiamat");
                items.AddBool("Waveclear.UseHDR", "Use Hydra");
                items.AddBool("Waveclear.UseYMU", "Use Youmu", false);

                menu.AddBool("Waveclear.UseQ", "Use Q");
                menu.AddBool("Waveclear.UseQ2", "Use Q - Tornado");
                menu.AddSlider("Waveclear.Qcount", "Minions for Q (Tornado)", 1, 1, 10);
                menu.AddBool("Waveclear.UseE", "Use E");
                menu.AddBool("Waveclear.ETower", "Use E under Tower", false);
                menu.AddBool("Waveclear.UseENK", "Use E even if not killable");
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
                menu.AddBool("Evade.Enabled", "Evade Enabled").Permashow(true, "Yasuo| Evade");
                menu.AddBool("Evade.OnlyDangerous", "Evade only Dangerous", false).Permashow(true, "Yasuo| Only Dangerous");
                // menu.AddKeyBind("Evade.OnlyDangerous", "Dodge only dangerous", 32, KeyBindType.Press).Permashow(true, "Yasuo| Only Dangerous");
                menu.AddSlider("Evade.MinDangerLevelWW", "Min Danger Level WindWall", 3, 1 , 5);
                menu.AddSlider("Evade.MinDangerLevelE", "Min Danger Level Dash", 1, 1, 5);
                menu.AddBool("Evade.WTS", "Windwall Targetted");
                menu.AddBool("Evade.WSS", "Windwall Skillshots");
                menu.AddBool("Evade.UseW", "Evade with Windwall");
                menu.AddBool("Evade.UseE", "Evade with E");
                menu.AddSList("Flee.Mode", "Flee Mode", new[] { "To Nexus", "To Allies", "To Cursor" }, 0);

            }
        }


        struct Misc
        {
            internal static void Attach(Menu menu)
            {
                menu.AddBool("Misc.AutoR", "Auto Ultimate").Permashow();
                menu.AddSlider("Misc.RMinHit", "Min Enemies for Autoult", 1, 1, 5);

                menu.AddSList("Hitchance.Q", "Q Hitchance", new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1);
                menu.AddSlider("Misc.Healthy", "Healthy Amount HP", 20, 0, 100);
                menu.AddBool("Misc.AG", "Use Q (Tornado) on Gapcloser");
                menu.AddBool("Misc.Interrupter", "Use Q (Tornado) to Interrupt");
                menu.AddBool("Misc.Debug", "Debug", false);
            }
        }

        struct Drawings
        {
            internal static void Attach(Menu menu)
            {
                menu.AddBool("Drawing.Active", "Disable Drawings");
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
