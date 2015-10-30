using System;
using System.Linq;
using Evade;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace YasuPro
{

    //Credits to Kortatu/Esk0r for his work on Evade which this assembly relies on heavily!

    internal class Yasuo : Helper
    {
        public Obj_AI_Hero CurrentTarget;


        public Yasuo()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private void OnLoad(EventArgs args)
        {
            if (Yasuo.CharData.BaseSkinName != "Yasuo")
            {
                return;
            }
            
            Game.PrintChat("YasuoPro Loaded!");

            InitSpells();
            YasuoMenu.Init(this);
            Program.Init();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            GameObject.OnCreate += OnCreate;
            AntiGapcloser.OnEnemyGapcloser += OnGapClose;
            Interrupter2.OnInterruptableTarget += OnInterruptable;
          //  Spellbook.OnCastSpell += CastSpell;
        }

        void CastSpell(Spellbook x, SpellbookCastSpellEventArgs args)
        {
            if (x.Owner.IsMe)
            {
                if (args.Slot == SpellSlot.E)
                {
                    var dpos = GetDashPos(args.Target as Obj_AI_Base);
                    DashPosition = dpos;
                }
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (Yasuo.IsDead)
            {
                return;
            }


            if (GetBool("Evade.WSS"))
            {
                Evade();
            }

            if (GetBool("Misc.AutoR"))
            {
                CastR(GetSlider("Misc.RMinHit"));
            }

            if (GetBool("Killsteal.Enabled"))
            {
                Killsteal();
            }

            if (GetKeyBind("Harass.KB"))
            {
                Harass();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Mixed();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LHSkills();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Waveclear();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
            }
        }

        void OnDraw(EventArgs args)
        {
            /*
            if (Debug)
            {
                Drawing.DrawCircle(DashPosition.To3D(), Yasuo.BoundingRadius, Color.Chartreuse);
            }
            */

            if (Yasuo.IsDead || !GetBool("Drawing.Active"))
            {
                return;
            }

            var drawq = GetCircle("Drawing.DrawQ");
            var drawe = GetCircle("Drawing.DrawE");
            var drawr = GetCircle("Drawing.DrawR");

            if (drawq.Active)
            {
                Render.Circle.DrawCircle(Yasuo.ServerPosition, Qrange, drawq.Color, 2);
            }
            if (drawe.Active)
            {
                Render.Circle.DrawCircle(Yasuo.ServerPosition, Spells[E].Range, drawe.Color, 2);
            }
            if (drawr.Active)
            {
                Render.Circle.DrawCircle(Yasuo.ServerPosition, Spells[R].Range, drawr.Color, 2);
            }
        }

        private void Combo()
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
            if (GetBool("Combo.UseR"))
            {
                CastR(GetSlider("Misc.RMinHit"));
            }
            if (GetBool("Combo.UseIgnite"))
            {
                CastIgnite();
            }
        }

        private void CastQ(Obj_AI_Hero target)
        {
            if (Spells[Q].IsReady() && target != null && target.IsInRange(Qrange))
            {
                UseQ(target, GetHitChance("Hitchance.Q"));
            }
        }

        private void CastE(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (SpellSlot.E.IsReady())
                {
                    if (DashCount >= 1 && target.IsDashable() && target.IsValidTarget(Spells[E].Range) &&
                        !GetDashPos(target).PointUnderEnemyTurret())
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
                                        x.IsValidTarget(Spells[E].Range) && x.IsDashable() 
                                         && target.Distance(GetDashPos(x)) < dist &&
                                        !GetDashPos(x).PointUnderEnemyTurret())
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
                                  .Where(x => x.IsValidTarget(Spells[E].Range) && x.IsDashable() && !GetDashPos(x).PointUnderEnemyTurret())
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
                                .Where(x => x.IsValidTarget(Spells[E].Range) && x.IsDashable() && !GetDashPos(x).PointUnderEnemyTurret())
                                .OrderBy(x => GetDashPos(x).Distance(target)).FirstOrDefault();

                        if (minion != null && GetDashPos(minion).IsCloser(target))
                        {
                            Spells[E].CastOnUnit(minion);
                        }
                    }

                }
            }
        }

        private void CastR(int minhit = 1)
        {
            if (Spells[R].IsReady())
            {
                Obj_AI_Hero BestHero = null;
                int amthit = 0;
                foreach (var knockedup in KnockedUp)
                {
                    if (!GetBool("Combo.UltTower") && knockedup.ServerPosition.To2D().PointUnderEnemyTurret())
                    {
                        continue;
                    }
                    var eneshit = knockedup.GetEnemiesInRange(350);

                    if (eneshit.Count >= amthit)
                    {
                        BestHero = knockedup;
                        amthit = eneshit.Count;
                    }
                }

                if (BestHero != null && KnockedUp.Count() >= minhit)
                {
                    Spells[R].CastOnUnit(BestHero);
                }
            }
        }

        private void Evade()
        {
            if (!GetBool("Evade.Enabled"))
            {
                return;
            }

            foreach (var skillshot in Program.DetectedSkillshots)
            {
                
                if (skillshot.Dodged)
                {
                    Game.PrintChat(skillshot.SpellData.SpellName + " " + "ddoged ");
                }
                
                if (skillshot.IsAboutToHit(250, Yasuo) &&
                    ((Program.NoSolutionFound ||
                      !Program.IsSafePath(Yasuo.GetWaypoints(), 250).IsSafe &&
                      !Program.IsSafe(Yasuo.Position.To2D()).IsSafe) || skillshot.SpellData.IsDangerous ||
                     skillshot.SpellData.DangerValue >= 3))
                {
                    if (!skillshot.Dodged && skillshot.SpellData.Type != SkillShotType.SkillshotCircle)
                    {
                        if (SpellSlot.W.IsReady() && GetBool("Evade.UseW") &&
                            skillshot.SpellData.CollisionObjects.Contains(CollisionObjectTypes.YasuoWall))
                        {
                            var castpos = Yasuo.ServerPosition.Extend(skillshot.MissilePosition.To3D(), 50);
                            var dist = Yasuo.Distance(skillshot.MissilePosition);
                            // var extpos = Yasuo.ServerPosition.To2D().Extend(skillshot.MissilePosition, dist * 0.25f);
                            //var castpos = Yasuo.ServerPosition.To2D() + Math.Abs(Spells[W].Range - (0.25f * dist))*(-skillshot.Direction);
                            bool WCasted = Spells[W].Cast(castpos);
                            Program.DetectedSkillshots.Remove(skillshot);
                            skillshot.Dodged = WCasted;
                            if (Debug && WCasted)
                            {
                                Game.PrintChat("Blocked " + skillshot.SpellData.SpellName + " with Windwall ");
                            }
                            return;
                        }
                        if (!skillshot.Dodged && SpellSlot.E.IsReady() && GetBool("Evade.UseE"))
                        {
                            var evadetarget =
                                ObjectManager.Get<Obj_AI_Base>()
                                    .Where(
                                        x =>
                                            x.IsValidTarget(Spells[E].Range) &&
                                            Program.IsSafe(GetDashPos(x)).IsSafe)
                                    .OrderBy(x => x.CountEnemiesInRange(400))
                                    .FirstOrDefault();
                            if (evadetarget != null)
                            {
                                Spells[E].CastOnUnit(evadetarget);
                                Program.DetectedSkillshots.Remove(skillshot);
                                skillshot.Dodged = true;
                                if (Debug)
                                {
                                    Game.PrintChat("Evading " + skillshot.SpellData.SpellName + " " + "using E to " + evadetarget.BaseSkinName + " " +
                                                    "with E dash");
                                }
                                return;
                            }
                        }
                    }

                    if (!skillshot.Dodged && skillshot.SpellData.Type == SkillShotType.SkillshotCircle)
                    {
                        if (SpellSlot.E.IsReady() && GetBool("Evade.UseE"))
                        {
                            var evadetarget =
                                ObjectManager.Get<Obj_AI_Base>()
                                    .Where(
                                        x =>
                                            x.IsValidTarget(Spells[E].Range) &&
                                            Program.IsSafe(GetDashPos(x)).IsSafe)
                                    .OrderBy(x => x.CountEnemiesInRange(400))
                                    .FirstOrDefault();
                            if (evadetarget != null)
                            {
                                Spells[E].CastOnUnit(evadetarget);
                                skillshot.Dodged = true;
                                Program.DetectedSkillshots.Remove(skillshot);
                                if (Debug)
                                {
                                    Game.PrintChat("Evading " + skillshot.SpellData.SpellName + " " + "using E to " + evadetarget.BaseSkinName + " " +
                                                   "with E dash");
                                }
                                return;
                            }
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
            if (SpellSlot.E.IsReady() && GetBool("Waveclear.UseE"))
            {
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsValidTarget(Spells[E].Range) && x.IsDashable() && !GetDashPos(x).PointUnderEnemyTurret() && (GetBool("Waveclear.UseENK") || x.Health < GetProperEDamage(x)));
                if (minion != null)
                {
                    Orbwalker.ForceTarget(minion);
                    Spells[E].Cast(minion);
                }
            }

            if (SpellSlot.Q.IsReady())
            {
                if (!TornadoReady && GetBool("Waveclear.UseQ"))
                {
                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .FirstOrDefault(x => x.IsValidTarget(Spells[Q].Range));
                    if (minion != null)
                    {
                        Orbwalker.ForceTarget(minion);
                        Spells[Q].Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Waveclear.UseQ2"))
                {
                    var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(Spells[Q2].Range));
                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Spells[Q2].Width, Spells[Q2].Range);
                    if (pred.MinionsHit >= 1)
                    {
                        Orbwalker.ForceTarget(minions.FirstOrDefault());
                        Spells[Q2].Cast(pred.Position);
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

            if (target != null && GetBool("Harass.UseE") && Spells[E].IsReady() && target.IsInRange(Spells[E].Range*3))
            {
                if (target.IsInRange(Spells[E].Range))
                {
                    Spells[E].CastOnUnit(target);
                    return;
                }

                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.Team != Yasuo.Team && x.IsInRange(Spells[E].Range))
                        .OrderBy(x => x.Distance(target))
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
                             .FirstOrDefault(x => x.IsValidTarget(Spells[Q].Range));
                    if (minion != null)
                    {
                        Spells[Q].Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Farm.UseQ2"))
                {
                    var minions = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(Spells[Q2].Range) && x.CanKill(SpellSlot.Q));
                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Spells[Q2].Width, Spells[Q2].Range);
                    if (pred.MinionsHit >= 1)
                    {
                        Spells[Q2].Cast(pred.Position);
                    }
                }
            }

            if (Spells[E].IsReady())
            {
                var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsValidTarget(Spells[E].Range) && x.Health < Yasuo.GetAutoAttackDamage(x, true));
                if (minion != null)
                {
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
                var targ = KnockedUp.Find(x => x.CanKill(SpellSlot.R) && x.IsValidTarget(Spells[R].Range));
                if (targ != null)
                {
                    Spells[R].Cast(targ);
                    return;
                }
            }

            if (GetBool("Killsteal.UseIgnite"))
            {
                CastIgnite();
            }
        }

        void OnCreate(GameObject sender, EventArgs args)
        {
            if (!GetBool("Evade.WTS"))
            {
                return;
            }

            var missile = sender as MissileClient;
            if (missile != null && missile.Target.IsMe && missile.IsValid)
            {
                if (DangerousSpell.Contains(missile.Name) || DangerousSpell.Contains(missile.SData.Name))
                {
                    var postocast = Yasuo.ServerPosition.Extend(-(missile.Orientation), 100);
                    Spells[W].Cast(postocast);
                    Game.PrintChat("Using SpellBlocker to Detect " + missile.Name + " or " + missile.SData.Name);
                }
            }
        }
    }
}
