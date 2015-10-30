using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace YasuPro
{
    static class Extensions
    {
        internal static Obj_AI_Hero Player = Helper.Yasuo;

        internal static bool IsDashable(this Obj_AI_Base unit)
        {
            return !unit.HasBuff("YasuoDashWrapper");
        }

        internal static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            return Vector3.Distance(unit.ServerPosition, Helper.Yasuo.ServerPosition) <= range;
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
            var midwpnum = target.GetWaypoints().Count / 2;
            var midwp = target.GetWaypoints()[midwpnum];
            return point.Distance(lastwp) < Player.Distance(lastwp) || point.Distance(lastwp) < Player.Distance(midwp);
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

        internal static void AddBool(this Menu menu, string name, string displayname, bool @defaultvalue = true)
        {
            menu.AddItem(new MenuItem(name, displayname).SetValue(@defaultvalue));
        }

        internal static void AddKeyBind(this Menu menu, string name, string displayname, uint key, KeyBindType type)
        {
            menu.AddItem(new MenuItem(name, displayname).SetValue(new KeyBind(key, type)));
        }

        internal static void AddCircle(this Menu menu, string name, string dname, float range, System.Drawing.Color col)
        {
            menu.AddItem(new MenuItem(name, name).SetValue(new Circle(true, col, range)));
        }

        internal static void AddSlider(this Menu menu, string name, string displayname, int initial = 0, int min = 0, int max = 100)
        {
            menu.AddItem(new MenuItem(name, displayname).SetValue(new Slider(initial, min, max)));
        }

        internal static void AddSList(this Menu menu, string name, string displayname, string[] stringlist, int @default = 0)
        {
            menu.AddItem(new MenuItem(name, displayname).SetValue(new StringList(stringlist, @default)));
        }
    }
}
