using System;
using System.Linq;
using Evade;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;


namespace YasuoPro
{

    //Credits to Kortatu/Esk0r for his work on Evade which this assembly relies on heavily!

    internal class Yasuo : Helper
    {
        public Obj_AI_Hero CurrentTarget;


        public Yasuo()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }
        
        void OnLoad(EventArgs args)
        {
            if (Yasuo.CharData.BaseSkinName != "Yasuo")
            {
                return;
            }

            Game.PrintChat("<font color='#1d87f2'>YasuoPro by Seph Loaded. Good Luck!</font>");

            InitItems();
            InitSpells();
            YasuoMenu.Init(this);
            Orbwalker.RegisterCustomMode("Flee", YasuoMenu.KeyCode("Z"));
            Program.Init();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnGapClose;
            Interrupter2.OnInterruptableTarget += OnInterruptable;
            Obj_AI_Base.OnProcessSpellCast += TargettedDanger.SpellCast;
        }

        void OnUpdate(EventArgs args)
        {
            if (Yasuo.IsDead)
            {
                return;
            }

            var Fleeing = Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.CustomMode;

            if (GetBool("Misc.AutoR") && !Fleeing)
            {
                CastR(GetSlider("Misc.RMinHit"));
            }

            if (GetBool("Killsteal.Enabled") && !Fleeing)
            {
                Killsteal();
            }

            if (GetKeyBind("Harass.KB") && !Fleeing)
            {
                Harass();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    Mixed();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    LHSkills();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    Orbwalker.SetAttack(true);
                    Waveclear();
                    break;
                case Orbwalking.OrbwalkingMode.CustomMode:
                    Flee();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
            }
        }
        
        void OnDraw(EventArgs args)
        { 
            
            if (Debug)
            {
                Drawing.DrawCircle(DashPosition.To3D(), Yasuo.BoundingRadius, System.Drawing
                    .Color.Chartreuse);
            }
            

            if (Yasuo.IsDead || !GetBool("Drawing.Active"))
            {
                return;
            }

            var drawq = GetCircle("Drawing.DrawQ");
            var drawe = GetCircle("Drawing.DrawE");
            var drawr = GetCircle("Drawing.DrawR");

            if (drawq.Active)
            {
                Render.Circle.DrawCircle(Yasuo.Position, Qrange, drawq.Color);
            }
            if (drawe.Active)
            {
                Render.Circle.DrawCircle(Yasuo.Position, Spells[E].Range, drawe.Color);
            }
            if (drawr.Active)
            {
                Render.Circle.DrawCircle(Yasuo.Position, Spells[R].Range, drawr.Color);
            }
        }
    


        void Combo()
        {
            CurrentTarget = TargetSelector.GetTarget(Spells[R].Range, TargetSelector.DamageType.Physical);

            if (GetBool("Combo.UseQ"))
            {
                CastQ(CurrentTarget);
            }

            if (GetBool("Combo.UseE"))
            {
                CastE(CurrentTarget);
            }

            if (GetBool("Combo.StackQ") && !TornadoReady && !CurrentTarget.IsValidEnemy(Spells[Q].Range))
            {
                var closest =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(x => x.IsValidEnemy(Spells[Q].Range))
                        .MinOrDefault(x => x.Distance(Yasuo));

                var pred = Spells[Q].GetPrediction(closest);
                if (pred.Hitchance >= HitChance.Low)
                {
                    Spells[Q].Cast(closest.ServerPosition);
                }
            }

            if (GetBool("Combo.UseR"))
            {
                CastR(GetSlider("Combo.RMinHit"));
            }
            if (GetBool("Combo.UseIgnite"))
            {
                CastIgnite();
            }

            if (GetBool("Items.Enabled"))
            {
                if (GetBool("Items.UseTIA"))
                {
                    Tiamat.Cast(null);
                }
                if (GetBool("Items.UseHDR"))
                {
                    Hydra.Cast(null);
                }
                if (GetBool("Items.UseBRK") && CurrentTarget != null)
                {
                    Blade.Cast(CurrentTarget);
                }
                if (GetBool("Items.UseBLG") && CurrentTarget != null)
                {
                    Bilgewater.Cast(CurrentTarget);
                }
                if (GetBool("Items.UseYMU"))
                {
                    Youmu.Cast(null);
                }
            }
        }

