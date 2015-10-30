using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephZiggs
{
    class ZiggsMenu
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Obj_AI_Hero> BlackList = new List<Obj_AI_Hero>();
        public static Menu CreateMenu()
        {
            Config = new Menu("SephZiggs", "Ziggs", true);
			LeagueSharp.Common.TargetSelector TargetSelector = new LeagueSharp.Common.TargetSelector();
            Menu TSMenu = new Menu("Target Selector", "TS", false);
            TargetSelector.AddToMenu(TSMenu);
            Config.AddSubMenu(TSMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking", false));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Menu Combo = new Menu("Combo", "Combo", false);
            Combo.AddItem(new MenuItem("Combo.UseQ", "Use Q", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseW", "Use W", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseE", "Use E", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseR", "Use R", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.Rcount", "R Count Combo").SetValue(new Slider(1, 1, 5))).Permashow();
            Combo.AddItem(new MenuItem("Combo.RKillable", "R only killable", false).SetValue(false)).Permashow();
			Config.AddSubMenu(Combo);

            Menu Auto = new Menu("Auto", "Auto", false);
            Auto.AddItem(new MenuItem("Auto.R", "R Automatically").SetValue(true)).Permashow();
            Auto.AddItem(new MenuItem("Auto.Rcount", "Min Enemies R").SetValue(new Slider(1, 1, 5))).Permashow();
            Auto.AddItem(new MenuItem("Auto.W", "W Automatically").SetValue(true)).Permashow();
            Auto.AddItem(new MenuItem("Auto.Whp", "Health % for W").SetValue(new Slider(40, 0, 100)));
            Config.AddSubMenu(Auto);

            Menu KillSteal = new Menu("Killsteal", "Killsteal", false);
            KillSteal.AddItem(new MenuItem("Killsteal", "Killsteal", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseQ", "Use Q", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseW", "Use W", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseE", "Use E", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseR", "Use R", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseIgnite", "Use Ignite", false).SetValue(true));
            Config.AddSubMenu(KillSteal);


            Menu Harass = new Menu("Harass", "Harass", false);
            Harass.AddItem(new MenuItem("Keys.HarassT", "Harass Toggle", false).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle))).Permashow();
			Harass.AddItem(new MenuItem("Harass.InMixed", "Harass in Mixed Mode").SetValue(true));
			Harass.AddItem(new MenuItem("Harass.UseQ", "Use Q", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseW", "Use W", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseE", "Use E", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.Mana", "Min. % Mana for Harass", false).SetValue(new Slider(50, 0, 100)));
            Config.AddSubMenu(Harass);

            Menu Farm = new Menu("Farm (Last Hit)", "Farm", false);
            Farm.AddItem(new MenuItem("Farm.Enable", "Enable Abilities for Farming").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.Mana", "Min. % Mana for Farming").SetValue(new Slider(50, 0, 100)));
            Farm.AddItem(new MenuItem("Farm.UseQ", "Use Q").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseW", "Use W").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseE", "Use E").SetValue(true));
            Config.AddSubMenu(Farm);

            Menu Waveclear = new Menu("Waveclear", "Waveclear", false);
            Waveclear.AddItem(new MenuItem("Waveclear.UseQ", "Use Q").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseR", "Use R").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.Rcount", "Minions to Use R").SetValue(new Slider(10, 0, 20)));
            Waveclear.AddItem(new MenuItem("Waveclear.UseE", "Use E").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseE2", "Use E2").SetValue(true));
            Config.AddSubMenu(Waveclear);

            Menu Interrupter = new Menu("Interrupter", "Interrupter +", false);
            Interrupter.AddItem(new MenuItem("Interrupter.UseW", "Use W on Dangerous Spells").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AG.UseW", "Anti-Gapclose with W").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AG.UseW", "Anti-Gapclose with E").SetValue(true));
            Config.AddSubMenu(Interrupter);

            Menu Blist = new Menu("Ultimate Blacklist", "BlackList", false);
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

            Menu Misc = new Menu("Misc", "Misc");
            Misc.AddItem(new MenuItem("Misc.Debug", "Debug").SetValue(false));
            Misc.AddItem(new MenuItem("Misc.RKey", "Ult Key").SetValue(new KeyBind(114, KeyBindType.Press)));
            Config.AddSubMenu(Misc);

            Menu HC = new Menu("Hit Chance Settings", "HC", false);
            HC.AddItem(new MenuItem("Hitchance.Q", "Q Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 1)));
            HC.AddItem(new MenuItem("Hitchance.W", "Q Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 1)));
            HC.AddItem(new MenuItem("Hitchance.E", "E Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 1)));
            HC.AddItem(new MenuItem("Hitchance.R", "R Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 1)));
            Config.AddSubMenu(HC);



            Menu Drawings = new Menu("Drawings", "Drawing", false);
            Drawings.AddItem(new MenuItem("Drawing.Disable", "Disable All").SetValue(false));
            Drawings.AddItem(new MenuItem("Drawing.DrawQ", "Draw Q").SetValue(true));
            Drawings.AddItem(new MenuItem("Drawing.DrawE", "Draw E").SetValue(true));
            Drawings.AddItem(new MenuItem("Drawing.DrawR", "Draw R").SetValue(true));
            Drawings.AddItem(new MenuItem("Drawing.DrawRMM", "Draw R Minimap").SetValue(true));
            Config.AddSubMenu(Drawings);

            return Config;
        }
    }
}
