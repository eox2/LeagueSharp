using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

// This assembly was an experimental one to get use the LeagueSharp api. There is better assemblies for Elise out there. Since I don't play Elise anymore and she is weak, I will not work on improving this unless i have nothing better to do. 

namespace SephElise
{
    internal class Program
    {
        private const string ChampionName = "Elise";
        private static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, QS, WS, ES;
        public static float Qrange, Wrange, Erange;

        private static Menu Config;
        private static SpellSlot IgniteSlot;
        private static Obj_AI_Hero Player;
        private static bool HumanForm;
        private static bool SpiderForm;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 950f);
            E = new Spell(SpellSlot.E, 1100f);

            QS = new Spell(SpellSlot.Q, 475f);
            WS = new Spell(SpellSlot.W, 0);
            ES = new Spell(SpellSlot.E, 750f);
            R = new Spell(SpellSlot.R, 0);
            W.SetSkillshot(0.25f, 100f, 1000, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 55f, 1450, true, SkillshotType.SkillshotLine);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");


            Config = new Menu("SephElise", "Elise", true);


            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQHuman", "Use Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWHuman", "Use W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEHuman", "Use E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQSpider", "Use Q Spider")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWSpider", "Use W Spider")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseESpider", "Use E Spider")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboMode", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));


            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassMode", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0],
                        KeyBindType.Press)));

            //Farm
            Config.AddSubMenu(new Menu("Farm/Clear", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q (Spider)")).SetValue(true);
            Config.SubMenu("Farm").AddItem(new MenuItem("UseSpiderEFarm", "Use E (Spider)")).SetValue(false);
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W (Spider)")).SetValue(true);
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FarmMode", "Farm Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Kill Steal
            Config.AddSubMenu(new Menu("KillSteal", "Ks"));
            Config.SubMenu("Ks").AddItem(new MenuItem("KSMode", "Use KillSteal")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseQKsSpider", "Use Q (Spider)")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseWKsSpider", "Use W (Spider)")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEKsSpider", "Use E (Spider)")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);

            Config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("<font color='#1d87f2'>SephElise has been Loaded.</font>");
        }


        private static void OnDraw(EventArgs args) 
        {
            if (Config.Item("DrawQ").GetValue<bool>()) 
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Qrange, System.Drawing.Color.White);
            }
            if (Config.Item("DrawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Wrange, System.Drawing.Color.Red);
            }
            if (Config.Item("DrawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Erange, System.Drawing.Color.Blue);
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(true);
            CheckForm();

            if (Config.Item("ComboMode").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("HarassMode").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (Config.Item("FarmMode").GetValue<KeyBind>().Active)
            {
                Farm();
                JungleFarm();
            }
            if (Config.Item("KSMode").GetValue<bool>())
            {
                KillSteal();
            }
        }

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (target != null)
            {
                if (HumanForm)
                {
                    if (Player.Distance(target) <= Q.Range && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast(target);
                    }

                    if (Player.Distance(target) <= W.Range && Config.Item("UseWHarass").GetValue<bool>() &&
                        W.IsReady())
                    {
                        W.Cast(target);
                    }
                }
            }
        }


        private static void JungleFarm()
        {
            List<Obj_AI_Base> mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);

            if (Config.Item("UseQFarm").GetValue<bool>())
            {
                foreach (Obj_AI_Base minion in mobs)
                    if (HumanForm)
                    {
                        if (QS.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= Q.Range)
                        {
                            Q.Cast(minion);
                        }
                        if (W.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= W.Range)
                        {
                            W.Cast();
                        }
                        if (!Q.IsReady() && !W.IsReady())
                        {
                            R.Cast();
                        }
                    }
                if (!HumanForm)
                {
                    foreach (Obj_AI_Base minion in mobs)
                    {
                        if (QS.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= QS.Range)
                        {
                            QS.Cast(minion);
                        }
                        if (WS.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= WS.Range)
                        {
                            WS.Cast();
                        }
                        if (ES.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= ES.Range &&
                            Config.Item("UseSpiderEFarm").GetValue<bool>())
                        {
                            ES.Cast(minion);
                        }
                    }
                }
            }
        }

        private static void Farm()
        {
            List<Obj_AI_Base> allminions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Config.Item("UseQFarm").GetValue<bool>())
            {
                foreach (Obj_AI_Base minion in allminions) { 
                    if (HumanForm)
                    {
                        if (QS.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= Q.Range)
                        {
                            Q.Cast(minion);
                        }
                        if (W.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= W.Range)
                        {
                            W.Cast();
                        }
                        R.Cast();
                    }
                if (!HumanForm)
                {
                    if (QS.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= QS.Range)
                    {
                        QS.Cast(minion);
                    }
                    if (WS.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= WS.Range)
                    {
                        WS.Cast();
                    }
                    if (ES.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= ES.Range &&
                        Config.Item("UseSpiderEFarm").GetValue<bool>())
                    {
                        ES.Cast(minion);
                    }
                }
            }
        }
        }


        private static void KillSteal()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            double igniteDmg = Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            double QHDmg = Player.GetSpellDamage(target, SpellSlot.Q, 0);
            double QSDmg = Player.GetSpellDamage(target, SpellSlot.Q, 1);
            double WHDmg = Player.GetSpellDamage(target, SpellSlot.W);
            double WSDmg = Player.GetSpellDamage(target, SpellSlot.W, 1);

            if (target != null && Config.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (igniteDmg >= target.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (Q.IsReady() && Player.Distance(target) <= Q.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>() && !SpiderForm)
            {
                if (target.Health <= QHDmg)
                {
                    Q.Cast(target);
                }
            }
            if (QS.IsReady() && Player.Distance(target) <= QS.Range && target != null &&
                Config.Item("UseQKs").GetValue<bool>() && SpiderForm)
            {
                if (target.Health <= QSDmg)
                {
                    Q.Cast(target);
                }
            }
            if (W.IsReady() && Player.Distance(target) <= W.Range && target != null &&
                Config.Item("UseWKs").GetValue<bool>() && HumanForm)
            {
                if (target.Health <= WHDmg)
                {
                    W.Cast(target);
                }
            }

            if (W.IsReady() && Player.Distance(target) <= WS.Range && target != null &&
             Config.Item("UseWKs").GetValue<bool>() && SpiderForm)
            {
                if (target.Health <= WSDmg)
                {
                    W.Cast(target);
                }
            }

        }


        private static void CheckForm()
        {
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseHumanW" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseHumanE")
            {
              
                HumanForm = true;
                SpiderForm = false;
                Qrange = Q.Range;
                Wrange = W.Range;
                Erange = E.Range;

                // Game.PrintChat("We are in Human form.");
            }

            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseSpiderQCast" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseSpiderW" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseSpiderEInitial")
            {
                //Game.PrintChat("We are in Spider form.");
              //  float QScd, WScd, EScd;
              //  QScd = (Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown);
               // WScd = (Player.Spellbook.GetSpell(SpellSlot.W).Cooldown);
              //  EScd = (Player.Spellbook.GetSpell(SpellSlot.E).Cooldown);

                HumanForm = false;
                SpiderForm = true;
                Qrange = QS.Range;
                Wrange = WS.Range;
                Erange = ES.Range;
            }
        }


        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            Orbwalker.SetAttack((!Q.IsReady() || !E.IsReady() || !W.IsReady()));

            if (target != null)
            {
                if (HumanForm)
                {
                    // Human Rotation
                    if (Player.Distance(target) <= Q.Range && Config.Item("UseQHuman").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast(target);
                    }
                    if (Player.Distance(target) <= W.Range && Config.Item("UseWHuman").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast(target);
                    }
                    if (Player.Distance(target) <= E.Range && Config.Item("UseEHuman").GetValue<bool>() && E.IsReady())
                    {
                        var pred = E.GetPrediction(target);
                        E.Cast(pred.CastPosition);
                    }
                    if (!Q.IsReady() && !W.IsReady() && !E.IsReady() && Player.Distance(target) <= 750 &&
                        Config.Item("UseR").GetValue<bool>())
                    {
                        R.Cast();
                    }
                    if (!Q.IsReady() && !W.IsReady() && Player.Distance(target) <= 750 &&
                        Config.Item("UseQHuman").GetValue<bool>())
                    {
                        R.Cast();
                    }
                }
                // Spider Rotation
                if (SpiderForm)
                {
                    if (Player.Distance(target) <= QS.Range && Config.Item("UseQSpider").GetValue<bool>() &&
                        QS.IsReady())
                    {
                        QS.Cast(target);
                    }
                    if (Player.Distance(target) <= 140 && Config.Item("UseWSpider").GetValue<bool>() && WS.IsReady())
                    {
                        WS.Cast();
                    }
                    if (Player.Distance(target) <= ES.Range && Player.Distance(target) > QS.Range &&
                        Config.Item("UseESpider").GetValue<bool>() && ES.IsReady())
                    {
                        ES.Cast(target);
                    }
                    if (Player.Distance(target) > QS.Range && !ES.IsReady() && R.IsReady() &&
                        Player.Distance(target) <= 1075 && Config.Item("UseR").GetValue<bool>())
                    {
                        R.Cast();
                    }
                    if (!QS.IsReady() && Player.Distance(target) >= 125 && !ES.IsReady() && R.IsReady() &&
                        Player.Distance(target) <= 1075 && Config.Item("UseR").GetValue<bool>())
                    {
                        R.Cast();
                    }
                    if (ES.IsReady() && Player.Distance(target) > QS.Range && Config.Item("UseESpider").GetValue<bool>())
                    {
                        ES.Cast(target);
                    }
                }
            }
        }

 
    }
}