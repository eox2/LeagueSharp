using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephSyndra
{
    class Helper
    {
        internal Orbwalking.Orbwalker Orbwalker;
        internal static Obj_AI_Hero Syndra;

        public void OnLoad()
        {
            SpellManager.Init();
            SyndraMenu.Init();
            Syndra = ObjectManager.Player;
            Orbwalker = SyndraMenu.Orbwalker;
        }

        public Obj_AI_Base GrabbedObject = null;

        internal int GrabbedID
        {
            get { return GrabbedObject != null ? GrabbedObject.NetworkId : -1;  }
        }

        public List<Obj_AI_Minion> GetAllOrbs()
        {
            return ObjectManager.Get<Obj_AI_Minion>()
                        .Where(obj => obj.IsValid && obj.Distance(Syndra) < 700 && obj.IsOrb()).ToList();
        }

        public List<Obj_AI_Minion> GetEOrbs()
        {
            return ObjectManager.Get<Obj_AI_Minion>()
                        .Where(obj => (obj.NetworkId != GrabbedID || !HasSecondW) && obj.IsValid && obj.Distance(Syndra) < 1200 && obj.IsOrb()).ToList();
        }

        public int GetOrbcount()
        {
            return SpellManager.R.Instance.Ammo;
        }


        public bool HasSecondW
        {
            get
            {
                return Syndra.HasBuff("SyndraW");
                   // return Syndra.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2;
            }
        }

        public static string[] BigNeutral = new string[] { "SRU_Blue", "SRU_Red" };



        public List<GrabData> GrabbableObjects
        {
            get
            {
                List<GrabData> objects = new List<GrabData>();
                foreach (var min in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsGrabbable()))
                {
                    GrabData grab = new GrabData();
                    grab.unit = min;
                    if (min.HasExtraEffects()) {
                        grab.priority = 5;
                    }
                    else if (min.IsOrb())
                    {
                        grab.priority = 4;
                    }

                    else
                    {
                        grab.priority = 3;
                    }

                    objects.Add(grab);
                }
                return objects;
            }
        }

        public struct GrabData
        {
            public int priority;
            public Obj_AI_Minion unit;
        }

        internal static bool GetBool(string name)
        {
            return SyndraMenu.Config.Item(name).GetValue<bool>();
        }

        internal static bool GetKeyBind(string name)
        {
            return SyndraMenu.Config.Item(name).GetValue<KeyBind>().Active;
        }

        internal static int GetSliderInt(string name)
        {
            return SyndraMenu.Config.Item(name).GetValue<Slider>().Value;
        }

        internal static float GetSliderFloat(string name)
        {
            return SyndraMenu.Config.Item(name).GetValue<Slider>().Value;
        }


        internal static int GetSL(string name)
        {
            return SyndraMenu.Config.Item(name).GetValue<StringList>().SelectedIndex;
        }

        internal static Circle GetCircle(string name)
        {
            return SyndraMenu.Config.Item(name).GetValue<Circle>();
        }



    }
}
