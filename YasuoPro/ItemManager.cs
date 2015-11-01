using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;


namespace YasuoPro
{
    static class ItemManager
    {

        private static Obj_AI_Hero Player = ObjectManager.Player;

        internal class Item
        {
            internal ItemCastType type;
            internal int eneinrangecount;
            internal Items.Item item;
            internal int minioncount;

            public Item(int itemid, float range, ItemCastType type, int eneinrange = 1, int minioncount = 1)
            {
                this.type = type;
                this.eneinrangecount = eneinrange;
                item = new Items.Item(itemid, range);
            }


            internal bool Cast(Obj_AI_Hero target, bool farmcast = false)
            {
                if (!item.IsReady())
                {
                    return false;
                }

                if (!farmcast)
                {
                    if ((type == ItemCastType.SelfCast || type == ItemCastType.RangeCast) &&
                        Player.CountEnemiesInRange(item.Range) >= eneinrangecount)
                    {
                        item.Cast();
                        return true;
                    }

                    if (type == ItemCastType.TargettedCast && target.IsInRange(item.Range))
                    {
                        item.Cast(target);
                        return true;
                    }
                }

                else if (farmcast)
                {
                    if ((type == ItemCastType.SelfCast || type == ItemCastType.RangeCast) && ObjectManager.Get<Obj_AI_Minion>().Count(x=> x.IsValidTarget(item.Range)) >= minioncount)
                    {
                        item.Cast();
                        return true;
                    }
                }


                return false;
            }
        }

        internal enum ItemCastType
        {
            SelfCast,
            TargettedCast,
            RangeCast,
            SkillshotCast
        }
    }
}
