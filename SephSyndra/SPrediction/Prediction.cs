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
using System.Linq;
using System.Threading.Tasks;
using System.IO;
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
        /// <summary>
        /// Neccesary input structure for prediction calculations
        /// </summary>
        public struct Input
        {
            public Obj_AI_Base Target;
            public float SpellDelay;
            public float SpellMissileSpeed;
            public float SpellWidth;
            public float SpellRange;
            public bool SpellCollisionable;
            public SkillshotType SpellSkillShotType;
            public List<Vector2> Path;
            public float AvgReactionTime;
            public float LastMovChangeTime;
            public float AvgPathLenght;
            public float LastAngleDiff;
            public Vector3 From;
            public Vector3 RangeCheckFrom;

            public Input(Obj_AI_Base _target, Spell s, Vector3 _from, Vector3 _rangeCheckFrom)
            {
                Target = _target;
                SpellDelay = s.Delay;
                SpellMissileSpeed = s.Speed;
                SpellWidth = s.Width;
                SpellRange = s.Range;
                SpellCollisionable = s.Collision;
                SpellSkillShotType = s.Type;
                Path = Target.GetWaypoints();
                if (Target is Obj_AI_Hero)
                {
                    Obj_AI_Hero t = Target as Obj_AI_Hero;
                    AvgReactionTime = t.AvgMovChangeTime();
                    LastMovChangeTime = t.LastMovChangeTime();
                    AvgPathLenght = t.AvgPathLenght();
                    LastAngleDiff = t.LastAngleDiff();
                }
                else
                {
                    AvgReactionTime = 0;
                    LastMovChangeTime = 0;
                    AvgPathLenght = 0;
                    LastAngleDiff = 0;
                }
                From = _from;
                RangeCheckFrom = _rangeCheckFrom;
            }
        }

        /// <summary>
        /// structure for prediction results
        /// </summary>
        public struct Result
        {
            public Vector2 CastPosition;
            public Vector2 UnitPosition;
            public HitChance HitChance;
            public Collision.Result CollisionResult;
        }

        /// <summary>
        /// structure for aoe prediction result
        /// </summary>
        public struct AoeResult
        {
            public Vector2 CastPosition;
            public Collision.Result CollisionResult;
            public int HitCount;
        }

        private static bool blInitialized;
        internal static Menu predMenu;

        #region stuff for prediction drawings
        internal static string lastDrawHitchance;
        internal static Vector2 lastDrawPos;
        internal static Vector2 lastDrawDirection;
        internal static int lastDrawTick;
        internal static int lastDrawWidth;
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
        public static void Initialize(Menu mainMenu)
        {
            SPrediction.PathTracker.Initialize();
            SPrediction.Collision.Initialize();

            if (mainMenu != null)
            {
                predMenu = new Menu("SPrediction", "SPRED");
                predMenu.AddItem(new MenuItem("PREDICTONLIST", "Prediction Method").SetValue(new StringList(new[] { "SPrediction", "Common Predicion" }, 0)));
                predMenu.AddItem(new MenuItem("SPREDWINDUP", "Check for target AA Windup").SetValue(true));
                predMenu.AddItem(new MenuItem("SPREDMAXRANGEIGNORE", "Max Range Dodge Ignore (%)").SetValue(new Slider(50, 0, 100)));
                predMenu.AddItem(new MenuItem("SPREDREACTIONDELAY", "Ignore Rection Delay").SetValue<Slider>(new Slider(0, 0, 200)));
                predMenu.AddItem(new MenuItem("SPREDDELAY", "Spell Delay").SetValue<Slider>(new Slider(0, 0, 200)));
                predMenu.AddItem(new MenuItem("SPREDHC", "Count HitChance").SetValue<KeyBind>(new KeyBind(32, KeyBindType.Press)));
                predMenu.AddItem(new MenuItem("SPREDDRAWINGS", "Enable Drawings").SetValue(true));
                mainMenu.AddSubMenu(predMenu);
            }

            Obj_AI_Base.OnBuffAdd += Obj_AI_Hero_OnBuffAdd;
            CustomEvents.Unit.OnDash += Unit_OnDash;

            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Obj_AI_Hero.OnDamage += Obj_AI_Hero_OnDamage;
            Game.OnEnd += Game_OnGameEnd;
            blInitialized = true;
        }
        
        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="input">Neccesary inputs for prediction calculations</param>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        internal static Result GetPrediction(Input input)
        {
            return GetPrediction(input.Target, input.SpellWidth, input.SpellDelay, input.SpellMissileSpeed, input.SpellRange, input.SpellCollisionable, input.SpellSkillShotType, input.Path, input.AvgReactionTime, input.LastMovChangeTime, input.AvgPathLenght, input.LastAngleDiff, input.From.To2D(), input.RangeCheckFrom.To2D());
        }

        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="width">Spell width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="collisionable">Spell collisionable</param>
        /// <param name="type">Spell skillshot type</param>
        /// <param name="from">Spell casted position</param>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        internal static Result GetPrediction(Obj_AI_Hero target, float width, float delay, float missileSpeed, float range, bool collisionable, SkillshotType type)
        {
            return GetPrediction(target, width, delay, missileSpeed, range, collisionable, type, target.GetWaypoints(), target.AvgMovChangeTime(), target.LastMovChangeTime(), target.AvgPathLenght(), target.LastAngleDiff(), ObjectManager.Player.ServerPosition.To2D(), ObjectManager.Player.ServerPosition.To2D());
        }

        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="width">Spell width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="collisionable">Spell collisionable</param>
        /// <param name="type">Spell skillshot type</param>
        /// <param name="path">Waypoints of target</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="avgp">Average Path Lenght</param>
        /// <param name="from">Spell casted position</param>
        /// <param name="rangeCheckFrom"></param>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        internal static Result GetPrediction(Obj_AI_Base target, float width, float delay, float missileSpeed, float range, bool collisionable, SkillshotType type, List<Vector2> path, float avgt, float movt, float avgp, float anglediff, Vector2 from, Vector2 rangeCheckFrom)
        {
            Prediction.AssertInitializationMode();

            Result result = new Result();

            try
            {
                if (type == SkillshotType.SkillshotCircle)
                    range += width;

                //to do: hook logic ? by storing average movement direction etc
                if (path.Count <= 1 && movt > 100 && (Environment.TickCount - PathTracker.EnemyInfo[target.NetworkId].LastAATick > 300 || !predMenu.Item("SPREDWINDUP").GetValue<bool>())) //if target is not moving, easy to hit (and not aaing)
                {
                    result.HitChance = HitChance.VeryHigh;
                    result.CastPosition = target.ServerPosition.To2D();
                    result.UnitPosition = result.CastPosition;
                    result.CollisionResult = Collision.GetCollisions(from, result.CastPosition, width, delay, missileSpeed);

                    if (collisionable && (result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions) || result.CollisionResult.Objects.HasFlag(Collision.Flags.YasuoWall)))
                        result.HitChance = HitChance.Collision;

                    if (from.Distance(result.CastPosition) > range - GetArrivalTime(from.Distance(result.CastPosition), delay, missileSpeed) * target.MoveSpeed * (100 - predMenu.Item("SPREDMAXRANGEIGNORE").GetValue<Slider>().Value) / 100f)
                        result.HitChance = HitChance.OutOfRange;

                    return result;
                }

                if (target is Obj_AI_Hero)
                {
                    if (((Obj_AI_Hero)target).IsChannelingImportantSpell())
                    {
                        result.HitChance = HitChance.VeryHigh;
                        result.CastPosition = target.ServerPosition.To2D();
                        result.UnitPosition = result.CastPosition;
                        result.CollisionResult = Collision.GetCollisions(from, result.CastPosition, width, delay, missileSpeed);

                        //check collisions
                        if (collisionable && (result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions) || result.CollisionResult.Objects.HasFlag(Collision.Flags.YasuoWall)))
                            result.HitChance = HitChance.Collision;

                        //check if target can dodge with moving backward
                        if (from.Distance(result.CastPosition) > range - GetArrivalTime(from.Distance(result.CastPosition), delay, missileSpeed) * target.MoveSpeed * (100 - predMenu.Item("SPREDMAXRANGEIGNORE").GetValue<Slider>().Value) / 100f)
                            result.HitChance = HitChance.OutOfRange;

                        return result;
                    }

                    if (Environment.TickCount - PathTracker.EnemyInfo[target.NetworkId].LastAATick < 300 && predMenu.Item("SPREDWINDUP").GetValue<bool>())
                    {
                        if (target.AttackCastDelay * 1000 + PathTracker.EnemyInfo[target.NetworkId].AvgOrbwalkTime + avgt - width / 2f / target.MoveSpeed >= GetArrivalTime(target.ServerPosition.To2D().Distance(from), delay, missileSpeed))
                        {
                            result.HitChance = HitChance.High;
                            result.CastPosition = target.ServerPosition.To2D();
                            result.UnitPosition = result.CastPosition;
                            result.CollisionResult = Collision.GetCollisions(from, result.CastPosition, width, delay, missileSpeed);

                            //check collisions
                            if (collisionable && (result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions) || result.CollisionResult.Objects.HasFlag(Collision.Flags.YasuoWall)))
                                result.HitChance = HitChance.Collision;

                            return result;
                        }
                    }

                    //to do: find a fuking logic
                    if (avgp < 400 && movt < 100 && path.PathLength() <= avgp)
                    {
                        result.HitChance = HitChance.High;
                        result.CastPosition = path.Last();
                        result.UnitPosition = result.CastPosition;
                        result.CollisionResult = Collision.GetCollisions(from, result.CastPosition, width, delay, missileSpeed);

                        //check collisions
                        if (collisionable && (result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions) || result.CollisionResult.Objects.HasFlag(Collision.Flags.YasuoWall)))
                            result.HitChance = HitChance.Collision;

                        //check if target can dodge with moving backward
                        if (from.Distance(result.CastPosition) > range - GetArrivalTime(from.Distance(result.CastPosition), delay, missileSpeed) * target.MoveSpeed * (100 - predMenu.Item("SPREDMAXRANGEIGNORE").GetValue<Slider>().Value) / 100f)
                            result.HitChance = HitChance.OutOfRange;

                        return result;
                    }
                }

                if (target.IsDashing()) //if unit is dashing
                    return GetDashingPrediction(target, width, delay, missileSpeed, range, collisionable, type, from);

                if (Utility.IsImmobileTarget(target)) //if unit is immobile
                    return GetImmobilePrediction(target, width, delay, missileSpeed, range, collisionable, type, from);

                result = WaypointAnlysis(target, width, delay, missileSpeed, range, collisionable, type, path, avgt, movt, avgp, anglediff, from);
                
                float d = result.CastPosition.Distance(target.ServerPosition.To2D());
                if (d >= (avgt - movt) * target.MoveSpeed && d >= avgp)
                    result.HitChance = HitChance.Medium;

                //check collisions
                if (collisionable && (result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions) || result.CollisionResult.Objects.HasFlag(Collision.Flags.YasuoWall)))
                    result.HitChance = HitChance.Collision;

                //check if target can dodge with moving backward
                if (from.Distance(result.CastPosition) > range - GetArrivalTime(from.Distance(result.CastPosition), delay, missileSpeed) * target.MoveSpeed * (100 - predMenu.Item("SPREDMAXRANGEIGNORE").GetValue<Slider>().Value) / 100f)
                    result.HitChance = HitChance.OutOfRange;

                return result;
            }
            finally
            {
                //check if movement changed while prediction calculations
                if (!target.GetWaypoints().SequenceEqual(path))
                    result.HitChance = HitChance.Medium;
            }
        }

        /// <summary>
        /// Gets fast-predicted unit position
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <param name="from">Spell casted position</param>
        /// <returns></returns>
        public static Vector2 GetFastUnitPosition(Obj_AI_Base target, float delay, float missileSpeed = 0, Vector2? from = null, float distanceSet = 0)
        {
            List<Vector2> path = target.GetWaypoints();
            if (from == null)
                from = ObjectManager.Player.ServerPosition.To2D();

            if (path.Count <= 1 || (target is Obj_AI_Hero && ((Obj_AI_Hero)target).IsChannelingImportantSpell()) || Utility.IsImmobileTarget(target))
                return target.ServerPosition.To2D();

            if (target.IsDashing())
                return target.GetDashInfo().Path.Last();

            float distance = distanceSet;

            if (distance == 0)
            {
                float targetDistance = from.Value.Distance(target.ServerPosition);
                float flyTime = targetDistance / missileSpeed;

                if (missileSpeed != 0 && path.Count == 2)
                {
                    Vector2 Vt = (path[1] - path[0]).Normalized() * target.MoveSpeed;
                    Vector2 Vs = (target.ServerPosition.To2D() - from.Value).Normalized() * missileSpeed;
                    Vector2 Vr = Vt - Vs;

                    flyTime = targetDistance / Vr.Length();
                }

                float t = flyTime + delay + Game.Ping / 2000f;
                distance = t * target.MoveSpeed;
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                float d = path[i + 1].Distance(path[i]);
                if (distance == d)
                    return path[i + 1];
                else if (distance < d)
                    return path[i] + distance * (path[i + 1] - path[i]).Normalized();
                else distance -= d;
            }

            return path.Last();
        }

        /// <summary>
        /// Gets fast-predicted unit position
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="path">Path</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <param name="from">Spell casted position</param>
        /// <param name="moveSpeed">Move speed</param>
        /// <param name="distanceSet"></param>
        /// <returns></returns>
        public static Vector2 GetFastUnitPosition(Obj_AI_Base target, List<Vector2> path, float delay, float missileSpeed = 0, Vector2? from = null, float moveSpeed = 0, float distanceSet = 0)
        {
            if (from == null)
                from = target.ServerPosition.To2D();

            if (moveSpeed == 0)
                moveSpeed = target.MoveSpeed;

            if (path.Count <= 1 || (target is Obj_AI_Hero && ((Obj_AI_Hero)target).IsChannelingImportantSpell()) || Utility.IsImmobileTarget(target))
                return target.ServerPosition.To2D();

            if (target.IsDashing())
                return target.GetDashInfo().Path.Last();

            float distance = distanceSet;

            if (distance == 0)
            {
                float targetDistance = from.Value.Distance(target.ServerPosition);
                float flyTime = 0f;

                if (missileSpeed != 0) //skillshot with a missile
                {
                    Vector2 Vt = (path[path.Count - 1] - path[0]).Normalized() * moveSpeed;
                    Vector2 Vs = (target.ServerPosition.To2D() - from.Value).Normalized() * missileSpeed;
                    Vector2 Vr = Vs - Vt;

                    flyTime = targetDistance / Vr.Length();

                    if (path.Count > 5) //complicated movement
                        flyTime = targetDistance / missileSpeed;
                }

                float t = flyTime + delay + Game.Ping / 2000f + SpellDelay / 1000f;
                distance = t * moveSpeed;
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                float d = path[i + 1].Distance(path[i]);
                if (distance == d)
                    return path[i + 1];
                else if (distance < d)
                    return path[i] + distance * (path[i + 1] - path[i]).Normalized();
                else distance -= d;
            }

            return path.Last();
        }

        /// <summary>
        /// Gets Prediction result while unit is dashing
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="width">Spell width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="collisionable">Spell collisionable</param>
        /// <param name="type">Spell skillshot type</param>
        /// <param name="from">Spell casted position</param>
        /// <returns></returns>
        internal static Result GetDashingPrediction(Obj_AI_Base target, float width, float delay, float missileSpeed, float range, bool collisionable, SkillshotType type, Vector2 from)
        {
            Result result = new Result();

            if (target.IsDashing())
            {
                var dashInfo = target.GetDashInfo();
                if (dashInfo.IsBlink)
                {
                    result.HitChance = HitChance.Impossible;
                    return result;
                }

                //define hitboxes
                var dashHitBox = ClipperWrapper.MakePaths(ClipperWrapper.DefineRectangle(dashInfo.StartPos, dashInfo.EndPos + (dashInfo.EndPos - dashInfo.StartPos).Normalized() * 500, target.BoundingRadius * 2));
                var myHitBox = ClipperWrapper.MakePaths(ClipperWrapper.DefineCircle(from, from == ObjectManager.Player.ServerPosition.To2D() ? ObjectManager.Player.BoundingRadius : width));

                if (ClipperWrapper.IsIntersects(myHitBox, dashHitBox))
                {
                    result.HitChance = HitChance.Dashing;
                    result.CastPosition = target.ServerPosition.To2D();
                    result.UnitPosition = result.CastPosition;
                    result.CollisionResult = Collision.GetCollisions(from, result.CastPosition, width, delay, missileSpeed);

                    //check collisions
                    if (collisionable && result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions))
                        result.HitChance = HitChance.Collision;

                    return result;
                }

                result.CastPosition = GetFastUnitPosition(target, dashInfo.Path, delay, missileSpeed, from, dashInfo.Speed);
                result.HitChance = HitChance.Dashing;

                //check range
                if (result.CastPosition.Distance(from) > range)
                    result.HitChance = HitChance.OutOfRange;

                //check collisions
                if (collisionable && (result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions) || result.CollisionResult.Objects.HasFlag(Collision.Flags.YasuoWall)))
                    result.HitChance = HitChance.Collision;
            }
            else
                result.HitChance = HitChance.Impossible;
            return result;
        }

        /// <summary>
        /// Gets Prediction result while unit is immobile
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="width">Spell width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="collisionable">Spell collisionable</param>
        /// <param name="type">Spell skillshot type</param>
        /// <param name="from">Spell casted position</param>
        /// <returns></returns>
        internal static Result GetImmobilePrediction(Obj_AI_Base target, float width, float delay, float missileSpeed, float range, bool collisionable, SkillshotType type, Vector2 from)
        {
            Result result = new Result();
            result.CastPosition = target.ServerPosition.To2D();
            result.UnitPosition = result.CastPosition;

            //calculate spell arrival time
            float t = delay + Game.Ping / 2000f;
            if (missileSpeed != 0)
                t += from.Distance(target.ServerPosition) / missileSpeed;

            if (type == SkillshotType.SkillshotCircle)
                t += width / target.MoveSpeed / 2f;

            if (t >= Utility.LeftImmobileTime(target))
            {
                result.HitChance = HitChance.Immobile;
                result.CollisionResult = Collision.GetCollisions(from, result.CastPosition, width, delay, missileSpeed);

                if (collisionable && result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions))
                    result.HitChance = HitChance.Collision;

                if (from.Distance(result.CastPosition) > range - GetArrivalTime(from.Distance(result.CastPosition), delay, missileSpeed) * target.MoveSpeed * (100 - predMenu.Item("SPREDMAXRANGEIGNORE").GetValue<Slider>().Value) / 100f)
                    result.HitChance = HitChance.OutOfRange;

                return result;
            }

            if (target is Obj_AI_Hero)
                result.HitChance = GetHitChance(t - Utility.LeftImmobileTime(target), ((Obj_AI_Hero)target).AvgMovChangeTime(), 0, 0, 0);
            else
                result.HitChance = HitChance.High;

            //check collisions
            if (collisionable && result.CollisionResult.Objects.HasFlag(Collision.Flags.Minions))
                result.HitChance = HitChance.Collision;

            //check range
            if (from.Distance(result.CastPosition) > range - GetArrivalTime(from.Distance(result.CastPosition), delay, missileSpeed) * target.MoveSpeed * (100 - predMenu.Item("SPREDMAXRANGEIGNORE").GetValue<Slider>().Value) / 100f)
                result.HitChance = HitChance.OutOfRange;

            return result;
        }

        /// <summary>
        /// Get HitChance
        /// </summary>
        /// <param name="t">Arrive time to target (in ms)</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="avgp">Average Path Lenght</param>
        /// <returns>HitChance</returns>
        internal static HitChance GetHitChance(float t, float avgt, float movt, float avgp, float anglediff)
        {
            if (avgp > 400)
            {
                if (movt > 50)
                {
                    if (avgt >= t * 1.25f)
                    {
                        if (anglediff < 30)
                            return HitChance.VeryHigh;
                        else
                            return HitChance.High;
                    }
                    else if (avgt - movt >= t)
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

        /// <summary>
        /// Gets spell arrival time to cast position
        /// </summary>
        /// <param name="distance">Distance from to to</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <returns></returns>
        internal static float GetArrivalTime(float distance, float delay, float missileSpeed = 0)
        {
            if (missileSpeed != 0)
                return distance / missileSpeed + delay;

            return delay;
        }

        /// <summary>
        /// Calculates cast position with target's path
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="width">Spell width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="missileSpeed">Spell missile speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="collisionable">Spell collisionable</param>
        /// <param name="type">Spell skillshot type</param>
        /// <param name="path">Waypoints of target</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="avgp">Average Path Lenght</param>
        /// <param name="from">Spell casted position</param>
        /// <returns></returns>
        internal static Result WaypointAnlysis(Obj_AI_Base target, float width, float delay, float missileSpeed, float range, bool collisionable, SkillshotType type, List<Vector2> path, float avgt, float movt, float avgp, float anglediff, Vector2 from, float moveSpeed = 0, bool isDash = false)
        {
            if (moveSpeed == 0)
                moveSpeed = target.MoveSpeed;

            Result result = new Result();

            float flyTimeMax = 0f;

            if (missileSpeed != 0) //skillshot with a missile
                flyTimeMax = range / missileSpeed;

            float tMin = delay + Game.Ping / 2000f + SpellDelay / 1000f;
            float tMax = flyTimeMax + delay + Game.Ping / 1000f + SpellDelay / 1000f;
            float pathTime = 0f;
            int[] pathBounds = new int[] { -1, -1 };

            //find bounds
            for (int i = 0; i < path.Count - 1; i++)
            {
                float t = path[i + 1].Distance(path[i]) / moveSpeed;

                if (pathTime <= tMin && pathTime + t >= tMin)
                    pathBounds[0] = i;
                if (pathTime <= tMax && pathTime + t >= tMax)
                    pathBounds[1] = i;

                if (pathBounds[0] != -1 && pathBounds[1] != -1)
                    break;

                pathTime += t;
            }

            //calculate cast & unit position
            if (pathBounds[0] != -1 && pathBounds[1] != -1)
            {
                for (int k = pathBounds[0]; k <= pathBounds[1]; k++)
                {
                    Vector2 direction = (path[k + 1] - path[k]).Normalized();
                    float distance = width;

                    int steps = (int)Math.Floor(path[k].Distance(path[k + 1]) / distance);
                    //split & anlyse current path
                    for (int i = 1; i < steps - 1; i++)
                    {
                        Vector2 pCenter = path[k] + (direction * distance * i);
                        Vector2 pA = pCenter - (direction * target.BoundingRadius);
                        Vector2 pB = pCenter + (direction * target.BoundingRadius);
                        
                        float flytime = missileSpeed != 0 ? from.Distance(pCenter) / missileSpeed : 0f;
                        float t = flytime + delay + Game.Ping / 2000f + SpellDelay / 1000f;
                        
                        Vector2 currentPosition = target.ServerPosition.To2D();

                        float arriveTimeA = currentPosition.Distance(pA) / moveSpeed;
                        float arriveTimeB = currentPosition.Distance(pB) / moveSpeed;

                        if (Math.Min(arriveTimeA, arriveTimeB) <= t && Math.Max(arriveTimeA, arriveTimeB) >= t)
                        {
                            result.HitChance = GetHitChance(t, avgt, movt, avgp, anglediff);
                            result.CastPosition = pCenter;
                            result.UnitPosition = pCenter; //+ (direction * (t - Math.Min(arriveTimeA, arriveTimeB)) * moveSpeed);
                            /*if (currentPosition.IsBetween(ObjectManager.Player.ServerPosition.To2D(), result.CastPosition))
                            {
                                result.CastPosition = currentPosition;
                                Console.WriteLine("corrected");
                            }*/
                            result.CollisionResult = Collision.GetCollisions(from, result.CastPosition, width, delay, missileSpeed);
                            return result;
                        }
                    }
                }
            }

            result.HitChance = HitChance.Impossible;

            return result;
        }

        /// <summary>
        /// Gets Ignored reaction time (sets with menu)
        /// </summary>
        public static int IgnoreReactionDelay
        {
            get
            {
                if (predMenu == null)
                    return 0;
                return predMenu.Item("SPREDREACTIONDELAY").GetValue<Slider>().Value;
            }
        }

        /// <summary>
        /// Gets extra spell delay (sets with menu)
        /// </summary>
        public static int SpellDelay
        {
            get
            {
                if (predMenu == null)
                    return 0;
                return predMenu.Item("SPREDDELAY").GetValue<Slider>().Value;
            }
        }

        /// <summary>
        /// Initialization assert
        /// </summary>
        internal static void AssertInitializationMode()
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");
        }

        private static void Unit_OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            
        }

        private static void Obj_AI_Hero_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {

        }

        /// <summary>
        /// OnDraw event for prediction drawings
        /// </summary>
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

        #region events for hitcount drawings
        private static void Obj_AI_Hero_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            lock (LastSpells)
            {
                LastSpells.RemoveAll(p => Environment.TickCount - p.tick > 2000);
                if (args.SourceNetworkId == ObjectManager.Player.NetworkId && HeroManager.Enemies.Exists(p => p.NetworkId == args.TargetNetworkId))
                {
                    if (LastSpells.Count != 0)
                    {
                        LastSpells.RemoveAt(0);
                        hitCount++;
                    }
                }
            }
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            lock (LastSpells)
            {
                LastSpells.RemoveAll(p => Environment.TickCount - p.tick > 2000);
                if (sender.IsMe && !args.SData.IsAutoAttack() && predMenu.Item("SPREDHC").GetValue<KeyBind>().Active)
                {
                    if (args.Slot == SpellSlot.Q && !LastSpells.Exists(p => p.name == args.SData.Name))
                    {
                        LastSpells.Add(new _lastSpells(args.SData.Name, Environment.TickCount));
                        castCount++;
                    }
                }
            }
        }

        private static void Game_OnGameEnd(EventArgs args)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("sprediction_{0}_{1}_{2}.txt", ObjectManager.Player.ChampionName, DateTime.Now.ToString("dd-MM"), Environment.TickCount.ToString("x8")));
            File.WriteAllText(file,
                String.Format("Champion : {1}{0}Casted Spell Count: {2}{0}Hit Spell Count: {3}{0}Hitchance(%) : {4}{0}",
                Environment.NewLine,
                ObjectManager.Player.ChampionName,
                castCount,
                hitCount,
                castCount > 0 ? (((float)hitCount / castCount) * 100).ToString("00.00") : "n/a"));
        }
        #endregion

    }
}
