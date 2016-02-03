using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SPrediction;
using Color = System.Drawing.Color;

namespace SephSyndra
{
    class Syndra : Helper
    {
        //Credits to Esk0r 

        public List<PolyUtils.Polygon> Polygons = new List<PolyUtils.Polygon>();

        public float LastWTick;

        static void Main(string[] args)
        {
            Syndra syn = new Syndra();
        }

        public Syndra()
        {
            CustomEvents.Game.OnGameLoad += Load;
        }

        void Load(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Syndra")
            {
                return;
            }

            this.OnLoad();
            Game.OnUpdate += OnUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnPauseAnimation += Obj_AI_Base_OnPauseAnimation;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            CustomEvents.Unit.OnDash += Unit_OnDash;
        }


        void Obj_AI_Base_OnPauseAnimation(Obj_AI_Base sender, Obj_AI_BasePauseAnimationEventArgs args)
        {
            if (Syndra.IsRecalling() || Syndra.IsDead)
            {
                return;
            }
            if (sender is Obj_AI_Minion && sender.Name == "Seed" && sender.Team == Syndra.Team)
            {
                GrabbedObject = sender;
            }
        }


        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Syndra.IsRecalling() || Syndra.IsDead)
            {
                return;
            }

            if (!sender.IsMe)
            {
                return;
            }

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && GetBool("c.e")) || (GetKeyBind("h.enabled") && GetBool("h.e")))
            {
                var name = args.SData.Name;
                var castpos = args.End;

                if (name == "SyndraQ" || name == "syndrawcast")
                {
                    var radius = 48;
                    var predictedposition = castpos.Extend(Syndra.ServerPosition, -Math.Max((1200 - castpos.Distance(Syndra.ServerPosition)), 700)).To2D();
                    var rect = new PolyUtils.Rectangle(castpos.To2D(), predictedposition, radius).ToPolygon();
                    Polygons.Add(rect);
                    var enemies = HeroManager.Enemies.Where(x => x.IsValidTarget(1300));
                    foreach (var enemy in enemies)
                    {
                        var delay = Syndra.Distance(enemy) / SpellManager.E.Speed;
                        var pos = LeagueSharp.Common.Prediction.GetPrediction(enemy, delay).CastPosition.To2D();
                        if (rect.PointInPolygon(pos) == 1)
                        {
                            LeagueSharp.Common.Utility.DelayAction.Add(100, () => SpellManager.E.Cast(castpos));
                        }
                    }
                }
            }
        }



        void OnUpdate(EventArgs args)
        {
            if(Syndra.IsRecalling() || Syndra.IsDead)
            {
                return;
            }

            if (GetKeyBind("h.enabled"))
            {
                Harass();
            }

            if (GetBool("m.utilw"))
            {
                UtilizeW();
            }

            if (GetBool("ks.enabled"))
            {
                KillSteal();
            }
            

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (GetBool("h.inmixed"))
                {
                    Harass();
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Waveclear();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                FarmHandler();
            }
        }

        void Combo()
        {
            if (SpellManager.Q.IsReady() && GetBool("c.q"))
            {
                CastQ();
            }

            if (SpellManager.W.IsReady() && GetBool("c.w"))
            {
                CastW(null);
            }

            if (SpellManager.E.IsReady() && GetBool("c.e"))
            {
                CastE();
            }

            if (SpellManager.R.IsReady() && GetBool("c.r"))
            {
                CastR();
            }
        }

        void Harass()
        {
            if (GetBool("h.q"))
            {
                CastQ(true);
            }
            if (GetBool("h.w"))
            {
                CastW(null);
            }
            if (GetBool("h.e")) {
                CastE();
            }
        }

        void CastQ(bool harass = false)
        {
            var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Magical);
            if (SpellManager.Q.IsReady())
            {
                var pred = SpellManager.Q.GetAoeSPrediction();
                if (pred.HitCount > 0)
                {
                    SpellManager.Q.Cast(pred.CastPosition);
                }

                else if (target != null)
                {
                    if (target.Distance(Syndra) <= 48)
                    {
                        SpellManager.Q.Cast(Syndra.ServerPosition);
                    }
                    var preds = SpellManager.Q.GetSPrediction(target);
                    if (preds.HitChance >= HitChance.Medium)
                    {
                        SpellManager.Q.Cast(pred.CastPosition);
                    }
                }
                else if (!harass)
                {
                    CastQE();
                }
            }
        }

        void CastQE()
        {
            if (SpellManager.E.IsReady())
            {
                CastE();
            }
            var mana = SpellManager.Q.ManaCost + SpellManager.E.ManaCost;
            if (Syndra.Mana < mana * 1.5)
            {
                return;
            }
            if (SpellManager.Q.IsReady() && SpellManager.E.IsReady())
            {
                var targ = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);
                if (targ != null)
                {
                    var qpos = Syndra.ServerPosition.Extend(targ.ServerPosition, SpellManager.Q.Range / 1.4f);
                    var xdd = qpos.Extend(Syndra.ServerPosition, -Math.Max((1200 - qpos.Distance(Syndra.ServerPosition)), 700)).To2D();
                    var rect = new PolyUtils.Rectangle(qpos.To2D(), xdd, 48).ToPolygon();
                    Polygons.Add(rect);
                    if (rect.PointInPolygon(targ.ServerPosition.To2D()) == 1)
                    {
                        SpellManager.Q.Cast(qpos);
                    }
                }
            }
        }


        void CastW(Obj_AI_Hero paramtarget)
        {
            var targ = paramtarget == null ? TargetSelector.GetTarget(SpellManager.W.Range, TargetSelector.DamageType.Magical) : paramtarget;

            if (targ == null)
            {
                return;
            }

            if (HasSecondW)
            {
                var result = SpellManager.W2.GetAoeSPrediction();

                if (result.HitCount > 0)
                {
                    SpellManager.W2.Cast(result.CastPosition);
                    return;
                }

                else if (result.HitCount == 0)
                {
                    var pred = SpellManager.W2.GetSPrediction(targ);
                    if (pred.HitChance >= HitChance.Medium)
                    {
                        SpellManager.W2.Cast(pred.CastPosition);
                        return;
                    }
                    //Common if SPrediction cant find
                    else {
                        var predc = SpellManager.W2.GetPrediction(targ);
                        if (predc.Hitchance >= HitChance.VeryHigh)
                        {
                            SpellManager.W2.Cast(predc.CastPosition);
                            return;
                        }
                    }
                }
            }



            //Add if W about to run out, throw at closeby minions?
            //if first W
            else if (WGood)
            {
                var target = paramtarget == null ? TargetSelector.GetTarget(SpellManager.W.Range, TargetSelector.DamageType.Magical) : paramtarget;
                if (target != null)
                {
                    var bestmin = GrabbableObjects.MaxOrDefault(x => x.priority).unit;
                    if (bestmin != null)
                    {
                        if (ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, bestmin))
                        {
                            LastWTick = Utils.TickCount;
                        }
                    }
                }
            }
        }

        void CastE()
        {
            Polygons.Clear();
            var orbs = GetEOrbs();
            foreach (var orb in orbs)
            {
                var predictedposition = orb.ServerPosition.Extend(Syndra.ServerPosition, -Math.Max((1200 - orb.Distance(Syndra.ServerPosition)), 700)).To2D();
                var rect = new PolyUtils.Rectangle(orb.ServerPosition.To2D(), predictedposition, orb.BoundingRadius).ToPolygon();
                Polygons.Add(rect);
                var enemies = HeroManager.Enemies.Where(x => x.IsValidTarget(1300));
                foreach (var enemy in enemies)
                {
                    var delay = Syndra.Distance(enemy) / SpellManager.E.Speed;
                    var pos = LeagueSharp.Common.Prediction.GetPrediction(enemy, delay).CastPosition.To2D();
                    if (rect.PointInPolygon(pos) == 1)
                    {
                        SpellManager.E.Cast(orb.Position);
                    }
                }
            }
        }

        void CastR()
        {
            var candidates = HeroManager.Enemies.Where(x => x.IsValidTarget(SpellManager.R.Range)).OrderBy(x => TargetSelector.GetPriority(x));
            var orbcount = GetOrbcount();
            foreach (var candidate in candidates.Where(x => !x.IsBlackListed()))
            {
                if (candidate.HealthPercent < GetSliderFloat("c.rminh"))
                {
                    continue;
                }
                var basedamage = Syndra.GetSpellDamage(candidate, SpellSlot.R);
                var orbdamage = Syndra.GetSpellDamage(candidate, SpellSlot.R, 1) * orbcount;
                var total = basedamage + orbdamage;
                if (candidate.Health <= total)
                {
                    SpellManager.R.Cast(candidate);
                }
            }
        }

        void CastRSpecific(Obj_AI_Hero candidate)
        {
            var orbcount = GetOrbcount();
            if (candidate.HealthPercent < GetSliderFloat("c.rminh"))
            {
                return;
            }
            var basedamage = Syndra.GetSpellDamage(candidate, SpellSlot.R);
            var orbdamage = Syndra.GetSpellDamage(candidate, SpellSlot.R, 1) * orbcount;
            var total = basedamage + orbdamage;
            if (candidate.Health <= total)
            {
                SpellManager.R.Cast(candidate);
            }
        }
    
        bool WGood
        {
            get
            {
                return Utils.TickCount - LastWTick > 150 + Game.Ping;
            }
        }
        
        void KillSteal()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }
            double dmg = 0;
            List<Spell> spells = new List<Spell>();
            SPrediction.Prediction.Result qres, wres;
            qres = new SPrediction.Prediction.Result();
            wres = new SPrediction.Prediction.Result();
            if (GetBool("ks.q") && SpellManager.Q.IsReady() && target.IsValidTarget(SpellManager.Q.Range))
            {
                var pred = SpellManager.Q.GetSPrediction(target);
                if (pred.HitChance >= HitChance.Medium)
                {
                    qres = pred;
                    var qdmg = SpellManager.Q.GetDamage(target);
                    dmg += qdmg;
                    spells.Add(SpellManager.Q);
                }
            }

            if (GetBool("ks.w") && SpellManager.W.IsReady() && target.IsValidTarget(SpellManager.W.Range))
            {
                    var wdmg = SpellManager.W.GetDamage(target);
                    if (target.Health < wdmg)
                    {
                        CastW(target);
                    }
            }

            if (GetBool("ks.e") && SpellManager.E.IsReady() && target.IsValidTarget(SpellManager.E.Range))
            {
                var edmg = SpellManager.E.GetDamage(target);
                if (target.Health < edmg)
                {
                    CastE();
                }
            }

            if (GetBool("ks.r") && SpellManager.R.IsReady() && target.IsValidTarget(SpellManager.R.Range))
            {
                if (!target.IsBlackListed())
                {
                    var rdmg = SpellManager.R.GetDamage(target);
                    dmg += rdmg;
                    spells.Add(SpellManager.R);
                }
            }

            if (GetBool("ks.ignite") && SpellManager.Ignite.IsReady() && target.IsValidTarget(SpellManager.Ignite.Range))
            {
                var igdmg = SpellManager.Ignite.GetDamage(target);
                dmg += igdmg;
                spells.Add(SpellManager.Ignite);
            }

            if (dmg > target.Health) {
                foreach (var spell in spells)
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        SpellManager.Q.Cast(qres.CastPosition);
                    }

                    if (spell.Slot == SpellSlot.R)
                    {
                        CastRSpecific(target);
                    }

                    if (spell.Slot == SpellManager.Ignite.Slot)
                    {
                        SpellManager.Ignite.Cast(target);
                    }
                }
            }
        }

        void Waveclear()
        {
            if (Syndra.ManaPercent < GetSliderFloat("lc.mana"))
            {
                return;
            }
            var Minions =
             ObjectManager.Get<Obj_AI_Minion>()
                 .Where(
                     m =>
                         m.IsValidTarget() &&
                         (Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.Q.Range ||
                          Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.W.Range||
                          Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.E.Range));


            if (SpellSlot.Q.IsReady() && GetBool("lc.q"))
            {
                var qminions =
                Minions.Where(m => Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.Q.Range);
                MinionManager.FarmLocation QLocation = MinionManager.GetBestCircularFarmLocation(qminions.Select(m => m.ServerPosition.To2D()).ToList(), SpellManager.Q.Width, SpellManager.Q.Range);
                if (QLocation.Position != null && QLocation.MinionsHit > 1)
                {
                    SpellManager.Q.Cast(QLocation.Position);
                }
            }

            if (SpellSlot.W.IsReady() && GetBool("lc.w"))
            {
                    var wminions =
                        Minions.Where(
                            m => Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.W.Range);
                    MinionManager.FarmLocation wLocation =
                        MinionManager.GetBestCircularFarmLocation(wminions.Select(m => m.ServerPosition.To2D()).ToList(),
                            SpellManager.W2.Width, SpellManager.W2.Range);
                    if (wLocation.Position != null && wLocation.MinionsHit > 0)
                    {

                    if (!HasSecondW && WGood)
                    {
                        ForceGrab();
                    }
                    else if (HasSecondW)
                    {
                        SpellManager.W.Cast(wLocation.Position);
                    }
                    }
            }

            if (SpellSlot.E.IsReady() && GetBool("lc.e"))
            {
                int hitcount = 0;
                foreach (var min in Minions.Where(x => x.IsInRange(SpellManager.E.Range))) { 
                    var predictedposition = min.ServerPosition.Extend(Syndra.ServerPosition, -Math.Max((1200 - min.Distance(Syndra.ServerPosition)), 700)).To2D();
                    var rect = new PolyUtils.Rectangle(min.ServerPosition.To2D(), predictedposition, min.BoundingRadius).ToPolygon();
                    Polygons.Add(rect);
                    if (rect.PointInPolygon(min.ServerPosition.To2D()) == 1)
                    {
                        hitcount++;
                    }

                    if (hitcount >= 3)
                    {
                        SpellManager.E.Cast(min.ServerPosition);
                    }
                }
            }
        }


        void FarmHandler()
        {
            if (Syndra.ManaPercent < GetSliderFloat("lh.mana"))
            {
                return;
            }

            var Minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        m =>
                            m.IsValidTarget() &&
                            (Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.Q.Range ||
                             Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.W.Range ||
                             Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) <= SpellManager.E.Range));

            if (SpellSlot.Q.IsReady() && GetBool("lh.q"))
            {
                var KillableMinionsQ = Minions.Where(m => m.Health < Syndra.GetSpellDamage(m, SpellSlot.Q) && Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) > Syndra.AttackRange);
                if (KillableMinionsQ.Any())
                {
                    SpellManager.Q.Cast(KillableMinionsQ.FirstOrDefault().ServerPosition);
                    return;
                }
            }
            if (SpellSlot.W.IsReady() && GetBool("lh.w"))
            {
                var KillableMinionsW = Minions.Where(m => m.Health < Syndra.GetSpellDamage(m, SpellSlot.W) && Vector3.Distance(Syndra.ServerPosition, m.ServerPosition) < SpellManager.W.Range);
                var unit = KillableMinionsW.FirstOrDefault();
                if (unit != null)
                {
                    SpellManager.W.CastOnUnit(unit);
                    return;
                }
            }

            if (SpellSlot.E.IsReady() && GetBool("lh.e"))
            {
                var KillableMinionsE = Minions.Where(m => m.Health < Syndra.GetSpellDamage(m, SpellSlot.E) && Vector3.Distance(m.ServerPosition, Syndra.ServerPosition) > Syndra.AttackRange);
                if (KillableMinionsE.Any())
                {
                    SpellManager.E.Cast(KillableMinionsE.FirstOrDefault().ServerPosition);
                    return;
                }
            }
        }


        void ForceGrab()
        {
            var bestmin = GrabbableObjects.MaxOrDefault(x => x.priority).unit;
            if (bestmin != null)
            {
                if (ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, bestmin))
                {
                    LastWTick = Utils.TickCount;
                }
            }
        }

        bool HarassEnabled
        {
            get
            {
                return (GetKeyBind("h.enabled") || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && GetBool("h.inmixed"))) && Syndra.Mana > GetSliderFloat("h.mana");
            }
        }

        void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Hero)
            {
                if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && GetBool("c.q")) || HarassEnabled && GetBool("h.q"))
                {
                    if (SpellManager.Q.IsReady() && sender.IsValidTarget() && args.EndPos.Distance(Syndra) < SpellManager.Q.Range)
                    {
                        SpellManager.Q.Cast(args.EndPos);
                    }
                }
            }
        }

        void UtilizeW()
        {
            var wbuff = Syndra.GetBuff("SyndraW");
            if (wbuff != null)
            {
                var duration = wbuff.EndTime - wbuff.StartTime;
                var timeleft = wbuff.EndTime - Game.Time;
                var percentleft = timeleft / duration * 100;
                if (percentleft <= 3)
                {
                    var minion = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(SpellManager.W.Range)).FirstOrDefault();
                    if (minion != null)
                    {
                        SpellManager.W2.Cast(minion.ServerPosition);
                    }
                    else
                    {
                        var pos = Syndra.ServerPosition.Extend(Syndra.ServerPosition, -350);
                        SpellManager.W2.Cast(pos);
                    }
                }
            }
        }
    

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Syndra.IsRecalling() || Syndra.IsDead)
            {
                return;
            }
            if (GetBool("m.interrupter")) {
                if (SpellSlot.Q.IsReady() && SpellSlot.E.IsReady())
                {
                    if (sender.IsValidTarget(SpellManager.E.Range))
                    {
                        SpellManager.Q.Cast(sender.ServerPosition);
                        SpellManager.E.Cast(sender.ServerPosition);
                    }
                    else if (sender.IsValidTarget(1200))
                    {
                        var dist = Syndra.Distance(sender);
                        var pos = sender.ServerPosition.Extend(Syndra.ServerPosition, dist);
                        SpellManager.Q.Cast(pos);
                        CastE();
                        LeagueSharp.Common.Utility.DelayAction.Add(200, () => CastE());
                    }
                }
            }
        }
    

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (GetBool("m.ag"))
            {
                var sender = gapcloser.Sender;
                if (SpellSlot.Q.IsReady() && SpellSlot.E.IsReady())
                {
                    if (sender.IsValidTarget(SpellManager.E.Range))
                    {
                        SpellManager.Q.Cast(sender.ServerPosition);
                        SpellManager.E.Cast(sender.ServerPosition);
                    }
                    else if (sender.IsValidTarget(1200))
                    {
                        var dist = Syndra.Distance(sender);
                        var pos = sender.ServerPosition.Extend(Syndra.ServerPosition, dist);
                        SpellManager.Q.Cast(pos);
                        CastE();
                        LeagueSharp.Common.Utility.DelayAction.Add(200, () => CastE());
                    }
                }
            }
        }

        void OnDraw(EventArgs args)
        {
            if (Syndra.IsRecalling() || Syndra.IsDead)
            {
                return;
            }

            if (!GetBool("d.enabled"))
            {
                return;
            }

            var drawq = GetCircle("d.q");
            var draww = GetCircle("d.w");
            var drawe = GetCircle("d.e");
            var drawr = GetCircle("d.r");

            if (drawq.Active)
            {
                Render.Circle.DrawCircle(Syndra.Position, SpellManager.Q.Range, drawq.Color);
            }

            if (draww.Active)
            {
                Render.Circle.DrawCircle(Syndra.Position, SpellManager.W.Range, draww.Color);
            }

            if (drawe.Active)
            {
                Render.Circle.DrawCircle(Syndra.Position, SpellManager.E.Range, drawe.Color);
            }

            if (drawr.Active)
            {
                Render.Circle.DrawCircle(Syndra.Position, SpellManager.R.Range, drawr.Color);
            }

            if (GetBool("m.dbg"))
            {
                foreach (var poly in Polygons)
                {
                    poly.Draw(Color.Aqua, 5);
                }
            }
        }


    }
}
