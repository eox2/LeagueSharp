#region imports
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion;


namespace SephCassiopeia
{

    #region Initiliazation

    internal static class Cassiopeia
    {
        #region vars

        public static Obj_AI_Hero Player;
        public static Menu Config;
        private static SpellSlot IgniteSlot = SpellSlot.Summoner1;
        private static bool DontMove;
        private static float edelay = 0;
        private static float laste = 0;

        #endregion

        #region OnLoad

        public static void CassMain()
        {
            CustomEvents.Game.OnGameLoad += CassMain;
        }

        private static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 850f, TargetSelector.DamageType.Magical) },
            { SpellSlot.W, new Spell(SpellSlot.W, 850f, TargetSelector.DamageType.Magical) },
            { SpellSlot.E, new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Magical) },
            { SpellSlot.R, new Spell(SpellSlot.R, 825f, TargetSelector.DamageType.Magical) },
            { IgniteSlot, new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 550f) }
        };

        private static void InitializeSpells()
        {
            Spells[SpellSlot.Q].SetSkillshot(0.6f, 70f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            Spells[SpellSlot.W].SetSkillshot(0.5f, 275f, 2500f, false, SkillshotType.SkillshotCircle);
            Spells[SpellSlot.R].SetSkillshot(0.3f, 150f, float.MaxValue, false, SkillshotType.SkillshotCone);
        }

        private static void CassMain(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (ObjectManager.Player.BaseSkinName != "Cassiopeia")
            {
                return;
            }


            Config = CassiopeiaMenu.CreateMenu();

            Config.AddToMainMenu();

            InitializeSpells();


            AntiGapcloser.OnEnemyGapcloser += OnGapClose;

            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;

            Game.OnUpdate += OnUpdate;
            Game.OnUpdate += CheckKillable;
            Game.OnUpdate += AutoSpells;
            Drawing.OnDraw += OnDraw;
        }

        #endregion

        #endregion


        #region AutoSpells

        static void AutoSpells(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }
            if (SpellSlot.E.IsReady() && CassioUtils.Active("Combo.UseE"))
            {
                var etarg = HeroManager.Enemies.FirstOrDefault(h => h.IsValidTarget(Spells[SpellSlot.E].Range) && h.isPoisoned());
                if (etarg != null)
                {
                    if ((Utils.GameTimeTickCount - laste) > edelay)
                    {
                        Spells[SpellSlot.E].Cast(etarg);
                        laste = Utils.GameTimeTickCount;
                    }
                }
            }
            if (SpellSlot.R.IsReady() && CassioUtils.Active("Combo.UseR") && CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var targs = HeroManager.Enemies.Where(h => h.IsValidTarget(Spells[SpellSlot.R].Range));
                Dictionary<Vector3, double> Hitatpos = new Dictionary<Vector3, double>();
                Dictionary<Vector3, double> Hitatposfacing = new Dictionary<Vector3, double>();
                foreach (var t in targs)
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(t, true);
                    var enemshit = pred.CastPosition.GetEnemiesInRange(Spells[SpellSlot.R].Width);
                    var counthit = enemshit.Count;
                    var hitfacing = enemshit.Count(x => x.IsFacing(Player));
                    var anymovingtome = enemshit.Any(x => x.isMovingToMe());


                    if (pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.R") && anymovingtome)
                    {
                         Hitatposfacing.Add(pred.CastPosition, hitfacing);
                    }
                    if (CassioUtils.Active("Combo.UseRNF") && pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.R"))
                    {
                        Hitatpos.Add(pred.CastPosition, counthit);
                    }
                }
                if (Hitatposfacing.Any())
                {
                    var bestpos = Hitatposfacing.Find(pos => pos.Value.Equals(Hitatposfacing.Values.Max())).Key;
                    if (bestpos.IsValid() && bestpos.CountEnemiesInRange(Spells[SpellSlot.R].Width) >= 1)
                    {
                        Spells[SpellSlot.R].Cast(bestpos);
                    }
                }
                else if (Hitatpos.Any() && CassioUtils.Active("Combo.UseRNF") &&
                         CassioUtils.GetSlider("Combo.Rcountnf") >= Hitatpos.Values.Max())
                {
                    var bestposnf = Hitatpos.Find(pos => pos.Value.Equals(Hitatpos.Values.Max())).Key;
                    if (bestposnf.IsValid() && bestposnf.CountEnemiesInRange(Spells[SpellSlot.R].Width) >= 1)
                    {
                        Spells[SpellSlot.R].Cast(bestposnf);
                    }
                }
            }   
    }
            

        
        #endregion
        #region OnUpdate

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }

            edelay = CassioUtils.GetSlider("Combo.edelay");

            Killsteal();

            var target = TargetSelector.GetTarget(
                Spells[SpellSlot.Q].Range, TargetSelector.DamageType.Magical, true, CassiopeiaMenu.BlackList);

            if (target != null && CassioUtils.ActiveKeyBind("Keys.HarassT"))
            {
                Harass(target);
            }
            var Orbwalkmode = CassiopeiaMenu.Orbwalker.ActiveMode;
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
            if (Spells[SpellSlot.Q].IsReady() && CassioUtils.Active("Combo.UseQ"))
            {
                var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
                if (pred.Hitchance > CassioUtils.GetHitChance("Hitchance.Q"))
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.W].IsReady() && CassioUtils.Active("Combo.UseW"))
            {
                var pred = Spells[SpellSlot.W].GetPrediction(target, true);
                if (pred.Hitchance > CassioUtils.GetHitChance("Hitchance.W"))
                {
                    Spells[SpellSlot.W].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.E].IsReady() && CassioUtils.Active("Combo.UseE"))
            {
                if (target.isPoisoned())
                {
                    if ((Utils.GameTimeTickCount - laste) > edelay)
                    {
                        Spells[SpellSlot.E].Cast(target);
                        laste = Utils.GameTimeTickCount;
                    }
                }
      
            }
            /*
            if (SpellSlot.R.IsReady() && CassioUtils.Active("Combo.UseR"))
            {
                    var pred = Spells[SpellSlot.R].GetPrediction(target, true);
                    var enemshit = pred.CastPosition.GetEnemiesInRange(Spells[SpellSlot.R].Width);
                    var counthit = enemshit.Count;
                    var hitfacing = enemshit.Count(x => x.IsFacing(Player));
                    var anymovingtome = enemshit.Any(x => x.isMovingToMe());
                    if (hitfacing >= CassioUtils.GetSlider("Combo.Rcount") && pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.R") && anymovingtome || CassioUtils.Active("Combo.UseRNF") && counthit >= CassioUtils.GetSlider("Combo.Rcountnf") && pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.R"))
                    {
                         Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                }
             * */
            }

        #endregion


        #region ExaminetargetWP

        static bool isMovingToMe(this Obj_AI_Base target)
        {
            var x = target.GetWaypoints().Last();
            var mypos2d = Player.ServerPosition.To2D();
            if (Vector2.Distance(mypos2d, x) <= Vector2.Distance(mypos2d, target.ServerPosition.To2D()))
            {
               
                return true;

            }
            return false;
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

            if (SpellSlot.Q.IsReady() && CassioUtils.Active("Waveclear.UseQ"))
            {
                var qminions =
                    Minions.Where(
                        m =>
                            Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range &&
                            m.IsValidTarget());
                MinionManager.FarmLocation QLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        qminions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.Q].Width,
                        Spells[SpellSlot.Q].Range);
                if (QLocation.MinionsHit > 1)
                {
                    Spells[SpellSlot.Q].Cast(QLocation.Position);
                }
            }


            if (SpellSlot.W.IsReady() && CassioUtils.Active("Waveclear.UseW"))
            {
                var wminions =
                    Minions.Where(
                        m =>
                            Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.W].Range &&
                            m.IsValidTarget());
                MinionManager.FarmLocation WLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        wminions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.W].Width,
                        Spells[SpellSlot.W].Range);
                if (WLocation.MinionsHit > 1)
                {
                    Spells[SpellSlot.W].Cast(WLocation.Position);
                }
            }

            if (SpellSlot.E.IsReady() && CassioUtils.Active("Waveclear.UseE"))
            {

                var KillableMinionE = Minions.FirstOrDefault(m => m.Health < Player.GetSpellDamage(m, SpellSlot.E));
                
                if (KillableMinionE != null)
                {
                    Spells[SpellSlot.E].Cast(KillableMinionE);
                }
            }

            if (SpellSlot.R.IsReady() && CassioUtils.Active("Waveclear.UseR"))
            {
                MinionManager.FarmLocation RLocation =
                    MinionManager.GetBestLineFarmLocation(
                        Minions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.R].Width,
                        Spells[SpellSlot.R].Range);
                if (RLocation.MinionsHit > CassioUtils.GetSlider("Waveclear.Rcount"))
                {
                    Spells[SpellSlot.R].Cast(RLocation.Position);
                }
            }
        }
    #endregion Waveclear


        #region MixedModeLogic

        static void MixedModeLogic(Obj_AI_Hero target, bool isMixed)
        {
   
            if (!CassioUtils.Active("Farm.Enable") || Player.ManaPercent < CassioUtils.GetSlider("Farm.Mana"))
            {
                return;
            }

            var Minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            (Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range));

            if (!Minions.Any())
            {
                return;
            }
            if (SpellSlot.Q.IsReady() && CassioUtils.Active("Farm.UseQ"))
            {
                var KillableMinionsQ = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.Q) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) > Player.AttackRange);
                if (KillableMinionsQ.Any())
                {
                    Spells[SpellSlot.Q].Cast(KillableMinionsQ.FirstOrDefault().ServerPosition);
                }
            }
            if (SpellSlot.W.IsReady() && CassioUtils.Active("Farm.UseW"))
            {
                var KillableMinionsW = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.W) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) > Player.AttackRange);
                if (KillableMinionsW.Any())
                {
                    Spells[SpellSlot.W].Cast(KillableMinionsW.FirstOrDefault().ServerPosition);
                }
            }
            if (SpellSlot.E.IsReady() && CassioUtils.Active("Farm.UseE"))
            {

                var KillableMinionE = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.E));
                if (KillableMinionE.Any())
                {
                    Spells[SpellSlot.E].Cast(KillableMinionE.FirstOrDefault());
                }
            }
        }
        #endregion MixedModeLogic

        #region Harass

        static void Harass(Obj_AI_Hero target)
        {
            if (!CassioUtils.ActiveKeyBind("Keys.HarassT"))
            {
                return;
            }
            if (Spells[SpellSlot.Q].IsReady() && CassioUtils.Active("Harass.UseQ"))
            {
                var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
                if (pred.Hitchance > CassioUtils.GetHitChance("Hitchance.Q"))
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.W].IsReady() && CassioUtils.Active("Harass.UseW"))
            {
                var pred = Spells[SpellSlot.W].GetPrediction(target, true);
                if (pred.Hitchance > CassioUtils.GetHitChance("Hitchance.W"))
                {
                    Spells[SpellSlot.W].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.E].IsReady() && CassioUtils.Active("Harass.UseE"))
            {
                if (target.isPoisoned())
                {
                    if ((Utils.GameTimeTickCount - laste) > edelay)
                    {
                        Spells[SpellSlot.E].Cast(target);
                        laste = Utils.GameTimeTickCount;
                    }
                }
            }
            }
        
        #endregion

        #region KillSteal

        static void Killsteal()
        {
            if (!CassioUtils.Active("Killsteal"))
            {
                return;
            }
            var targets = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsInvulnerable & !x.IsZombie);

            if (SpellSlot.Q.IsReady() && CassioUtils.Active("Killsteal.UseQ"))
            {
                Obj_AI_Hero qtarget =
                    targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.Q].Range)
                    .MinOrDefault(x => x.Health);
                if (qtarget != null)
                {
                    var qdmg = Player.GetSpellDamage(qtarget, SpellSlot.Q);
                    if (qtarget.Health < qdmg)
                    {
                        var pred = Spells[SpellSlot.Q].GetPrediction(qtarget, false);
                        if (pred != null && pred.Hitchance > HitChance.Medium)
                        {
                            Spells[SpellSlot.Q].Cast(pred.CastPosition);
                            return;
                        }
                    }
                }
            }

            if (SpellSlot.W.IsReady() && CassioUtils.Active("Killsteal.UseW"))
            {
                Obj_AI_Hero wtarget =
                    targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.W].Range)
                        .MinOrDefault(x => x.Health);
                if (wtarget != null)
                {
                    var wdmg = Player.GetSpellDamage(wtarget, SpellSlot.W);
                    if (wtarget.Health < wdmg)
                    {
                        var pred = Spells[SpellSlot.Q].GetPrediction(wtarget, false);
                        if (pred != null && pred.Hitchance > HitChance.Medium)
                        {
                            Spells[SpellSlot.W].Cast(pred.CastPosition);
                            return;
                        }
                    }
                }
            }
            if (SpellSlot.E.IsReady() && CassioUtils.Active("Killsteal.UseE"))
            {

                Obj_AI_Hero etarget =
                    targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.E].Range)
                    .MinOrDefault(x => x.Health);
                    if (etarget != null)
                    {
                            var edmg = Player.GetSpellDamage(etarget, SpellSlot.E);
                            if (etarget.Health < edmg)
                            {
                                if ((Utils.GameTimeTickCount - laste) > edelay)
                                {
                                    Spells[SpellSlot.E].Cast(etarget);
                                    laste = Utils.GameTimeTickCount;
                                }
                            }
                        }
                }
            


            if (SpellSlot.R.IsReady() && CassioUtils.Active("Killsteal.UseR"))
            {
           
                var targ = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Spells[SpellSlot.R].Range && x.Health < Player.GetSpellDamage(x, SpellSlot.R));

                if (targ != null)
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(targ, true);
                    if (pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.R"))
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);

                    }
                }
            }

            if (Spells[IgniteSlot].IsReady() && CassioUtils.Active("Killsteal.UseIgnite"))
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


        #region PoisonCheck

        static bool isPoisoned(this Obj_AI_Base target)
        {
            return target.HasBuffOfType(BuffType.Poison);
        }
        #endregion 

        #region Killable

        public static List<Obj_AI_Hero> Killable = new List<Obj_AI_Hero>();
        private static void CheckKillable(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            Killable.Clear();
            foreach (var hero in HeroManager.Enemies)
            {
                if (hero.IsValidTarget(10000) && hero.canKill())
                {
                    Killable.Add(hero);
                }
            }
        }

        public static bool canKill(this Obj_AI_Hero target)
        {
            double totaldmgavailable = 0;
            if (SpellSlot.Q.IsReady() && CassioUtils.Active("Combo.UseQ"))
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (SpellSlot.E.IsReady() && CassioUtils.Active("Combo.UseE"))
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (SpellSlot.R.IsReady() && CassioUtils.Active("Combo.UseR"))
            {
                totaldmgavailable += Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (Spells[IgniteSlot].IsReady() && CassioUtils.Active("Killsteal.UseIgnite"))
            {
                totaldmgavailable += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }
            return totaldmgavailable > target.Health;
        }
        #endregion 


        #region AntiGapcloser

        static void OnGapClose(ActiveGapcloser args)
        {
            if (Player.IsDead)
            {
                return;
            }
            var sender = args.Sender;

            if (CassioUtils.Active("Interrupter.AntiGapClose") && sender.IsValidTarget())
            {
                if (CassioUtils.Active("Interrupter.AG.UseR") && Vector3.Distance(args.End, Player.ServerPosition) <= Spells[SpellSlot.R].Range && sender.IsFacing(Player))
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(sender);
                    if (pred.Hitchance >= HitChance.VeryHigh && sender.IsFacing(Player))
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                }
            }
        }
        #endregion


        #region Interrupter
        static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (sender.IsValidTarget())
            {
                if (CassioUtils.Active("Interrupter.UseR") && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.R].Range && sender.IsFacing(Player))
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(sender);
                    if (pred.Hitchance >= HitChance.VeryHigh && sender.IsFacing(Player))
                    {
                        Spells[SpellSlot.R].Cast(pred.CastPosition);
                    }
                }
            }
        }
        #endregion


        #region Drawing

        static void OnDraw(EventArgs args)
        {
            if (Player.IsDead || CassioUtils.Active("Drawing.Disable"))
            {
                return;
            }
            foreach (var x in Killable)
            {
                var pos = Drawing.WorldToScreen(x.ServerPosition);
                Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.Azure, "Killable");
            }

            if (CassioUtils.Active("Drawing.DrawQ"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.Q].Range, System.Drawing.Color.White);
            }
            if (CassioUtils.Active("Drawing.DrawW"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.Q].Range, System.Drawing.Color.Green);
            }
            if (CassioUtils.Active("Drawing.DrawE"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.E].Range, System.Drawing.Color.RoyalBlue);
            }
            if (CassioUtils.Active("Drawing.DrawR"))
            {
                Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.R].Range, System.Drawing.Color.Red);
            }

        }
        #endregion

    }
   }



    

