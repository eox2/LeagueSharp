using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp.Common;
using LeagueSharp;
using System.Reflection;

namespace SephSyndra
{
    static class Extensions
    {
        public static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            return ObjectManager.Player.Distance(unit) <= range;
        }

        public static bool IsGrabbable(this Obj_AI_Base unit)
        {
            if (unit == null || !unit.IsValid || unit.IsDead || unit.Name == "WardCorpse" || (unit.Team == ObjectManager.Player.Team && unit.Name != "Seed") || (unit.Name == "Seed" && unit.IsMoving))
            {
                return false;
            }

            if (unit.Distance(ObjectManager.Player) <= SpellManager.W.Range)
            {
                return true;
            }

            return false;
        }

        public static bool HasExtraEffects(this Obj_AI_Base unit)
        {
            return Helper.BigNeutral.Contains(unit.CharData.BaseSkinName);
        }

        public static bool IsOrb(this Obj_AI_Base unit)
        {
            return unit.Team == ObjectManager.Player.Team && !unit.IsDead && unit.Name == "Seed";
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
            return menu.AddItem(new MenuItem(name, dname).SetValue(new Circle(true, col, range)));
        }

        internal static MenuItem AddSlider(this Menu menu, string name, string displayname, int initial = 0, int min = 0, int max = 100)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(new Slider(initial, min, max)));
        }

        internal static MenuItem AddSList(this Menu menu, string name, string displayname, string[] stringlist, int @default = 0)
        {
            return menu.AddItem(new MenuItem(name, displayname).SetValue(new StringList(stringlist, @default)));
        }

        internal static bool IsBlackListed(this Obj_AI_Hero h)
        {
            return Helper.GetBool("noult." + h.ChampionName);
        }
    }
}
