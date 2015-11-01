using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace YasuoPro
{
    class Helper
    {

        internal static Obj_AI_Hero Yasuo = ObjectManager.Player;

        internal static int Q = 1, Q2 = 2, W = 3, E = 4, R = 5, Ignite = 6;

        internal string[] DangerousSpell =
        {
            "syndrar", "veigarprimordialburst", "dazzle", "leblancchaosorb",
            "judicatorreckoning", "iceblast", "disintegrate"
        };

        internal const float LaneClearWaitTimeMod = 2f;

        internal static ItemManager.Item Hydra, Tiamat, Blade, Bilgewater, Youmu;

        internal Orbwalking.Orbwalker Orbwalker
        {
            get { return YasuoMenu.Orbwalker; }
        }

        internal static Dictionary<int, Spell> Spells = new Dictionary<int, Spell>
        {
            { 1, new Spell(SpellSlot.Q, 450f) },
            { 2, new Spell(SpellSlot.Q, 1150f) },
            { 3, new Spell(SpellSlot.W, 450f) },
            { 4, new Spell(SpellSlot.E, 475f) },
            { 5, new Spell(SpellSlot.R, 1250f) },
            { 6, new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600) }
        };

        internal void InitSpells()
        {
            Spells[Q].SetSkillshot(0.250f, 20f, int.MaxValue, false, SkillshotType.SkillshotLine);
            Spells[Q2].SetSkillshot(0.250f, 90, 1500f, false, SkillshotType.SkillshotLine);
        }

        internal float Qrange
        {
            get { return TornadoReady ? Spells[Q2].Range : Spells[Q].Range; }
        }

        internal float Erange
        {
            get { return Spells[E].Range; }
        }

        internal float Rrange
        {
            get { return Spells[R].Range; }
        }

        internal bool TornadoReady
        {
            get { return Yasuo.HasBuff("yasuoq3w"); }
        }

        internal static int DashCount
        {
            get
            {
                var bc = Yasuo.GetBuffCount("yasuodashscalar");
                if (bc == -1)
                {
                    return 0;
                }
                if (bc == 0)
                {
                    return 1;
                }

                if (bc == 2)
                {
                    return 2;
                }
                return 0;
            }
        }

        internal bool UseQ(Obj_AI_Hero target, HitChance minhc = HitChance.Medium)
        {
            if (target == null)
            {
                return false;
            }

            Spell sp = TornadoReady ? Spells[Q2] : Spells[Q];
            PredictionOutput pred = sp.GetPrediction(target);
            if (pred.Hitchance >= minhc)
            {
                return sp.Cast(pred.CastPosition);
            }
            return false;
        }

 
        internal IEnumerable<Obj_AI_Hero> KnockedUp
        {
            get
            {
                return HeroManager.Enemies.Where(x => x.IsInRange(Spells[R].Range) && (x.HasBuffOfType(BuffType.Knockup) || x.HasBuffOfType(BuffType.Knockback)));
            }
        }

        internal static bool isHealthy
        {
            get { return Yasuo.HealthPercent > GetSlider("Misc.Healthy"); }
        }

        internal static bool GetBool(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<bool>();
        }

        internal static bool GetKeyBind(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<KeyBind>().Active;
        }

        internal static int GetSlider(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<Slider>().Value;
        }

        internal static int GetSL(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<StringList>().SelectedIndex;
        }

        internal static Circle GetCircle(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<Circle>();
        }

        internal static Vector2 DashPosition;

        internal static Vector2 GetDashPos(Obj_AI_Base @base)
        {
            var predictedposition = Yasuo.ServerPosition.Extend(@base.Position, Yasuo.Distance(@base) + 475 - Yasuo.Distance(@base)).To2D();
            DashPosition = predictedposition;
            return predictedposition;
        }

        internal static double GetProperEDamage(Obj_AI_Base target)
        {
            double dmg = Yasuo.GetSpellDamage(target, SpellSlot.E);
            float amplifier = 0;
            if (DashCount == 0)
            {
                amplifier = 0;
            }
            else if (DashCount == 1)
            {
                amplifier = 0.25f;
            }
            else if (DashCount == 2)
            {
                amplifier = 0.50f;
            }
            dmg += dmg * amplifier;
            return dmg;
        }

        internal static bool Debug
        {
            get { return GetBool("Misc.Debug"); }
        }

        internal static HitChance GetHitChance(String search)
        {
            var hitchance = YasuoMenu.Config.Item(search).GetValue<StringList>();
            switch (hitchance.SList[hitchance.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }


        internal FleeType FleeMode
        {
            get
            {
                var GetFM = GetSL("Flee.Mode");
                if (GetFM == 0)
                {
                    return FleeType.ToNexus;
                }
                if (GetFM == 1)
                {
                    return FleeType.ToAllies;
                }
                return FleeType.ToCursor;
            }
        } 

        internal enum FleeType
        {
            ToNexus,
            ToAllies,
            ToCursor,
        }

        internal void InitItems()
        {
            Hydra = new ItemManager.Item(3074, 225f, ItemManager.ItemCastType.RangeCast, 1, 2);
            Tiamat = new ItemManager.Item(3077, 225f, ItemManager.ItemCastType.RangeCast, 1, 2);
            Blade = new ItemManager.Item(3153, 450f, ItemManager.ItemCastType.TargettedCast, 1);
            Bilgewater = new ItemManager.Item(3144, 450f, ItemManager.ItemCastType.TargettedCast, 1);
            Youmu = new ItemManager.Item(3142, 185f, ItemManager.ItemCastType.SelfCast, 1, 3);
        }
    }
}
