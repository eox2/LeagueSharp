#region imports
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion;


namespace SephZiggs
{

    #region Initiliazation

    internal static class Ziggs
    {
        #region vars

        public static Obj_AI_Hero Player;
        public static Menu Config;
        private static MissileClient ZiggsE;
        private static SpellSlot IgniteSlot = SpellSlot.Summoner1;

        #endregion

        #region OnLoad

        public static void Main()
        {
            CustomEvents.Game.OnGameLoad += ZiggsMain;
        }

        private static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 850f, TargetSelector.DamageType.Magical)},
            {SpellSlot.W, new Spell(SpellSlot.W, 1000f)},
            {SpellSlot.E, new Spell(SpellSlot.E, 900f, TargetSelector.DamageType.Magical)},
            {SpellSlot.R, new Spell(SpellSlot.R, 5300f, TargetSelector.DamageType.Magical)},
            {IgniteSlot, new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 550f)}
        };

        private static void InitializeSpells()
        {
            Spells[SpellSlot.Q].SetSkillshot(0.250f, 140f, 1700f, true, SkillshotType.SkillshotCircle);
            Spells[SpellSlot.W].SetSkillshot(0.250f, 275f, 1750f, false, SkillshotType.SkillshotCircle);
            Spells[SpellSlot.E].SetSkillshot(0.500f, 235f, 1750f, false, SkillshotType.SkillshotCircle);
            Spells[SpellSlot.R].SetSkillshot(0.500f, 500f, int.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private static void ZiggsMain(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.CharData.BaseSkinName != "Ziggs")
            {
                return;
            }

            Config = ZiggsMenu.CreateMenu();

            Config.AddToMainMenu();

            InitializeSpells();

            ObjectHandling(); 

            AntiGapcloser.OnEnemyGapcloser += OnGapClose;

            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;

            Game.OnUpdate += OnUpdate;
            Game.OnUpdate += AutoSpells;
            
            Drawing.OnDraw += OnDraw;
        }

        #endregion

        #endregion

        #region OnUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }

            CheckKillable();

            if (ZiggsUtils.ActiveKeyBind("Misc.RKey"))
            {
                CastR();
            }

            Killsteal();

            var target = TargetSelector.GetTarget(
                Spells[SpellSlot.Q].Range, TargetSelector.DamageType.Magical, true, ZiggsMenu.BlackList);

            if (target != null && ZiggsUtils.ActiveKeyBind("Keys.HarassT"))
            {
                Harass(target);
            }

            var Orbwalkmode = ZiggsMenu.Orbwalker.ActiveMode;

            switch (Orbwalkmode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (target != null)
                    {
                        Combo(target);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    WaveClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    MixedModeLogic(target, true);
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    MixedModeLogic(target, false);
                    break;
            }
        }

        #endregion


        #region KeyPressR

        static void CastR()
        {
            var targ = TargetSelector.GetTarget(Spells[SpellSlot.R].Range, TargetSelector.DamageType.Magical);
            if (targ != null)
            {
                var pred = Spells[SpellSlot.R].GetPrediction(targ);
                if (pred.Hitchance >= HitChance.Medium)
                {
                    Spells[SpellSlot.R].Cast(pred.CastPosition);
                }
            }
        }

        #endregion KeyPressR


        #region AutoSpells

        private static void AutoSpells(EventArgs args)
        {
            foreach (var target in HeroManager.Enemies)
            {
                if (ZiggsUtils.Active("Combo.UseE2") && ZiggsE != null &&
                    Vector3.Distance(ZiggsE.Position, target.ServerPosition) <=
                    ZiggsE.BoundingRadius + target.BoundingRadius)
                {
                    Spells[SpellSlot.E].Cast();
                }
            }

            if (SpellSlot.R.IsReady() && ZiggsUtils.Active("Auto.R"))
            {
                List<Obj_AI_Hero> targets = new List<Obj_AI_Hero>();
                targets = HeroManager.Enemies.Where(x => x.Distance(Player) <= Spells[SpellSlot.R].Range).ToList();
                List<CastPosition1> Positions = new List<CastPosition1>();


                foreach (var h in targets)
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(h, true);

                    //List<Obj_AI_Base> collobjs =
                       // pred.CollisionObjects.FindAll(x => x.Type == GameObjectType.obj_AI_Hero && x.IsEnemy);

                    if (pred != null)
                    {
                        CastPosition1 cp = new CastPosition1
                        {
                            pos = pred.CastPosition,
                            hc = pred.Hitchance,
                            numberhit = pred.AoeTargetsHitCount + 1,
                            hasbuff = h.HasBuff("ZiggsLightBindingMis")
                        };

                        Positions.Add(cp);
                    }
                }

                var bestpossibleposition =
                    Positions.OrderByDescending(x => x.numberhit)
                        .ThenByDescending(x => x.hc)
                        .ThenByDescending(h => h.hasbuff)
                        .FirstOrDefault();

                if (bestpossibleposition != null && bestpossibleposition.hc >= ZiggsUtils.GetHitChance("Hitchance.R") &&
                    bestpossibleposition.numberhit >= ZiggsUtils.GetSlider("Auto.Rcount"))
                {
                    Spells[SpellSlot.R].Cast(bestpossibleposition.pos);
                }
            }

            if (SpellSlot.W.IsReady() && ZiggsUtils.Active("Auto.W"))
            {
                var list = HeroManager.Allies;

                var lowhealthallies =
                    list.Where(ally => ally.HealthPercent <= ZiggsUtils.GetSlider("Auto.Whp") && Player.Distance(ally) <= Spells[SpellSlot.W].Range);
                Obj_AI_Hero besthero = null;
                int amthit = 0;
                foreach (var hero in lowhealthallies)
                {
                    var pred = Prediction.GetPrediction(WInput(hero));
                    if (pred.Hitchance >= HitChance.Collision || pred.Hitchance >= HitChance.Low)
                    {
                        var coll = pred.CollisionObjects.Count;
                        if (coll >= amthit)
                        {
                            amthit = coll;
                            besthero = hero;
                        }
                    }
                }
                if (besthero != null && amthit >= ZiggsUtils.GetSlider("Auto.Wcount"))
                {
                    Spells[SpellSlot.W].Cast(besthero.ServerPosition);
                }
            }
        }

        #endregion AutoSpells   

        #region Combo

        class CastPosition1
        {
            public int numberhit;
            public Vector3 pos;
            public HitChance hc;
            public bool hasbuff;


        }

        private static void Combo(Obj_AI_Hero target)
        {
            if (Spells[SpellSlot.Q].IsReady() && ZiggsUtils.Active("Combo.UseQ") && Player.Distance(target) <= Spells[SpellSlot.Q].Range)
            {
                var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
                if (pred.CollisionObjects.Count <= 1 && pred.Hitchance >= ZiggsUtils.GetHitChance("Hitchance.Q"))
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
				
            }
            if (Spells[SpellSlot.E].IsReady() && ZiggsE == null && ZiggsUtils.Active("Combo.UseE") && Player.Distance(target) <= Spells[SpellSlot.E].Range)
            {
				/*
                var pred = Spells[SpellSlot.E].GetPrediction(target, true);
                if (pred.Hitchance >= ZiggsUtils.GetHitChance("Hitchance.E"))
                {
                    Spells[SpellSlot.E].Cast(pred.CastPosition);
                }
				*/
			}
            else if (ZiggsUtils.Active("Combo.UseE2") && ZiggsE != null &&
                     Vector3.Distance(ZiggsE.Position, target.ServerPosition) <=
                     ZiggsE.BoundingRadius + target.BoundingRadius)
            {
                Spells[SpellSlot.E].Cast();
            }

            if (SpellSlot.R.IsReady() && ZiggsUtils.Active("Combo.UseR"))
            {
                List<Obj_AI_Hero> targets = new List<Obj_AI_Hero>();
                if (ZiggsUtils.Active("Combo.RKillable"))
                {
                    targets =
                        HeroManager.Enemies.Where(
                            x =>
                                x.Health <= Player.GetSpellDamage(x, SpellSlot.R) &&
                                x.Distance(Player) <= Spells[SpellSlot.R].Range).ToList();
                }
                else
                {
                    targets = HeroManager.Enemies.Where(x => x.Distance(Player) <= Spells[SpellSlot.R].Range).ToList();
                }

                List<CastPosition1> Positions = new List<CastPosition1>();

                    foreach (var h in targets)
                    {
                        var pred = Spells[SpellSlot.R].GetPrediction(h, true);
                        int hit = pred.AoeTargetsHitCount + 1;
                       // List<Obj_AI_Base> collobjs =
                          //  pred.CollisionObjects.FindAll(x => x.Type == GameObjectType.obj_AI_Hero && x.IsEnemy);

                        CastPosition1 cp = new CastPosition1
                        {
                            pos = pred.CastPosition,
                            hc = pred.Hitchance,
                            numberhit = hit,
                            hasbuff = h.HasBuff("ZiggsLightBindingMis")
                        };
                        Positions.Add(cp);
                    }

                    var bestpossibleposition =
                        Positions.OrderByDescending(x => x.numberhit)
                            .ThenByDescending(x => x.hc)
                            .ThenByDescending(h => h.hasbuff)
                            .FirstOrDefault();
                if (bestpossibleposition != null && bestpossibleposition.hc >= ZiggsUtils.GetHitChance("Hitchance.R") &&
                    bestpossibleposition.numberhit >= ZiggsUtils.GetSlider("Combo.Rcount"))
                    {
                        Spells[SpellSlot.R].Cast(bestpossibleposition.pos);
                    }
            }


            if (SpellSlot.W.IsReady() && ZiggsUtils.Active("Combo.UseW"))
            {
                var list = HeroManager.Allies;

                var lowhealthallies =
                    list.Where(ally => ally.HealthPercent <= 40 && Player.Distance(ally) <= Spells[SpellSlot.W].Range);
                Obj_AI_Hero besthero = null;
                int amthit = 0;
                foreach (var hero in lowhealthallies)
                {
                    var pred = Prediction.GetPrediction(WInput(hero));
                    if (pred.Hitchance >= HitChance.Collision || pred.Hitchance >= HitChance.Low)
                    {
                        var coll = pred.CollisionObjects.Count;
                        if (coll >= amthit)
                        {
                            amthit = coll;
                            besthero = hero;
                        }
                    }
                }
                if (besthero != null && Player.HealthPercent <= 30 && Player.CountEnemiesInRange(800) > 0)
                {
                    Spells[SpellSlot.W].Cast(besthero.ServerPosition);
                }
            }
        }

        public static CollisionableObjects[] CollObjects =
        {
            CollisionableObjects.Allies
        };

        private static PredictionInput WInput(Obj_AI_Hero target)
        {
            var input = new PredictionInput
            {
                Unit = target,
                CollisionObjects = CollObjects,
                From = Player.ServerPosition,
                Collision = true,
                Delay = Spells[SpellSlot.W].Delay,
                Radius = Spells[SpellSlot.W].Width,
                Range = Spells[SpellSlot.W].Range,
                Speed = Spells[SpellSlot.W].Speed,
                Type = SkillshotType.SkillshotLine
                
            };

            return input;
        }



    #endregion

        #region Waveclear

        private static void WaveClear()
        {
            var Minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            (Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.R].Range));

            if (SpellSlot.Q.IsReady() && ZiggsUtils.Active("Waveclear.UseQ"))
            {
                var qminions =
                    Minions.Where(
                        m =>
                            Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range &&
                            m.IsValidTarget());
                MinionManager.FarmLocation QLocation =
                    MinionManager.GetBestLineFarmLocation(
                        qminions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.Q].Width,
                        Spells[SpellSlot.Q].Range);
                if (QLocation.MinionsHit > 1)
                {
                    Spells[SpellSlot.Q].Cast(QLocation.Position);
                }
            }

            if (SpellSlot.E.IsReady() && ZiggsUtils.Active("Waveclear.UseE"))
            {
                if (ZiggsE == null)
                {
                    var Eminions =
                        Minions.Where(
                            m =>
                                Vector3.Distance(m.ServerPosition, Player.ServerPosition) <=
                                Spells[SpellSlot.E].Range + Spells[SpellSlot.E].Width);
                    MinionManager.FarmLocation ELocation =
                        MinionManager.GetBestCircularFarmLocation(
                            Eminions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.E].Width,
                            Spells[SpellSlot.E].Range);
                    if (ELocation.MinionsHit >= 2)
                    {
                        Spells[SpellSlot.E].Cast(ELocation.Position);
                    }
                }

                if (ZiggsUtils.Active("Waveclear.UseE2") && ZiggsE != null && ZiggsE.Position == ZiggsE.EndPosition)
                {
                    Spells[SpellSlot.E].Cast();
                }
            }


            if (SpellSlot.R.IsReady() && ZiggsUtils.Active("Waveclear.UseR"))
            {
                MinionManager.FarmLocation RLocation =
                    MinionManager.GetBestLineFarmLocation(
                        Minions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.R].Width,
                        Spells[SpellSlot.R].Range);
                if (RLocation.MinionsHit > ZiggsUtils.GetSlider("Waveclear.Rcount"))
                {
                    Spells[SpellSlot.R].Cast(RLocation.Position);
                }
            }
        }
    #endregion Waveclear


        #region MixedModeLogic

        static void MixedModeLogic(Obj_AI_Hero target, bool isMixed)
        {
	        if (isMixed && target != null)
	        {
		        Harass(target);
	        }

            if (!ZiggsUtils.Active("Farm.Enable") || target == null || Player.ManaPercent < ZiggsUtils.GetSlider("Farm.Mana"))
            {
                return;
            }

            var Minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            (Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.E].Range)).ToList();

            if (!Minions.Any())
            {
                return;
            }
            if (SpellSlot.Q.IsReady() && ZiggsUtils.Active("Farm.UseQ"))
            {
                var KillableMinionsQ = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.Q) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) > Player.AttackRange);
                if (KillableMinionsQ.Any())
                {
                    Spells[SpellSlot.Q].Cast(KillableMinionsQ.FirstOrDefault().ServerPosition);
                }
            }
            if (SpellSlot.E.IsReady() && ZiggsUtils.Active("Farm.UseE"))
            {
                var KillableMinionsE = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.W) && Vector3.Distance(Player.ServerPosition, m.ServerPosition) < Spells[SpellSlot.E].Range);
                if (KillableMinionsE.Any())
                {
                    Spells[SpellSlot.E].Cast(KillableMinionsE.FirstOrDefault().ServerPosition);
                }
            }

            if (ZiggsE != null && ZiggsE.Position == ZiggsE.EndPosition)
            {
                Spells[SpellSlot.E].Cast();
            }

        }
        #endregion MixedModeLogic

        #region Harass

        static void Harass(Obj_AI_Hero target)
        {
            if (Player.ManaPercent < ZiggsUtils.GetSlider("Harass.Mana"))
            {
                if (ZiggsUtils.Active("Harass.UseE") && ZiggsE != null &&
                     Vector3.Distance(ZiggsE.Position, target.ServerPosition) <=
                     ZiggsE.BoundingRadius + target.BoundingRadius)
                {
                    Spells[SpellSlot.E].Cast();
                }
                return;
            }
            
            if (Spells[SpellSlot.Q].IsReady() && ZiggsUtils.Active("Harass.UseQ") && Player.Distance(target) <= Spells[SpellSlot.Q].Range)
            {
                var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
                if (pred.CollisionObjects.Count <= 1 && pred.Hitchance >= ZiggsUtils.GetHitChance("Hitchance.Q"))
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.E].IsReady() && ZiggsE == null && ZiggsUtils.Active("Harass.UseE") && Player.Distance(target) <= Spells[SpellSlot.E].Range)
            {
				/*
                var pred = Spells[SpellSlot.E].GetPrediction(target, true);
                if (pred.Hitchance >= ZiggsUtils.GetHitChance("Hitchance.E"))
                {
                    Spells[SpellSlot.E].Cast(pred.CastPosition);
                }
				*/
			}
            else if (ZiggsUtils.Active("Harass.UseE") && ZiggsE != null &&
                     Vector3.Distance(ZiggsE.Position, target.ServerPosition) <=
                     ZiggsE.BoundingRadius + target.BoundingRadius)
            {
                Spells[SpellSlot.E].Cast();
            }
        }
        #endregion

        #region KillSteal

        static void Killsteal()
        {
            if (!ZiggsUtils.Active("Killsteal"))
            {
                return;
            }
            var targets = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsInvulnerable & !x.IsZombie);

            if (SpellSlot.Q.IsReady() && ZiggsUtils.Active("Killsteal.UseQ"))
            {

                Obj_AI_Hero qtarget =
                    targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.Q].Range)
                    .MinOrDefault(x => x.Health);
                if (qtarget != null)
                {
                    var qdmg = Player.GetSpellDamage(qtarget, SpellSlot.Q);
                    if (qtarget.Health < qdmg)
                    {
                        var pred = Spells[SpellSlot.Q].GetPrediction(qtarget, true);
                        if (pred != null && pred.Hitchance >= HitChance.Medium && pred.CollisionObjects.Count <= 1)
                        {
                            Spells[SpellSlot.Q].Cast(pred.CastPosition);
                            return;
                        }
                    }
                }
            }
            if (SpellSlot.E.IsReady() && ZiggsUtils.Active("Killsteal.UseE"))
            {
                if (ZiggsE != null && ZiggsE.Position == ZiggsE.EndPosition && HeroManager.Enemies.Any(h => Vector3.Distance(ZiggsE.Position, h.ServerPosition) <= ZiggsE.BoundingRadius + h.BoundingRadius && h.Health <= Player.GetSpellDamage(h, SpellSlot.E)))
                {
                    Spells[SpellSlot.E].Cast();
                }

                else if (ZiggsE == null) { 
                Obj_AI_Hero etarget =
                    targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.E].Range)
                    .MinOrDefault(x => x.Health);
                    if (etarget != null)
                    {
                        if (ZiggsE == null)
                        {
                            var edmg = Player.GetSpellDamage(etarget, SpellSlot.E);
                            if (etarget.Health < edmg)
                            {
                                var pred = Spells[SpellSlot.E].GetPrediction(etarget, true);
                                if (pred != null && pred.Hitchance >= HitChance.Medium)
                                {
                                    Spells[SpellSlot.E].Cast(pred.CastPosition);
                                }
                            }
                        }
                    }
                }
            }


            if (SpellSlot.R.IsReady() && ZiggsUtils.Active("Killsteal.UseR"))
            {

                    List<Obj_AI_Hero> targetss = new List<Obj_AI_Hero>();

                    targetss = HeroManager.Enemies.Where(x => x.Health <= Player.GetSpellDamage(x, SpellSlot.R) && x.Distance(Player) <= Spells[SpellSlot.R].Range).ToList();
 
                    List<CastPosition1> Positions = new List<CastPosition1>();

                    foreach (var h in targetss)
                    {
                        var pred = Spells[SpellSlot.R].GetPrediction(h, true);
                       // List<Obj_AI_Base> collobjs =
                         // pred.CollisionObjects.FindAll(x => x.Type == GameObjectType.obj_AI_Hero && x.IsEnemy);
                        CastPosition1 cp = new CastPosition1 { pos = pred.CastPosition, hc = pred.Hitchance, numberhit = pred.AoeTargetsHitCount + 1, hasbuff = h.HasBuff("ZiggsLightBindingMis") };
                        Positions.Add(cp);
                    }

                    var bestpossibleposition = Positions.OrderByDescending(x => x.numberhit).ThenByDescending(x => x.hc).ThenByDescending(h => h.hasbuff).FirstOrDefault();
                    if (bestpossibleposition != null && bestpossibleposition.hc >= ZiggsUtils.GetHitChance("Hitchance.R"))
                    {
                        Spells[SpellSlot.R].Cast(bestpossibleposition.pos);
                    }
                
                /*
                var targ = HeroManager.Enemies.FirstOrDefault(h => h.HasBuff("ZiggsLightBindingMis") && h.IsValidTarget(Spells[SpellSlot.R].Range) && h.Health < Player.GetSpellDamage(h, SpellSlot.R));

                if (targ == null)
                {
                    targ = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Spells[SpellSlot.R].Range && x.Health < Player.GetSpellDamage(x, SpellSlot.R));
                }
               
                if (targ != null)
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(targ, true);
                    if (pred.Hitchance >= ZiggsUtils.GetHitChance("Hitchance.R"))
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                    else if (pred.AoeTargetsHitCount >= 3 || pred.CollisionObjects.Count(x => x.Type == GameObjectType.obj_AI_Hero) >= 2)
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                }
                */
            }

            if (Spells[IgniteSlot].IsReady() && ZiggsUtils.Active("Killsteal.UseIgnite"))
            {
               var targ =
                    HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() &&
                            Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Spells[IgniteSlot].Range && x.Health < (Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite)));
                if (targ != null)
                {
                    Spells[IgniteSlot].Cast(targ);
                }
              
            }
        }
        #endregion KillSteal


        #region Killable

        public static List<Obj_AI_Hero> Killable = new List<Obj_AI_Hero>();
        private static void CheckKillable()
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }
            Killable.Clear();
            foreach (var hero in HeroManager.Enemies)
            {
                if (hero.IsValidTarget(Spells[SpellSlot.R].Range) && hero.IsKillableFromPoint(Player.ServerPosition))
                {
                    Killable.Add(hero);
                }
            }
        }

        public static bool IsKillableFromPoint(this Obj_AI_Hero target, Vector3 Point)
        {
            double totaldmgavailable = 0;
            if (SpellSlot.Q.IsReady() && ZiggsUtils.Active("Combo.UseQ") &&
                Vector3.Distance(Point, target.ServerPosition) < Spells[SpellSlot.Q].Range)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (SpellSlot.E.IsReady() && ZiggsUtils.Active("Combo.UseE") && Vector3.Distance(Point, target.ServerPosition) < Spells[SpellSlot.E].Range + 35 && ZiggsE == null)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (SpellSlot.R.IsReady() && ZiggsUtils.Active("Combo.UseR") && Vector3.Distance(Point, target.ServerPosition) < Spells[SpellSlot.R].Range)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (Spells[IgniteSlot].IsReady() && ZiggsUtils.Active("Killsteal.UseIgnite") && Vector3.Distance(Point, target.ServerPosition) < Spells[IgniteSlot].Range)
            {
                totaldmgavailable += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }
            return totaldmgavailable > target.Health;
        }
        #endregion 


        #region AntiGapcloser

        static void OnGapClose(ActiveGapcloser args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }
            var sender = args.Sender;

            if (sender != null && ZiggsUtils.Active("Interrupter.AntiGapClose") && sender.IsValidTarget())
            {
                if (ZiggsUtils.Active("Interrupter.AG.UseQ") && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range)
                {
                    Spells[SpellSlot.Q].Cast(sender.ServerPosition);
                }
            }
        }
        #endregion


        #region Interrupter
        static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }
            if (sender.IsValidTarget())
            {
                if (ZiggsUtils.Active("Interrupter.UseQ") && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range)
                {
                    Spells[SpellSlot.Q].Cast(sender.Position);
                }
            }
        }
        #endregion

        #region ZiggsEObjectHandling
        static void ObjectHandling()
            {
                GameObject.OnCreate += (sender, args) =>
                {
                    if (sender.Name.Contains("ZiggsLightstrike_tar"))
                    {
                        var miss = sender as MissileClient;
                        if (sender is MissileClient && sender.IsValid && miss.SpellCaster.IsMe)
                        {
                            ZiggsE = miss;
                        }
                    }
                };

                GameObject.OnDelete += (sender, args) =>
                {
                    if (sender.Name.Contains("ZiggsLightstrike_tar"))
                    {
                        var miss = sender as MissileClient;
                        if (sender is MissileClient && sender.IsValid && miss.SpellCaster.IsMe)
                        {
                            ZiggsE = null;
                        }
                    }
                };
            }
        #endregion

        #region Drawing

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling() || ZiggsUtils.Active("Drawing.Disable"))
            {
                return;
            }
            foreach (var x in Killable)
            {
                var pos = Drawing.WorldToScreen(x.ServerPosition);
                Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.Azure, "Killable");
            }

            if (ZiggsUtils.Active("Drawing.DrawQ"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.Q].Range, System.Drawing.Color.White);
            }
            if (ZiggsUtils.Active("Drawing.DrawE"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.E].Range, System.Drawing.Color.RoyalBlue);
            }
            if (ZiggsUtils.Active("Drawing.DrawR"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.R].Range, System.Drawing.Color.Aqua);
            }

            if (ZiggsUtils.Active("Drawing.DrawRMM"))
            {
                Utility.DrawCircle(Player.Position, Spells[SpellSlot.R].Range, System.Drawing.Color.Aqua, 1, 23, true);
            }
        }

        #endregion
        }

    }



    

