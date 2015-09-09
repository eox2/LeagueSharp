using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Version = System.Version;

namespace SephLissandra
{
     static class Lissandra
        {

        /* To Do
         * Split into different classes
         * April 12 2015
         */

        public static Version version = Assembly.GetExecutingAssembly().GetName().Version;
        public static Menu Config;
        public static Obj_AI_Hero Player;
        public static bool jumping;
        private static Vector3 MissilePosition;
        private static MissileClient LissEMissile;

        private static Dictionary<String, Spell> Spells = new Dictionary<String, Spell>
        {
            { "Q", new Spell(SpellSlot.Q, 715f) },
            { "Qtest", new Spell(SpellSlot.Q, 715f) },
            { "Q2", new Spell(SpellSlot.Q, 825f) },
            { "W", new Spell(SpellSlot.W, 450f) },
            { "E", new Spell(SpellSlot.E, 1050f) },
            { "R", new Spell(SpellSlot.R, 550f) },
            { "Ignite", new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600) }
        };


        public static void LissandraMain()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        static void OnLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.CharData.BaseSkinName != "Lissandra")
            {
                return;
            }
            Game.PrintChat("<font color=\"#2EFEC8\"><b>SephLissandra</b> " + version + " by Seph");
            Config = LissMenu.CreateMenu();
            Config.AddToMainMenu();
            DefineSpells();
            Game.OnUpdate += GameTick;
            Game.OnUpdate += MonitorMissilePosition;
            AntiGapcloser.OnEnemyGapcloser += OnGapClose;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Drawing.OnDraw += OnDraw;
        }



        static void DefineSpells()
        {
            Spells["Q"].SetSkillshot(0.250f, 75f, 2200f, false, SkillshotType.SkillshotLine);
            Spells["Qtest"].SetSkillshot(0.250f, 75f, 2200f, true, SkillshotType.SkillshotLine);
            Spells["Q2"].SetSkillshot(0.250f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            Spells["E"].SetSkillshot(0.250f, 125f, 850f, false, SkillshotType.SkillshotLine);
        }


        static void GameTick(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (Config.Item("Misc.EMouse").GetValue<KeyBind>().Active)
            {
                EToMouse(Game.CursorPos);
            }

            if (LissUtils.Active("Killsteal"))
            {
                KillSteal();
            }
            if (LissUtils.ActiveKeyBind("Keys.Combo"))
            {
                ComboHandler();
            }
            if (LissUtils.ActiveKeyBind("Keys.Harass") || LissUtils.ActiveKeyBind("Keys.HarassT"))
            {
                HarassHandler();
            }
            if (LissUtils.ActiveKeyBind("Keys.Farm"))
            {
                FarmHandler();
            }
            if (LissUtils.ActiveKeyBind("Keys.Waveclear"))
            {
                WaveClearHandler();
            }
            
        }

        private static void EToMouse(Vector3 Position)
         {
             if (Spells["E"].IsReady() && LissEMissile == null && !LissUtils.CanSecondE())
             {
                 Spells["E"].Cast(Position);
                 jumping = true;
             }
         }

  
         private static void MonitorMissilePosition(EventArgs args)
         {
             if (LissEMissile == null || Player.IsDead)
             {
                 return;
             }
             MissilePosition = LissEMissile.Position;
             if (jumping)
             {
                 if ((Vector2.Distance(LissEMissile.Position.To2D(), LissEMissile.EndPosition.To2D()) < 40))
                 {
                     Spells["E"].CastOnUnit(Player);
                     jumping = false;
                 }
                 Utility.DelayAction.Add(2000, delegate { jumping = false; });
             }
            
         }


         private static void ComboHandler()
         {
            var Target = TargetSelector.GetTarget(Spells["E"].Range * 0.94f, TargetSelector.DamageType.Magical);

            if (Target == null || !Target.IsValidTarget())
            {
                Target = HeroManager.Enemies.FirstOrDefault(h => h.IsValidTarget() && (Vector3.Distance(h.ServerPosition, Player.ServerPosition) < Spells["E"].Range * 0.94) && !h.IsZombie);
            }

            if (Target != null && !Target.IsInvulnerable)
            {
                if (LissUtils.Active("Combo.UseQ") && SpellSlot.Q.IsReady())
                {
                    CastQ(Target);
                }
                if (LissUtils.Active("Combo.UseW") && SpellSlot.W.IsReady())
                {
                    CastW(Target);
                }
                if (LissUtils.Active("Combo.UseE") && SpellSlot.E.IsReady())
                {
                    CastE(Target);
                }
                if (LissUtils.Active("Combo.UseR") && SpellSlot.R.IsReady() && !Target.IsZombie)
                {
                    CastR(Target);
                }
            }
        }


         static void OnGapClose(ActiveGapcloser args)
         {
             if (Player.IsDead)
             {
                 return;
             }
             var sender = args.Sender;
          
             if (LissUtils.Active("Interrupter.AntiGapClose") && sender.IsValidTarget())
             {
                 if (LissUtils.Active("Interrupter.AG.UseW") && Vector3.Distance(args.End, Player.ServerPosition) <= Spells["W"].Range)
                 {
                     Spells["W"].CastOnUnit(Player);
                     return;
                 }
                 if (LissUtils.Active("Interrupter.AG.UseR") && !LissUtils.Active("Blacklist." + sender.ChampionName) && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells["R"].Range)
                 {
                     Spells["R"].Cast(sender);
                 }
             }
         }


         static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
         {
             if (Player.IsDead)
             {
                 return;
             }
             if (LissUtils.Active("Interrupter") && sender.IsValidTarget())
             {
                 if (LissUtils.Active("Interrupter.UseW") && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells["W"].Range)
                 {
                     Spells["W"].CastOnUnit(Player);
                     return;
                 }
                 if (LissUtils.Active("Interrupter.UseR") && !LissUtils.Active("Blacklist." + sender.ChampionName) && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells["R"].Range)
                 {
                     Spells["R"].Cast(sender);
                 }
             }
         }

         private static void CastQ(Obj_AI_Hero target)
         {
            Dictionary<Vector3, int> maxhit = (from hero in HeroManager.Enemies.Where(h => h.IsValidTarget() && !h.IsInvulnerable && Vector3.Distance(h.ServerPosition, Player.ServerPosition) < Spells["Q2"].Range) select Spells["Q2"].GetPrediction(hero) into prediction where prediction.CollisionObjects.Count > 0 && prediction.Hitchance >= LissUtils.GetHitChance("Hitchance.Q") let enemieshit = prediction.CollisionObjects.Where(x => x is Obj_AI_Hero) select prediction).ToDictionary(prediction => prediction.CastPosition, prediction => prediction.CollisionObjects.Count);

             var bestpair = maxhit.MaxOrDefault(x => x.Value);
             if (bestpair.Value > 0)
             {
                 Vector3 bestpos = bestpair.Key;
                 Spells["Q2"].Cast(bestpos);
                 return;
             }


            var distbw = Vector3.Distance(Player.ServerPosition, target.ServerPosition);

             if (distbw < Spells["Q"].Range)
             {
                 var prediction2 = Spells["Q"].GetPrediction(target);
                 if (prediction2.Hitchance >= LissUtils.GetHitChance("Hitchance.Q"))
                 {
                     Spells["Q"].Cast(prediction2.CastPosition);
                     return;
                 }
             }

             if (distbw > Spells["Qtest"].Range && distbw < Spells["Q2"].Range)
             {
                 var testQ = Spells["Qtest"].GetPrediction(target);
                 var collobjs = testQ.CollisionObjects;
                 if ((testQ.Hitchance == HitChance.Collision || collobjs.Count > 0) && collobjs.All(x => x.IsTargetable))
                 {
                     var pred = Spells["Q2"].GetPrediction(target);
                     if (pred.Hitchance >= LissUtils.GetHitChance("Hitchance.Q"))
                     {
                        Spells["Q2"].Cast(pred.CastPosition);
                    }
                 }
             }
        }

        private static void CastW(Obj_AI_Hero target)
        {
            if (Vector3.Distance(target.ServerPosition, Player.ServerPosition) <= Spells["W"].Range)
            {
                Spells["W"].CastOnUnit(Player);
                return;
            }
             if (HeroManager.Enemies.Any(h => h.IsValidTarget() && (Vector3.Distance(h.ServerPosition, Player.ServerPosition) < Spells["W"].Range) && !h.IsZombie))
            {
                Spells["W"].CastOnUnit(Player);
            }

        }

        private static void CastE(Obj_AI_Hero target)
        {
            if (LissEMissile == null && !LissUtils.CanSecondE())
            {
                var PredManager =
                    HeroManager.Enemies.Where(
                        h =>
                            h.IsValidTarget() && !h.IsZombie &&
                            Vector3.Distance(h.ServerPosition, Player.ServerPosition) <= Spells["E"].Range)
                        .Select(hero => Spells["E"].GetPrediction(hero))
                        .Select(
                            pred =>
                                new Tuple<Vector3, int, HitChance, List<Obj_AI_Hero>>(pred.CastPosition,
                                    pred.AoeTargetsHitCount, pred.Hitchance, pred.AoeTargetsHit));

                var BestLocation = PredManager.MaxOrDefault(x => x.Item4.Count);
                if (BestLocation.Item3 >= LissUtils.GetHitChance("Hitchance.E") && Spells["E"].IsReady())
                {
                    Spells["E"].Cast(BestLocation.Item1);
                }
            }
            SecondEChecker(target);
        }

        //return asap to check the most amount of times 
        static void SecondEChecker(Obj_AI_Hero target)
        {
            if (LissUtils.AutoSecondE() && LissUtils.isHealthy() && LissEMissile != null && Spells["E"].IsReady())
            {
                if (Vector3.Distance(MissilePosition, target.ServerPosition) < Vector3.Distance(Player.ServerPosition, target.ServerPosition) && !LissUtils.PointUnderEnemyTurret(MissilePosition) && Vector3.Distance(target.ServerPosition, LissEMissile.EndPosition) > Vector3.Distance(Player.ServerPosition, target.ServerPosition)) 
                {
                     Spells["E"].CastOnUnit(Player);
                     return;
                }
                    var Enemiesatpoint = LissEMissile.Position.GetEnemiesInRange(Spells["R"].Range);
                    var enemiesatpointR = Enemiesatpoint.Count;

                    if ((enemiesatpointR >= Config.Item("Combo.ecountR").GetValue<Slider>().Value && SpellSlot.R.IsReady()) || (Enemiesatpoint.Any(e => e.IsKillableFromPoint(LissEMissile.Position) && Vector3.Distance(LissEMissile.Position, e.ServerPosition) < Vector3.Distance(Player.ServerPosition, e.ServerPosition))))
                    {
                            if (LissUtils.PointUnderEnemyTurret(LissEMissile.Position) && LissUtils.Active("Misc.DontETurret"))
                            {
                                return;
                            }
                                Spells["E"].CastOnUnit(Player);
                                return;
                    }
                    var enemiesatpointW = LissEMissile.Position.CountEnemiesInRange(Spells["W"].Range);
                    if (enemiesatpointW >= LissUtils.GetSlider("Combo.ecountW") && SpellSlot.W.IsReady())
                    {
                            if (LissUtils.PointUnderEnemyTurret(LissEMissile.Position) && LissUtils.Active("Misc.DontETurret"))
                            {
                                return;
                            }
                                Spells["E"].CastOnUnit(Player);
                    }
            }
        }

        public static bool IsKillableFromPoint(this Obj_AI_Hero target, Vector3 Point, bool ExcludeE = false)
        {
            double totaldmgavailable = 0;
            if (SpellSlot.Q.IsReady() && LissUtils.Active("Combo.UseQ") && Vector3.Distance(Point, target.ServerPosition) < Spells["Q"].Range + 35) 
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (SpellSlot.W.IsReady() && LissUtils.Active("Combo.UseW") && Vector3.Distance(Point, target.ServerPosition) < Spells["W"].Range + 35)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.W);
            }
            if (SpellSlot.E.IsReady() && LissUtils.Active("Combo.UseE") && Vector3.Distance(Point, target.ServerPosition) < Spells["E"].Range + 35 && !LissUtils.CanSecondE() && LissEMissile == null && !ExcludeE)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (SpellSlot.R.IsReady() && LissUtils.Active("Combo.UseR") && Vector3.Distance(Point, target.ServerPosition) < Spells["Q"].Range + 35)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (Spells["Ignite"].IsReady() && LissUtils.Active("Killsteal.UseIgnite") && Vector3.Distance(Point, target.ServerPosition) < Spells["Ignite"].Range + 15)
            {
                totaldmgavailable += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                
            }
            return totaldmgavailable > target.Health;
        }


        static void CastR(Obj_AI_Hero currenttarget)
        {
            var Check =
                HeroManager.Enemies
                    .Where(
                        h => h.IsValidTarget(Spells["R"].Range) && h.CountEnemiesInRange(Spells["R"].Range) >= LissUtils.GetSlider("Combo.Rcount") && !LissUtils.Active("Blacklist." + h.ChampionName)).ToList();

            if (Player.CountEnemiesInRange(Spells["R"].Range) >= LissUtils.GetSlider("Combo.Rcount"))
            {
                Check.Add(Player);
            }
            if (Check != null)
            {
                if (Check.Contains(Player) && !LissUtils.isHealthy())
                {
                    Spells["R"].CastOnUnit(Player);
                    return;
                }
                var target = Check.FirstOrDefault();
                if (target != null)
                {
                    Spells["R"].Cast(target);
                    return;
                }
            }
            if (LissUtils.Active("Blacklist." + currenttarget.ChampionName))
            {
                return;
            }
            if (currenttarget.IsKillableFromPoint(Player.ServerPosition))
            {
                    Spells["R"].Cast(currenttarget);
                    return;
            }


            if (LissUtils.PointUnderAllyTurret(currenttarget.ServerPosition))
            {
                Spells["R"].Cast(currenttarget);
                return;
            }

            var dmgto = Player.GetSpellDamage(currenttarget, SpellSlot.R);
            if (dmgto > currenttarget.Health && currenttarget.Health >= 0.40 * dmgto)
            {
                Spells["R"].Cast(currenttarget);
                return;
            }

            var enemycount = LissUtils.GetSlider("Combo.Rcount");
            if (!LissUtils.isHealthy() && Player.CountEnemiesInRange(Spells["R"].Range - 100) >= enemycount)
            {
                Spells["R"].CastOnUnit(Player);
                return;
            }

            var possibilities = HeroManager.Enemies.Where(h => (h.IsValidTarget() && Vector3.Distance(h.ServerPosition, Player.ServerPosition) <= Spells["R"].Range || (h.IsKillableFromPoint(Player.ServerPosition) && h.IsValidTarget() && !h.IsInvulnerable)) && !LissUtils.Active("Blacklist." + h.ChampionName)).ToList();

            var arranged = possibilities.OrderByDescending(h => h.CountEnemiesInRange(Spells["R"].Range));
            if (LissUtils.Active("Misc.PrioritizeUnderTurret"))
            {
                var EnemyUnderTurret = arranged.Where(h => LissUtils.PointUnderAllyTurret(h.ServerPosition) && !h.IsInvulnerable);
                if (EnemyUnderTurret != null)
                {
                    var Enemytofocus = EnemyUnderTurret.MaxOrDefault(h => h.CountEnemiesInRange(Spells["R"].Range));
                    if (Enemytofocus != null)
                    {
                        Spells["R"].Cast(Enemytofocus);
                        return;
                    }
                }
            }

            var UltTarget = arranged.FirstOrDefault();

            if (UltTarget != null)
            {
                if (!LissUtils.isHealthy() &&
                    Player.CountEnemiesInRange(Spells["R"].Range) >
                    UltTarget.CountEnemiesInRange(Spells["R"].Range) + 1)
                {
                    Spells["R"].CastOnUnit(Player);
                    return;
                }
                    Spells["R"].Cast(UltTarget);
                
            }
        }

        static void KillSteal()
        {
            var targets =
                HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsInvulnerable & !x.IsZombie);

            var objAiHeroes = targets as IList<Obj_AI_Hero> ?? targets.ToList();
            if (SpellSlot.Q.IsReady() && LissUtils.Active("Killsteal.UseQ"))
            {
                Obj_AI_Hero qtarget =
                    objAiHeroes.Where(x => x.Distance(Player.Position) < Spells["Q"].Range)
                    .MinOrDefault(x => x.Health);
                if (qtarget != null)
                {
                    var qdmg = Player.GetSpellDamage(qtarget, SpellSlot.Q);
                    if (qtarget.Health < qdmg)
                    {
                        var pred = Spells["Q"].GetPrediction(qtarget, false);
                        if (pred != null)
                        {
                            Spells["Q"].Cast(pred.CastPosition);
                        }
                    }
                }
            }
            if (SpellSlot.W.IsReady() && LissUtils.Active("Killsteal.UseW"))
            {
                Obj_AI_Hero wtarget =
                    objAiHeroes.Where(x => x.Distance(Player.Position) < Spells["W"].Range)
                        .MinOrDefault(x => x.Health);
                if (wtarget != null)
                {
                    var wdmg = Player.GetSpellDamage(wtarget, SpellSlot.W);
                    if (wtarget.Health < wdmg)
                    {
                        Spells["W"].CastOnUnit(Player);
                    }
                }
            }

            Obj_AI_Hero etarget =
            objAiHeroes.Where(x => x.Distance(Player.Position) < Spells["E"].Range).MinOrDefault(x => x.Health);
            if (SpellSlot.E.IsReady() && LissEMissile == null && !LissUtils.CanSecondE() && LissUtils.Active("Killsteal.UseE"))
            {
                if (etarget != null)
                {
                    var edmg = Player.GetSpellDamage(etarget, SpellSlot.E);
                    if (etarget.Health < edmg)
                    {
                        var pred = Spells["E"].GetPrediction(etarget, false);
                        if (pred != null)
                        {
                            Spells["E"].Cast(pred.CastPosition);
                        }
                    }
                }
            }

            if (LissEMissile != null && etarget != null && etarget.HealthPercent > 5 && etarget.HealthPercent < 15 && LissUtils.isHealthy() && LissUtils.Active("Killsteal.UseE2"))
            {
                if (Vector3.Distance(LissEMissile.Position, etarget.Position) < Spells["Q"].Range && SpellSlot.Q.IsReady() && etarget.Health < Player.GetSpellDamage(etarget, SpellSlot.Q))
                {
                    if (LissUtils.PointUnderEnemyTurret(LissEMissile.Position) && LissUtils.Active("Misc.DontETurret"))
                    {
                        return;
                    }
                    Spells["E"].CastOnUnit(Player);
                }
            }

            if (Spells["Ignite"].IsReady() && LissUtils.Active("Killsteal.UseIgnite"))
            {
                Obj_AI_Hero igntarget =
                    objAiHeroes.Where(x => x.Distance(Player.Position) < Spells["Ignite"].Range)
                        .MinOrDefault(x => x.Health);
                if (igntarget != null)
                {
                    var igniteDmg = Player.GetSummonerSpellDamage(igntarget, Damage.SummonerSpell.Ignite);
                    if (igniteDmg > igntarget.Health)
                    {
                        Spells["Ignite"].Cast(igntarget);
                    }
                }
            }

            if (SpellSlot.R.IsReady() && LissUtils.Active("Killsteal.UseR"))
            {
                var Rtarget = objAiHeroes.Where(h =>
                           (Vector3.Distance(Player.ServerPosition, h.ServerPosition) < Spells["R"].Range) && h.CountEnemiesInRange(Spells["R"].Range) > 1 && h.Health < Player.GetSpellDamage(h, SpellSlot.R) && !LissUtils.Active("Blacklist." + h.ChampionName)).MinOrDefault(h => h.Health);
                if (Rtarget != null)
                {
                    Spells["R"].Cast(Rtarget);
                }
            }
        }
        
 
        static void OnCreate(GameObject sender, EventArgs args)
        {
            var miss = sender as MissileClient;
            if (miss != null && miss.IsValid)
            {
                if (miss.SpellCaster.IsMe && miss.SpellCaster.IsValid && miss.SData.Name == "LissandraEMissile")
                {
                    LissEMissile = miss;
                }
            }
        }

         static void OnDelete(GameObject sender, EventArgs args)
        {
            var miss = sender as MissileClient;
            if (miss == null || !miss.IsValid) return;
            if (miss.SpellCaster is Obj_AI_Hero && miss.SpellCaster.IsValid && miss.SpellCaster.IsMe && miss.SData.Name == "LissandraEMissile")
            {
                LissEMissile = null;
            }
        }


         private static void HarassHandler()
        {
            if (Player.ManaPercent < LissUtils.GetSlider("Harass.Mana"))
            {
                return;
            }
            if (LissUtils.Active("Harass.UseQ"))
            {
                var target = TargetSelector.GetTarget(Spells["Q2"].Range, TargetSelector.DamageType.Magical);
                if (target != null)
                {
                    CastQ(target);
                }
            }
            if (LissUtils.Active("Harass.UseW"))
            {
                var target = TargetSelector.GetTarget(Spells["W"].Range, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget())
                {
                    CastW(target);
                }
            }
            if (LissUtils.Active("Harass.UseE") && LissEMissile == null && !LissUtils.CanSecondE())
            {
                var target = TargetSelector.GetTarget(Spells["E"].Range, TargetSelector.DamageType.Magical);
                if (target != null && !target.IsInvulnerable)
                {
                    CastE(target);
                }
            }
        }

        private static void FarmHandler()
        {
            if (Player.ManaPercent < LissUtils.GetSlider("Farm.Mana"))
            {
                return;
            }
            var Minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            (Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["Q"].Range ||
                             Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["W"].Range ||
                             Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["E"].Range));
           
            if (SpellSlot.Q.IsReady() && LissUtils.Active("Farm.UseQ"))
            {
                var KillableMinionsQ = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.Q) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) > Player.AttackRange);
                if (KillableMinionsQ.Any())
                {
                    Spells["Q"].Cast(KillableMinionsQ.FirstOrDefault().ServerPosition);
                }
            }
            if (SpellSlot.W.IsReady() && LissUtils.Active("Farm.UseW"))
            {
                var KillableMinionsW = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.W) && Vector3.Distance(Player.ServerPosition, m.ServerPosition) < Spells["W"].Range);
                if (KillableMinionsW.Any())
                {
                    Spells["W"].CastOnUnit(Player);
                }
            }

            if (SpellSlot.E.IsReady() && LissUtils.Active("Farm.UseE") && LissEMissile == null && !LissUtils.CanSecondE() && LissEMissile == null)
            {
                var KillableMinionsE = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.E) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) > Player.AttackRange);
                if (KillableMinionsE.Any())
                {
                    Spells["E"].Cast(KillableMinionsE.FirstOrDefault().ServerPosition);
                }
            }
        }

        private static void WaveClearHandler()
        {
               var Minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            (Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["Q"].Range ||
                             Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["W"].Range ||
                             Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["E"].Range));
            
 

            if (SpellSlot.Q.IsReady() && LissUtils.Active("Waveclear.UseQ"))
            {
                var qminions =
                Minions.Where(m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["Q"].Range && m.IsValidTarget());
                MinionManager.FarmLocation QLocation = MinionManager.GetBestLineFarmLocation(qminions.Select(m => m.ServerPosition.To2D()).ToList(), Spells["Q"].Width, Spells["Q"].Range);
                if (QLocation.Position != null && QLocation.MinionsHit > 1) 
                {
                    Spells["Q"].Cast(QLocation.Position);
                }
            }

            if (SpellSlot.E.IsReady() && LissUtils.Active("Waveclear.UseE"))
            {
                var eminions = Minions.Where(m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["E"].Range && m.IsValidTarget());
                if (LissEMissile == null && !LissUtils.CanSecondE())
                {
                    var Eminions =
                        Minions.Where(
                            m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["E"].Range);
                    MinionManager.FarmLocation ELocation =
                        MinionManager.GetBestLineFarmLocation(Eminions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Spells["E"].Width, Spells["E"].Range);
                    if (ELocation.Position != null && ELocation.MinionsHit > 0)
                    {
                            Spells["E"].Cast(ELocation.Position);
                        
                    }
                }
                else if (LissEMissile != null && LissUtils.Active("Waveclear.UseE2") && Vector3.Distance(LissEMissile.Position, LissEMissile.EndPosition) <= 15 && SpellSlot.E.IsReady())
                {
                    if (LissUtils.PointUnderEnemyTurret(LissEMissile.Position) && LissUtils.Active("Misc.DontETurret"))
                    {
                        return;
                    }
                    Spells["E"].CastOnUnit(Player);
                }
            }

            if (SpellSlot.W.IsReady() && LissUtils.Active("Waveclear.UseW"))
            {
                var wminions = Minions.Where(m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells["W"].Range && m.IsValidTarget());
                if (wminions.Count() > LissUtils.GetSlider("Waveclear.Wcount"))
                {
                    Spells["W"].CastOnUnit(Player);
                }
            }
        }


     
        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (LissUtils.Active("Misc.Debug"))
            {
                if (LissEMissile != null)
                {
                    var misposwts = Drawing.WorldToScreen(LissEMissile.Position);
                    Drawing.DrawText(misposwts.X, misposwts.Y, Color.Red, LissEMissile.Position.ToString());
                    Render.Circle.DrawCircle(LissEMissile.Position, 200, Color.Red);

                }
            }

            var DrawQ = Config.Item("Drawing.DrawQ").GetValue<Circle>();
            var DrawW = Config.Item("Drawing.DrawW").GetValue<Circle>();
            var DrawE = Config.Item("Drawing.DrawE").GetValue<Circle>();
            var DrawR = Config.Item("Drawing.DrawR").GetValue<Circle>();

            if (DrawQ.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Spells["Q"].Range, DrawQ.Color);
            }
            if (DrawW.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Spells["W"].Range, DrawW.Color);
            }
            if (DrawE.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Spells["E"].Range, DrawE.Color);
            }
            if (DrawR.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Spells["R"].Range, DrawE.Color);
            }
        }

     


    }


}

