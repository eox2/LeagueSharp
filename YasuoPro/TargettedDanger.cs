
using System;
using System.Collections.Generic;
using System.Linq;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Win32;

namespace YasuoPro
{
    class TargettedDanger
    {
        public class SData
        {
            internal string spellName;
            internal string championName;
            internal SpellSlot spellSlot;
            internal float delay;
        }

        internal static List<SData> spellList = new List<SData>();

        //Credits to h3h3 for spellnames

        static TargettedDanger()
        {
            AddSpell("Syndra", "syndrar", SpellSlot.R);
            AddSpell("VeigarR", "veigarprimordialburst", SpellSlot.R);
            AddSpell("Malzahar", "alzaharnethergrasp", SpellSlot.R);
            AddSpell("Caitlyn", "CaitlynAceintheHole", SpellSlot.R, 1000);
            AddSpell("Caitlyn", "CaitlynHeadshotMissile", SpellSlot.Unknown);
            AddSpell("Brand", "BrandWildfire", SpellSlot.R);
            AddSpell("Brand", "brandconflagrationmissile", SpellSlot.E);
            AddSpell("Kayle", "judicatorreckoning", SpellSlot.Q);
            AddSpell("Malphite", "seismicshard", SpellSlot.Q);
            AddSpell("Pantheon", "PantheonQ", SpellSlot.Q);
            AddSpell("Taric", "Dazzle", SpellSlot.Q);
            AddSpell("TwistedFate", "GoldCardAttack", SpellSlot.W);
            AddSpell("Viktor", "viktorpowertransfer", SpellSlot.Q);
            AddSpell("Ahri", "ahrifoxfiremissiletwo", SpellSlot.W);
            AddSpell("Elise", "EliseHumanQ", SpellSlot.Q);
            AddSpell("Shaco", "TwoShivPoison", SpellSlot.E);
            AddSpell("Urgot", "UrgotHeatseekingHomeMissile", SpellSlot.Q);
            AddSpell("Lucian", "LucianPassiveShot", SpellSlot.Unknown);
            AddSpell("Baron", "BaronAcidBall", SpellSlot.Unknown);
            AddSpell("Baron", "BaronAcidBall2", SpellSlot.Unknown);
            AddSpell("Baron", "BaronDeathBreathProj1", SpellSlot.Unknown);
            AddSpell("Baron", "BaronDeathBreathProj3", SpellSlot.Unknown);
            AddSpell("Baron", "BaronSpike", SpellSlot.Unknown);
            AddSpell("Leblanc", "LeblancChaosOrbM", SpellSlot.Q);
            AddSpell("Annie", "disintegrate", SpellSlot.Q);
        }

        static void AddSpell(string champname, string spellname, SpellSlot  slot, float del = 0)
        {
            spellList.Add(new SData{ championName =  champname, spellName =  spellname, spellSlot =  slot, delay = del });
        }

        public static SData GetSpell(string spellName)
        {
            return spellList.FirstOrDefault(spell => string.Equals(spell.spellName, spellName, StringComparison.CurrentCultureIgnoreCase));
        }


        internal static void SpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsAlly || !args.Target.IsMe || !Helper.GetBool("Evade.WTS") && !SpellSlot.W.IsReady())
            {
                return;
            }
                var sdata = GetSpell(args.SData.Name);
                if (sdata != null)
                {
                    var castpos = Helper.Yasuo.ServerPosition.Extend(args.Start, 50);
                    Helper.Spells[Helper.W].Cast(castpos);
                }
            }
            catch (Exception e)
            {
                
            }
        }
    }
}
