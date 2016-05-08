using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephLissandra
{
    class LissMenu
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu CreateMenu()
        {
            Config = new Menu("SephLissandra", "Liss", true);

            LeagueSharp.Common.TargetSelector TargetSelector = new LeagueSharp.Common.TargetSelector();
            Menu TSMenu = new Menu("Target Selector", "TS", false);
            TargetSelector.AddToMenu(TSMenu);
            Config.AddSubMenu(TSMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking", false));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Menu Combo = new Menu("Combo", "Combo", false);
            Combo.AddItem(new MenuItem("Keys.Combo", "Combo Key", false).SetValue(new KeyBind(32, KeyBindType.Press)));
            Combo.AddItem(new MenuItem("Combo.UseQ", "Use Q", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseW", "Use W", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseE", "Use E", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseE2", "Use E2", false).SetValue(true));
            Combo.AddItem(new MenuItem("Combo.UseR", "Use R", false).SetValue(true));
            Combo.AddItem((new MenuItem("Combo.ecountW", "Enemies count for 2nd E (W)").SetValue(new Slider(2, 0, 5))));
            Combo.AddItem((new MenuItem("Combo.ecountR", "Enemies count for 2nd E (R)").SetValue(new Slider(2, 0, 5))));
            Combo.AddItem((new MenuItem("Combo.Rcount", "Enemies count for self Ult").SetValue(new Slider(2, 0, 5))));
            Combo.AddItem((new MenuItem("Combo.MinRHealth", "Min Enemy Health To Ult them").SetValue(new Slider(25, 1, 100))));
            Config.AddSubMenu(Combo);


            Menu KillSteal = new Menu("Killsteal", "Killsteal", false);
            KillSteal.AddItem(new MenuItem("Killsteal", "KillSteal", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseQ", "Use Q", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseW", "Use W", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseE", "Use E", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseE2", "Use E2", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseR", "Use R", false).SetValue(true));
            KillSteal.AddItem(new MenuItem("Killsteal.UseIgnite", "Use Ignite", false).SetValue(true));
            Config.AddSubMenu(KillSteal);


            Menu Harass = new Menu("Harass", "Harass", false);
            Harass.AddItem(new MenuItem("Keys.HarassT", "Harass Toggle", false).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle))).Permashow();
            Harass.AddItem(new MenuItem("Keys.Harass", "Harass Key", false).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Harass.AddItem(new MenuItem("Harass.UseQ", "Use Q", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseW", "Use W", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.UseE", "Use E", false).SetValue(true));
            Harass.AddItem(new MenuItem("Harass.Mana", "Min mana for harass (%)", false).SetValue(new Slider(50, 0, 100)));
            Config.AddSubMenu(Harass);

            Menu Farm = new Menu("Farm (LH)", "Farm", false);
            Farm.AddItem(new MenuItem("Keys.Farm", "Farm Key", false).SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Farm.AddItem(new MenuItem("Farm.Mana", "Minimum Mana %").SetValue(new Slider(50, 0, 100)));
            Farm.AddItem(new MenuItem("Farm.UseQ", "Use Q").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseW", "Use W").SetValue(true));
            Farm.AddItem(new MenuItem("Farm.UseE", "Use E").SetValue(true));
            Config.AddSubMenu(Farm);

            Menu Waveclear = new Menu("Waveclear", "Waveclear", false);
            Waveclear.AddItem(new MenuItem("Keys.Waveclear", "Waveclear Key", false).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Waveclear.AddItem(new MenuItem("Waveclear.UseQ", "Use Q").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseW", "Use W").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.Wcount", "Minions for W").SetValue(new Slider(2, 0, 20)));
            Waveclear.AddItem(new MenuItem("Waveclear.UseE", "Use E").SetValue(true));
            Waveclear.AddItem(new MenuItem("Waveclear.UseE2", "Use E2").SetValue(true));
            Config.AddSubMenu(Waveclear);

            Menu Interrupter = new Menu("Interrupter", "Interrupter +", false);
            Interrupter.AddItem(new MenuItem("Interrupter", "Interrupt Important Spells").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.UseW", "Use W").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.UseR", "Use R").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AntiGapClose", "AntiGapClosers").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AG.UseW", "AntiGapClose with W").SetValue(true));
            Interrupter.AddItem(new MenuItem("Interrupter.AG.UseR", "AntiGapClose with R").SetValue(true));
            Config.AddSubMenu(Interrupter);

            Menu BlackList = new Menu("Ultimate BlackList", "BlackList", false);
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                BlackList.AddItem(new MenuItem("Blacklist." + hero.ChampionName, hero.ChampionName, false).SetValue(false));
            }
            Config.AddSubMenu(BlackList);

            Menu Misc = new Menu("Misc", "Misc", false);
            Misc.AddItem(new MenuItem("Misc.PrioritizeUnderTurret", "Prioritize Targets Under Turret", false).SetValue(true));
            Misc.AddItem(new MenuItem("Misc.DontETurret", "Don't 2nd E Turret Range", false).SetValue(true));
            Misc.AddItem(new MenuItem("Misc.EMouse", "E to Mouse Key", false).SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            Misc.AddItem(new MenuItem("spaceholder", ""));
            Misc.AddItem(new MenuItem("Hitchance.Q", "Q Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));
            Misc.AddItem(new MenuItem("Hitchance.E", "E Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));
            Misc.AddItem(new MenuItem("Misc.Debug", "Debug", false).SetValue(false));
            Config.AddSubMenu(Misc);

            Menu Drawings = new Menu("Drawings", "Drawing", false);

            var dmgAfterE = new MenuItem("DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill =
                new MenuItem("DrawColour", "Fill colour", true).SetValue(
                    new Circle(true, Color.Goldenrod));
            Drawings.AddItem(drawFill);
            Drawings.AddItem(dmgAfterE);

            DamageIndicator.DamageToUnit = Lissandra.GetAvailableDamage;
            DamageIndicator.Enabled = dmgAfterE.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged +=
                delegate (object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Drawings.AddItem(new MenuItem("Drawing.DrawQ", "Draw Q").SetValue(new Circle(false, Color.White)));
            Drawings.AddItem(new MenuItem("Drawing.DrawW", "Draw W").SetValue(new Circle(false, Color.Green)));
            Drawings.AddItem(new MenuItem("Drawing.DrawE", "Draw E").SetValue(new Circle(false, Color.RoyalBlue)));
            Drawings.AddItem(new MenuItem("Drawing.DrawR", "Draw R").SetValue(new Circle(false, Color.Red)));

            Config.AddSubMenu(Drawings);

            return Config;
        }
    }
}
