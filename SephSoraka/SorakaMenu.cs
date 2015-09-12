using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace SephSoraka
{
    class SorakaMenu
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Obj_AI_Hero> BlackList = new List<Obj_AI_Hero>();
        public static Menu CreateMenu()
        {
            try
            {
                Config = new Menu("SephSoraka", "Soraka", true);
                Menu TSMenu = new Menu("Target Selector", "TS", false);
                TargetSelector.AddToMenu(TSMenu);
                Config.AddSubMenu(TSMenu);

                Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking", false));
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

                Menu Healing = new Menu("Healing", "Auto");
                Healing.AddItem(new MenuItem("Healing.UseW", "Use W").SetValue(true));
                Healing.AddItem(new MenuItem("Healing.UseR", "Use R").SetValue(true));
                Healing.AddItem(new MenuItem("Healing.Priority", "Priority Type").SetValue(new StringList(new[] { "Lowest Health", "Priority List"})));

                Config.AddSubMenu(Healing);


                // WManager Options
                Menu WManager = new Menu("W Settings", "WManager");
                WManager.AddItem(new MenuItem("onlywincdmg", "Only heal if incoming damage").SetValue(false));
                WManager.AddItem(new MenuItem("wdamagedetection", "Disable damage detection").SetValue(false));
                WManager.AddItem(new MenuItem("wcheckdmgafter", "Take HP-after-Damage into consideration").SetValue(true));
                WManager.AddItem(new MenuItem("wonlyadc", "Heal only ADC").SetValue(false));

                Menu Wsubmenu = new Menu("Heal %", "healpercent");
                foreach (var hero in HeroManager.Allies.Where(x => !x.IsMe))
                {
                    Wsubmenu.AddItem(new MenuItem("w" + hero.ChampionName, "Heal " + hero.ChampionName).SetValue(true));
                    Wsubmenu.AddItem(
                        new MenuItem("wpct" + hero.ChampionName, "Health % " + hero.ChampionName).SetValue(new Slider(
                            45, 0, 100)));
                }

                WManager.AddSubMenu(Wsubmenu);

                Config.AddSubMenu(WManager);

                // RManager Options
                Menu RManager = new Menu("R Settings", "RManager");

                RManager.AddItem(
                    new MenuItem("ultmode", "Ultimate Mode").SetValue(new StringList(new[] {"Default", "Advanced"})));

                Menu AntiIgnite = new Menu("Anti-Ignite", "Antiignite");
                AntiIgnite.AddItem(new MenuItem("ultifadcignited", "Use R if ADC is Ignited").SetValue(false));
                AntiIgnite.AddItem(new MenuItem("adcignitedhealth", "ADC below HP").SetValue(new Slider(40, 0, 100)));

                RManager.AddSubMenu(AntiIgnite);

                RManager.AddItem(new MenuItem("onlyrincdmg", "Only if incoming damage").SetValue(false));
                RManager.AddItem(new MenuItem("ultonlyadc", "Only if ADC needs healing").SetValue(false));
                RManager.AddItem(new MenuItem("rcheckdmgafter", "Take HP-after-damage into consideration").SetValue(true));

                RManager.AddItem(
                    new MenuItem("minallies", "Minimum # of allies under set health").SetValue(new Slider(1, 0, 5)));

                Menu Rsubmenu = new Menu("Heal %", "Healpercents");
                foreach (var hero in HeroManager.Allies)
                {
                    Rsubmenu.AddItem(
                        new MenuItem("r" + hero.ChampionName, "Heal " + hero.ChampionName).SetValue(true));
                    Rsubmenu.AddItem(
                        new MenuItem("rpct" + hero.ChampionName, "Health % " + hero.ChampionName)
                            .SetValue(new Slider(30, 0, 100)));
                }

                RManager.AddSubMenu(Rsubmenu);
                Config.AddSubMenu(RManager);

                Menu Priorities = new Menu("Priorities", "Priorities");


                string[] allynames = new string[HeroManager.Allies.Count];

                foreach (var hero in HeroManager.Allies)
                {
                    var index = HeroManager.Allies.IndexOf(hero);
                    allynames[index] = hero.ChampionName;
                }

                int iof = 0;
                var detected = Soraka.DetectADC();
                if (detected != null)
                {
                    iof = Array.IndexOf(allynames, allynames.FirstOrDefault(x => x == detected.ChampionName));
                }

                    Priorities.AddItem((new MenuItem("adc", "ADC").SetValue(new StringList(allynames, iof)))).Permashow();

                Priorities.AddItem(
                    new MenuItem("Priorities.Mikaels", "Min priority for Mikaels").SetValue(new Slider(5, 1, 5)));

                foreach (var ally in HeroManager.Allies)
                {
                    var priority = ally.loadpriority();
                    Priorities.AddItem(
                        new MenuItem("p" + ally.ChampionName, ally.ChampionName).SetValue(new Slider(priority, 0, 5)));
                }

                Config.AddSubMenu(Priorities);


                Menu Combo = new Menu("Combo", "Combo");
                Combo.AddItem(new MenuItem("Combo.UseQ", "Use Q").SetValue(true));
                Combo.AddItem(new MenuItem("Combo.UseE", "Use E").SetValue(true));
                Config.AddSubMenu(Combo);


                Menu KillSteal = new Menu("Killsteal", "Killsteal");
                KillSteal.AddItem(new MenuItem("Killsteal", "Killsteal").SetValue(true));
                KillSteal.AddItem(new MenuItem("Killsteal.UseQ", "Use Q").SetValue(false));
                KillSteal.AddItem(new MenuItem("Killsteal.UseE", "Use E").SetValue(false));
                KillSteal.AddItem(new MenuItem("Killsteal.UseIgnite", "Use Ignite").SetValue(false));
                Config.AddSubMenu(KillSteal);


                Menu Harass = new Menu("Harass", "Harass");
                Harass.AddItem(
                    new MenuItem("Keys.HarassT", "Harass Toggle").SetValue(new KeyBind("H".ToCharArray()[0],
                        KeyBindType.Toggle)));
                Harass.AddItem(new MenuItem("Harass.InMixed", "Harass in Mixed Mode").SetValue(true));
                Harass.AddItem(new MenuItem("Harass.UseQ", "Use Q").SetValue(true));
                Harass.AddItem(new MenuItem("Harass.UseE", "Use E").SetValue(true));
                Harass.AddItem(
                    new MenuItem("Harass.Mana", "Min mana for harass (%)", false).SetValue(new Slider(50, 0, 100)));
                Config.AddSubMenu(Harass);

                Menu Farming = new Menu("Farming", "Farming");
                Farming.AddItem(new MenuItem("Farm.Disableauto", "Dont auto minions").SetValue(true));
                Farming.AddItem(new MenuItem("Farm.Range", "Min ally-distance to auto attack").SetValue(new Slider(1400, 700, 2000)));
                Farming.AddItem(new MenuItem("Farm.UseQ", "Use Q").SetValue(true));

                Config.AddSubMenu(Farming);

                Menu Misc = new Menu("Misc Settings", "Misc", false);
                Misc.AddItem(
                    new MenuItem("Hitchance.Q", "Q Hit Chance").SetValue(
                        new StringList(
                            new[]
                            {
                                HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                                HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString()
                            }, 1)));
                Misc.AddItem(
                    new MenuItem("Hitchance.E", "E Hit Chance").SetValue(
                        new StringList(
                            new[]
                            {
                                HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(),
                                HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString()
                            }, 1)));
				Menu Debug = new Menu("Debug", "Debugmenu");
	            Debug.AddItem(new MenuItem("Debug", "Debug")).SetValue(false);
;	            Misc.AddSubMenu(Debug);
                Misc.AddItem(new MenuItem("Misc.UseMikael", "Use Mikaels").SetValue(true));
                Misc.AddItem(new MenuItem("Misc.AutoEStunned", "Auto E immobile champions").SetValue(false));
                Misc.AddItem(new MenuItem("Misc.Nohealshop", "Dont heal if ally in base").SetValue(true));
                Config.AddSubMenu(Misc);

                Menu Interrupter = new Menu("Interrupter", "Interrupter +");
                Interrupter.AddItem(new MenuItem("Interrupter.UseQ", "Use Q").SetValue(true));
                Interrupter.AddItem(new MenuItem("Interrupter.UseE", "Use E").SetValue(true));
                Interrupter.AddItem(new MenuItem("Seperator", "----- Anti-Gapcloser -----"));
                Interrupter.AddItem(new MenuItem("Interrupter.AG.UseQ", "Anti-Gapclose with Q").SetValue(true));
                Interrupter.AddItem(new MenuItem("Interrupter.AG.UseE", "Anti-Gapclose with E").SetValue(true));

                Config.AddSubMenu(Interrupter);


                Menu Drawings = new Menu("Drawings", "Drawing", false);
                Drawings.AddItem(new MenuItem("Drawing.Disable", "Disable All").SetValue(false));
                Drawings.AddItem(new MenuItem("Drawing.DrawQ", "Draw Q").SetValue(new Circle(true, Color.White)));
                Drawings.AddItem(new MenuItem("Drawing.DrawW", "Draw W").SetValue(new Circle(true, Color.DarkCyan)));
                Drawings.AddItem(new MenuItem("Drawing.DrawE", "Draw E").SetValue(new Circle(true, Color.RoyalBlue)));
                Drawings.AddItem(new MenuItem("Drawing.Drawfarm", "Farming-range indicator").SetValue(true));
                Config.AddSubMenu(Drawings);
                return Config;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Config;
        }

        internal enum UltMode
        {
            Default,
            Advanced
        }

        internal enum HealPriority
        {
            PriorityList,
            LowestHealth
        }

        internal static HealPriority PriorityType()
        {
            switch (Config.Item("Healing.Priority").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HealPriority.LowestHealth;
                case 1:
                    return HealPriority.PriorityList;
                default:
                    return HealPriority.LowestHealth;
            }
        }


    }
}
