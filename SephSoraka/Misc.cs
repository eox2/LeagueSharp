using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SephSoraka
{
    class Misc
    {
        public static int GetSlider(String name)
        {
            return Soraka.Config.Item(name).GetValue<Slider>().Value;
        }

        public static bool Active(String MenuItemName)
        {
            return Soraka.Config.Item(MenuItemName).GetValue<bool>();
        }

        public static bool ActiveKeyBind(String itemname)
        {
            return Soraka.Config.Item(itemname).GetValue<KeyBind>().Active;
        }

        public static HitChance GetHitChance(String search)
        {
            var hitchance = Soraka.Config.Item(search).GetValue<StringList>();
            switch (hitchance.SList[hitchance.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "VeryHigh":
                    return HitChance.VeryHigh;
                case "Immobile":
                    return HitChance.Immobile;
            }
            return HitChance.Medium;
        }

        private static Obj_AI_Hero Player = Soraka.Player;

        public static bool isHealthy()
        {
            return Player.HealthPercent > 25;
        }

        public static bool PointUnderEnemyTurret(Vector3 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy && Vector3.Distance(t.Position, Point) < 900f);
            return EnemyTurrets.Any();
        }

        public static bool PointUnderAllyTurret(Vector3 Point)
        {
            var AllyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsAlly && Vector3.Distance(t.Position, Point) < 900f);
            return AllyTurrets.Any();
        }
        public static PredictionOutput GetQPrediction(Obj_AI_Base target)
         {
             var divider = target.Position.Distance(ObjectManager.Player.Position) / Soraka.Spells[SpellSlot.Q].Range;
            Soraka.Spells[SpellSlot.Q].Delay = 0.2f + 0.8f * divider;

            return Soraka.Spells[SpellSlot.Q].GetPrediction(target,true);
       }
    }
}
