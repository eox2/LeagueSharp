using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace YasuoPro
{
    static class Extensions
    {
        internal static Obj_AI_Hero Player = Helper.Yasuo;

        internal static bool IsDashable(this Obj_AI_Base unit, float range = 475)
        {
            if (unit == null || unit.Team == Player.Team || unit.Distance(Player) > range || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable)
            {
                return false;
            }
            return !unit.HasBuff("YasuoDashWrapper") && (unit is Obj_AI_Hero || unit is Obj_AI_Minion);
        }

        internal static bool IsValidAlly(this Obj_AI_Base unit, float range = 50000)
        {
            if (unit == null || unit.Distance(Player) > range || unit.Team != Player.Team || !unit.IsValid || unit.IsDead || !unit.IsVisible || unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsValidEnemy(this Obj_AI_Base unit, float range = 50000)
        {
            if (unit == null || unit.Distance(Player) > range || unit.Team == Player.Team || !unit.IsValid || unit.IsDead || !unit.IsVisible || unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            if (unit != null)
            {
                return Vector3.Distance(unit.ServerPosition, Helper.Yasuo.ServerPosition) <= range;
            }
            return false;
        }

        internal static bool PointUnderEnemyTurret(this Vector2 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy && Vector2.Distance(t.Position.To2D(), Point) < 900f);
            return EnemyTurrets.Any();
        }

        internal static bool CanKill(this Obj_AI_Base @base, SpellSlot slot)
        {
            if (slot == SpellSlot.E)
            {
                return Helper.GetProperEDamage(@base) >= @base.Health;
            }
            return Player.GetSpellDamage(@base, slot) >= @base.Health;
        }

        internal static bool IsCloser(this Vector2 point, Obj_AI_Base target)
        {
            var lastwp = target.GetWaypoints().LastOrDefault();
            var midwpnum = target.GetWaypoints().Count() / 2;
            var midwp = target.GetWaypoints()[midwpnum];
            return point.Distance(lastwp) < Player.Distance(lastwp) || point.Distance(midwp) < Player.Distance(midwp);
        }

        internal static bool IsCloser(this Obj_AI_Base @base, Obj_AI_Base target)
        {
            return Helper.GetDashPos(@base).Distance(target) < Player.Distance(target);
        }

        internal static Vector3 WTS(this Vector3 vect)
        {
            return Drawing.WorldToScreen(vect).To3D();
        }


        //Menu Extensions

        internal static Menu AddSubMenu(this Menu menu, string disp)
        {
            return menu.AddSubMenu(new Menu(disp, Assembly.GetExecutingAssembly().GetName() + "." + disp));
        }

        internal static MenuItem AddBool(this Menu menu, string name, string displayname, bool @defaultvalue = true)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(@defaultvalue));
        }

        internal static MenuItem AddKeyBind(this Menu menu, string name, string displayname, uint key, KeyBindType type)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(new KeyBind(key, type)));
        }

        internal static MenuItem AddCircle(this Menu menu, string name, string dname, float range, System.Drawing.Color col)
        {
            return menu.AddItem(new MenuItem(name, name).SetValue(new Circle(true, col, range)));
        }

        internal static MenuItem AddSlider(this Menu menu, string name, string displayname, int initial = 0, int min = 0, int max = 100)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(new Slider(initial, min, max)));
        }

        internal static MenuItem AddSList(this Menu menu, string name, string displayname, string[] stringlist, int @default = 0)
        {
           return menu.AddItem(new MenuItem(name, displayname).SetValue(new StringList(stringlist, @default)));
        }

        internal static bool IsTargetValid(this AttackableUnit unit,
        float range = float.MaxValue,
        bool checkTeam = true,
        Vector3 from = new Vector3())
        {
            if (unit == null || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable ||
                unit.IsInvulnerable)
            {
                return false;
            }

            var @base = unit as Obj_AI_Base;
            if (@base != null)
            {
                if (@base.HasBuff("kindredrnodeathbuff") && @base.HealthPercent <= 10)
                {
                    return false;
                }
            }

            if (checkTeam && unit.Team == ObjectManager.Player.Team)
            {
                return false;
            }

            var unitPosition = @base != null ? @base.ServerPosition : unit.Position;

            return !(range < float.MaxValue) ||
                   !(Vector2.DistanceSquared(
                       (@from.To2D().IsValid() ? @from : ObjectManager.Player.ServerPosition).To2D(),
                       unitPosition.To2D()) > range * range);
        }

        internal static bool QCanKill(this Obj_AI_Base minion, bool isQ2 = false)
        {
            var hpred =
                HealthPrediction.GetHealthPrediction(minion, 0, 500 + Game.Ping / 2);
           return hpred < 0.95 * Player.GetSpellDamage(minion, SpellSlot.Q) && hpred > 0;
        }

        internal static bool isBlackListed(this Obj_AI_Hero unit)
        {
            return !Helper.GetBool("ult" + unit.CharData.BaseSkinName);
        }
    }
}
