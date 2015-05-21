#region imports
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion;


namespace SephCassiopeia
{

    #region Initiliazation

    internal static class Cassiopeia
    {
        #region vars

        public static Obj_AI_Hero Player;
        private static Obj_AI_Hero target;
        public static Menu Config;
        private static SpellSlot IgniteSlot = SpellSlot.Summoner1;
        private static bool DontMove;
        private static float edelay = 0;
        private static float laste = 0;
        private static int[] skillorder = { 1, 3, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };

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

          
            new CommonAutoLevel(skillorder);
            
            AntiGapcloser.OnEnemyGapcloser += OnGapClose;

            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;

            Game.OnUpdate += OnUpdate;
            Game.OnUpdate += CheckKillable;
            Game.OnUpdate += AutoSpells;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += BeforeAuto;
        }

        #endregion

        #endregion

        #region BeforeAuto

        private static void BeforeAuto(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!CassioUtils.Active("Combo.Useauto") && CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                    args.Process = false;
                    return;
                
            }
            if (CassioUtils.Active("Combo.Disableautoifspellsready") && CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (SpellSlot.Q.IsReady() || SpellSlot.W.IsReady() || SpellSlot.E.IsReady() || SpellSlot.R.IsReady())
                {
                    args.Process = false;
                    return;
                }
            }
            if (!CassioUtils.Active("Waveclear.Useauto") && CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                    args.Process = false;
                    return;
            }
            if (!CassioUtils.Active("Farm.Useauto") && (CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit))
            {
                args.Process = false;
                return;
            }


        }

        #endregion

        #region AutoSpells

        static void AutoSpells(EventArgs args)
        {
            if (Player.IsDead || Player.recalling())
            {
                return;
            }
            if (SpellSlot.E.IsReady() && CassioUtils.Active("Combo.UseE") && (CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || CassioUtils.Active("Misc.autoe")))
            {
                Obj_AI_Hero etarg;
                etarg = target;
                if (etarg == null)
                {
                    etarg = HeroManager.Enemies.FirstOrDefault(h => h.IsValidTarget(Spells[SpellSlot.E].Range) && h.isPoisoned() && !h.IsInvulnerable && !h.IsZombie);
                }
                if (etarg != null && etarg.isPoisoned())
                {
                    if ((Utils.GameTimeTickCount - laste) > edelay)
                    {
                        Spells[SpellSlot.E].Cast(etarg);
                        laste = Utils.GameTimeTickCount;
                    }
                }
            }


            var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(Spells[SpellSlot.R].Range) && !x.IsZombie).OrderBy(x => x.Health);

            foreach (var targ in targets) {
            if (SpellSlot.R.IsReady() && CassioUtils.Active("Combo.UseR") &&
                CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (targ.IsFacing(Player))
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(targ);
                    if (pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.R"))
                    {
                        int enemhitpred = 0;
                        int enemfacingpred = 0;
                        foreach (var hero in targets)
                        {
                            if (Spells[SpellSlot.R].WillHit(hero, pred.CastPosition, 0))
                            {
                                enemhitpred++;

                                if (hero.IsFacing(Player))
                                {
                                    enemfacingpred++;
                                }
                            }
                        }

                        if (enemfacingpred >= CassioUtils.GetSlider("Combo.Rcount"))
                        {
                            Spells[SpellSlot.R].Cast(pred.CastPosition);
                            return;
                        }
                        if (enemhitpred >= CassioUtils.GetSlider("Combo.Rcountnf") && CassioUtils.Active("Combo.UseRNF"))
                        {
                            Spells[SpellSlot.R].Cast(pred.CastPosition);
                            return;
                        }
                    }
                }
                }
            }

            /* © ® ™ Work on patented algorithms in the future! © ® ™ 
            if (SpellSlot.R.IsReady() && CassioUtils.Active("Combo.UseR") && CassiopeiaMenu.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var easycheck =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                            !x.IsInvulnerable && !x.IsZombie && x.IsValidTarget(Spells[SpellSlot.R].Range) &&
                            x.IsFacing(Player) && x.isImmobile());

                if (easycheck != null)
                {
                    Spells[SpellSlot.R].Cast(easycheck.ServerPosition);
                    DontMove = true;
                    Utility.DelayAction.Add(100, () => DontMove = false);
                    return;
                }
                var targs = HeroManager.Enemies.Where(h => h.IsValidTarget(Spells[SpellSlot.R].Range));
                Dictionary<Vector3, double> Hitatpos = new Dictionary<Vector3, double>();
                Dictionary<Vector3, double> Hitatposfacing = new Dictionary<Vector3, double>();
                foreach (var t in targs)
                {
                    var pred = Spells[SpellSlot.R].GetPrediction(t, true);
                    var enemshit = pred.CastPosition.GetEnemiesInRange(Spells[SpellSlot.R].Width);
                    var counthit = enemshit.Count;
                    var hitfacing = enemshit.Count(x => x.IsFacing(Player) && !x.IsDashing() && !x.IsZombie && !x.IsInvulnerable);
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
                        DontMove = true;
                        Utility.DelayAction.Add(100, () => DontMove = false);
                    }
                }
                else if (Hitatpos.Any() && CassioUtils.Active("Combo.UseRNF") &&
                         CassioUtils.GetSlider("Combo.Rcountnf") >= Hitatpos.Values.Max())
                {
                    var bestposnf = Hitatpos.Find(pos => pos.Value.Equals(Hitatpos.Values.Max())).Key;
                    if (bestposnf.IsValid() && bestposnf.CountEnemiesInRange(Spells[SpellSlot.R].Width) >= 1)
                    {
                        Spells[SpellSlot.R].Cast(bestposnf);
                        DontMove = true;
                        Utility.DelayAction.Add(100, () => DontMove = false);
                    }
                }
            
            }   
             */
           
    }
            
        #endregion

        #region Immobility Check

        private static bool isImmobile(this Obj_AI_Hero hero)
        {
            foreach (var buff in hero.Buffs)
            {
                if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt || buff.Type == BuffType.Charm ||
                    buff.Type == BuffType.Fear || buff.Type == BuffType.Knockup || buff.Type == BuffType.Polymorph ||
                    buff.Type == BuffType.Snare || buff.Type == BuffType.Suppression || buff.Type == BuffType.Flee ||
                    buff.Type == BuffType.Slow && target.MoveSpeed <= 0.90 * target.MoveSpeed)
                {
                    var tenacity = hero.PercentCCReduction;
                    var buffEndTime = buff.EndTime - (tenacity * (buff.EndTime - buff.StartTime));
                    var cctimeleft = buffEndTime - Game.Time;
                    if (cctimeleft > Game.Ping / 1000f + Spells[SpellSlot.R].Delay)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

          #region OnUpdate

            private static void OnUpdate(EventArgs args) {

            if (Player.IsDead || Player.recalling())
            {
                return;
            }


            edelay = CassioUtils.GetSlider("Combo.edelay");

            Killsteal();
            target = TargetSelector.GetTarget(Spells[SpellSlot.Q].Range, TargetSelector.DamageType.Magical, true, CassiopeiaMenu.BlackList);
            if (target != null && CassioUtils.ActiveKeyBind("Keys.HarassT") && Player.ManaPercent >= CassioUtils.GetSlider("Harass.Mana") && !target.IsInvulnerable && !target.IsZombie)
            {
                Harass(target);
            }
 
            var Orbwalkmode = CassiopeiaMenu.Orbwalker.ActiveMode;
            
            switch (Orbwalkmode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (target != null && !target.IsInvulnerable && !target.IsZombie)
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

            #region Recallcheck
            public static bool recalling(this Obj_AI_Hero unit)
            {
                return unit.Buffs.Any(buff => buff.Name.ToLower().Contains("recall") && buff.Name != "MasteryImprovedRecallBuff");
            }
            #endregion Recallcheck

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
            if (Vector2.Distance(mypos2d, x) <= Vector2.Distance(mypos2d, target.ServerPosition.To2D()) && target.GetWaypoints().Count >= 3 || !target.IsMoving)
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
                if (QLocation.MinionsHit >= 1)
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
                if (WLocation.MinionsHit >= 1)
                {
                    Spells[SpellSlot.W].Cast(WLocation.Position);
                }
            }

            if (SpellSlot.E.IsReady() && CassioUtils.Active("Waveclear.UseE"))
            {
                Obj_AI_Minion KillableMinionE = null;
                if (CassioUtils.Active("Waveclear.useekillable"))
                {
                    KillableMinionE = Minions.FirstOrDefault(m => m.Health < Player.GetSpellDamage(m, SpellSlot.E));
                }
                else
                {
                    KillableMinionE = Minions.OrderBy(x => x.Health).FirstOrDefault();
                }

                if (KillableMinionE != null)
                {
                    if (CassioUtils.Active("Waveclear.useepoison"))
                    {
                        if (KillableMinionE.isPoisoned())
                            Spells[SpellSlot.E].Cast(KillableMinionE);
                    }
                    else
                    {
                        Spells[SpellSlot.E].Cast(KillableMinionE);
                    }
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
                    DontMove = true;
                    Utility.DelayAction.Add(200, () => DontMove = false);
                }
            }
        }
    #endregion Waveclear


        #region MixedModeLogic

        static void MixedModeLogic(Obj_AI_Hero target, bool isMixed)
        {
            if (isMixed && CassioUtils.Active("Harass.InMixed") && Player.ManaPercent > CassioUtils.GetSlider("Harass.Mana"))
            {
                if (target != null && !target.IsInvulnerable && !target.IsZombie)
                {
                    Harass(target);
                }
            }

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
                var KillableMinionsQ = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.Q));
                if (KillableMinionsQ.Any())
                {
                    Spells[SpellSlot.Q].Cast(KillableMinionsQ.FirstOrDefault().ServerPosition);
                }
            }
            if (SpellSlot.W.IsReady() && CassioUtils.Active("Farm.UseW"))
            {
                var KillableMinionsW = Minions.Where(m => m.Health < Player.GetSpellDamage(m, SpellSlot.W));
                if (KillableMinionsW.Any())
                {
                    Spells[SpellSlot.W].Cast(KillableMinionsW.FirstOrDefault().ServerPosition);
                }
            }
            if (SpellSlot.E.IsReady() && CassioUtils.Active("Farm.UseE"))
            {

                var KillableMinionE = Minions.FirstOrDefault(m => m.Health < Player.GetSpellDamage(m, SpellSlot.E));
                if (KillableMinionE != null)
                {
                    if (CassioUtils.Active("Farm.useepoison"))
                    {
                        if (KillableMinionE.isPoisoned())
                            Spells[SpellSlot.E].Cast(KillableMinionE);
                    }
                    else
                    {
                        Spells[SpellSlot.E].Cast(KillableMinionE);
                    }
                }
            }
        }
        #endregion MixedModeLogic

        #region Harass

        static void Harass(Obj_AI_Hero target)
        {
  
            if (Spells[SpellSlot.Q].IsReady() && CassioUtils.Active("Harass.UseQ"))
            {
                var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
                if (pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.Q"))
                {
                    Spells[SpellSlot.Q].Cast(pred.CastPosition);
                }
            }
            if (Spells[SpellSlot.W].IsReady() && CassioUtils.Active("Harass.UseW"))
            {
                var pred = Spells[SpellSlot.W].GetPrediction(target, true);
                if (pred.Hitchance >= CassioUtils.GetHitChance("Hitchance.W"))
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
                        if (pred != null && pred.Hitchance >= HitChance.Medium)
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
                        if (pred != null && pred.Hitchance >= HitChance.Medium)
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
           
                var targ = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Spells[SpellSlot.R].Range - 200 && x.Health < Player.GetSpellDamage(x, SpellSlot.R) && !x.IsZombie && !x.IsInvulnerable);

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
            if (Player.IsDead || Player.recalling())
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



    

