/*
 Copyright 2015 - 2015 SPrediction
 VectorPrediction.cs is part of SPrediction
 
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
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SephSorka.SPrediction
{
    /// <summary>
    /// Vector prediction class
    /// </summary>
    public static class VectorPrediction
    {
        /// <summary>
        /// structure for Vector prediction results
        /// </summary>
        public struct Result
        {
            public Vector2 CastTargetPosition;
            public Vector2 CastSourcePosition;
            public Vector2 UnitPosition;
            public HitChance HitChance;
            public Collision.Result CollisionResult;
        }

        /// <summary>
        /// structure for aoe Vector prediction results
        /// </summary>
        public struct AoeResult
        {
            public Vector2 CastTargetPosition;
            public Vector2 CastSourcePosition;
            public Collision.Result CollisionResult;
            public int HitCount;
        }

        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="input">Neccesary inputs for prediction calculations</param>
        /// <param name="vectorLenght">Vector Lenght</param>
        /// <returns>Prediction result as <see cref="Prediction.Vector.Result"/></returns>
        public static Result GetPrediction(Prediction.Input input, float vectorLenght)
        {
            return GetPrediction(input.Target, input.SpellWidth, input.SpellDelay, input.SpellMissileSpeed, input.SpellRange, vectorLenght, input.Path, input.AvgReactionTime, input.LastMovChangeTime, input.AvgPathLenght, input.RangeCheckFrom.To2D());
        }

        /// <summary>
        /// Gets Prediction result
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="width">Vector width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="vectorSpeed">Vector speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="vectorLenght">Vector lenght</param>
        /// <returns>Prediction result as <see cref="Prediction.Vector.Result"/></returns>
        public static Result GetPrediction(Obj_AI_Hero target, float width, float delay, float vectorSpeed, float range, float vectorLenght)
        {
            return GetPrediction(target, width, delay, vectorSpeed, range, vectorSpeed, target.GetWaypoints(), target.AvgMovChangeTime(), target.LastMovChangeTime(), target.AvgPathLenght(), ObjectManager.Player.ServerPosition.To2D());
        }

        /// <summary>
        /// Gets prediction result
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="width">Vector width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="vectorSpeed">Vector speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="vectorLenght">Vector lenght</param>
        /// <param name="path">Waypoints of target</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="avgp">Average Path Lenght</param>
        /// <param name="rangeCheckFrom"></param>
        /// <returns>Prediction result as <see cref="Prediction.Vector.Result"/></returns>
        public static Result GetPrediction(Obj_AI_Base target, float width, float delay, float vectorSpeed, float range, float vectorLenght, List<Vector2> path, float avgt, float movt, float avgp, Vector2 rangeCheckFrom)
        {
            Prediction.AssertInitializationMode();

            Result result = new Result();

            Vector2 immobileFrom = rangeCheckFrom + (target.ServerPosition.To2D() - rangeCheckFrom).Normalized() * range;

            if (path.Count <= 1) //if target is not moving, easy to hit
            {
                result.HitChance = HitChance.VeryHigh;
                result.CastTargetPosition = target.ServerPosition.To2D();
                result.UnitPosition = result.CastTargetPosition;
                result.CollisionResult = Collision.GetCollisions(immobileFrom, result.CastTargetPosition, width, delay, vectorSpeed);


                if (immobileFrom.Distance(result.CastTargetPosition) > vectorLenght - Prediction.GetArrivalTime(immobileFrom.Distance(result.CastTargetPosition), delay, vectorSpeed) * target.MoveSpeed)
                    result.HitChance = HitChance.OutOfRange;

                return result;
            }

            if (target is Obj_AI_Hero)
            {
                if (((Obj_AI_Hero)target).IsChannelingImportantSpell())
                {
                    result.HitChance = HitChance.VeryHigh;
                    result.CastTargetPosition = target.ServerPosition.To2D();
                    result.UnitPosition = result.CastTargetPosition;
                    result.CollisionResult = Collision.GetCollisions(immobileFrom, result.CastTargetPosition, width, delay, vectorSpeed);

                    //check if target can dodge with moving backward
                    if (immobileFrom.Distance(result.CastTargetPosition) > range - Prediction.GetArrivalTime(immobileFrom.Distance(result.CastTargetPosition), delay, vectorSpeed) * target.MoveSpeed)
                        result.HitChance = HitChance.OutOfRange;

                    return result;
                }

                //to do: find a fuking logic
                if (avgp < 400 && movt < 100)
                {
                    result.HitChance = HitChance.High;
                    result.CastTargetPosition = target.ServerPosition.To2D();
                    result.UnitPosition = result.CastTargetPosition;
                    result.CollisionResult = Collision.GetCollisions(immobileFrom, result.CastTargetPosition, width, delay, vectorSpeed);

                    //check if target can dodge with moving backward
                    if (immobileFrom.Distance(result.CastTargetPosition) > range - Prediction.GetArrivalTime(immobileFrom.Distance(result.CastTargetPosition), delay, vectorSpeed) * target.MoveSpeed)
                        result.HitChance = HitChance.OutOfRange;

                    return result;
                }
            }

            if (target.IsDashing())
                return Prediction.GetDashingPrediction(target, width, delay, vectorSpeed, range, false, SkillshotType.SkillshotLine, immobileFrom).AsVectorResult(immobileFrom);

            if (SPrediction.Utility.IsImmobileTarget(target))
                return Prediction.GetImmobilePrediction(target, width, delay, vectorSpeed, range, false, SkillshotType.SkillshotLine, immobileFrom).AsVectorResult(immobileFrom);
            
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 point = Geometry.ClosestCirclePoint(rangeCheckFrom, range, path[i], path[i + 1]);
                Prediction.Result res = Prediction.WaypointAnlysis(target, width, delay, vectorSpeed, vectorLenght, false, SkillshotType.SkillshotLine, path, avgt, movt, avgp, point);
                if (res.HitChance >= HitChance.Low)
                    return res.AsVectorResult(point);
            }

            result.CastSourcePosition = immobileFrom;
            result.CastTargetPosition = target.ServerPosition.To2D();
            result.HitChance = HitChance.Impossible;
            return result;
        }

        /// <summary>
        /// Gets Aoe Prediction result
        /// </summary>
        /// <param name="width">Spell width</param>
        /// <param name="delay">Spell delay</param>
        /// <param name="vectorSpeed">Vector speed</param>
        /// <param name="range">Spell range</param>
        /// <param name="vectorLenght">Vector lenght</param>
        /// <param name="rangeCheckFrom"></param>
        /// <returns>Prediction result as <see cref="Prediction.AoeResult"/></returns>
        public static AoeResult GetAoePrediction(float width, float delay, float vectorSpeed, float range, float vectorLenght, Vector2 rangeCheckFrom)
        {
            AoeResult result = new AoeResult();
            var enemies = HeroManager.Enemies.Where(p => p.IsValidTarget() && Prediction.GetFastUnitPosition(p, delay, 0, rangeCheckFrom).Distance(rangeCheckFrom) < range);

            foreach (Obj_AI_Hero enemy in enemies)
            {
                List<Vector2> path = enemy.GetWaypoints();
                if (path.Count <= 1)
                {
                    Vector2 from = rangeCheckFrom + (enemy.ServerPosition.To2D() - rangeCheckFrom).Normalized() * range;
                    Vector2 to = from + (enemy.ServerPosition.To2D() - from).Normalized() * vectorLenght;
                    Collision.Result colResult = Collision.GetCollisions(from, to, width, delay, vectorSpeed);

                    if (colResult.Objects.HasFlag(Collision.Flags.EnemyChampions))
                    {
                        int collisionCount = colResult.Units.Count(p => p.IsEnemy && p.IsChampion());
                        if (collisionCount > result.HitCount)
                        {
                            result = new AoeResult
                                     {
                                         CastSourcePosition = from,
                                         CastTargetPosition = enemy.ServerPosition.To2D(),
                                         HitCount = collisionCount,
                                         CollisionResult = colResult
                                     };
                        }
                    }
                }
                else
                {
                    if (!enemy.IsDashing())
                    {
                        for (int i = 0; i < path.Count - 1; i++)
                        {
                            Vector2 point = Geometry.ClosestCirclePoint(rangeCheckFrom, range, path[i], path[i + 1]);
                            Prediction.Result prediction = Prediction.GetPrediction(enemy, width, delay, vectorSpeed, vectorLenght, false, SkillshotType.SkillshotLine, path, enemy.AvgMovChangeTime(), enemy.LastMovChangeTime(), enemy.AvgPathLenght(), point, rangeCheckFrom);
                            if (prediction.HitChance > HitChance.Medium)
                            {
                                Vector2 to = point + (prediction.CastPosition - point).Normalized() * vectorLenght;
                                Collision.Result colResult = Collision.GetCollisions(point, to, width, delay, vectorSpeed, false);
                                if (colResult.Objects.HasFlag(Collision.Flags.EnemyChampions))
                                {
                                    int collisionCount = colResult.Units.Count(p => p.IsEnemy && p.IsChampion());
                                    if (collisionCount > result.HitCount)
                                    {
                                        result = new AoeResult
                                                 {
                                                     CastSourcePosition = point,
                                                     CastTargetPosition = prediction.CastPosition,
                                                     HitCount = collisionCount,
                                                     CollisionResult = colResult
                                                 };
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
