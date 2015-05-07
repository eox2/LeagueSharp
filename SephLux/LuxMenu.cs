using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephLux
{
    class LuxMenu
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Obj_AI_Hero> BlackList = new List<Obj_AI_Hero>(); 
        public static Menu CreateMenu()
        {
            Config = new Menu("SephLux", "Lux", true);
            LeagueSharp.Common.TargetSelector TargetSelector = new LeagueSharp.Common.TargetSelector();
            Menu TSMenu = new Menu("Target Selector", "TS", false);
            TargetSelector.AddToMenu(TSMenu);
            Config.AddSubMenu(TSMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking", false));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Menu Combo = new Menu("Combo", "Combo", false);
            Combo.AddItem(new MenuItem("Combo.UseQ", "Use Q", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseE", "Use E", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseE2", "Use E2", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseR", "Use R", false).SetValue(true));
            Config.AddSubMenu(Combo);


            Menu KillSteal = new Menu("Killsteal", "Killsteal", false);
            KillSteal.AddItem(new MenuItem("Killsteal", "KillSteal", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseQ", "Use Q", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseE", "Use E", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseE2", "Use E", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseR", "Use R", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseIgnite", "Use Ignite", false).SetValue(true));
            Config.AddSubMenu(KillSteal);


            Menu Harass = new Menu("Harass", "Harass", false);
            Harass.AddItem(new MenuItem("Keys.HarassT", "Harass Toggle", false).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            Harass.AddItem(new MenuItem("Harass.UseQ", "Use Q", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseW", "Use W", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseE", "Use E", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.Mana", "Min mana for harass (%)", false).SetValue(new Slider(50, 0, 100)));
            Config.AddSubMenu(Harass);

            Menu Farm = new Menu("Farm (LH)", "Farm", false);
            Farm.AddItem(new MenuItem("Farm.Enable", "Enable abilities for farming").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.Mana", "Minimum Mana %").SetValue(new Slider(50, 0, 100)));
            Farm.AddItem(new MenuItem("Farm.UseQ", "Use Q").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseE", "Use E").SetValue(true));
            Config.AddSubMenu(Farm);

            Menu Waveclear = new Menu("Waveclear", "Waveclear", false);
            Waveclear.AddItem(new MenuItem("Waveclear.UseQ", "Use Q").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseR", "Use R").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.Rcount", "Minions for R").SetValue(new Slider(10, 0, 20)));
            Waveclear.AddItem(new MenuItem("Waveclear.UseE", "Use E").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseE2", "Use E2").SetValue(true));
            Config.AddSubMenu(Waveclear);

            Menu Interrupter = new Menu("Interrupter", "Interrupter +", false);
            Interrupter.AddItem(new MenuItem("Interrupter.UseQ", "Use Q").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AntiGapClose", "AntiGapClosers").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AG.UseQ", "AntiGapClose with Q").SetValue(true));
            Config.AddSubMenu(Interrupter);
            
            Menu Blist = new Menu("Ultimate BlackList", "BlackList", false);
            foreach (var hero in HeroManager.Enemies)
            {
                var champ = hero;
                var addhero = Blist.AddItem(new MenuItem("Blacklist." + hero.ChampionName, hero.ChampionName, false).SetValue(false));
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
             

            Menu Misc = new Menu("Misc", "Misc", false);
            Misc.AddItem(new MenuItem("Misc.PrioritizeUnderTurret", "Prioritize Targets Under Turret", false).SetValue(true));
            Misc.AddItem(new MenuItem("spaceholder", ""));
            Misc.AddItem(new MenuItem("Hitchance.Q", "Q Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 1)));
            Misc.AddItem(new MenuItem("Hitchance.E", "E Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() , HitChance.Immobile.ToString() }, 1)));
            Misc.AddItem(new MenuItem("Hitchance.R", "R Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 4)));
            Misc.AddItem(new MenuItem("Misc.Debug", "Debug", false).SetValue(false));
            Config.AddSubMenu(Misc);

            Menu Drawings = new Menu("Drawings", "Drawing", false);
            Drawings.AddItem(new MenuItem("Drawing.Disable", "Disable all").SetValue(false));
            Drawings.AddItem(new MenuItem("Drawing.DrawQ", "Draw Q").SetValue(true));
            Drawings.AddItem(new MenuItem("Drawing.DrawW", "Draw W").SetValue(true));
            Drawings.AddItem(new MenuItem("Drawing.DrawE", "Draw E").SetValue(true));
            Drawings.AddItem(new MenuItem("Drawing.DrawR", "Draw R").SetValue(true));
            Config.AddSubMenu(Drawings);
            return Config;
        }
    }
}
