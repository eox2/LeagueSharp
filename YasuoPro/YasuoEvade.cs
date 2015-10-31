using System.Linq;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using YasuPro;

namespace YasuoPro
{
    class YasuoEvade
    {

        internal static void Evade()
        {
            if (!Helper.GetBool("Evade.Enabled"))
            {
                return;
            }

            foreach (var skillshot in Program.DetectedSkillshots)
            {

                if (skillshot.Dodged)
                {
                    if (Helper.Debug)
                        Game.PrintChat(skillshot.SpellData.SpellName + " Dodged already");
                    return;
                }

                if (Helper.GetBool("Evade.OnlyDangerous") && !skillshot.SpellData.IsDangerous || !(skillshot.SpellData.DangerValue >= Helper.GetSlider("Evade.MinDangerLevel")))
                {
                    return;
                }

                if (skillshot.SpellData.Type == SkillShotType.SkillshotCircle && !SpellSlot.E.IsReady())
                {
                    return;
                }

                if (skillshot.IsAboutToHit(250, Helper.Yasuo) &&
                    ((Program.NoSolutionFound ||
                      !Program.IsSafePath(Helper.Yasuo.GetWaypoints(), 250).IsSafe &&
                      !Program.IsSafe(Helper.Yasuo.Position.To2D()).IsSafe)))
                {
                    if (skillshot.SpellData.Type != SkillShotType.SkillshotCircle)
                    {
                        if (skillshot.SpellData.CollisionObjects.Contains(CollisionObjectTypes.YasuoWall) && skillshot.Evade(SpellSlot.W) && !Helper.Yasuo.IsDashing() &&
                            Helper.GetBool("Evade.UseW"))
                        {
                            var castpos = Helper.Yasuo.ServerPosition.Extend(skillshot.MissilePosition.To3D(), 50);
                            bool WCasted = Helper.Spells[Helper.W].Cast(castpos);
                            Program.DetectedSkillshots.Remove(skillshot);
                            skillshot.Dodged = WCasted;
                            if (Helper.Debug && WCasted)
                            {
                                Game.PrintChat("Blocked " + skillshot.SpellData.SpellName + " with Windwall ");
                            }
                        }
                        return;
                    }
                    if (skillshot.Evade(SpellSlot.E) && !Helper.Yasuo.IsDashing() && !skillshot.Dodged && Helper.GetBool("Evade.UseE"))
                    {
                        var evadetarget =
                            ObjectManager.Get<Obj_AI_Base>()
                                .Where(
                                    x =>
                                        x.Team != Helper.Yasuo.Team && (x is Obj_AI_Minion || x is Obj_AI_Hero) && x.IsValidTarget(Helper.Spells[Helper.E].Range) &&
                                        Program.IsSafe(Helper.GetDashPos(x)).IsSafe)
                                .OrderBy(x => x.IsMinion)
                                .ThenByDescending(x => x.CountEnemiesInRange(400))
                                .FirstOrDefault();
                        if (evadetarget != null)
                        {
                            Helper.Spells[Helper.E].CastOnUnit(evadetarget);
                            Program.DetectedSkillshots.Remove(skillshot);
                            skillshot.Dodged = true;
                            if (Helper.Debug)
                            {
                                Game.PrintChat("Evading " + skillshot.SpellData.SpellName + " " + "using E to " + evadetarget.BaseSkinName);
                            }
                            return;
                        }
                    }
                }
            }
        }

    }
}
