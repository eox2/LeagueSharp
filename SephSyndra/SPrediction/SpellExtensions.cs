/*
 Copyright 2015 - 2015 SPrediction
 SpellExtensions.cs is part of SPrediction
 
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
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SPrediction
{
    /// <summary>
    /// Spell extensions for SPrediction
    /// </summary>
    public static class SpellExtensions
    {
        #region Prediction methods
        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        public static Prediction.Result GetSPrediction(this Spell s, Obj_AI_Hero target)
        {
            switch (s.Type)
            {
                case SkillshotType.SkillshotLine:
                    return LinePrediction.GetPrediction(target, s.Width, s.Delay, s.Speed, s.Range, s.Collision, target.GetWaypoints(), target.AvgMovChangeTime(), target.LastMovChangeTime(), target.AvgPathLenght(), target.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());
                case SkillshotType.SkillshotCircle:
                    return CirclePrediction.GetPrediction(target, s.Width, s.Delay, s.Speed, s.Range, s.Collision, target.GetWaypoints(), target.AvgMovChangeTime(), target.LastMovChangeTime(), target.AvgPathLenght(), target.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());
                case SkillshotType.SkillshotCone:
                    return ConePrediction.GetPrediction(target, s.Width, s.Delay, s.Speed, s.Range, s.Collision, target.GetWaypoints(), target.AvgMovChangeTime(), target.LastMovChangeTime(), target.AvgPathLenght(), target.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());
            }

            throw new NotSupportedException("Unknown skill shot type");
        }

        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        public static Prediction.Result GetArcSPrediction(this Spell s, Obj_AI_Hero target)
        {
            return ArcPrediction.GetPrediction(target, s.Width, s.Delay, s.Speed, s.Range, s.Collision, target.GetWaypoints(), target.AvgMovChangeTime(), target.LastMovChangeTime(), target.AvgPathLenght(), target.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());
        }

        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="vectorLenght">Vector Lenght</param>
        /// <returns>Prediction result as <see cref="Prediction.Vector.Result"/></returns>
        public static VectorPrediction.Result GetVectorSPrediction(this Spell s, Obj_AI_Hero target, float vectorLenght)
        {
            return VectorPrediction.GetPrediction(target, s.Width, s.Delay, s.Speed, s.Range, vectorLenght, target.GetWaypoints(), target.AvgMovChangeTime(), target.LastMovChangeTime(), target.AvgPathLenght(), s.RangeCheckFrom.To2D());
        }

        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="s"></param>
        /// <param name="target">Target</param>
        /// <param name="ringRadius">Ring radius</param>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        public static Prediction.Result GetRingSPrediction(this Spell s, Obj_AI_Hero target, float ringRadius)
        {
            return RingPrediction.GetPrediction(target, s.Width, ringRadius, s.Delay, s.Speed, s.Range, s.Collision);
        }

        /// <summary>
        /// Gets aoe prediction result
        /// </summary>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        public static Prediction.AoeResult GetAoeSPrediction(this Spell s)
        {
            if (s.Collision)
                throw new InvalidOperationException("Collisionable spell");

            switch (s.Type)
            {
                case SkillshotType.SkillshotLine:
                    return LinePrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());
                case SkillshotType.SkillshotCircle:
                    return CirclePrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());
                case SkillshotType.SkillshotCone:
                    return ConePrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());
            }

            throw new NotSupportedException("Unknown skill shot type");
        }

        /// <summary>
        /// Gets aoe arc prediction
        /// </summary>
        /// <returns>Prediction result as <see cref="Prediction.Result"/></returns>
        public static Prediction.AoeResult GetAoeArcSPrediction(this Spell s)
        {
            if (s.Collision)
                throw new InvalidOperationException("Collisionable spell");

            return ArcPrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());
        }

        /// <summary>
        /// Gets aoe vector prediction
        /// </summary>
        /// <param name="vectorLenght">Vector lenght</param>
        /// <returns>Prediction result as <see cref="VectorPrediction.AoeResult"/></returns>
        public static VectorPrediction.AoeResult GetAoeVectorSPrediction(this Spell s, float vectorLenght)
        {
            if (s.Collision)
                throw new InvalidOperationException("Collisionable spell");

            return VectorPrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, vectorLenght, s.RangeCheckFrom.To2D());
        }
        #endregion

        #region Collision methods
        /// <summary>
        /// Checks collisions
        /// </summary>
        /// <param name="to">End position</param>
        /// <param name="checkMinion">Check minion collisions</param>
        /// <param name="checkEnemyHero">Check Enemy collisions</param>
        /// <param name="checkYasuoWall">Check Yasuo wall collisions</param>
        /// <param name="checkAllyHero">Check Ally collisions</param>
        /// <param name="checkWall">Check wall collisions</param>
        /// <param name="isArc">Checks collision for arc spell</param>
        /// <returns>true if collision found</returns>
        public static bool CheckCollision(this Spell s, Vector2 to, bool checkMinion = true, bool checkEnemyHero = false, bool checkYasuoWall = true, bool checkAllyHero = false, bool checkWall = false, bool isArc = false)
        {
            return Collision.CheckCollision(s.From.To2D(), to, s.Width, s.Delay, s.Speed, checkMinion, checkEnemyHero, checkYasuoWall, checkAllyHero, checkWall, isArc);
        }

        /// <summary>
        /// Checks minion collisions
        /// </summary>
        /// <param name="to">End position</param>
        /// <param name="isArc">Checks collision for arc spell</param>
        /// <returns>true if collision found</returns>
        public static bool CheckMinionCollision(this Spell s, Vector2 to, bool isArc = false)
        {
            return Collision.CheckMinionCollision(s.From.To2D(), to, s.Width, s.Delay, s.Speed, isArc);
        }

        /// <summary>
        /// Checks enemy hero collisions
        /// </summary>
        /// <param name="to">End position</param>
        /// <param name="isArc">Checks collision for arc spell</param>
        /// <returns>true if collision found</returns>
        public static bool CheckEnemyHeroCollision(this Spell s, Vector2 to, bool isArc = false)
        {
            return Collision.CheckEnemyHeroCollision(s.From.To2D(), to, s.Width, s.Delay, s.Speed, isArc);
        }

        /// <summary>
        /// Checks ally hero collisions
        /// </summary>
        /// <param name="to">End position</param>
        /// <param name="isArc">Checks collision for arc spell</param>
        /// <returns>true if collision found</returns>
        public static bool CheckAllyHeroCollision(this Spell s, Vector2 to, bool isArc = false)
        {
            return Collision.CheckAllyHeroCollision(s.From.To2D(), to, s.Width, s.Delay, s.Speed, isArc);
        }

        /// <summary>
        /// Checks wall collisions
        /// </summary>
        /// <param name="to">End position</param>
        /// <returns>true if collision found</returns>
        public static bool CheckWallCollision(this Spell s, Vector2 to)
        {
            return Collision.CheckWallCollision(s.From.To2D(), to);
        }

        /// <summary>
        /// Check Yasuo wall collisions
        /// </summary>
        /// <param name="to">End position</param>
        /// <param name="isArc">Check collision for arc spell</param>
        /// <returns>true if collision found</returns>
        public static bool CheckYasuoWallCollision(this Spell s, Vector2 to, bool isArc = false)
        {
            return Collision.CheckYasuoWallCollision(s.From.To2D(), to, s.Width, isArc);
        }

        /// <summary>
        /// Gets collision flags
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="isArc">Check collision for arc spell</param>
        /// <returns>true if collision found</returns>
        public static Collision.Flags GetCollisionFlags(this Spell s, Vector2 from, Vector2 to, bool isArc = false)
        {
            Collision.Flags colFlags = Collision.Flags.None;
            if (s.CheckMinionCollision(to))
                colFlags |= Collision.Flags.Minions;

            if (s.CheckEnemyHeroCollision(to))
                colFlags |= Collision.Flags.EnemyChampions;

            if (s.CheckAllyHeroCollision(to))
                colFlags |= Collision.Flags.AllyChampions;

            if (s.CheckWallCollision(to))
                colFlags |= Collision.Flags.Wall;

            if (s.CheckYasuoWallCollision(to))
                colFlags |= Collision.Flags.YasuoWall;

            return colFlags;
        }

        /// <summary>
        /// Gets collided units & flags
        /// </summary>
        /// <param name="to">End position</param>
        /// <param name="isArc">Checks collision for arc spell</param>
        /// <returns>Collision result as <see cref="Collision.Result"/></returns>
        public static Collision.Result GetCollisions(this Spell s, Vector2 to, bool isArc = false)
        {
            return Collision.GetCollisions(s.From.To2D(), to, s.Width, s.Delay, s.Speed, isArc);
        }
        #endregion

        #region Cast methods
        /// <summary>
        /// Spell extension for cast spell with SPrediction
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
            if (Prediction.predMenu != null && Prediction.predMenu.Item("PREDICTONLIST").GetValue<StringList>().SelectedIndex == 1)
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
                return SPredictionCastAoe(s, minHit);

            if (t.HealthPercent > filterHPPercent)
                return false;

            if (Monitor.TryEnter(PathTracker.EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    float avgp = t.AvgPathLenght();
                    var waypoints = t.GetWaypoints();

                    Prediction.Result result;

                    switch (s.Type)
                    {
                        case SkillshotType.SkillshotLine: result = LinePrediction.GetPrediction(t, s.Width, s.Delay, s.Speed, s.Range, s.Collision, waypoints, avgt, movt, avgp, t.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());
                            break;
                        case SkillshotType.SkillshotCircle: result = CirclePrediction.GetPrediction(t, s.Width, s.Delay, s.Speed, s.Range, s.Collision, waypoints, avgt, movt, avgp, t.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());
                            break;
                        case SkillshotType.SkillshotCone: result = ConePrediction.GetPrediction(t, s.Width, s.Delay, s.Speed, s.Range, s.Collision, waypoints, avgt, movt, avgp, t.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());
                            break;
                        default:
                            throw new InvalidOperationException("Unknown spell type");
                    }

                    Prediction.lastDrawTick = Utils.TickCount;
                    Prediction.lastDrawPos = result.CastPosition;
                    Prediction.lastDrawHitchance = result.HitChance.ToString();
                    Prediction.lastDrawDirection = (result.CastPosition - s.From.To2D()).Normalized().Perpendicular();
                    Prediction.lastDrawWidth = (int)s.Width;

                    if (result.HitChance >= hc)
                    {
                        s.Cast(result.CastPosition);
                        return true;
                    }

                    Monitor.Pulse(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                    return false;
                }
                finally
                {
                    Monitor.Exit(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                }
            }

            return false;
        }

        /// <summary>
        /// Spell extension for cast arc spell with SPrediction
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
            if (Prediction.predMenu != null && Prediction.predMenu.Item("PREDICTONLIST").GetValue<StringList>().SelectedIndex == 1)
                throw new NotSupportedException("Arc Prediction not supported in Common prediction");

            if (minHit > 1)
                return SPredictionCastAoeArc(s, minHit);

            if (t.HealthPercent > filterHPPercent)
                return false;

            if (rangeCheckFrom == null)
                rangeCheckFrom = ObjectManager.Player.ServerPosition;

            if (Monitor.TryEnter(PathTracker.EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    float avgp = t.AvgPathLenght();
                    var result = ArcPrediction.GetPrediction(t, s.Width, s.Delay, s.Speed, s.Range, s.Collision, t.GetWaypoints(), avgt, movt, avgp, t.LastAngleDiff(), s.From.To2D(), s.RangeCheckFrom.To2D());

                    if (result.HitChance >= hc)
                    {
                        s.Cast(result.CastPosition);
                        return true;
                    }

                    Monitor.Pulse(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                    return false;
                }
                finally
                {
                    Monitor.Exit(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                }
            }

            return false;
        }

        /// <summary>
        /// Spell extension for cast vector spell with SPrediction
        /// </summary>
        /// <param name="s">Spell to cast</param>
        /// <param name="t">Target for spell</param>
        /// <param name="vectorLenght">Vector lenght</param>
        /// <param name="hc">Minimum HitChance to cast</param>
        /// <param name="reactionIgnoreDelay">Delay to ignore target's reaction time</param>
        /// <param name="minHit">Minimum Hit Count to cast</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
        /// <returns>true if spell has casted</returns>
        public static bool SPredictionCastVector(this Spell s, Obj_AI_Hero t, float vectorLenght, HitChance hc, int reactionIgnoreDelay = 0, byte minHit = 1, Vector3? rangeCheckFrom = null, float filterHPPercent = 100)
        {
            if (Prediction.predMenu != null && Prediction.predMenu.Item("PREDICTONLIST").GetValue<StringList>().SelectedIndex == 1)
                throw new NotSupportedException("Vector Prediction not supported in Common prediction");

            if (minHit > 1)
                return SPredictionCastAoeVector(s, vectorLenght, minHit);

            if (t.HealthPercent > filterHPPercent)
                return false;

            if (rangeCheckFrom == null)
                rangeCheckFrom = ObjectManager.Player.ServerPosition;

            if (Monitor.TryEnter(PathTracker.EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    float avgp = t.AvgPathLenght();
                    var result = VectorPrediction.GetPrediction(t, s.Width, s.Delay, s.Speed, s.Range, vectorLenght, t.GetWaypoints(), avgt, movt, avgp, s.RangeCheckFrom.To2D());

                    if (result.HitChance >= hc)
                    {
                        s.Cast(result.CastSourcePosition, result.CastTargetPosition);
                        return true;
                    }

                    Monitor.Pulse(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                    return false;
                }
                finally
                {
                    Monitor.Exit(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                }
            }

            return false;
        }

        /// <summary>
        /// Spell extension for cast vector spell with SPrediction
        /// </summary>
        /// <param name="s">Spell to cast</param>
        /// <param name="t">Target for spell</param>
        /// <param name="ringRadius">Ring Radius</param>
        /// <param name="hc">Minimum HitChance to cast</param>
        /// <param name="reactionIgnoreDelay">Delay to ignore target's reaction time</param>
        /// <param name="minHit">Minimum Hit Count to cast</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
        /// <returns>true if spell has casted</returns>
        public static bool SPredictionCastRing(this Spell s, Obj_AI_Hero t, float ringRadius, HitChance hc, bool onlyEdge = true, int reactionIgnoreDelay = 0, byte minHit = 1, Vector3? rangeCheckFrom = null, float filterHPPercent = 100)
        {
            if (Prediction.predMenu != null && Prediction.predMenu.Item("PREDICTONLIST").GetValue<StringList>().SelectedIndex == 1)
                throw new NotSupportedException("Vector Prediction not supported in Common prediction");

            if (minHit > 1)
                throw new NotSupportedException("Ring aoe prediction not supported yet");

            if (t.HealthPercent > filterHPPercent)
                return false;

            if (rangeCheckFrom == null)
                rangeCheckFrom = ObjectManager.Player.ServerPosition;

            if (Monitor.TryEnter(PathTracker.EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    float avgp = t.AvgPathLenght();
                    Prediction.Result result;
                    if (onlyEdge)
                        result = RingPrediction.GetPrediction(t, s.Width, ringRadius, s.Delay, s.Speed, s.Range, s.Collision, t.GetWaypoints(), avgt, movt, avgp, s.From.To2D(), rangeCheckFrom.Value.To2D());
                    else
                        result = CirclePrediction.GetPrediction(t, s.Width, s.Delay, s.Speed, s.Range + ringRadius, s.Collision, t.GetWaypoints(), avgt, movt, avgp, 360, s.From.To2D(), rangeCheckFrom.Value.To2D());

                    Prediction.lastDrawTick = Utils.TickCount;
                    Prediction.lastDrawPos = result.CastPosition;
                    Prediction.lastDrawHitchance = result.HitChance.ToString();
                    Prediction.lastDrawDirection = (result.CastPosition - s.From.To2D()).Normalized().Perpendicular();
                    Prediction.lastDrawWidth = (int)ringRadius;

                    if (result.HitChance >= hc)
                    {
                        s.Cast(result.CastPosition);
                        return true;
                    }

                    Monitor.Pulse(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                    return false;
                }
                finally
                {
                    Monitor.Exit(PathTracker.EnemyInfo[t.NetworkId].m_lock);
                }
            }

            return false;
        }

        /// <summary>
        /// Spell extension for cast aoe spell with SPrediction
        /// </summary>
        /// <param name="minHit">Minimum aoe hits to cast</param>
        /// <returns></returns>
        public static bool SPredictionCastAoe(this Spell s, int minHit)
        {
            if (minHit < 2)
                throw new InvalidOperationException("Minimum aoe hit count cannot be less than 2");

            if (s.Collision)
                throw new InvalidOperationException("Collisionable spell");

            Prediction.AoeResult result;

            switch (s.Type)
            {
                case SkillshotType.SkillshotLine: result = LinePrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());
                    break;
                case SkillshotType.SkillshotCircle: result = CirclePrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());
                    break;
                case SkillshotType.SkillshotCone: result = ConePrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());
                    break;
                default:
                    throw new InvalidOperationException("Unknown spell type");
            }

            Prediction.lastDrawTick = Utils.TickCount;
            Prediction.lastDrawPos = result.CastPosition;
            Prediction.lastDrawHitchance = String.Format("Aoe Cast (Hits: {0})", result.HitCount);
            Prediction.lastDrawDirection = (result.CastPosition - s.From.To2D()).Normalized().Perpendicular();
            Prediction.lastDrawWidth = (int)s.Width;

            if (result.HitCount >= minHit)
                return s.Cast(result.CastPosition);

            return false;
        }

        /// <summary>
        /// Spell extension for cast aoe arc spell with SPrediction
        /// </summary>
        /// <param name="minHit">Minimum aoe hits to cast</param>
        /// <returns></returns>
        public static bool SPredictionCastAoeArc(this Spell s, int minHit)
        {
            if (minHit < 2)
                throw new InvalidOperationException("Minimum aoe hit count cannot be less than 2");

            if (s.Collision)
                throw new InvalidOperationException("Collisionable spell");

            Prediction.AoeResult result = ArcPrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, s.From.To2D(), s.RangeCheckFrom.To2D());

            Prediction.lastDrawTick = Utils.TickCount;
            Prediction.lastDrawPos = result.CastPosition;
            Prediction.lastDrawHitchance = String.Format("Arc Aoe Cast (Hits: {0})", result.HitCount);
            Prediction.lastDrawDirection = (result.CastPosition - s.From.To2D()).Normalized().Perpendicular();
            Prediction.lastDrawWidth = (int)s.Width;

            if (result.HitCount >= minHit)
                return s.Cast(result.CastPosition);

            return false;
        }

        /// <summary>
        /// Spell extension for cast aoe vector spell with SPrediction
        /// </summary>
        /// <param name="vectorLenght">Vector lenght</param>
        /// <param name="minHit">Minimum aoe hits to cast</param>
        /// <returns></returns>
        public static bool SPredictionCastAoeVector(this Spell s, float vectorLenght, int minHit)
        {
            if (minHit < 2)
                throw new InvalidOperationException("Minimum aoe hit count cannot be less than 2");

            if (s.Collision)
                throw new InvalidOperationException("Collisionable spell");

            VectorPrediction.AoeResult result = VectorPrediction.GetAoePrediction(s.Width, s.Delay, s.Speed, s.Range, vectorLenght, s.RangeCheckFrom.To2D());

            Prediction.lastDrawTick = Utils.TickCount;
            Prediction.lastDrawPos = result.CastTargetPosition;
            Prediction.lastDrawHitchance = String.Format("Arc Aoe Cast (Hits: {0})", result.HitCount);
            Prediction.lastDrawDirection = (result.CastTargetPosition - s.From.To2D()).Normalized().Perpendicular();
            Prediction.lastDrawWidth = (int)s.Width;

            if (result.HitCount >= minHit)
                return s.Cast(result.CastSourcePosition, result.CastTargetPosition);

            return false;
        }
        #endregion
    }
}
