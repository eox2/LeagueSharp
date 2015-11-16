using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;

namespace SephKayle
{
    class Program
    {
        private static Menu Config;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player;
        private static float incrange = 525;
        private static Spell Q, W, E, R, Ignite;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void CreateMenu()
        {
         
            Config = new Menu("SephKayle", "SephKayle", true);
            Menu OWMenu = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            //OrbWalker
            Orbwalker = new Orbwalking.Orbwalker(OWMenu);

            //TargetSelector
            Menu targetselector = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetselector);

            // Combo Options
            Menu Combo = new Menu("Combo", " Combo");
            Combo.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Combo.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Combo.AddItem(new MenuItem("UseR", "Use R").SetValue(true));

            // Waveclear Options
            Menu WaveClear = new Menu("Waveclear", "Waveclear");
            WaveClear.AddItem(new MenuItem("UseQwc", "Use Q").SetValue(true));
            WaveClear.AddItem(new MenuItem("UseEwc", "Use E").SetValue(true));

            // Farm Options
            Menu Farm = new Menu("Farm", "Farm");
            Farm.AddItem(new MenuItem("UseQfarm", "Use Q").SetValue(true));
            Farm.AddItem(new MenuItem("UseEfarm", "Use E").SetValue(true));

            // HealManager Options
            Menu HealManager = new Menu("HealManager", "Heal Manager");
            HealManager.AddItem(new MenuItem("onlyhincdmg", "Only heal if incoming damage").SetValue(false));
            HealManager.AddItem(new MenuItem("hdamagedetection", "Disable damage detection").SetValue(false));
            HealManager.AddItem(new MenuItem("hcheckdmgafter", "Take HP after damage into consideration").SetValue(true));

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly))
            {
                HealManager.AddItem(new MenuItem("heal" + hero.ChampionName, "Heal " + hero.ChampionName).SetValue(true));
                HealManager.AddItem(new MenuItem("hpct" + hero.ChampionName, "Health % " + hero.ChampionName).SetValue(new Slider(35, 0, 100)));
            }

            // UltimateManager Options
            Menu UltimateManager = new Menu("UltManager", "Ultimate Manager");
            UltimateManager.AddItem(new MenuItem("onlyuincdmg", "Only ult if incoming damage").SetValue(true));
            UltimateManager.AddItem(new MenuItem("udamagedetection", "Disable damage detection").SetValue(false));
            UltimateManager.AddItem(new MenuItem("ucheckdmgafter", "Take HP after damage into consideration").SetValue(true));

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly))
            {
                UltimateManager.AddItem(new MenuItem("ult" + hero.ChampionName, "Ultimate " + hero.ChampionName).SetValue(true));
                UltimateManager.AddItem(new MenuItem("upct" + hero.ChampionName, "Health % " + hero.ChampionName).SetValue(new Slider(25, 0 , 100)));
            }

            // Misc Options
            Menu Misc = new Menu("Misc", "Misc");
            Misc.AddItem(new MenuItem("killsteal", "Killsteal").SetValue(true));
            Misc.AddItem(new MenuItem("UseElh", "Use E to lasthit").SetValue(true));
            Misc.AddItem(new MenuItem("Healingon", "Healing On").SetValue(true));
            Misc.AddItem(new MenuItem("Ultingon", "Ulting On").SetValue(true));
            Misc.AddItem(new MenuItem("Recallcheck", "Recall check").SetValue(false));
            Misc.AddItem(new MenuItem("Debug", "Debug On").SetValue(true));

            Menu Drawing = new Menu("Drawing", "Drawing");
            Drawing.AddItem(new MenuItem("disableall", "Disable all").SetValue(true));
            Drawing.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            Drawing.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            Drawing.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            Drawing.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));

            // Add to Main Menu
            Config.AddSubMenu(targetselector);
            Config.AddSubMenu(Combo);
            Config.AddSubMenu(WaveClear);
            Config.AddSubMenu(Farm);
            Config.AddSubMenu(HealManager);
            Config.AddSubMenu(UltimateManager);
            Config.AddSubMenu(Misc);
            Config.AddSubMenu(Drawing);
            Config.AddToMainMenu();
        }

        private static bool debug()
        {
            return GetBool("Debug");
        }

        static void OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.CharData.BaseSkinName != "Kayle")
            {
                return;
            }
            Game.PrintChat("SephKayle Loaded");
            CreateMenu();
            DefineSpells();
            Game.OnUpdate += GameTick;
            Obj_AI_Base.OnProcessSpellCast += HealUltTrigger;
            Drawing.OnDraw += OnDraw;
        }

        static void OnDraw(EventArgs args)
        {
            if (GetBool("disableall"))
            {
                return;
            }

            if (GetBool("DrawQ"))
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Aqua);
            }
            if (GetBool("DrawW"))
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Azure);
            }
            if (GetBool("DrawE"))
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Crimson);
            }
            if (GetBool("DrawR"))
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Red);
            }
        }

        private static void KillSteal()
        {
            var target = ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsInvulnerable && !x.IsDead && x.IsEnemy && !x.IsZombie && x.IsValidTarget() && x.Distance(Player.Position) <= 800)
                .OrderBy(x => x.Health).FirstOrDefault();
            double igniteDmg = Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            double QDmg = Player.GetSpellDamage(target, SpellSlot.Q);
            var totalksdmg = igniteDmg + QDmg;
            
            if (target.Health <= QDmg && Player.Distance(target) <= Q.Range)
            {
                Q.CastOnUnit(target);
            }
            if (target.Health <= igniteDmg && Player.Distance(target) <= Ignite.Range)
            {
                Player.Spellbook.CastSpell(Ignite.Slot, target);
            }
            if (target.Health <= totalksdmg && Player.Distance(target) <= Q.Range)
            {
                Q.CastOnUnit(target);
                Player.Spellbook.CastSpell(Ignite.Slot, target);
            }
        }

        private static bool Eon
        {
            get { return ObjectManager.Player.AttackRange > 400f; }
        }


        private static bool GetBool(String itemname)
        {
            return Config.Item(itemname).GetValue<bool>();
        }

        private static int Getslider(String itemname)
        {
            return Config.Item(itemname).GetValue<Slider>().Value;
        }

        private static void Combo()
        {
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var etarget = TargetSelector.GetTarget(incrange, TargetSelector.DamageType.Magical);

            if (GetBool("UseQ") && qtarget != null && Q.IsReady())
            {
                Q.Cast(qtarget);
            }
            if (GetBool("UseE") && etarget != null && E.IsReady() && !Eon)
            {
                E.CastOnUnit(Player);
            }

        }

        private static void WaveClear()
        {
     
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && Player.Distance(m) <= incrange);
            if (minions.Any() && Config.Item("UseEwc").GetValue<bool>() && E.IsReady() && !Eon)
            {
                E.CastOnUnit(Player);
            }
            
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Config.Item("UseQwc").GetValue<bool>() && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in
                   allMinions.Where(
               minion =>
                   minion.IsValidTarget() &&
                   HealthPrediction.GetHealthPrediction(minion, (int)((Player.Distance(minion) * 1000) / 1500) + 300 + Game.Ping / 2) <
                   0.75 * Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && Player.Distance(minion) <= Q.Range)
                    {
                        Orbwalker.SetAttack(false);
                        Q.CastOnUnit(minion);
                        Orbwalker.SetAttack(true);
                        return;
                    }
                }
            }
        }


        static Obj_AI_Hero GetHero(string Name)
        {
            var hero = ObjectManager.Get<Obj_AI_Hero>().First(h => h.Name == Name);
            if (hero != null)
            {
                return hero;
            }
            return null;
        }

        static void HealUltTrigger(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (GetBool("Recallcheck") && Player.IsRecalling())
            {
                return;
            }

            var target = args.Target as Obj_AI_Hero;
            var senderhero = sender as Obj_AI_Hero;
            var senderturret = sender as Obj_AI_Turret;

            if (sender.IsAlly || (target == null) || !target.IsAlly)
            {
                return;
            }
            float setvaluehealth = Getslider("hpct" + target.ChampionName);
            float setvalueult = Getslider("upct" + target.ChampionName);

            bool triggered = false;

            if (W.IsReady() && GetBool("heal" + target.ChampionName) && (target.HealthPercent <= setvaluehealth))
            {
                HealUltManager(true, false, target);
                triggered = true;
            }
            if (R.IsReady() && GetBool("ult" + target.ChampionName) && (target.HealthPercent <= setvalueult) && target.Distance(Player) <= R.Range)
            {
                if (args.SData.Name.ToLower().Contains("minion") && target.HealthPercent > 5)
                {
                    return;
                }
                if (debug())
                {
                    Game.PrintChat("Ult target: " + target.ChampionName +" Ult reason: Target hp percent below set value of: " + setvalueult + " Current value is: " + target.HealthPercent + " Triggered by: Incoming spell: + " + args.SData.Name);
                }
                HealUltManager(false, true, target);
                triggered = true;
            }

            if (triggered)
            {
                return;
            }

                var damage = sender.GetSpellDamage(target, args.SData.Name);
                var afterdmg = ((target.Health - damage) / (target.MaxHealth)) * 100f;

                if (W.IsReady() && Player.Distance(target) <= W.Range && GetBool("heal" + target.ChampionName) && (target.HealthPercent <= setvaluehealth || (GetBool("hcheckdmgafter") && afterdmg <= setvaluehealth)))
            {
                if (GetBool("hdamagedetection")) {
                    HealUltManager(true, false, target);
                }
            }

            if (R.IsReady() && Player.Distance(target) <= R.Range && GetBool("ult" + target.ChampionName) && (target.HealthPercent <= setvalueult || (GetBool("ucheckdmgafter") && afterdmg <= setvalueult)) && (senderhero != null || senderturret != null || target.HealthPercent < 5f))
            {
                if (GetBool("udamagedetection"))
                {
                    if (args.SData.Name.ToLower().Contains("minion") && target.HealthPercent > 5)
                    {
                        return;
                    }
                    if (debug())
                    {
                        if (afterdmg <= setvalueult)
                        {
                            Game.PrintChat("Ult target: " + target.ChampionName + " Ult reason: Incoming spell damage will leave us below set value of " + setvalueult + " Current value is: " + target.HealthPercent + " and after spell health left is: " + afterdmg + " Triggered by: Incoming spell: + " + args.SData.Name);
                        }

                        else
                        {
                         Game.PrintChat("Ult target: " + target.ChampionName + " Ult reason: Incoming spell damage and health below set value of " + setvalueult + " Current value is: " + target.HealthPercent + " Triggered by: Incoming spell: + " + args.SData.Name);
                        }
                    }
                    HealUltManager(false, true, target);
                }
            }
        }


        static void HealUltManager(bool forceheal = false, bool forceult = false, Obj_AI_Hero target = null)
        {
            if (forceheal && target != null && W.IsReady() && Player.Distance(target) <= W.Range)
            {
                W.CastOnUnit(target);
                return;
            }
            if (forceult && target != null && R.IsReady() && Player.Distance(target) <= R.Range)
            {
                if (debug())
                {
                    Game.PrintChat("Forceult");
                }
                R.CastOnUnit(target);
                return;
            }

            if (GetBool("Healingon") && !GetBool("onlyhincdmg"))
            {
                var herolistheal = ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                            (h.IsAlly || h.IsMe) && !h.IsZombie && !h.IsDead && GetBool("heal" + h.ChampionName) &&
                            h.HealthPercent <= Getslider("hpct" + h.ChampionName) && Player.Distance(h) <= R.Range)
                    .OrderByDescending(i => i.IsMe)
                    .ThenBy(i => i.HealthPercent);
                
                if (W.IsReady())
                {
                    if (herolistheal.Contains(Player) && !Player.IsRecalling() && !Player.InFountain())
                    {
                        W.CastOnUnit(Player);
                        return;
                    }
                    else if (herolistheal.Any())
                    {
                        var hero = herolistheal.FirstOrDefault();

                        if (Player.Distance(hero) <= R.Range && !Player.IsRecalling() && !hero.IsRecalling() && !hero.InFountain())
                        {
                            W.CastOnUnit(hero);
                            return;
                        }
                    }
                }
            }

            if (GetBool("Ultingon") && !GetBool("onlyuincdmg"))
                {
                    Console.WriteLine(Player.HealthPercent);
                    var herolist = ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            h =>
                                (h.IsAlly || h.IsMe) && !h.IsZombie && !h.IsDead &&
                                 GetBool("ult" + h.ChampionName) &&
                                h.HealthPercent <= Getslider("upct" + h.ChampionName) &&
                                Player.Distance(h) <= R.Range && Player.CountEnemiesInRange(500) > 0).OrderByDescending(i => i.IsMe).ThenBy(i => i.HealthPercent);

                    if (R.IsReady())
                    {
                        if (herolist.Contains(Player))
                        {
                        if (debug())
                        {
                            Game.PrintChat("regultself");
                        }
                        R.CastOnUnit(Player);
                            return;
                        }

                        else if (herolist.Any())
                        {
                            var hero = herolist.FirstOrDefault();

                            if (Player.Distance(hero) <= R.Range)
                            {
                            if (debug())
                            {
                                Game.PrintChat("regultotherorself");
                            }
                            R.CastOnUnit(hero);
                                return;
                            }
                        }
                    }
                }
            }

        static void GameTick(EventArgs args)
        {
            if (Player.IsDead || GetBool("Recallcheck") && Player.IsRecalling())
            {
                return;
            }

            if (!Config.Item("onlyhincdmg").GetValue<bool>() || !Config.Item("onlyuincdmg").GetValue<bool>())
            {
                HealUltManager();
            }

            if (!Config.Item("killsteal").GetValue<bool>())
            {
                KillSteal();
            }

            var Orbwalkmode = Orbwalker.ActiveMode;
            switch(Orbwalkmode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    WaveClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    MixedLogic();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LHlogic();
                    break;
            }
        }



        private static void LHlogic()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Config.Item("UseQfarm").GetValue<bool>() && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in
                    allMinions.Where(
                        minion =>
                            minion.IsValidTarget() &&
                            HealthPrediction.GetHealthPrediction(minion, (int) ((Player.Distance(minion)*1000)/1500) + 300 + Game.Ping / 2) <
                            0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && Player.Distance(minion) <= Q.Range)
                    {
                        Orbwalker.SetAttack(false);
                        Q.CastOnUnit(minion, false);
                        Orbwalker.SetAttack(true);
                        if (Config.Item("UseEfarm").GetValue<bool>() && E.IsReady() && !Eon)
                        {
                            E.CastOnUnit(Player);
                        }
                        return;
                    }
                }

                //TODO Better Calculations + More Logic for E activation
            }
        }

        private static void MixedLogic()
        {
            if (Config.Item("UseEfarm").GetValue<bool>())
            {
                var minions = ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && Player.Distance(m) <= incrange);
                if (minions.Any() && E.IsReady() && Config.Item("UseEfarm").GetValue<bool>() && !Eon)
                {
                    E.CastOnUnit(Player);
                }
            }

            var Targ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Targ != null && Q.IsReady() && GetBool("UseQ") && Player.Distance(Targ) <= Q.Range)
            {
                Q.Cast(Targ);
            }

            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Config.Item("UseQfarm").GetValue<bool>() && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in
                allMinions.Where(
               minion =>
                   minion.IsValidTarget() &&
                   HealthPrediction.GetHealthPrediction(minion, (int)((Player.Distance(minion) * 1000) / 1500) + 300 + Game.Ping / 2) <
                   0.75 * Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && Player.Distance(minion) <= Q.Range)
                    {
                        Orbwalker.SetAttack(false);
                        Q.CastOnUnit(minion, false);
                        Orbwalker.SetAttack(true);
                        return;
                    }
                }
            }

            //TODO Better Calculations + More Logic for E activation
        }


        static void DefineSpells()
        {
           Q = new Spell(SpellSlot.Q, 650);
           W = new Spell(SpellSlot.W, 900);
           E = new Spell(SpellSlot.E, 0);
           R = new Spell(SpellSlot.R, 900);
           SpellDataInst ignite = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("summonerdot"));
           if (ignite.Slot != SpellSlot.Unknown)
           {
               Ignite = new Spell(ignite.Slot, 600);
           }
        }
    }
}
