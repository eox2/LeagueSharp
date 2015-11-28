
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
            internal bool IsEnabled
            {
                get
                {
                    return Helper.GetBool("enabled." + spellName);
                }
            }
        }

        internal static List<SData> spellList = new List<SData>();

        //Credits to h3h3 for spellnames

        static TargettedDanger()
        {
            AddSpells();
        }

        static void AddSpells()
        {
            AddSpell("Syndra", "syndrar", SpellSlot.R);
            AddSpell("Veigar", "veigarprimordialburst", SpellSlot.R);
            AddSpell("Malzahar", "alzaharnethergrasp", SpellSlot.R);
            AddSpell("Caitlyn", "CaitlynAceintheHole", SpellSlot.R, 1000);
            AddSpell("Caitlyn", "CaitlynHeadshotMissile", SpellSlot.Unknown);
            AddSpell("Brand", "BrandWildfire", SpellSlot.R);
            AddSpell("Brand", "brandconflagrationmissile", SpellSlot.E);
            AddSpell("Kayle", "judicatorreckoning", SpellSlot.Q);
            AddSpell("Pantheon", "PantheonQ", SpellSlot.Q);
            AddSpell("Taric", "Dazzle", SpellSlot.Q);
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
            AddSpell("TwistedFate", "goldcardpreattack", SpellSlot.W);
            AddSpell("TwistedFate", "bluecardpreattack", SpellSlot.W);
            AddSpell("TwistedFate", "redcardpreattack", SpellSlot.W);
            AddSpell("Kassadin", "NullLance", SpellSlot.Q);
            AddSpell("Teemo", "BlindingDart", SpellSlot.Q);
            AddSpell("Malphite", "SeismicShard", SpellSlot.Q);
            AddSpell("Vayne", "VayneCondemn", SpellSlot.E);
            AddSpell("Nunu", "IceBlast", SpellSlot.E);
            AddSpell("Tristana", "BusterShot", SpellSlot.R);
            AddSpell("Cassiopeia", "CassiopeiaTwinFang", SpellSlot.E);
            AddSpell("Pantheon", "Pantheon_Throw", SpellSlot.Q);
            AddSpell("Akali", "AkaliMota", SpellSlot.Q);
            AddSpell("Anivia", "Frostbite", SpellSlot.E);
            AddSpell("Katarina", "KatarinaQ", SpellSlot.Q);
            AddSpell("Katarina", "KatarinaRSound", SpellSlot.R);
            AddSpell("Fiddlesticks", "FiddlesticksDarkWind", SpellSlot.E);
            AddSpell("MissFortune", "MissFortuneBulletTime", SpellSlot.E);
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
                if (sender.IsAlly || !args.Target.IsMe || !Helper.GetBool("Evade.WTS") || !SpellSlot.W.IsReady() || (!Helper.GetBool("Evade.FOW") && !sender.IsVisible))
                {
                    return;
                }
                //Console.WriteLine(args.SData.Name + " " + sender.BaseSkinName);
                var sdata = GetSpell(args.SData.Name);
                if (sdata != null && sdata.IsEnabled)
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
