using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephCassiopeia
{
    class CassiopeiaMenu
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Obj_AI_Hero> BlackList = new List<Obj_AI_Hero>(); 
        public static Menu CreateMenu()
        {
            Config = new Menu("SephCassio", "Lux", true);
            LeagueSharp.Common.TargetSelector TargetSelector = new LeagueSharp.Common.TargetSelector();
            Menu TSMenu = new Menu("Target Selector", "TS", false);
            TargetSelector.AddToMenu(TSMenu);
            Config.AddSubMenu(TSMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking", false));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Menu Combo = new Menu("Combo", "Combo", false);
            Combo.AddItem(new MenuItem("Combo.Disableautoifspellsready", "Disable autos only when spells up", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.Useauto", "Use auto attacks", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseQ", "Use Q", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseW", "Use W", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseE", "Use E", false).SetValue(true));
            Combo.AddItem((new MenuItem("Combo.edelay", "Edelay").SetValue(new Slider(0, 0, 1000))));
            Combo.AddItem(new MenuItem("Combo.UseR", "Use R", false).SetValue(true));
            Combo.AddItem((new MenuItem("Combo.Rcount", "Enemies count for Ult").SetValue(new Slider(1, 0, 5))));
            Combo.AddItem(new MenuItem("Combo.UseRNF", "Use R even if targets not facing", false).SetValue(false));
            Combo.AddItem((new MenuItem("Combo.Rcountnf", "Enemies count if not facing").SetValue(new Slider(3, 0, 5))));
            Config.AddSubMenu(Combo);




            Menu KillSteal = new Menu("Killsteal", "Killsteal", false);
            KillSteal.AddItem(new MenuItem("Killsteal", "KillSteal", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseQ", "Use Q", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseW", "Use E", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseE", "Use E", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseR", "Use R", false).SetValue(false));
            KillSteal.AddItem(new MenuItem("Killsteal.UseIgnite", "Use Ignite", false).SetValue(true));
            Config.AddSubMenu(KillSteal);


            Menu Harass = new Menu("Harass", "Harass", false);
            Harass.AddItem(new MenuItem("Keys.HarassT", "Harass Toggle", false).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
            Harass.AddItem(new MenuItem("Harass.InMixed", "Harass in Mixed Mode", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseQ", "Use Q", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseW", "Use W", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseE", "Use E", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.Mana", "Min mana for harass (%)", false).SetValue(new Slider(50, 0, 100)));
            Config.AddSubMenu(Harass);

            Menu Farm = new Menu("Farm (LH)", "Farm", false);
            Farm.AddItem(new MenuItem("Farm.Enable", "Enable abilities for farming").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.Mana", "Minimum Mana %").SetValue(new Slider(50, 0, 100)));
            Farm.AddItem(new MenuItem("Farm.Useauto", "Enable autos").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseQ", "Use Q").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseW", "Use W").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseE", "Use E").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.useepoison", "Use E only if poisoned").SetValue(true));
            Config.AddSubMenu(Farm);

            Menu Waveclear = new Menu("Waveclear", "Waveclear", false);
            Waveclear.AddItem(new MenuItem("Waveclear.Useauto", "Enable autos").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseQ", "Use Q").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseW", "Use W").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseE", "Use E").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.useepoison", "Use E only if poisoned").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.useekillable", "Use E only if poisoned").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseR", "Use R").SetValue(false));
            Waveclear.AddItem(new MenuItem("Waveclear.Rcount", "Minions for R").SetValue(new Slider(10, 0, 20)));
            Config.AddSubMenu(Waveclear);

            Menu Interrupter = new Menu("Interrupter", "Interrupter +", false);
            Interrupter.AddItem(new MenuItem("Interrupter.UseR", "Interrupt with R").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AntiGapClose", "AntiGapClosers").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AG.UseR", "AntiGapClose with R").SetValue(true));
            Config.AddSubMenu(Interrupter);
            
            Menu Blist = new Menu("BlackList", "BlackList", false);
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
             

            Menu hc = new Menu("Hitchance Settings", "hc", false);
            hc.AddItem(new MenuItem("Hitchance.Q", "Q Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 2)));
            hc.AddItem(new MenuItem("Hitchance.W", "E Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() , HitChance.Immobile.ToString() }, 2)));
            hc.AddItem(new MenuItem("Hitchance.R", "R Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString(), HitChance.Immobile.ToString() }, 3)));
            Config.AddSubMenu(hc);

            Menu misc = new Menu("Misc", "Misc", false);
            var autolvl = misc.AddItem(new MenuItem("Misc.autolevel", "Autolevel", false).SetValue(false));
            autolvl.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    CommonAutoLevel.Enabled(true);
                }
                else
                {
                    CommonAutoLevel.Enabled(false);
                }
            };
            misc.AddItem(new MenuItem("Misc.autoe", "Auto use e when possible (no buttons pressed)", false).SetValue(false));
            misc.AddItem(new MenuItem("Misc.Debug", "Debug", false).SetValue(false));
            Config.AddSubMenu(misc);


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
