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
        private static Spell Q, W, E, R;
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
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly))
            {
                HealManager.AddItem(new MenuItem("heal" + hero.ChampionName, "Heal " + hero.ChampionName).SetValue(true));
                HealManager.AddItem(new MenuItem("hpct" + hero.ChampionName, "Health % " + hero.ChampionName).SetValue(new Slider(35, 0, 100)));
            }

            // UltimateManager Options
            Menu UltimateManager = new Menu("UltManager", "Ultimate Manager");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly))
            {
                UltimateManager.AddItem(new MenuItem("ult" + hero.ChampionName, "Ultimate " + hero.ChampionName).SetValue(true));
                UltimateManager.AddItem(new MenuItem("upct" + hero.ChampionName, "Health % " + hero.ChampionName).SetValue(new Slider(25, 0 , 100)));
            }

            // Misc Options
            Menu Misc = new Menu("Misc", "Misc");
            Misc.AddItem(new MenuItem("UseElh", "Use E to lasthit").SetValue(true));
            Misc.AddItem(new MenuItem("Healingon", "Healing On").SetValue(true));
            Misc.AddItem(new MenuItem("Ultingon", "Ulting On").SetValue(true)); 

            //TODO Add Drawings


            // Add to Main Menu
            Config.AddSubMenu(targetselector);
            Config.AddSubMenu(Combo);
            Config.AddSubMenu(WaveClear);
            Config.AddSubMenu(Farm);
            Config.AddSubMenu(HealManager);
            Config.AddSubMenu(UltimateManager);
            Config.AddSubMenu(Misc);
            Config.AddToMainMenu();

        }

        static void OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.ChampionName != "Kayle")
            {
                return;
            }
            Game.PrintChat("Kayle -  The Judicator -- By Seph -- Loaded");
            CreateMenu();
            DefineSpells();
           
            Game.OnGameUpdate += GameTick;
            Game.OnGameUpdate += HealUlt;
          //  Obj_AI_Hero.OnProcessSpellCast += HealUltTrigger;
        }

        private static void HealUlt(EventArgs args)
        {
            HealUltManager();
        }

        private static bool Eon
        {
            get { return ObjectManager.Player.AttackRange == 500f; }
        }

        private static Object GetSettings(string itemname, bool isbool = false, bool isslider = false)
        {
            if (isbool)
            {
                return Config.Item(itemname).GetValue<bool>();
            }
            if (isslider)
            {
                return Config.Item(itemname).GetValue<Slider>().Value;
            }
            return null;
        }


        private static void Combo()
        {
            if (Player.IsDead)
            {
                return;
            }

            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var etarget = TargetSelector.GetTarget(incrange, TargetSelector.DamageType.Magical);

            if ((bool) GetSettings("UseQ", true) && qtarget != null && Q.IsReady())
            {
                Q.Cast(qtarget);
            }
            if ((bool) GetSettings("UseE", true) && etarget != null && E.IsReady() && !Eon)
            {
                E.CastOnUnit(Player);
            }

        }

        private static void WaveClear()
        {
            if (Player.IsDead)
            {
                return;
            }
            
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && Player.Distance(m) <= incrange);
            if (minions.Any() && Config.Item("UseEwc").GetValue<bool>() && E.IsReady())
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
                            HealthPrediction.GetHealthPrediction(
                                minion, (int) (ObjectManager.Player.Distance(minion)*1000/1400)) <
                            0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
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

        /*

        static void HealUltTrigger(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || sender.IsMinion)
            {
                return;
            }

            if (sender.Type != GameObjectType.obj_AI_Hero || !sender.IsChampion(sender.Name))
            {
                Game.PrintChat("return");
                return;
            }
    
             var target = (Obj_AI_Hero) args.Target;
             if (sender.IsEnemy && args.Target.IsAlly && ((bool) GetSettings("heal" + target.ChampionName, true) || (bool) GetSettings("ult" + target.ChampionName, true)))
            {
                var incomingspelldmg = sender.GetSpellDamage(target, args.SData.Name);
                var spelldmg = sender.GetDamageSpell(target, args.SData.Name);
                var calcdmg  = Damage.CalcDamage(
                        sender, target, spelldmg.DamageType, incomingspelldmg);

              var percenthealthsetheal = (float)GetSettings("hpct" + target.ChampionName, false, true) / 100f;
              var percenthealthsethult = (float)GetSettings("upct" + target.ChampionName, false, true) / 100f;
              var pctafterdmg = (target.Health - calcdmg) / 100f;
                if (target.HealthPercentage() <= percenthealthsetheal || target.HealthPercentage() <= pctafterdmg)
                {
                    Game.PrintChat("helth low heal");
                    if (W.IsReady() && (bool) GetSettings("heal" + target.ChampionName, true))
                    {
                        Game.PrintChat("Activating heal");
                        HealUltManager(true, false, target);
                    }
                }
                if (target.HealthPercentage() <= percenthealthsethult || target.HealthPercentage() <= pctafterdmg)
                { 
                    Game.PrintChat("helth low ult");
                   if (R.IsReady() && (bool) GetSettings("ult" + target.ChampionName, true))
                  {
                      Game.PrintChat("Activating ult");
                      HealUltManager(false, true, target);
                  }
              }

                 
                if ((bool) GetSettings("heal" + target.ChampionName, true))
                {
                 Game.PrintChat(calcdmg.ToString());
                   Console.WriteLine(calcdmg / target.Health);
                   if (calcdmg/target.Health >= ((float) GetSettings("hpct" + target.Name, false, true)/100f) &&
                    W.IsReady())
                    {
                        Game.PrintChat("Activating heal");
                        HealUltManager(true, false, target);
                    }
                }

                if ((bool) GetSettings("ult" + target.ChampionName, true) && calcdmg / target.Health >= ((float) GetSettings("upct" + target.Name, false, true) / 100f) && R.IsReady())
                {
                    Game.PrintChat("Activating ult");
                    HealUltManager(false, true, target);
                }
                 
            }
    
        }
        */

        static void HealUltManager(bool forceheal = false, bool forceult = false, Obj_AI_Hero target = null)
        {
            if (!W.IsReady() && !R.IsReady())
            {
                return;
            }
            if (forceheal && target != null && W.IsReady() && Player.Distance(target) <= W.Range)
            {
                W.CastOnUnit(target);
                return;
            }
            if (forceult && target != null && R.IsReady() && Player.Distance(target) <= R.Range)
            {
                R.CastOnUnit(target);
                return;
            }
            //I killed LINQ guys... 

            if ((bool) GetSettings("Healingon", true))
            {
            var herolistheal = ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                h =>
                                    (h.IsAlly || h.IsMe) && !h.IsZombie && !h.IsDead && (bool) GetSettings("heal" + h.ChampionName, true) &&
                                    h.HealthPercentage() <= (int) GetSettings("hpct" + h.ChampionName, false, true) && Player.Distance(h) <= R.Range).OrderByDescending(i => i == Player).ThenBy(i => i);
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

                if ((bool) GetSettings("Ultingon", true))
                {

                    var herolist = ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            h =>
                                (h.IsAlly || h.IsMe) && !h.IsZombie && !h.IsDead &&
                                (bool) GetSettings("ult" + h.ChampionName, true) &&
                                h.HealthPercentage() <= (int) GetSettings("upct" + h.ChampionName, false, true) &&
                                Player.Distance(h) <= R.Range).OrderByDescending(i => i == Player).ThenBy(i => i);

                    if (R.IsReady())
                    {
                        if (herolist.Contains(Player))
                        {
                            R.CastOnUnit(Player);
                            return;
                        }

                        else if (herolist.Any())
                        {
                            var hero = herolist.FirstOrDefault();

                            if (Player.Distance(hero) <= R.Range)
                            {
                                R.CastOnUnit(hero);
                                return;
                            }
                        }
                    }
                }


            }

  

        static void GameTick(EventArgs args)
        {
            HealUltManager();
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
            /*
            var minio = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsEnemy && Player.Distance(m) <= incrange);
            if (minio.Any())
            {
                E.CastOnUnit(Player);
            }
            */

            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in
                    allMinions.Where(
                        minion =>
                            minion.IsValidTarget() &&
                            HealthPrediction.GetHealthPrediction(
                                minion, (int) (ObjectManager.Player.Distance(minion)*1000/1400)) <
                            0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && Player.Distance(minion) <= Q.Range)
                    {
                        Orbwalker.SetAttack(false);
                        Q.CastOnUnit(minion, false);
                        Orbwalker.SetAttack(true);
                        if (Config.Item("UseEFarm").GetValue<bool>())
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
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && Player.Distance(m) <= incrange);
            if (minions.Any())
            {
                E.CastOnUnit(Player);
            }

            var Targ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Q.IsReady() && (bool) GetSettings("UseQH", true) && Player.Distance(Targ) <= Q.Range)
            {
                Q.Cast(Targ);
            }

            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in
                    allMinions.Where(
                        minion =>
                            minion.IsValidTarget() &&
                            HealthPrediction.GetHealthPrediction(
                                minion, (int) (ObjectManager.Player.Distance(minion)*1000/1400)) <
                            0.75*Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && Player.Distance(minion) <= Q.Range)
                    {
                        Orbwalker.SetAttack(false);
                        Q.CastOnUnit(minion, false);
                        Orbwalker.SetAttack(true);
                        if (Config.Item("UseEFarm").GetValue<bool>())
                        {
                            E.CastOnUnit(Player);
                        }
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
        }


    }

}
