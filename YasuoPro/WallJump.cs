using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;

namespace YasuoPro
{
    class WallJump
    {

        static InventorySlot getItem
        {
            get
            {
                foreach (var item in player.InventoryItems)
                {
                    if (item.Id == ItemId.Warding_Totem_Trinket || item.Id == ItemId.Greater_Vision_Totem_Trinket || item.Id == ItemId.Greater_Stealth_Totem_Trinket)
                    {
                        return item;
                    }

                    if (item.Id == ItemId.Farsight_Orb_Trinket)
                    {
                        return item;
                    }

                    if (item.Id == ItemId.Stealth_Ward)
                    {
                        return item;
                    }

                    if (item.Id == ItemId.Vision_Ward)
                    {
                        return item;
                    }

                    if (item.Id == ItemId.Sightstone)
                    {
                        return item;
                    }
                }
                return null;
            }
        }

        enum TrinketType
        {
            Red,
            Blue,
            Yellow
        }

        internal class JumpSpot
        {
            public Vector3 From;
            public Vector3 To;
            public Vector3 CastPos;

            double x, y, z;

            public bool Contains(Vector3 position)
            {
                return position.Distance(From) <= player.BoundingRadius;
            }
        }

        public static List<JumpSpot> Jumpspots = new List<JumpSpot>();

        static Vector3 Vector(double x, double y, double z)
        {
            return new Vector3((float)x, (float)z, (float)y);
        }

        public static void Initialize()
        {
            Game.OnWndProc += OnWndProc;
        }

        //Credits to Tungkh1711 for the jumpspots
        static WallJump()
        {
            Jumpspots.Add(new JumpSpot { From = Vector(7372, 52.565307617188, 5858), To = Vector(7372, 52.565307617188, 5858), CastPos = Vector(7110, 58.387092590332, 5612) });
            Jumpspots.Add(new JumpSpot { From = Vector(8222, 51.648384094238, 3158), To = Vector(8222, 51.648384094238, 3158), CastPos = Vector(8372, 51.130004882813, 2908) });
            Jumpspots.Add(new JumpSpot { From = Vector(3674, 50.331886291504, 7058), To = Vector(3674, 50.331886291504, 7058), CastPos = Vector(3674, 52.459594726563, 6708) });
            Jumpspots.Add(new JumpSpot { From = Vector(3788, 51.77613067627, 7422), To = Vector(3788, 51.77613067627, 7422), CastPos = Vector(3774, 52.108779907227, 7706) });
            Jumpspots.Add(new JumpSpot { From = Vector(8372, 50.384059906006, 9606), To = Vector(8372, 50.384059906006, 9606), CastPos = Vector(7923, 53.530361175537, 9351) });
            Jumpspots.Add(new JumpSpot { From = Vector(6650, 53.829689025879, 11766), To = Vector(6650, 53.829689025879, 11766), CastPos = Vector(6426, 56.47679901123, 12138) });
            Jumpspots.Add(new JumpSpot { From = Vector(1678, 52.838096618652, 8428), To = Vector(1678, 52.838096618652, 8428), CastPos = Vector(2050, 51.777256011963, 8416) });
            Jumpspots.Add(new JumpSpot { From = Vector(10822, 52.152740478516, 7456), To = Vector(10822, 52.152740478516, 7456), CastPos = Vector(10894, 51.722988128662, 7192) });
            Jumpspots.Add(new JumpSpot { From = Vector(11160, 52.205154418945, 7504), To = Vector(11160, 52.205154418945, 7504), CastPos = Vector(11172, 51.725219726563, 7208) });
            Jumpspots.Add(new JumpSpot { From = Vector(6424, 48.527244567871, 5208), To = Vector(6424, 48.527244567871, 5208), CastPos = Vector(6824, 48.720901489258, 5308) });
            Jumpspots.Add(new JumpSpot { From = Vector(13172, 54.201187133789, 6508), To = Vector(13172, 54.201187133789, 6508), CastPos = Vector(12772, 51.666019439697, 6458) });
            Jumpspots.Add(new JumpSpot { From = Vector(11222, 52.210571289063, 7856), To = Vector(11222, 52.210571289063, 7856), CastPos = Vector(11072, 62.272243499756, 8156) });
            Jumpspots.Add(new JumpSpot { From = Vector(10372, 61.73225402832, 8456), To = Vector(10372, 61.73225402832, 8456), CastPos = Vector(10772, 63.136688232422, 8456) });
            Jumpspots.Add(new JumpSpot { From = Vector(4324, 51.543388366699, 6258), To = Vector(4324, 51.543388366699, 6258), CastPos = Vector(4024, 52.466369628906, 6358) });
            Jumpspots.Add(new JumpSpot { From = Vector(6488, 56.632884979248, 11192), To = Vector(6488, 56.632884979248, 11192), CastPos = Vector(66986, 53.771095275879, 10910) });
            Jumpspots.Add(new JumpSpot { From = Vector(7672, 52.87260055542, 8906), To = Vector(7672, 52.87260055542, 8906), CastPos = Vector(7822, 52.446697235107, 9306) });
        }

        static Obj_AI_Hero player = ObjectManager.Player;

        internal static void OnDraw()
        {
            foreach (var point in Jumpspots)
            {
                Render.Circle.DrawCircle(point.From.To2D().To3D(), player.BoundingRadius, System.Drawing.Color.White);
            }
        }

        internal static bool Jumping = false;
        internal static JumpSpot jumpspot;

        internal static void OnUpdate()
        {
            if (Jumping && jumpspot != null)
            {
                Utility.DelayAction.Add(500, delegate { Jumping = false;  });
                var minion = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValid && x.Team == GameObjectTeam.Neutral && x.IsVisible).MinOrDefault(x => x.Distance(jumpspot.From));
                if (minion == null && !player.IsMoving && player.Distance(jumpspot.From) <= 0.25 * player.BoundingRadius)
                {
                    if (getItem != null && getItem.SpellSlot.IsReady() && jumpspot.CastPos.Distance(jumpspot.From) <= 400)
                    {
                        player.Spellbook.CastSpell(getItem.SpellSlot, jumpspot.CastPos);
                    }

                    else if (SpellSlot.W.IsReady())
                    {
                        Helper.Spells[Helper.W].Cast(jumpspot.CastPos);
                    }
                }

                if (minion != null && Vector3.Distance(minion.Position, jumpspot.CastPos) <= 300)
                {
                    Helper.Spells[Helper.E].CastOnUnit(minion);
                    Jumping = false;
                    jumpspot = null;
                    return;
                }
            }
        }

        internal static void OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN && SpellSlot.E.IsReady())
            {
                var jumpoint = Jumpspots.Find(x => x.Contains(Game.CursorPos));
                if (jumpoint != null)
                {
                    Jumping = true;
                    jumpspot = jumpoint;
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, jumpoint.From);
                }
            }

        }
    }
}

