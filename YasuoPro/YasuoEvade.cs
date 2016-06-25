using System.Collections.Generic;
using System.Linq;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;

namespace YasuoPro
{
    static class YasuoEvade
    {
        private static Random rand = new Random();

        internal static void Evade()
        {
            if (!Helper.GetBool("Evade.Enabled"))
            {
                return;
            }

            if (!Helper.Spells[Helper.W].IsReady() && !Helper.Spells[Helper.E].IsReady())
            {
                return;
            }

            var skillshots = Program.DetectedSkillshots.Where(x => !x.Dodged).OrderBy(x => x.SpellData.DangerValue);

            foreach (var skillshot in skillshots)
            {
                if (skillshot.Dodged)
                {
                    continue;
                }

                //Avoid trying to evade while dashing
                if (Helper.Yasuo.IsDashing())
                {
                    return;
                }

                //Avoid dodging the skillshot if it is not set as dangerous
                if (Helper.GetBool("Evade.OnlyDangerous") && !skillshot.SpellData.IsDangerous)
                {
                    continue;
                }

                var randDist = Helper.Yasuo.BoundingRadius + rand.Next(0, 20);
                //Avoid dodging the skillshot if there is no room/time to safely block it
                if (skillshot.Start.Distance(Helper.Yasuo.ServerPosition) < randDist)
                {
                    continue;
                }


                if (((Program.NoSolutionFound ||
                      !Program.IsSafePath(Helper.Yasuo.GetWaypoints(), 500).IsSafe &&
                      !Program.IsSafe(Helper.Yasuo.ServerPosition.To2D()).IsSafe)))
                {
                    Helper.DontDash = true;
                    bool windWallable = true;
                    if (skillshot.IsAboutToHit(500, Helper.Yasuo))
                    {
                        if (Helper.GetBool("Evade.WFilter"))
                        {
                            windWallable =
                                skillshot.SpellData.CollisionObjects.Contains(CollisionObjectTypes.YasuoWall) &&
                                skillshot.SpellData.Type !=
                                SkillShotType.SkillshotCircle;
                        }

                        if (Helper.GetBool("Evade.UseW") && windWallable)
                        {
                            if (skillshot.Evade(SpellSlot.W)
                                && skillshot.SpellData.DangerValue >= Helper.GetSliderInt("Evade.MinDangerLevelWW"))
                            {
                                var castpos = Helper.Yasuo.ServerPosition.Extend(skillshot.MissilePosition.To3D(), randDist);
                                var delay = Helper.GetSliderInt("Evade.Delay");
                                if (Helper.TickCount - skillshot.StartTick >=
                                    skillshot.SpellData.setdelay +
                                    rand.Next(delay - 77 > 0 ? delay - 77 : 0, delay + 65))
                                {
                                    bool WCasted = Helper.Spells[Helper.W].Cast(castpos);
                                    Program.DetectedSkillshots.Remove(skillshot);
                                    skillshot.Dodged = WCasted;
                                    if (WCasted)
                                    {
                                        if (Helper.Debug)
                                        {
                                            Game.PrintChat("Blocked " + skillshot.SpellData.SpellName +
                                                           " with Windwall ");
                                        }
                                    }
                                }
                            }
                        }

                       else if (Helper.GetBool("Evade.UseE")) {
                            if (skillshot.Evade(SpellSlot.E) && !skillshot.Dodged &&
                                skillshot.SpellData.DangerValue >= Helper.GetSliderInt("Evade.MinDangerLevelE"))
                            {
                                var evadetarget =
                                    ObjectManager
                                        .Get<Obj_AI_Base>()
                                        .Where(
                                            x =>
                                                x.IsDashable() && !Helper.GetDashPos(x).PointUnderEnemyTurret() &&
                                                Program.IsSafe(x.ServerPosition.To2D()).IsSafe &&
                                                Program.IsSafePath(x.GeneratePathTo(), 0, 1200, 250).IsSafe)
                                        .MinOrDefault(x => x.Distance(Helper.shop));

                                if (evadetarget != null)
                                {
                                    Helper.Spells[Helper.E].CastOnUnit(evadetarget);
                                    Program.DetectedSkillshots.Remove(skillshot);
                                    skillshot.Dodged = true;
                                    if (Helper.Debug)
                                    {
                                        Game.PrintChat("Evading " + skillshot.SpellData.SpellName + " " + "using E to " +
                                                       evadetarget.BaseSkinName);
                                    }
                                }
                            }
                        }
                    }
                    Helper.DontDash = false;
                }
            }
        }



        static List<Vector2> GeneratePathTo(this Obj_AI_Base unit)
        {
            List<Vector2> path = new List<Vector2>();
            path.Add(Helper.Yasuo.ServerPosition.To2D());
            path.Add(Helper.GetDashPos(unit));
            return path;
        }

    }
}
