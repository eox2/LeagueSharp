using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using SharpDX;
using System.Linq;

namespace SephKhazix
{
    class Helper
    {
        internal static Obj_AI_Hero Khazix = ObjectManager.Player;

        internal static KhazixMenu Config;

        internal Items.Item Hydra, Tiamat, Blade, Bilgewater, Youmu, Titanic;

        internal Spell Q, W, WE, E, R, Ignite;

        internal const float Wangle = 22 * (float) Math.PI / 180;

        internal static bool EvolvedQ, EvolvedW, EvolvedE, EvolvedR;

        internal static SpellSlot IgniteSlot;
        internal static List<Obj_AI_Hero> HeroList;
        internal static List<Vector3> EnemyTurretPositions = new List<Vector3>();
        internal static Vector3 NexusPosition;
        internal static Vector3 Jumppoint1, Jumppoint2;
        internal static bool Jumping;

        internal Orbwalking.Orbwalker Orbwalker
        {
            get
            {
                return Config.Orbwalker;
            }
        }
        internal void InitSkills()
        {
            Q = new Spell(SpellSlot.Q, 325f);
            W = new Spell(SpellSlot.W, 1000f);
            WE = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R, 0);
            W.SetSkillshot(0.225f, 80f, 828.5f, true, SkillshotType.SkillshotLine);
            WE.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);
            Ignite = new Spell(IgniteSlot, 550f);

            Hydra = new Items.Item(3074, 225f);
            Tiamat = new Items.Item(3077, 225f);
            Blade = new Items.Item(3153, 450f);
            Bilgewater = new Items.Item(3144, 450f);
            Youmu = new Items.Item(3142, 185f);
            Titanic = new Items.Item(3748, 225f);

        }

        internal void EvolutionCheck()
        {
            if (!EvolvedQ && Khazix.HasBuff("khazixqevo"))
            {
                Q.Range = 375;
                EvolvedQ = true;
            }
            if (!EvolvedW && Khazix.HasBuff("khazixwevo"))
            {
                EvolvedW = true;
                W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            }

            if (!EvolvedE && Khazix.HasBuff("khazixeevo"))
            {
                E.Range = 1000;
                EvolvedE = true;
            }
        }

        internal void UseItems(Obj_AI_Base target)
        {
            var KhazixServerPosition = Khazix.ServerPosition.To2D();
            var targetServerPosition = target.ServerPosition.To2D();

            if (Hydra.IsReady() && Khazix.Distance(target) <= Hydra.Range)
            {
                Hydra.Cast();
            }
            if (Tiamat.IsReady() && Khazix.Distance(target) <= Tiamat.Range)
            {
                Tiamat.Cast();
            }
            if (Titanic.IsReady() && Khazix.Distance(target) <= Tiamat.Range)
            {
                Tiamat.Cast();
            }
            if (Blade.IsReady() && Khazix.Distance(target) <= Blade.Range)
            {
                Blade.Cast(target);
            }
            if (Youmu.IsReady() && Khazix.Distance(target) <= Youmu.Range)
            {
                Youmu.Cast(target);
            }
            if (Bilgewater.IsReady() && Khazix.Distance(target) <= Bilgewater.Range)
            {
                Bilgewater.Cast(target);
            }
        }

        internal double GetQDamage(Obj_AI_Base target)
        {
            if (Q.Range < 326)
            {
                return 0.984 * Khazix.GetSpellDamage(target, SpellSlot.Q, target.IsIsolated() ? 1 : 0);
            }
            if (Q.Range > 325)
            {
                var isolated = target.IsIsolated();
                if (isolated)
                {
                    return 0.984 * Khazix.GetSpellDamage(target, SpellSlot.Q, 3);
                }
                return Khazix.GetSpellDamage(target, SpellSlot.Q, 0);
            }
            return 0;
        }

        internal List<Obj_AI_Hero> GetIsolatedTargets()
        {
            var validtargets = HeroList.Where(h => h.IsValidTarget(E.Range) && h.IsIsolated()).ToList();
            return validtargets;
        }

        internal static HitChance HarassHitChance(KhazixMenu menu)
        {
            string hitchance = menu.GetSLVal("Harass.WHitchance");
            switch (hitchance)
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

        internal KhazixMenu GenerateMenu()
        {
            Config = new KhazixMenu();
            return Config;
        }

        internal bool IsHealthy
        {
            get
            {
                return Khazix.HealthPercent >= Config.GetSliderFloat("Safety.MinHealth");
            }
        }

        internal bool Override
        {
            get
            {
                return Config.GetKeyBind("Safety.Override");
            }
        }

        internal bool IsInvisible
        {
            get
            {
                return Khazix.HasBuff("khazixrstealth");
            }
        }

    }
}

