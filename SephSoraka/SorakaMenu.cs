using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

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

                Menu Automatic = new Menu("Healing", "Auto", false);
                Automatic.AddItem(new MenuItem("Healing.UseW", "Use W", false).SetValue(true));
                Automatic.AddItem(new MenuItem("Healing.UseR", "Use R", false).SetValue(true));
                Config.AddSubMenu(Automatic);

                // WManager Options
                Menu WManager = new Menu("W Settings", "WManager");
                WManager.AddItem(new MenuItem("onlywincdmg", "Only heal if incoming damage").SetValue(false));
                WManager.AddItem(new MenuItem("wdamagedetection", "Disable damage detection").SetValue(false));
                WManager.AddItem(new MenuItem("wcheckdmgafter", "Take HP after damage into consideration").SetValue(true));
                WManager.AddItem(new MenuItem("wonlyadc", "Heal only ADC").SetValue(false));

                foreach (var hero in HeroManager.Allies.Where(x => !x.IsMe))
                {
                    WManager.AddItem(new MenuItem("w" + hero.ChampionName, "Heal " + hero.ChampionName).SetValue(true));
                    WManager.AddItem(
                        new MenuItem("wpct" + hero.ChampionName, "Health % " + hero.ChampionName).SetValue(new Slider(
                            45, 0, 100)));
                }

                Config.AddSubMenu(WManager);

                // RManager Options
                Menu RManager = new Menu("R Settings", "RManager");

                RManager.AddItem(
                    new MenuItem("ultmode", "Ult Mode").SetValue(new StringList(new[] {"Default", "Advanced"})));

                RManager.AddItem(new MenuItem("onlyrincdmg", "Only if incoming damage").SetValue(false));
                RManager.AddItem(new MenuItem("ultonlyadc", "Only if adc needs it").SetValue(false));
                RManager.AddItem(new MenuItem("rcheckdmgafter", "Take HP after damage into consideration").SetValue(true));

                RManager.AddItem(
                    new MenuItem("minallies", "Minimum # of allies under set health").SetValue(new Slider(1, 0, 5)));

                foreach (var hero in HeroManager.Allies)
                {
                    RManager.AddItem(
                        new MenuItem("r" + hero.ChampionName, "Heal " + hero.ChampionName).SetValue(true));
                    RManager.AddItem(
                        new MenuItem("rpct" + hero.ChampionName, "Health % " + hero.ChampionName)
                            .SetValue(new Slider(45, 0, 100)));
                }

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

                    Priorities.AddItem((new MenuItem("adc", "ADC").SetValue(new StringList(allynames, iof))));
                



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
                KillSteal.AddItem(new MenuItem("Killsteal", "KillSteal").SetValue(true));
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
               // Farming.AddItem(new MenuItem("Farm.Disableauto", "Dont farm minions").SetValue(true));
                Farming.AddItem(new MenuItem("Farm.UseQ", "Use Q").SetValue(true));
                Farming.AddItem(new MenuItem("Farm.UseE", "Use E").SetValue(true));

                Config.AddSubMenu(Farming);

                Menu Interrupter = new Menu("Interrupter", "Interrupter +");
                Interrupter.AddItem(new MenuItem("Interrupter.UseQ", "Use Q").SetValue(true));
                Interrupter.AddItem(new MenuItem("Interrupter.UseW", "Use W").SetValue(true));
                Interrupter.AddItem(new MenuItem("Seperator", "----AntiGapCloser-----"));
                Interrupter.AddItem(new MenuItem("Interrupter.AntiGapClose", "AntiGapClosers").SetValue(true));
                Interrupter.AddItem(new MenuItem("Interrupter.AG.UseQ", "AntiGapClose with Q").SetValue(true));
                Interrupter.AddItem(new MenuItem("Interrupter.AG.UseE", "AntiGapClose with E").SetValue(true));

                Config.AddSubMenu(Interrupter);

                Menu Blist = new Menu("Heal BlackList", "BlackList");
                foreach (var hero in HeroManager.Allies)
                {
                    var champ = hero;
                    var addhero =
                        Blist.AddItem(new MenuItem("Blacklist." + hero.ChampionName, hero.ChampionName).SetValue(false));
                    addhero.ValueChanged += (sender, args) =>
                    {
                        if (args.GetNewValue<bool>())
                        {
                            BlackList.Add(champ);
                        }
                        else
                        {
                            BlackList.Remove(champ);
                        }
                    };
                }

                Config.AddSubMenu(Blist);


                Menu Misc = new Menu("Hitchance Settings", "Misc", false);
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
                Misc.AddItem(new MenuItem("Misc.Debug", "Debug", false).SetValue(false));
                Config.AddSubMenu(Misc);

                Menu Drawings = new Menu("Drawings", "Drawing", false);
                Drawings.AddItem(new MenuItem("Drawing.Disable", "Disable all").SetValue(false));
                Drawings.AddItem(new MenuItem("Drawing.DrawQ", "Draw Q").SetValue(true));
                Drawings.AddItem(new MenuItem("Drawing.DrawW", "Draw Q").SetValue(true));
                Drawings.AddItem(new MenuItem("Drawing.DrawE", "Draw E").SetValue(true));
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

    }
}