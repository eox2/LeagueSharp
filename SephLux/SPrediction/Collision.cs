using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SPrediction
{
    public class Collision
    {
        private static int yasuoWallCastedTick;
        private static int yasuoWallLevel;
        private static Vector2 yasuoWallCastedPos;        

        /// <summary>
        /// Constructor
        /// </summary>
        public Collision()
        {
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        /// <summary>
        /// Checks collisions
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="width">Width</param>
        /// <param name="checkMinion">Check minion collisions</param>
        /// <param name="checkHero">Check Enemy collisions</param>
        /// <param name="checkYasuoWall">Check Yasuo wall collisions</param>
        /// <param name="checkHeroAlly">Check Ally collisions</param>
        /// <param name="checkWall">Check wall collisions</param>
        /// <returns>true if collision found</returns>
        public bool CheckCollision(Vector2 from, Vector2 to, Spell s, bool checkMinion = true, bool checkHero = false, bool checkYasuoWall = true, bool checkHeroAlly = false, bool checkWall = false)
        {
            return (checkMinion && CheckMinionCollision(from, to, s)) ||
                    (checkHero && CheckHeroCollision(from, to, s.Width, checkHeroAlly)) ||
                    (checkYasuoWall && CheckYasuoWallCollision(from, to, s)) ||
                    (checkWall && CheckWallCollision(from, to, s));
        }

        /// <summary>
        /// Checks minion collisions
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="width">Width</param>
        /// <returns>true if collision found</returns>
        public bool CheckMinionCollision(Vector2 from, Vector2 to, Spell s)
        {
            Geometry.Polygon poly = ClipperWrapper.DefineRectangle(from, to, s.Width);           
            HitChance hc;
            return MinionManager.GetMinions(from.Distance(to) + 100, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None).AsParallel().Any(p => ClipperWrapper.IsIntersects(ClipperWrapper.MakePaths(ClipperWrapper.DefineCircle(Prediction.GetPrediction(p, s, p.GetWaypoints(), 0, 0, 0, out hc, p.ServerPosition), p.BoundingRadius)), ClipperWrapper.MakePaths(poly)));
        }

        /// <summary>
        /// Checks hero collisions
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="width">Width</param>
        /// <param name="checkAlly">Check ally heroes</param>
        /// <returns>true if collision found</returns>
        public bool CheckHeroCollision(Vector2 from, Vector2 to, float width, bool checkAlly = false)
        {
            Geometry.Polygon poly = ClipperWrapper.DefineRectangle(from, to, width);
            List<Obj_AI_Hero> listToCheck =  checkAlly ? HeroManager.AllHeroes : HeroManager.Enemies;
            return listToCheck.AsParallel().Any(p => ClipperWrapper.IsIntersects(ClipperWrapper.MakePaths(poly), ClipperWrapper.MakePaths(ClipperWrapper.DefineCircle(p.ServerPosition.To2D(), p.BoundingRadius))));
        }

        /// <summary>
        /// Checks wall collisions
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="width">Width</param>
        /// <returns>true if collision found</returns>
        public bool CheckWallCollision(Vector2 from, Vector2 to, Spell s)
        {
            float step = from.Distance(to) / 20;
            for (var i = 0; i < 20; i++)
            {
                var p = from.Extend(to, step * i);
                if (NavMesh.GetCollisionFlags(p.X, p.Y).HasFlag(CollisionFlags.Wall))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check Yasuo wall collisions
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="width">Width</param>
        /// <returns>true if collision found</returns>
        public bool CheckYasuoWallCollision(Vector2 from, Vector2 to, Spell s)
        {
            if (Utils.TickCount - yasuoWallCastedTick > 4000)
                return false;

            GameObject yasuoWall = ObjectManager.Get<GameObject>().Where(p =>p.IsValid && Regex.IsMatch(p.Name, "_w_windwall_enemy_0.\\.troy", RegexOptions.IgnoreCase)).FirstOrDefault();

            if (yasuoWall == null)
                return false;

            Vector2 yasuoWallDirection = (yasuoWall.Position.To2D() - yasuoWallCastedPos).Normalized().Perpendicular();
            float yasuoWallWidth = 300 + 50 * yasuoWallLevel;
            
            Vector2 yasuoWallStart = yasuoWall.Position.To2D() + yasuoWallWidth / 2f * yasuoWallDirection;
            Vector2 yasuoWallEnd = yasuoWallStart - yasuoWallWidth * yasuoWallDirection;
            
            Geometry.Polygon yasuoWallPoly = ClipperWrapper.DefineRectangle(yasuoWallStart, yasuoWallEnd, 5);
            Geometry.Polygon poly = ClipperWrapper.DefineRectangle(from, to, s.Width);

            return ClipperWrapper.IsIntersects(ClipperWrapper.MakePaths(yasuoWallPoly), ClipperWrapper.MakePaths(poly));
        }

        /// <summary>
        /// Checks Yasuo wall collisions
        /// </summary>
        /// <param name="poly">Polygon to check collision</param>
        /// <returns>true if collision found</returns>
        public bool CheckYasuoWallCollision(Geometry.Polygon poly)
        {
            if (Utils.TickCount - yasuoWallCastedTick > 4000)
                return false;

            GameObject yasuoWall = ObjectManager.Get<GameObject>().Where(p => p.IsValid && Regex.IsMatch(p.Name, "_w_windwall_enemy_0.\\.troy", RegexOptions.IgnoreCase)).FirstOrDefault();

            if (yasuoWall == null)
                return false;

            Vector2 yasuoWallDirection = (yasuoWall.Position.To2D() - yasuoWallCastedPos).Normalized().Perpendicular();
            float yasuoWallWidth = 300 + 50 * yasuoWallLevel;

            Vector2 yasuoWallStart = yasuoWall.Position.To2D() + yasuoWallWidth / 2f * yasuoWallDirection;
            Vector2 yasuoWallEnd = yasuoWallStart - yasuoWallWidth * yasuoWallDirection;

            Geometry.Polygon yasuoWallPoly = ClipperWrapper.DefineRectangle(yasuoWallStart, yasuoWallEnd, 5);

            return ClipperWrapper.IsIntersects(ClipperWrapper.MakePaths(yasuoWallPoly), ClipperWrapper.MakePaths(poly));
        }

        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsValid && sender.IsEnemy && args.SData.Name == "YasuoWMovingWall")
            {
                yasuoWallCastedTick = Utils.TickCount;
                yasuoWallLevel = args.Level;
                yasuoWallCastedPos = sender.ServerPosition.To2D();

                Console.WriteLine("Yasuo wall casted at ({0}, {1}) at {2} tick (wall level : {3})", yasuoWallCastedPos.X, yasuoWallCastedPos.Y, yasuoWallCastedTick, yasuoWallLevel);
            }
        }
    }
}
