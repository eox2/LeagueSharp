#region imports
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion;


namespace SephLux
{

    #region Initiliazation

    internal static class Lux
    {
        #region vars

        public static Obj_AI_Hero Player;
        public static Menu Config;
        private static Obj_SpellCircleMissile LuxE;
        private static SpellSlot IgniteSlot = SpellSlot.Summoner1;

        #endregion

        #region OnLoad

        public static void LuxMain()
        {
            CustomEvents.Game.OnGameLoad += LuxMain;
        }

        private static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 1300f, TargetSelector.DamageType.Magical)},
            {SpellSlot.W, new Spell(SpellSlot.W, 1150f)},
            {SpellSlot.E, new Spell(SpellSlot.E, 1100f, TargetSelector.DamageType.Magical)},
            {SpellSlot.R, new Spell(SpellSlot.R, 3500f, TargetSelector.DamageType.Magical)},
            {IgniteSlot, new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 550f)}
        };

        private static void InitializeSpells()
        {
            Spells[SpellSlot.Q].SetSkillshot(0.250f, 70f, 1300f, false, SkillshotType.SkillshotLine);
            Spells[SpellSlot.W].SetSkillshot(0.25f, 150f, 1200f, false, SkillshotType.SkillshotLine);
            Spells[SpellSlot.E].SetSkillshot(0.250f, 275f, 1300f, false, SkillshotType.SkillshotCircle);
            Spells[SpellSlot.R].SetSkillshot(1f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private static void LuxMain(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != "Lux")
            {
                return;
            }


            Config = LuxMenu.CreateMenu();

            Config.AddToMainMenu();

            InitializeSpells();

            ObjectHandling();

            AntiGapcloser.OnEnemyGapcloser += OnGapClose;

            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;

            Game.OnUpdate += OnUpdate;
            Game.OnUpdate += CheckKillable;
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

            Killsteal();

            var target = TargetSelector.GetTarget(
                Spells[SpellSlot.Q].Range, TargetSelector.DamageType.Magical, true, LuxMenu.BlackList);

            if (target != null && LuxUtils.ActiveKeyBind("Keys.HarassT"))
            {
                Harass(target);
            }
            var Orbwalkmode = LuxMenu.Orbwalker.ActiveMode;
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


        #region Combo

        private static void Combo(Obj_AI_Hero target)
        {
            if (Spells[SpellSlot.Q].IsReady() && LuxUtils.Active("Combo.UseQ"))
            {
                var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
                if (pred.CollisionObjects.Count <= 1 && pred.Hitchance > LuxUtils.GetHitChance("Hitchance.Q"))
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.E].IsReady() && LuxE == null && LuxUtils.Active("Combo.UseE"))
            {
                var pred = Spells[SpellSlot.E].GetPrediction(target, true);
                if (pred.Hitchance >= LuxUtils.GetHitChance("Hitchance.E"))
                {
                    Spells[SpellSlot.E].Cast(pred.CastPosition);
                }
            }
            else if (LuxUtils.Active("Combo.UseE2") && LuxE != null &&
                     Vector3.Distance(LuxE.Position, target.ServerPosition) <=
                     LuxE.BoundingRadius + target.BoundingRadius)
            {
                Spells[SpellSlot.E].Cast();
            }

            if (SpellSlot.R.IsReady() && LuxUtils.Active("Combo.UseR"))
            {
                var ulttarget = HeroManager.Enemies.FirstOrDefault(h => h.HasBuff("LuxLightBindingMis"));
                if (ulttarget != null)
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(ulttarget, true);
                    if (pred.Hitchance >= LuxUtils.GetHitChance("Hitchance.R"))
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                }
                else
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(target, true);
                    if (pred.Hitchance >= LuxUtils.GetHitChance("Hitchance.R") ||
                        pred.CollisionObjects.Count(x => x.Type == GameObjectType.obj_AI_Hero && x.IsEnemy) > 2 &&
                        pred.Hitchance >= HitChance.High)
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                }
            }


            if (SpellSlot.W.IsReady() && LuxUtils.Active("Combo.UseW"))
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
                if (besthero != null)
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

            if (SpellSlot.Q.IsReady() && LuxUtils.Active("Waveclear.UseQ"))
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

            if (SpellSlot.E.IsReady() && LuxUtils.Active("Waveclear.UseE"))
            {
                if (LuxE == null)
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

                if (LuxUtils.Active("Waveclear.UseE2") && LuxE != null && LuxE.Position == LuxE.EndPosition)
                {
                    Spells[SpellSlot.E].Cast();
                }
            }


            if (SpellSlot.R.IsReady() && LuxUtils.Active("Waveclear.UseR"))
            {
                MinionManager.FarmLocation RLocation =
                    MinionManager.GetBestLineFarmLocation(
                        Minions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.R].Width,
                        Spells[SpellSlot.R].Range);
                if (RLocation.MinionsHit > LuxUtils.GetSlider("Waveclear.Rcount"))
                {
                    Spells[SpellSlot.R].Cast(RLocation.Position);
                }
            }
        }
    #endregion Waveclear


        #region MixedModeLogic

        static void MixedModeLogic(Obj_AI_Hero target, bool isMixed)
        {
            if (!LuxUtils.Active("Farm.Enable") || target == null || Player.ManaPercent < LuxUtils.GetSlider("Farm.Mana"))
            {
                return;
            }

            var Minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            (Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.E].Range));

            if (!Minions.Any())
            {
                return;
            }
            if (SpellSlot.Q.IsReady() && LuxUtils.Active("Farm.UseQ"))
            {
                var KillableMinionsQ = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.Q) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) > Player.AttackRange);
                if (KillableMinionsQ.Any())
                {
                    Spells[SpellSlot.Q].Cast(KillableMinionsQ.FirstOrDefault().ServerPosition);
                }
            }
            if (SpellSlot.E.IsReady() && LuxUtils.Active("Farm.UseE"))
            {
                var KillableMinionsE = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.W) && Vector3.Distance(Player.ServerPosition, m.ServerPosition) < Spells[SpellSlot.E].Range);
                if (KillableMinionsE.Any())
                {
                    Spells[SpellSlot.E].Cast(KillableMinionsE.FirstOrDefault().ServerPosition);
                }
            }

            if (LuxE != null && LuxE.Position == LuxE.EndPosition)
            {
                Spells[SpellSlot.E].Cast();
            }

        }
        #endregion MixedModeLogic

        #region Harass

        static void Harass(Obj_AI_Hero target)
        {
            if (Spells[SpellSlot.Q].IsReady() && LuxUtils.Active("Harass.UseQ") && Player.ManaPercent > LuxUtils.GetSlider("Harass.Mana"))
            {
                var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
                if (pred.CollisionObjects.Count <= 1 && pred.Hitchance > LuxUtils.GetHitChance("Hitchance.Q"))
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.E].IsReady() && LuxE == null && LuxUtils.Active("Harass.UseE"))
            {
                var pred = Spells[SpellSlot.E].GetPrediction(target, true);
                if (pred.Hitchance >= LuxUtils.GetHitChance("Hitchance.E"))
                {
                    Spells[SpellSlot.E].Cast(pred.CastPosition);
                }
            }
            else if (LuxUtils.Active("Harass.UseE") && LuxE != null &&
                     Vector3.Distance(LuxE.Position, target.ServerPosition) <=
                     LuxE.BoundingRadius + target.BoundingRadius)
            {
                Spells[SpellSlot.E].Cast();
            }
        }
        #endregion

        #region KillSteal

        static void Killsteal()
        {
            if (!LuxUtils.Active("Killsteal"))
            {
                return;
            }
            var targets = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsInvulnerable & !x.IsZombie);

            if (SpellSlot.Q.IsReady() && LuxUtils.Active("Killsteal.UseQ"))
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
                        if (pred != null && pred.Hitchance > HitChance.Medium && pred.CollisionObjects.Count <= 1)
                        {
                            Spells[SpellSlot.Q].Cast(pred.CastPosition);
                            return;
                        }
                    }
                }
            }
            if (SpellSlot.E.IsReady() && LuxUtils.Active("Killsteal.UseE"))
            {
                if (LuxE != null && LuxE.Position == LuxE.EndPosition && HeroManager.Enemies.Any(h => Vector3.Distance(LuxE.Position, h.ServerPosition) <= LuxE.BoundingRadius + h.BoundingRadius && h.Health <= Player.GetSpellDamage(h, SpellSlot.E)))
                {
                    Spells[SpellSlot.E].Cast();
                }

                else if (LuxE == null) { 
                Obj_AI_Hero etarget =
                    targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.E].Range)
                    .MinOrDefault(x => x.Health);
                    if (etarget != null)
                    {
                        if (LuxE == null)
                        {
                            var edmg = Player.GetSpellDamage(etarget, SpellSlot.E);
                            if (etarget.Health < edmg)
                            {
                                var pred = Spells[SpellSlot.Q].GetPrediction(etarget, false);
                                if (pred != null && pred.Hitchance > HitChance.Medium)
                                {
                                    Spells[SpellSlot.E].Cast(pred.CastPosition);
                                }
                            }
                        }
                    }
                }
            }


            if (SpellSlot.R.IsReady() && LuxUtils.Active("Killsteal.UseR"))
            {
                var targ = HeroManager.Enemies.FirstOrDefault(h => h.HasBuff("LuxLightBindingMis") && h.IsValidTarget(Spells[SpellSlot.R].Range) && h.Health < Player.GetSpellDamage(h, SpellSlot.R));

                if (targ == null)
                {
                    targ = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Spells[SpellSlot.R].Range && x.Health < Player.GetSpellDamage(x, SpellSlot.R));
                }
               
                if (targ != null)
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(targ, true);
                    if (pred.Hitchance >= LuxUtils.GetHitChance("Hitchance.R"))
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                    else if (pred.AoeTargetsHitCount >= 3 || pred.CollisionObjects.Count(x => x.Type == GameObjectType.obj_AI_Hero) >= 2)
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                }
            }

            if (Spells[IgniteSlot].IsReady() && LuxUtils.Active("Killsteal.UseIgnite"))
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
        private static void CheckKillable(EventArgs args)
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
            if (SpellSlot.Q.IsReady() && LuxUtils.Active("Combo.UseQ") &&
                Vector3.Distance(Point, target.ServerPosition) < Spells[SpellSlot.Q].Range)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (SpellSlot.E.IsReady() && LuxUtils.Active("Combo.UseE") && Vector3.Distance(Point, target.ServerPosition) < Spells[SpellSlot.E].Range + 35 && LuxE == null)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (SpellSlot.R.IsReady() && LuxUtils.Active("Combo.UseR") && Vector3.Distance(Point, target.ServerPosition) < Spells[SpellSlot.R].Range)
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (Spells[IgniteSlot].IsReady() && LuxUtils.Active("Killsteal.UseIgnite") && Vector3.Distance(Point, target.ServerPosition) < Spells[IgniteSlot].Range)
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

            if (LuxUtils.Active("Interrupter.AntiGapClose") && sender.IsValidTarget())
            {
                if (LuxUtils.Active("Interrupter.AG.UseQ") && Vector3.Distance(args.End, Player.ServerPosition) <= Spells[SpellSlot.Q].Range)
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
                if (LuxUtils.Active("Interrupter.UseQ") && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range)
                {
                    Spells[SpellSlot.Q].Cast(sender.Position);
                }
            }
        }
        #endregion

        #region LuxEObjectHandling
        static void ObjectHandling()
            {
                GameObject.OnCreate += (sender, args) =>
                {
                    if (sender.Name.Contains("LuxLightstrike_tar"))
                    {
                        var miss = sender as Obj_SpellCircleMissile;
                        if (sender is Obj_SpellCircleMissile && sender.IsValid && miss.SpellCaster.IsMe)
                        {
                            LuxE = miss;
                        }
                    }
                };

                GameObject.OnDelete += (sender, args) =>
                {
                    if (sender.Name.Contains("LuxLightstrike_tar"))
                    {
                        var miss = sender as Obj_SpellCircleMissile;
                        if (sender is Obj_SpellCircleMissile && sender.IsValid && miss.SpellCaster.IsMe)
                        {
                            LuxE = null;
                        }
                    }
                };
            }
        #endregion

        #region Drawing

        static void OnDraw(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling() || LuxUtils.Active("Drawing.Disable"))
            {
                return;
            }
            foreach (var x in Killable)
            {
                var pos = Drawing.WorldToScreen(x.ServerPosition);
                Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.Azure, "Killable");
            }

            if (LuxUtils.Active("Drawing.DrawQ"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.Q].Range, System.Drawing.Color.White);
            }
            if (LuxUtils.Active("Drawing.DrawE"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.E].Range, System.Drawing.Color.RoyalBlue);
            }
            if (LuxUtils.Active("Drawing.DrawR"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.R].Range, System.Drawing.Color.Red);
            }

        }
        #endregion

    }
   }



    

