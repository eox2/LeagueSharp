﻿/*
 Copyright 2015 - 2015 SPrediction
 Prediction.cs is part of SPrediction
 
 SPrediction is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 SPrediction is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with SPrediction. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SPrediction
{
    /// <summary>
    /// Spacebar Prediction class
    /// </summary>
    public static class Prediction
    {
        public static Dictionary<int, EnemyData> EnemyInfo = new Dictionary<int, EnemyData>();
        private static bool blInitialized;
        private static Menu predMenu;
        private static SPrediction.Collision Collision;

        #region stuff for prediction drawings
        private static string lastDrawHitchance;
        private static Vector2 lastDrawPos;
        private static Vector2 lastDrawDirection;
        private static int lastDrawTick;
        private static int lastDrawWidth;
        #endregion

        #region stuff for hitchance drawings
        private static int hitCount = 0;
        private static int castCount = 0;

        private struct _lastSpells
        {
            public string name;
            public int tick;

            public _lastSpells(string n, int t)
            {
                name = n;
                tick = t;
            }
        }

        private static List<_lastSpells> LastSpells = new List<_lastSpells>();
        #endregion

        /// <summary>
        /// Initializes Prediction Services
        /// </summary>
        public static void Initialize(Menu mainMenu = null)
        {
            foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
                EnemyInfo.Add(enemy.NetworkId, new EnemyData(new List<Vector2>()));

            Obj_AI_Hero.OnNewPath += Obj_AI_Hero_OnNewPath;

            if (mainMenu != null)
            {
                predMenu = new Menu("SPrediction", "SPRED");
                predMenu.AddItem(new MenuItem("PREDICTONLIST", "Prediction Method").SetValue(new StringList(new[] { "SPrediction", "Common Predicion" }, 0)));
                predMenu.AddItem(new MenuItem("SPREDREACTIONDELAY", "Ignore Rection Delay").SetValue<Slider>(new Slider(0, 0, 200)));
                predMenu.AddItem(new MenuItem("SPREDDELAY", "Spell Delay").SetValue<Slider>(new Slider(0, 0, 200)));
                predMenu.AddItem(new MenuItem("SPREDHC", "Count HitChance").SetValue<KeyBind>(new KeyBind(32, KeyBindType.Press)));
                predMenu.AddItem(new MenuItem("SPREDDRAWINGS", "Enable Drawings").SetValue(true));
                mainMenu.AddSubMenu(predMenu);
            }

            Collision = new SPrediction.Collision();
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnDamage += Obj_AI_Hero_OnDamage;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            blInitialized = true;
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            lock (LastSpells)
            {
                LastSpells.RemoveAll(p => Environment.TickCount - p.tick > 2000);
                if (sender.IsMe && !args.SData.IsAutoAttack() && predMenu.Item("SPREDHC").GetValue<KeyBind>().Active)
                {
                    if (sender.Spellbook.Spells.Find(p => p.Name == args.SData.Name).Slot == SpellSlot.Q && !LastSpells.Exists(p => p.name == args.SData.Name))
                    {
                        LastSpells.Add(new _lastSpells(args.SData.Name, Environment.TickCount));
                        castCount++;
                    }
                }
            }
        }

        static void Obj_AI_Hero_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            lock (LastSpells)
            {
                LastSpells.RemoveAll(p => Environment.TickCount - p.tick > 2000);
                if (args.SourceNetworkId == ObjectManager.Player.NetworkId && HeroManager.Enemies.Exists(p => p.NetworkId == args.TargetNetworkId))
                {
                    if (LastSpells.Count != 0)
                    {
                        LastSpells.RemoveRange(0, 1);
                        hitCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Gets Predicted position
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="s">Spell to cast</param>
        /// <param name="path">Waypoint of target</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="avgp">Average Path Lenght</param>
        /// <param name="hc">Predicted HitChance</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <returns>Predicted position and HitChance out value</returns>
        public static Vector2 GetPrediction(Obj_AI_Base target, Spell s, List<Vector2> path, float avgt, float movt, float avgp, out HitChance hc, Vector3 rangeCheckFrom)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            if (path.Count <= 1) //if target is not moving, easy to hit
            {
                hc = HitChance.VeryHigh;
                return target.ServerPosition.To2D();
            }

            if (target is Obj_AI_Hero && ((Obj_AI_Hero)target).IsChannelingImportantSpell())
            {
                hc = HitChance.VeryHigh;
                return target.ServerPosition.To2D();
            }

            if (IsImmobileTarget(target))
                return GetImmobilePrediction(target, s, out hc, rangeCheckFrom);

            if (target.IsDashing())
                return GetDashingPrediction(target, s, out hc, rangeCheckFrom);

            //if (avgp < 400 && target.GetSpellCastable())
            //{
            //    hc = HitChance.High;
            //    target.SetSpellCastable(false);
            //    return target.ServerPosition.To2D() + target.SpellCastDirection() * s.Width / 2;
            //}

            float targetDistance = rangeCheckFrom.Distance(target.ServerPosition);
            float flyTime = 0f;

            if (s.Speed != 0) //skillshot with a missile
            {
                Vector2 Vt = (path[path.Count - 1] - path[0]).Normalized() * target.MoveSpeed;
                Vector2 Vs = (target.ServerPosition.To2D() - rangeCheckFrom.To2D()).Normalized() * s.Speed;
                Vector2 Vr = Vs - Vt;

                flyTime = targetDistance / Vr.Length();

                if (path.Count > 5) //complicated movement
                    flyTime = targetDistance / s.Speed;
            }

            float t = flyTime + s.Delay + Game.Ping / 2000f + SpellDelay / 1000f;
            float distance = t * target.MoveSpeed;
            hc = GetHitChance(t * 1000f, avgt, movt, avgp);

            for (int i = 0; i < path.Count - 1; i++)
            {
                float d = path[i + 1].Distance(path[i]);
                if (distance == d)
                    return path[i + 1];
                else if (distance < d)
                    return path[i] + distance * (path[i + 1] - path[i]).Normalized();
                else distance -= d;
            }

            if (s.Type != SkillshotType.SkillshotCircle)
                hc = HitChance.Impossible;

            return path[path.Count - 1];
        }

        /// <summary>
        /// beta
        /// </summary>
        /// <param name="target"></param>
        /// <param name="s"></param>
        /// <param name="path"></param>
        /// <param name="avgt"></param>
        /// <param name="avgp"></param>
        /// <param name="movt"></param>
        /// <param name="hc"></param>
        /// <param name="rangeCheckFrom"></param>
        /// <returns></returns>
        public static Vector2 GetPredictionMethod2(Obj_AI_Base target, Spell s, List<Vector2> path, float avgt, float movt, float avgp, out HitChance hc, Vector3 rangeCheckFrom)
        {
            try
            {
                if (!blInitialized)
                    throw new InvalidOperationException("Prediction is not initalized");

                //to do: hook logic ? by storing average movement direction etc
                if (path.Count <= 1) //if target is not moving, easy to hit
                {
                    hc = HitChance.VeryHigh;
                    return target.ServerPosition.To2D();
                }

                if (target is Obj_AI_Hero)
                {
                    if (((Obj_AI_Hero)target).IsChannelingImportantSpell())
                    {
                        hc = HitChance.VeryHigh;
                        return target.ServerPosition.To2D();
                    }

                    //to do: find a fuking logic
                    if (avgp < 400 && movt < 100)
                    {
                        hc = HitChance.High;
                        return target.ServerPosition.To2D();
                    }
                }

                if (IsImmobileTarget(target))
                    return GetImmobilePrediction(target, s, out hc, rangeCheckFrom);

                if (target.IsDashing())
                    return GetDashingPrediction(target, s, out hc, rangeCheckFrom);

                float targetDistance = rangeCheckFrom.Distance(target.ServerPosition);
                float flyTimeMin = 0f;
                float flyTimeMax = 0f;

                if (s.Speed != 0) //skillshot with a missile
                {
                    flyTimeMin = targetDistance / s.Speed;
                    flyTimeMax = s.Range / s.Speed;
                }

                //PATCH WARNING
                float tMin = 0f + s.Delay + Game.Ping / 2000f + SpellDelay / 1000f; //0f => flyTimeMin
                float tMax = flyTimeMax + s.Delay + Game.Ping / 1000f + SpellDelay / 1000f;
                float pathTime = 0f;
                int[] x = new int[] { -1, -1 };

                for (int i = 0; i < path.Count - 1; i++)
                {
                    float t = path[i + 1].Distance(path[i]) / target.MoveSpeed;

                    if (pathTime <= tMin && pathTime + t >= tMin)
                        x[0] = i;
                    if (pathTime <= tMax && pathTime + t >= tMax)
                        x[1] = i;

                    if (x[0] != -1 && x[1] != -1)
                        break;

                    pathTime += t;
                }

                //PATCH WARNING
                if (x[0] != -1 && x[1] != -1)
                {
                    for (int k = x[0]; k <= x[1]; k++)
                    {
                        Vector2 direction = (path[k + 1] - path[k]).Normalized();
                        float distance = s.Width;
                        int steps = (int)Math.Floor(path[k].Distance(path[k + 1]) / distance);
                        for (int i = 0; i < steps; i++)
                        {
                            Vector2 pA = path[k] + (direction * distance * i);
                            Vector2 pB = path[k] + (direction * distance * (i + 1));
                            Vector2 center = (pA + pB) / 2f;

                            float flytime = s.Speed != 0 ? rangeCheckFrom.To2D().Distance(center) / s.Speed : 0f;
                            float t = flytime + s.Delay + Game.Ping / 2000f + SpellDelay / 1000f;

                            float arriveTimeA = target.ServerPosition.To2D().Distance(pA) / target.MoveSpeed;
                            float arriveTimeB = target.ServerPosition.To2D().Distance(pB) / target.MoveSpeed;

                            if (Math.Min(arriveTimeA, arriveTimeB) <= t && Math.Max(arriveTimeA, arriveTimeB) >= t)
                            {
                                hc = GetHitChance(t, avgt, movt, avgp);
                                return center;
                            }
                        }

                        if (steps == 0)
                        {
                            float flytime = s.Speed != 0 ? rangeCheckFrom.To2D().Distance(path[x[1]]) / s.Speed : 0f;
                            float t = flytime + s.Delay + Game.Ping / 2000f + SpellDelay / 1000f;
                            hc = GetHitChance(t, avgt, movt, avgp);
                            return path[x[1]];
                        }
                    }
                }


                hc = HitChance.Impossible;

                //PATCH WARNING
                if (s.Type == SkillshotType.SkillshotCircle && (x[0] != -1 || x[1] != -1))
                    if (movt < 100)
                        hc = HitChance.High;
                    else
                        hc = HitChance.Medium;

                return path[path.Count - 1];
            }
            finally
            {
                //check if movement changed while prediction calculations
                if (!target.GetWaypoints().SequenceEqual(path))
                    hc = HitChance.Medium;
            }
        }

        /// <summary>
        /// Gets Predicted position for arc
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="s">Spell to cast</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="avgp">Average Path Lenght</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="hc">Predicted HitChance</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <returns>Predicted position and HitChance out value</returns>
        public static Vector2 GetArcPrediction(Obj_AI_Base target, Spell s, List<Vector2> path, float avgt, float movt, float avgp, out HitChance hc, Vector3 rangeCheckFrom)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            if (path.Count <= 1) //if target is not moving, easy to hit
            {
                hc = HitChance.Immobile;
                return target.ServerPosition.To2D();
            }

            if (target is Obj_AI_Hero && ((Obj_AI_Hero)target).IsChannelingImportantSpell())
            {
                hc = HitChance.VeryHigh;
                return target.ServerPosition.To2D();
            }

            if (IsImmobileTarget(target))
                return GetImmobilePrediction(target, s, out hc, rangeCheckFrom);

            if (target.IsDashing())
                return GetDashingPrediction(target, s, out hc, rangeCheckFrom);


            float targetDistance = rangeCheckFrom.Distance(target.ServerPosition);
            float flyTime = 0f;

            if (s.Speed != 0)
            {
                Vector2 Vt = (path[path.Count - 1] - path[0]).Normalized() * target.MoveSpeed;
                Vector2 Vs = (target.ServerPosition.To2D() - rangeCheckFrom.To2D()).Normalized() * s.Speed;
                Vector2 Vr = Vs - Vt;

                flyTime = targetDistance / Vr.Length();

                if (path.Count > 5)
                    flyTime = targetDistance / s.Speed;
            }

            float t = flyTime + s.Delay + Game.Ping / 1000f + SpellDelay / 1000f;
            float distance = t * target.MoveSpeed;

            hc = GetHitChance(t * 1000f, avgt, movt, avgp);

            #region arc collision test
            for (int i = 1; i < path.Count; i++)
            {
                Vector2 senderPos = rangeCheckFrom.To2D();
                Vector2 testPos = path[i];

                float multp = (testPos.Distance(senderPos) / 875.0f);

                var dianaArc = new SPrediction.Geometry.Polygon(
                                ClipperWrapper.DefineArc(senderPos - new Vector2(875 / 2f, 20), testPos, (float)Math.PI * multp, 410, 200 * multp),
                                ClipperWrapper.DefineArc(senderPos - new Vector2(875 / 2f, 20), testPos, (float)Math.PI * multp, 410, 320 * multp));

                if (!ClipperWrapper.IsOutside(dianaArc, target.ServerPosition.To2D()))
                {
                    hc = HitChance.VeryHigh;
                    return testPos;
                }
            }
            #endregion

            for (int i = 0; i < path.Count - 1; i++)
            {
                float d = path[i + 1].Distance(path[i]);
                if (distance == d)
                    return path[i + 1];
                else if (distance < d)
                    return path[i] + distance * (path[i + 1] - path[i]).Normalized();
                else distance -= d;
            }

            if (s.Type != SkillshotType.SkillshotCircle)
                hc = HitChance.Impossible;

            return path[path.Count - 1];
        }

        /// <summary>
        /// Gets Predicted position while target is dashing
        /// </summary>
        private static Vector2 GetDashingPrediction(Obj_AI_Base target, Spell s, out HitChance hc, Vector3 rangeCheckFrom)
        {
            if (target.IsDashing())
            {
                var dashInfo = target.GetDashInfo();

                float dashPassedDistance = (Utils.TickCount - dashInfo.StartTick) / 1000f * dashInfo.Speed;
                Vector2 currentDashPos = dashInfo.StartPos + (dashInfo.EndPos - dashInfo.StartPos).Normalized() * dashPassedDistance;

                float targetDistance = rangeCheckFrom.To2D().Distance(currentDashPos);
                float flyTime = 0f;

                if (s.Speed != 0) //skillshot with a missile
                {
                    Vector2 Vt = (dashInfo.Path[dashInfo.Path.Count - 1] - dashInfo.Path[0]).Normalized() * dashInfo.Speed;
                    Vector2 Vs = (target.ServerPosition.To2D() - rangeCheckFrom.To2D()).Normalized() * s.Speed;
                    Vector2 Vr = Vs - Vt;

                    flyTime = targetDistance / Vr.Length();
                }
                int dashLeftTime = dashInfo.EndTick - Utils.TickCount;
                float t = flyTime + s.Delay + Game.Ping / 1000f;

                if (dashLeftTime >= t * 1000f)
                {
                    float distance = t * dashInfo.Speed;
                    hc = HitChance.Dashing;

                    for (int i = 0; i < dashInfo.Path.Count - 1; i++)
                    {
                        float d = dashInfo.Path[i + 1].Distance(dashInfo.Path[i]);
                        if (distance == d)
                            return dashInfo.Path[i + 1];
                        else if (distance < d)
                            return dashInfo.Path[i] + distance * (dashInfo.Path[i + 1] - dashInfo.Path[i]).Normalized();
                        else distance -= d;
                    }
                }
            }

            hc = HitChance.Impossible;
            return rangeCheckFrom.To2D();
        }

        private static Vector2 GetImmobilePrediction(Obj_AI_Base target, Spell s, out HitChance hc, Vector3 rangeCheckFrom)
        {
            float t = s.Delay + Game.Ping / 2000f;
            if (s.Speed != 0)
                t += rangeCheckFrom.To2D().Distance(target.ServerPosition) / s.Speed;

            if (s.Type == SkillshotType.SkillshotCircle)
                t += s.Width / target.MoveSpeed / 2f;

            if (t >= LeftImmobileTime(target))
            {
                hc = HitChance.Immobile;
                return target.ServerPosition.To2D();
            }

            if (target is Obj_AI_Hero)
                hc = GetHitChance(t - LeftImmobileTime(target), ((Obj_AI_Hero)target).AvgMovChangeTime(), 0, 0);

            hc = HitChance.High;
            return target.ServerPosition.To2D();
        }

        /// <summary>
        /// Casts spell
        /// </summary>
        /// <param name="s">Spell to cast</param>
        /// <param name="t">Target for spell</param>
        /// <param name="hc">Minimum HitChance to cast</param>
        /// <param name="reactionIgnoreDelay">Delay to ignore target's reaction time</param>
        /// <param name="minHit">Minimum Hit Count to cast</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
        /// <returns>true if spell has casted</returns>
        public static bool SPredictionCast(this Spell s, Obj_AI_Hero t, HitChance hc, int reactionIgnoreDelay = 0, byte minHit = 1, Vector3? rangeCheckFrom = null, float filterHPPercent = 100)
        {
            if (rangeCheckFrom == null)
                rangeCheckFrom = ObjectManager.Player.ServerPosition;

            if (t == null)
                return s.Cast();

            if (!s.IsSkillshot)
                return s.Cast(t) == Spell.CastStates.SuccessfullyCasted;

            #region if common prediction selected
            if (predMenu != null && predMenu.Item("PREDICTONLIST").GetValue<StringList>().SelectedIndex == 1)
            {
                var pout = s.GetPrediction(t, minHit > 1);

                if (minHit > 1)
                    if (pout.AoeTargetsHitCount >= minHit)
                        return s.Cast(pout.CastPosition);
                    else return false;

                if (pout.Hitchance >= hc)
                    return s.Cast(pout.CastPosition);
                else
                    return false;
            }
            #endregion

            if (minHit > 1)
                return Aoe.SPredictionCast(s, t, hc, reactionIgnoreDelay, minHit, rangeCheckFrom, filterHPPercent);

            if (s.Collision && minHit > 1)
                throw new InvalidOperationException("You can't use collisionable spells with aoe");

            if (t.HealthPercent > filterHPPercent)
                return false;

            if (Monitor.TryEnter(EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    HitChance predictedhc;
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    float avgp = t.AvgPathLenght();
                    var waypoints = t.GetWaypoints();
                    //PATCH WARNING
                    Vector2 pos = GetPredictionMethod2(t, s, waypoints, avgt, movt, avgp, out predictedhc, rangeCheckFrom.Value);

                    //if (waypoints.PathLength() / t.MoveSpeed < 1f)
                    //    pos = GetPredictionMethod2(t, s, waypoints, avgt, movt, avgp, out predictedhc, rangeCheckFrom.Value);
                    //else
                    //    pos = GetPrediction(t, s, waypoints, avgt, movt, avgp, out predictedhc, rangeCheckFrom.Value);

                    if (rangeCheckFrom.Value.To2D().Distance(pos) > s.Range + (s.Type == SkillshotType.SkillshotCircle ? s.Width / 2 : 0) - t.BoundingRadius) //out of range
                    {
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }

                    lastDrawTick = Utils.TickCount;
                    lastDrawPos = pos;
                    lastDrawHitchance = predictedhc.ToString();
                    lastDrawDirection = (pos - rangeCheckFrom.Value.To2D()).Normalized().Perpendicular();
                    lastDrawWidth = (int)s.Width;

                    if (s.Collision && Collision.CheckCollision(rangeCheckFrom.Value.To2D(), pos, s, true, false, true))
                    {
                        lastDrawHitchance = HitChance.Collision.ToString();
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }

                    if (predictedhc >= hc)
                    {
                        s.Cast(pos);
                        return true;
                    }

                    Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                    return false;
                }
                finally
                {
                    Monitor.Exit(EnemyInfo[t.NetworkId].m_lock);
                }
            }

            return false;
        }

        /// <summary>
        /// Casts spell
        /// </summary>
        /// <param name="s">Spell to cast</param>
        /// <param name="t">Target for spell</param>
        /// <param name="hc">Minimum HitChance to cast</param>
        /// <param name="reactionIgnoreDelay">Delay to ignore target's reaction time</param>
        /// <param name="minHit">Minimum Hit Count to cast</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
        /// <returns>true if spell has casted</returns>
        public static bool SPredictionCastArc(this Spell s, Obj_AI_Hero t, HitChance hc, int reactionIgnoreDelay = 0, byte minHit = 1, Vector3? rangeCheckFrom = null, float filterHPPercent = 100)
        {
            if (minHit > 1)
                throw new NotSupportedException("Arc aoe prediction has not supported yet");

            if (predMenu != null && predMenu.Item("PREDICTONLIST").GetValue<StringList>().SelectedIndex == 1)
                throw new NotSupportedException("Arc Prediction not supported in Common prediction");


            if (t.HealthPercent > filterHPPercent)
                return false;

            if (rangeCheckFrom == null)
                rangeCheckFrom = ObjectManager.Player.ServerPosition;

            if (Monitor.TryEnter(EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    HitChance predictedhc;
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    float avgp = t.AvgPathLenght();
                    Vector2 pos = GetArcPrediction(t, s, t.GetWaypoints(), avgt, movt, avgp, out predictedhc, rangeCheckFrom.Value);

                    if (rangeCheckFrom.Value.To2D().Distance(pos) > s.Range + s.Width / 2 - t.BoundingRadius) //out of range
                    {
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }

                    float multp = (pos.Distance(rangeCheckFrom.Value.To2D()) / 875.0f);

                    var dianaArc = new SPrediction.Geometry.Polygon(
                                    ClipperWrapper.DefineArc(rangeCheckFrom.Value.To2D() - new Vector2(875 / 2f, 20), pos, (float)Math.PI * multp, 410, 200 * multp),
                                    ClipperWrapper.DefineArc(rangeCheckFrom.Value.To2D() - new Vector2(875 / 2f, 20), pos, (float)Math.PI * multp, 410, 320 * multp));

                    if (Collision.CheckYasuoWallCollision(dianaArc))
                    {
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }

                    if (predictedhc >= hc)
                    {
                        s.Cast(pos);
                        return true;
                    }

                    Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                    return false;
                }
                finally
                {
                    Monitor.Exit(EnemyInfo[t.NetworkId].m_lock);
                }
            }

            return false;
        }

        /// <summary>
        /// Class for aoe spell castings
        /// </summary>
        public static class Aoe
        {
            /// <summary>
            /// Casts aoe spell
            /// </summary>
            /// <param name="s">Spell to cast</param>
            /// <param name="t">Target for spell</param>
            /// <param name="hc">Minimum HitChance to cast</param>
            /// <param name="minHit">Minimum Hit Count to cast</param>
            /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
            /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
            /// <returns>true if spell has casted</returns>
            internal static bool SPredictionCast(Spell s, Obj_AI_Hero t, HitChance hc, int reactionIgnoreDelay = 0, byte minHit = 2, Vector3? rangeCheckFrom = null, float filterHPPercent = 0)
            {
                if (!blInitialized)
                    throw new Exception("Prediction is not initalized");

                if (rangeCheckFrom == null)
                    rangeCheckFrom = ObjectManager.Player.ServerPosition;

                if (Monitor.TryEnter(EnemyInfo[t.NetworkId].m_lock))
                {
                    try
                    {
                        HitChance predictedhc = HitChance.Impossible;
                        float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                        float movt = t.LastMovChangeTime();
                        float avgp = t.AvgPathLenght();

                        Vector2 pos = ObjectManager.Player.ServerPosition.To2D();
                        switch (s.Type)
                        {
                            case SkillshotType.SkillshotLine: pos = Line.GetPrediction(t, s, t.GetWaypoints(), avgt, movt, avgp, filterHPPercent, minHit, out predictedhc, rangeCheckFrom.Value);
                                break;
                            case SkillshotType.SkillshotCircle: pos = Circle.GetPrediction(t, s, t.GetWaypoints(), avgt, movt, avgp, filterHPPercent, minHit, out predictedhc, rangeCheckFrom.Value);
                                break;
                            case SkillshotType.SkillshotCone: pos = Cone.GetPrediction(t, s, t.GetWaypoints(), avgt, movt, avgp, filterHPPercent, minHit, out predictedhc, rangeCheckFrom.Value);
                                break;
                        }

                        if (predictedhc >= hc)
                        {
                            s.Cast(pos);

                            return true;
                        }

                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }
                    finally
                    {
                        Monitor.Exit(EnemyInfo[t.NetworkId].m_lock);
                    }
                }
                return false;
            }

            //modified common aoe classes, i think logic is nice

            /// <summary>
            /// aoe line prediction class
            /// </summary>
            public static class Line
            {
                private static IEnumerable<Vector2> GetHits(Vector2 start, Vector2 end, double radius, List<Vector2> points)
                {
                    return points.Where(p => p.Distance(start, end, true, true) <= radius * radius);
                }

                private static bool GetCandidates(Vector2 from, Vector2 to, float radius, float range, out Vector2[] vec)
                {
                    var middlePoint = (from + to) / 2;
                    var intersections = LeagueSharp.Common.Geometry.CircleCircleIntersection(
                        from, middlePoint, radius, from.Distance(middlePoint));

                    if (intersections.Length > 1)
                    {
                        var c1 = intersections[0];
                        var c2 = intersections[1];

                        c1 = from + range * (to - c1).Normalized();
                        c2 = from + range * (to - c2).Normalized();

                        vec = new[] { c1, c2 };
                        return true;
                    }

                    vec = new Vector2[] { };
                    return false;
                }

                public static Vector2 GetPrediction(Obj_AI_Hero t, Spell s, List<Vector2> path, float avgt, float movt, float avgp, float filterHPPercent, byte minHit, out HitChance hc, Vector3 rangeCheckFrom)
                {
                    Vector2 castPos = Prediction.GetPrediction(t, s, path, avgt, movt, avgp, out hc, rangeCheckFrom);

                    var posibleTargets = new List<PossibleTarget>
                    {
                        new PossibleTarget { Position = t.ServerPosition.To2D(), Unit = t }
                    };

                    if (hc >= HitChance.Low)
                    {
                        //Add the posible targets  in range:
                        posibleTargets.AddRange(GetPossibleTargets(t, s, rangeCheckFrom, filterHPPercent));
                        if (posibleTargets.Count < minHit)
                        {
                            hc = HitChance.Impossible;
                            return castPos;
                        }
                    }

                    if (posibleTargets.Count > 1)
                    {
                        var candidates = new List<Vector2>();
                        foreach (var target in posibleTargets)
                        {
                            Vector2[] v;
                            if (GetCandidates(rangeCheckFrom.To2D(), target.Position, s.Width, s.Range, out v))
                                candidates.AddRange(v);
                        }

                        var bestCandidateHits = -1;
                        var bestCandidate = new Vector2();
                        var bestCandidateHitPoints = new List<Vector2>();
                        var positionsList = posibleTargets.Select(p => p.Position).ToList();

                        foreach (var candidate in candidates)
                        {
                            if (GetHits(rangeCheckFrom.To2D(), candidate, s.Width, new List<Vector2> { posibleTargets[0].Position }).Count() == 1)
                            {
                                var hits = GetHits(rangeCheckFrom.To2D(), candidate, s.Width, positionsList).ToList();
                                var hitsCount = hits.Count;
                                if (hitsCount >= bestCandidateHits)
                                {
                                    bestCandidateHits = hitsCount;
                                    bestCandidate = candidate;
                                    bestCandidateHitPoints = hits.ToList();
                                }
                            }
                        }

                        if (bestCandidateHits > 1)
                        {
                            float maxDistance = -1;
                            Vector2 p1 = new Vector2(), p2 = new Vector2();

                            //Center the position
                            for (var i = 0; i < bestCandidateHitPoints.Count; i++)
                            {
                                for (var j = 0; j < bestCandidateHitPoints.Count; j++)
                                {
                                    var startP = rangeCheckFrom.To2D();
                                    var endP = bestCandidate;
                                    var proj1 = positionsList[i].ProjectOn(startP, endP);
                                    var proj2 = positionsList[j].ProjectOn(startP, endP);
                                    var dist = Vector2.DistanceSquared(bestCandidateHitPoints[i], proj1.LinePoint) +
                                               Vector2.DistanceSquared(bestCandidateHitPoints[j], proj2.LinePoint);
                                    if (dist >= maxDistance &&
                                        (proj1.LinePoint - positionsList[i]).AngleBetween(
                                            proj2.LinePoint - positionsList[j]) > 90)
                                    {
                                        maxDistance = dist;
                                        p1 = positionsList[i];
                                        p2 = positionsList[j];
                                    }
                                }
                            }

                            if (bestCandidateHits < minHit)
                                hc = HitChance.Impossible;

                            return (p1 + p2) * 0.5f;
                        }
                    }

                    hc = HitChance.Impossible;
                    return castPos;
                }
            }

            /// <summary>
            /// aoe circle prediction class
            /// </summary>
            public static class Circle
            {
                public static Vector2 GetPrediction(Obj_AI_Hero t, Spell s, List<Vector2> path, float avgt, float movt, float avgp, float filterHPPercent, byte minHit, out HitChance hc, Vector3 rangeCheckFrom)
                {
                    Vector2 castPos = Prediction.GetPrediction(t, s, path, avgt, movt, avgp, out hc, rangeCheckFrom);
                    var posibleTargets = new List<PossibleTarget>
                    {
                        new PossibleTarget { Position = t.ServerPosition.To2D(), Unit = t }
                    };

                    if (hc >= HitChance.Low)
                    {
                        //Add the posible targets  in range:
                        posibleTargets.AddRange(GetPossibleTargets(t, s, rangeCheckFrom, filterHPPercent));
                        if (posibleTargets.Count < minHit)
                        {
                            hc = HitChance.Impossible;
                            return castPos;
                        }
                    }


                    while (posibleTargets.Count > 1)
                    {
                        var mecCircle = MEC.GetMec(posibleTargets.Select(h => h.Position).ToList());

                        if (mecCircle.Radius <= s.Width - 10 && Vector2.DistanceSquared(mecCircle.Center, rangeCheckFrom.To2D()) < s.Range * s.Range)
                        {
                            if (posibleTargets.Count < minHit)
                                hc = HitChance.Impossible;

                            return mecCircle.Center;
                        }

                        float maxdist = -1;
                        var maxdistindex = 1;
                        for (var i = 1; i < posibleTargets.Count; i++)
                        {
                            var distance = Vector2.DistanceSquared(posibleTargets[i].Position, posibleTargets[0].Position);
                            if (distance > maxdist || maxdist.CompareTo(-1) == 0)
                            {
                                maxdistindex = i;
                                maxdist = distance;
                            }
                        }
                        posibleTargets.RemoveAt(maxdistindex);
                    }

                    hc = HitChance.Impossible;
                    return castPos;
                }
            }

            /// <summary>
            /// aoe cone prediction class
            /// </summary>
            public static class Cone
            {
                private static int GetHits(Vector2 end, double range, float angle, List<Vector2> points)
                {
                    return (from point in points
                            let edge1 = end.Rotated(-angle / 2)
                            let edge2 = edge1.Rotated(angle)
                            where
                                point.Distance(new Vector2(), true) < range * range && edge1.CrossProduct(point) > 0 &&
                                point.CrossProduct(edge2) > 0
                            select point).Count();
                }

                public static Vector2 GetPrediction(Obj_AI_Hero t, Spell s, List<Vector2> path, float avgt, float movt, float avgp, float filterHPPercent, byte minHit, out HitChance hc, Vector3 rangeCheckFrom)
                {
                    Vector2 castPos = Prediction.GetPrediction(t, s, path, avgt, movt, avgp, out hc, rangeCheckFrom);
                    var posibleTargets = new List<PossibleTarget>
                    {
                        new PossibleTarget { Position = t.ServerPosition.To2D(), Unit = t }
                    };

                    if (hc >= HitChance.Low)
                    {
                        //Add the posible targets  in range:
                        posibleTargets.AddRange(GetPossibleTargets(t, s, rangeCheckFrom, filterHPPercent));
                        if (posibleTargets.Count < minHit)
                        {
                            hc = HitChance.Impossible;
                            return castPos;
                        }
                    }

                    if (posibleTargets.Count > 1)
                    {
                        var candidates = new List<Vector2>();

                        foreach (var target in posibleTargets)
                        {
                            target.Position = target.Position - rangeCheckFrom.To2D();
                        }

                        for (var i = 0; i < posibleTargets.Count; i++)
                        {
                            for (var j = 0; j < posibleTargets.Count; j++)
                            {
                                if (i != j)
                                {
                                    var p = (posibleTargets[i].Position + posibleTargets[j].Position) * 0.5f;
                                    if (!candidates.Contains(p))
                                    {
                                        candidates.Add(p);
                                    }
                                }
                            }
                        }

                        var bestCandidateHits = -1;
                        var bestCandidate = new Vector2();
                        var positionsList = posibleTargets.Select(p => p.Position).ToList();

                        foreach (var candidate in candidates)
                        {
                            var hits = GetHits(candidate, s.Range, s.Width, positionsList);
                            if (hits > bestCandidateHits)
                            {
                                bestCandidate = candidate;
                                bestCandidateHits = hits;
                            }
                        }

                        if (bestCandidateHits < minHit)
                            hc = HitChance.Impossible;

                        if (bestCandidateHits > 1 && rangeCheckFrom.To2D().Distance(bestCandidate, true) > 50 * 50)
                        {
                            return bestCandidate;
                        }
                    }

                    hc = HitChance.Impossible;
                    return castPos;
                }
            }

            private class PossibleTarget
            {
                public Vector2 Position;
                public Obj_AI_Base Unit;
            }

            private static List<PossibleTarget> GetPossibleTargets(Obj_AI_Hero target, Spell s, Vector3 rangeCheckFrom, float filterHPPercent)
            {
                var result = new List<PossibleTarget>();
                var originalUnit = target;
                var enemies = HeroManager.Enemies.FindAll(h => h.NetworkId != originalUnit.NetworkId && h.IsValidTarget(s.Range, true, rangeCheckFrom) && h.Health / h.MaxHealth * 100 <= filterHPPercent);
                foreach (var enemy in enemies)
                {
                    HitChance hc;
                    var prediction = Prediction.GetPrediction(enemy, s, enemy.GetWaypoints(), enemy.AvgMovChangeTime(), enemy.LastMovChangeTime(), enemy.AvgPathLenght(), out hc, rangeCheckFrom);
                    if (hc >= HitChance.High)
                    {
                        result.Add(new PossibleTarget { Position = prediction, Unit = enemy });
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Get HitChance
        /// </summary>
        /// <param name="t">Arrive time to target (in ms)</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="avgp">Average Path Lenght</param>
        /// <returns>HitChance</returns>
        private static HitChance GetHitChance(float t, float avgt, float movt, float avgp)
        {
            if (avgp > 400)
            {
                if (movt > 50)
                {
                    if (avgt - movt >= t * 1.25f)
                        return HitChance.High;
                    else if (avgt - movt >= t * 0.5f)
                        return HitChance.Medium;
                    else
                        return HitChance.Low;
                }
                else
                    return HitChance.VeryHigh;
            }
            else
                return HitChance.High;
        }

        private static int IgnoreReactionDelay
        {
            get
            {
                if (predMenu == null)
                    return 0;
                return predMenu.Item("SPREDREACTIONDELAY").GetValue<Slider>().Value;
            }
        }

        private static int SpellDelay
        {
            get
            {
                if (predMenu == null)
                    return 0;
                return predMenu.Item("SPREDDELAY").GetValue<Slider>().Value;
            }
        }

        private static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (!sender.IsEnemy || !sender.IsChampion() || args.IsDash)
                return;

            EnemyData enemy = EnemyInfo[sender.NetworkId];

            lock (enemy.m_lock)
            {
                if (args.Path.Length < 2)
                {
                    if (!enemy.IsStopped)
                    {
                        enemy.StopTick = Environment.TickCount;
                        enemy.LastWaypointTick = Environment.TickCount;
                        enemy.IsStopped = true;
                        enemy.Count = 0;
                        enemy.AvgTick = 0;
                        enemy.AvgPathLenght = 0;
                    }
                }
                else
                {
                    List<Vector2> wp = args.Path.Select(p => p.To2D()).ToList();
                    if (!enemy.LastWaypoints.SequenceEqual(wp))
                    {
                        if (!enemy.IsStopped)
                        {
                            enemy.AvgTick = (enemy.Count * enemy.AvgTick + (Environment.TickCount - enemy.LastWaypointTick)) / ++enemy.Count;
                            enemy.AvgPathLenght = ((enemy.Count - 1) * enemy.AvgPathLenght + wp.PathLength()) / enemy.Count;
                        }
                        enemy.LastWaypointTick = Environment.TickCount;
                        enemy.IsStopped = false;
                        enemy.LastWaypoints = wp;
                    }
                }

                EnemyInfo[sender.NetworkId] = enemy;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (predMenu != null && predMenu.Item("SPREDDRAWINGS").GetValue<bool>())
            {
                foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
                {
                    var waypoints = enemy.GetWaypoints();
                    if (waypoints != null && waypoints.Count > 1)
                    {
                        for (int i = 0; i < waypoints.Count - 1; i++)
                        {
                            Vector2 posFrom = Drawing.WorldToScreen(waypoints[i].To3D());
                            Vector2 posTo = Drawing.WorldToScreen(waypoints[i + 1].To3D());
                            Drawing.DrawLine(posFrom, posTo, 2, System.Drawing.Color.Aqua);
                        }

                        Vector2 pos = Drawing.WorldToScreen(waypoints[waypoints.Count - 1].To3D());
                        Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.Black, (waypoints.PathLength() / enemy.MoveSpeed).ToString("0.00")); //arrival time
                    }
                }

                if (Utils.TickCount - lastDrawTick <= 2000)
                {
                    Vector2 centerPos = Drawing.WorldToScreen((lastDrawPos - lastDrawDirection * 5).To3D());
                    Vector2 startPos = Drawing.WorldToScreen((lastDrawPos - lastDrawDirection * lastDrawWidth).To3D());
                    Vector2 endPos = Drawing.WorldToScreen((lastDrawPos + lastDrawDirection * lastDrawWidth).To3D());
                    Drawing.DrawLine(startPos, endPos, 3, System.Drawing.Color.Gold);
                    Drawing.DrawText(centerPos.X, centerPos.Y, System.Drawing.Color.Red, lastDrawHitchance);
                }

                Drawing.DrawText(Drawing.Width - 200, 0, System.Drawing.Color.Red, String.Format("Casted Spell Count: {0}", castCount));
                Drawing.DrawText(Drawing.Width - 200, 20, System.Drawing.Color.Red, String.Format("Hit Spell Count: {0}", hitCount));
                Drawing.DrawText(Drawing.Width - 200, 40, System.Drawing.Color.Red, String.Format("Hitchance (%): {0}%", castCount > 0 ? (((float)hitCount / castCount) * 100).ToString("00.00") : "n/a"));
            }
        }

        #region Obj_AI_Hero extensions
        /// <summary>
        /// Gets passed time without moving
        /// </summary>
        /// <param name="t">target</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MovImmobileTime(this Obj_AI_Hero t)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            return EnemyInfo[t.NetworkId].IsStopped ? Environment.TickCount - EnemyInfo[t.NetworkId].StopTick : 0;
        }
        /// <summary>
        /// Gets passed time from last movement change
        /// </summary>
        /// <param name="t">target</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastMovChangeTime(this Obj_AI_Hero t)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            return Environment.TickCount - EnemyInfo[t.NetworkId].LastWaypointTick;
        }

        /// <summary>
        /// Gets average movement reaction time
        /// </summary>
        /// <param name="t"><target/param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AvgMovChangeTime(this Obj_AI_Hero t)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            return EnemyInfo[t.NetworkId].AvgTick + IgnoreReactionDelay;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AvgPathLenght(this Obj_AI_Hero t)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            return EnemyInfo[t.NetworkId].AvgPathLenght;
        }
        #endregion

        #region util funcs
        internal static bool IsImmobilizeBuff(BuffType type)
        {
            return type == BuffType.Snare || type == BuffType.Stun || type == BuffType.Charm || type == BuffType.Knockup || type == BuffType.Suppression;
        }

        internal static bool IsImmobileTarget(Obj_AI_Base target)
        {
            return target.Buffs.Count(p => IsImmobilizeBuff(p.Type)) > 0;
        }

        internal static float LeftImmobileTime(Obj_AI_Base target)
        {
            return target.Buffs.Where(p => p.IsActive && IsImmobilizeBuff(p.Type) && p.EndTime >= Game.Time).Max(q => q.EndTime - Game.Time);
        }
        #endregion

        public struct EnemyData
        {
            public bool IsStopped;
            public List<Vector2> LastWaypoints;
            public int LastWaypointTick;
            public int StopTick;
            public float AvgTick;
            public float AvgPathLenght;
            public int Count;
            public object m_lock;

            public EnemyData(List<Vector2> wp)
            {
                IsStopped = false;
                LastWaypoints = wp;
                LastWaypointTick = 0;
                StopTick = 0;
                AvgTick = 0;
                AvgPathLenght = 0;
                Count = 0;
                m_lock = new object();
            }
        }
    }
}