        void CastQ(Obj_AI_Hero target)
        {
            if (Spells[Q].IsReady() && target != null && target.IsInRange(Qrange))
            {
                UseQ(target, GetHitChance("Hitchance.Q"));
            }
        }

        void CastE(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (SpellSlot.E.IsReady())
                {
                    if (DashCount >= 1 && target.IsDashable() &&
                        (GetBool("Combo.ETower") || !GetDashPos(target).PointUnderEnemyTurret()))
                    {
                        Spells[E].CastOnUnit(target);
                        return;
                    }

                    if (DashCount == 0)
                    {
                        var dist = Yasuo.Distance(target);

                        var bestminion =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(
                                    x =>
                                         x.IsDashable() 
                                         && target.Distance(GetDashPos(x)) < dist &&
                                        (GetBool("Combo.ETower") || !GetDashPos(x).PointUnderEnemyTurret()))
                                .OrderBy(x => Vector2.Distance(GetDashPos(x), target.ServerPosition.To2D()))
                                .FirstOrDefault();
                        if (bestminion != null)
                        {
                            Spells[E].CastOnUnit(bestminion);
                        }

                        else
                        {
                            var minion =
                              ObjectManager.Get<Obj_AI_Base>()
                                  .Where(x => x.IsDashable() && !GetDashPos(x).PointUnderEnemyTurret())
                                  .OrderBy(x => GetDashPos(x).Distance(target)).FirstOrDefault();

                            if (minion != null && GetDashPos(minion).IsCloser(target))
                            {
                                Spells[E].CastOnUnit(minion);
                            }
                        }
                    }

                    else
                    {
                        var minion =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(x =>  x.IsDashable() && (GetBool("Combo.ETower") || !GetDashPos(x).PointUnderEnemyTurret()))
                                .OrderBy(x => GetDashPos(x).Distance(target)).FirstOrDefault();

                        if (minion != null && GetDashPos(minion).IsCloser(target))
                        {
                            Spells[E].CastOnUnit(minion);
                        }
                    }
                }
            }
        }

        void CastR(int minhit = 1)
        {
            if (SpellSlot.R.IsReady())
            {
                UltMode ultmode = GetUltMode();

                IOrderedEnumerable<Obj_AI_Hero> ordered = null;

                if (KnockedUp.Count() == 1 && KnockedUp.FirstOrDefault().isBlackListed())
                {
                    return;
                }


                if (ultmode == UltMode.Health)
                {
                     ordered = KnockedUp.OrderBy(x => x.Health).ThenByDescending(x => TargetSelector.GetPriority(x)).ThenByDescending(x => x.CountEnemiesInRange(350));
                }

                if (ultmode == UltMode.Priority)
                {
                    ordered = KnockedUp.OrderByDescending(x => TargetSelector.GetPriority(x)).ThenBy(x => x.Health).ThenByDescending(x => x.CountEnemiesInRange(350));
                }

                if (ultmode == UltMode.EnemiesHit)
                {
                    ordered = KnockedUp.OrderByDescending(x => x.CountEnemiesInRange(350)).ThenByDescending(x => TargetSelector.GetPriority(x)).ThenBy(x => x.Health);
                }

                if (GetBool("Combo.UltOnlyKillable"))
                {
                    var killable = ordered.FirstOrDefault(x => !x.isBlackListed() && x.Health <= Yasuo.GetSpellDamage(x, SpellSlot.R) && (GetBool("Combo.UltTower") || !x.Position.To2D().PointUnderEnemyTurret()));
                    if (killable != null)
                    {
                        Spells[R].CastOnUnit(killable);
                        return;
                    }
                }

                if (GetBool("Combo.RPriority"))
                {
                    var best = ordered.Find(x => !x.isBlackListed() && TargetSelector.GetPriority(x) == 5 && (GetBool("Combo.UltTower") || !x.Position.To2D().PointUnderEnemyTurret()));
                    if (best != null)
                    {
                        Spells[R].CastOnUnit(best);
                        return;
                    }
                }

                if (ordered.Count() >= minhit)
                {
                    var best2 = ordered.FirstOrDefault(x => !x.isBlackListed() && (GetBool("Combo.UltTower") || !x.Position.To2D().PointUnderEnemyTurret()));
                    if (best2 != null)
                    {
                        Spells[R].CastOnUnit(best2);
                    }
                    return;
                }

            }
        }


        void Flee()
        {
            Orbwalker.SetAttack(false);
            if (SpellSlot.Q.IsReady() && TornadoReady)
            {
                var qtarg = TargetSelector.GetTarget(Spells[Q2].Range, TargetSelector.DamageType.Physical);
                if (qtarg != null)
                {
                    Spells[Q2].Cast(qtarg.ServerPosition);
                }
            }

            if (FleeMode == FleeType.ToCursor)
            {
                Orbwalker.SetOrbwalkingPoint(Game.CursorPos);

                if (Spells[E].IsReady())
                {
                    var dashtarg =
                        ObjectManager.Get<Obj_AI_Base>()
                            .Where(x => x.IsDashable())
                            .MinOrDefault(x => GetDashPos(x).Distance(Game.CursorPos));

                    if (dashtarg != null && GetDashPos(dashtarg).Distance(Game.CursorPos) < Yasuo.Distance(Game.CursorPos))
                    {
                        Spells[E].Cast(dashtarg);

                        if (SpellSlot.Q.IsReady() && !TornadoReady)
                        {
                           Spells[Q].Cast(dashtarg.ServerPosition);
                        }
                    }
                }
            }

            if (FleeMode == FleeType.ToNexus)
            {
                var nexus = ObjectManager.Get<Obj_Shop>().FirstOrDefault(x => x.IsAlly);
                if (nexus != null)
                {
                    Orbwalker.SetOrbwalkingPoint(nexus.Position);
                    var bestminion = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsDashable()).MinOrDefault(x => GetDashPos(x).Distance(nexus.Position));
                    if (bestminion != null && GetDashPos(bestminion).Distance(nexus.Position) < Yasuo.Distance(nexus.Position))
                    {
                        Spells[E].Cast(bestminion);
                        if (SpellSlot.Q.IsReady() && !TornadoReady)
                        {
                            Spells[Q].Cast(bestminion.ServerPosition);
                        }
                    }
                }
            }

            if (FleeMode == FleeType.ToAllies)
            {
                Obj_AI_Base bestally = HeroManager.Allies.Where(x => !x.IsMe && x.CountEnemiesInRange(300) == 0).MinOrDefault(x => x.Distance(Yasuo));
                if (bestally == null)
                {
                    bestally =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsValidAlly(3000))
                            .MinOrDefault(x => x.Distance(Yasuo));
                }

                if (bestally != null)
                {
                    Orbwalker.SetOrbwalkingPoint(bestally.ServerPosition);
                    if (Spells[E].IsReady())
                    {
                        var besttarget =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(x => x.IsDashable())
                                .MinOrDefault(x => GetDashPos(x).Distance(bestally));
                        if (besttarget != null)
                        {
                            Spells[E].Cast(besttarget);
                            if (SpellSlot.Q.IsReady() && !TornadoReady)
                            {
                                Spells[Q].Cast(besttarget.ServerPosition);
                            }
                        }
                    }
                }

                else
                {
                    var nexus = ObjectManager.Get<Obj_Shop>().FirstOrDefault(x => x.IsAlly);
                    if (nexus != null)
                    {
                        Orbwalker.SetOrbwalkingPoint(nexus.Position);
                        var bestminion = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsDashable()).MinOrDefault(x => GetDashPos(x).Distance(nexus.Position));
                        if (bestminion != null && GetDashPos(bestminion).Distance(nexus.Position) < Yasuo.Distance(nexus.Position))
                        {
                            Spells[E].Cast(bestminion);
                        }
                    }
                }
            }
        }



        void CastIgnite()
        {
            var target =
                HeroManager.Enemies.Find(
                    x =>
                        x.IsValidTarget(Spells[Ignite].Range) &&
                        Yasuo.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) >= x.Health);
            if (Spells[Ignite].IsReady() && target != null) { 
                Spells[Ignite].Cast(target);
            }
        }

        void Waveclear()
        {

            if (SpellSlot.Q.IsReady())
            {
                if (!TornadoReady && GetBool("Waveclear.UseQ"))
                {
                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsValidTarget(Spells[Q].Range)  && (x.Health - Yasuo.GetSpellDamage(x, SpellSlot.Q)  >= 0.15 * x.MaxHealth || x.QCanKill())).MaxOrDefault(x => x.MaxHealth);
                    if (minion != null)
                    {
                        Orbwalker.ForceTarget(minion);
                        Spells[Q].Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Waveclear.UseQ2"))
                {
                    var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(Spells[Q2].Range) && (x.Health - Yasuo.GetSpellDamage(x, SpellSlot.Q) >= 0.10 * x.MaxHealth || x.CanKill(SpellSlot.Q)));
                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Spells[Q2].Width, Spells[Q2].Range);
                    if (pred.MinionsHit >= GetSlider("Waveclear.Qcount"))
                    {
                        Orbwalker.ForceTarget(minions.FirstOrDefault());
                        Spells[Q2].Cast(pred.Position);
                    }
                }
            }

            if (SpellSlot.E.IsReady() && GetBool("Waveclear.UseE"))
            {
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsDashable() && ((GetBool("Waveclear.UseENK") && (!GetBool("Waveclear.Smart") || x.Health > GetProperEDamage(x) * 2f)) || x.Health < GetProperEDamage(x)) && (GetBool("Waveclear.ETower") || !GetDashPos(x).PointUnderEnemyTurret()));
                if (minion != null)
                {
                    Orbwalker.ForceTarget(minion);
                    Spells[E].Cast(minion);
                }
            }

            if (GetBool("Waveclear.UseItems"))
            {
                if (GetBool("Waveclear.UseTIA"))
                {
                    Tiamat.minioncount = GetSlider("Waveclear.MinCountHDR");
                    Tiamat.Cast(null, true);
                }
                if (GetBool("Waveclear.UseHDR"))
                {
                    Hydra.minioncount = GetSlider("Waveclear.MinCountHDR");
                    Hydra.Cast(null, true);
                }
                if (GetBool("Waveclear.UseYMU"))
                {
                    Youmu.minioncount = GetSlider("Waveclear.MinCountYOU");
                    Youmu.Cast(null, true);
                }
            }
        }


        void Killsteal()
        {
            if (SpellSlot.Q.IsReady() && GetBool("Killsteal.UseQ"))
            {
                var targ = HeroManager.Enemies.Find(x => x.CanKill(SpellSlot.Q) && x.IsInRange(Qrange));
                if (targ != null)
                {
                    UseQ(targ, GetHitChance("Hitchance.Q"));
                    return;
                }
            }

            if (SpellSlot.E.IsReady() && GetBool("Killsteal.UseE"))
            {
                var targ = HeroManager.Enemies.Find(x => x.CanKill(SpellSlot.E) && x.IsInRange(Spells[E].Range));
                if (targ != null)
                {
                    Spells[E].Cast(targ);
                    return;
                }
            }

            if (SpellSlot.R.IsReady() && GetBool("Killsteal.UseR"))
            {
                var targ = KnockedUp.Find(x => x.CanKill(SpellSlot.R) && x.IsValidTarget(Spells[R].Range) && !x.isBlackListed());
                if (targ != null)
                {
                    Spells[R].Cast(targ);
                    return;
                }
            }

            if (GetBool("Killsteal.UseIgnite"))
            {
                CastIgnite();
                return;
            }

            if (GetBool("Killsteal.UseItems"))
            {
                if (Tiamat.item.IsReady())
                {
                    var targ =
                        HeroManager.Enemies.Find(
                            x =>
                                x.IsValidTarget(Tiamat.item.Range) &&
                                x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Tiamat));
                    if (targ != null)
                    {
                        Tiamat.Cast(null);
                    }
                }
                if (Hydra.item.IsReady())
                {
                    var targ =
                      HeroManager.Enemies.Find(
                      x =>
                          x.IsValidTarget(Hydra.item.Range) &&
                          x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Tiamat));
                    if (targ != null)
                    {
                        Hydra.Cast(null);
                    }
                }
                if (Blade.item.IsReady())
                {
                    var targ = HeroManager.Enemies.Find(
                     x =>
                         x.IsValidTarget(Blade.item.Range) &&
                         x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Botrk));
                    if (targ != null)
                    {
                        Blade.Cast(targ);
                    }
                }
                if (Bilgewater.item.IsReady())
                {
                    var targ = HeroManager.Enemies.Find(
                                   x =>
                                       x.IsValidTarget(Bilgewater.item.Range) &&
                                       x.Health <= Yasuo.GetItemDamage(x, Damage.DamageItems.Bilgewater));
                    if (targ != null)
                    {
                        Bilgewater.Cast(targ);
                    }
                }
            }
        }

        void Harass()
        {
            var target = TargetSelector.GetTarget(Spells[Q2].Range, TargetSelector.DamageType.Physical);
            if (GetBool("Harass.UseQ") && SpellSlot.Q.IsReady() && target != null && target.IsInRange(Qrange))
            {
                UseQ(target, GetHitChance("Hitchance.Q"));
            }

            if (target != null && isHealthy && GetBool("Harass.UseE") && Spells[E].IsReady() && target.IsInRange(Spells[E].Range*3))
            {
                if (target.IsInRange(Spells[E].Range))
                {
                    Spells[E].CastOnUnit(target);
                    return;
                }

                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.Team != Yasuo.Team && x.IsInRange(Spells[E].Range))
                        .OrderBy(x => GetDashPos(x).Distance(target))
                        .FirstOrDefault();

                if (minion != null && GetBool("Harass.UseEMinion"))
                {
                    Spells[E].Cast(minion);
                }
            }
        }

        void Mixed()
        {
            if (GetBool("Harass.InMixed"))
            {
                Harass();
            }
            LHSkills();
        }

        void LHSkills()
        {
            if (SpellSlot.Q.IsReady())
            {
                if (!TornadoReady && GetBool("Farm.UseQ"))
                {
                    var minion =
                         ObjectManager.Get<Obj_AI_Minion>()
                             .FirstOrDefault(x => x.IsValidTarget(Spells[Q].Range) && x.QCanKill());
                    if (minion != null)
                    {
                        Spells[Q].Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Farm.UseQ2"))
                {
                    var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(Spells[Q2].Range) && (x.QCanKill()));
                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Spells[Q2].Width, Spells[Q2].Range);
                    if (pred.MinionsHit >= GetSlider("Farm.Qcount"))
                    {
                        Spells[Q2].Cast(pred.Position);
                    }
                }
            }

            if (Spells[E].IsReady() && GetBool("Farm.UseE"))
            {
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsDashable() && x.CanKill(SpellSlot.E) && (GetBool("Waveclear.ETower") || !GetDashPos(x).PointUnderEnemyTurret()));
                if (minion != null)
                {
                    Orbwalker.ForceTarget(minion);
                    Spells[E].Cast(minion);
                }
            }
        }



        void OnGapClose(ActiveGapcloser args)
        {
            if (GetBool("Misc.AG") && TornadoReady && Yasuo.Distance(args.End) <= 500)
            {
                var pred = Spells[Q2].GetPrediction(args.Sender);
                if (pred.Hitchance >= GetHitChance("Hitchance.Q"))
                {
                    Spells[Q2].Cast(pred.CastPosition);
                }
            }
        }

        void OnInterruptable(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (GetBool("Misc.Interrupter") && TornadoReady && Yasuo.Distance(sender.ServerPosition) <= 500)
            {
                if (args.EndTime >= Spells[Q2].Delay)
                {
                    Spells[Q2].Cast(sender.ServerPosition);
                }
            }
        }
    }
}
