using System;
using System.Collections.Generic;
using System.Linq;

namespace SephZhonya
{
    public static class DangerousSpells
    {
        public static List<Data> AvoidableSpells = new List<Data>();

        public class Data
        {
            public String Name { get; set; }
            public String DisplayName { get; set; }
            public String Source { get; set; }
            public String RequiredBuff { get; set; }
            public double BaseDelay { get; set; }
            public double SSDelay { get; set; } // Spellshield delay
            public bool Buffrequirement { get; set; }
            public bool Zhonyable { get; set; }
            public bool SpellShieldable { get; set; }

        }



        static DangerousSpells()
        {
            // List of spells that we can avoid using Zhonya or Spellshields

            #region Azir Ultimate

            AvoidableSpells.Add(
                new Data
                {
                    Name = "AzirR",
                    DisplayName = "Azir R",
                    Source = "Azir",
                    BaseDelay = 0,
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion Azir Ultimate

            #region Karthus Ultimate

            AvoidableSpells.Add(
                new Data
                {
                    Name = "FallenOne",
                    DisplayName = "Karthus R",
                    Source = "Karthus",
                    BaseDelay = 2.5,
                    SSDelay = 2.5,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion Karthus Ultimate

            #region Vi Ultimate

            AvoidableSpells.Add(
                new Data
                {
                    Name = "ViR",
                    DisplayName = "Vi R",
                    Source = "Vi",
                    BaseDelay = 0.3,
                    SSDelay = 1.0,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion Vi Ultimate

            #region SyndraR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "SyndraR",
                    DisplayName = "Syndra R",
                    Source = "Syndra",
                    BaseDelay = 0,
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion SyndraR

            #region VeigarR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "VeigarPrimordialBurst",
                    DisplayName = "Veigar R",
                    Source = "Veigar",
                    BaseDelay = 0,
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion VeigarR

            #region MorganaR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "SoulShackles",
                    DisplayName = "Morgana R",
                    Source = "Morgana",
                    BaseDelay = 2.4,
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion MorganaR

            #region VeigarR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "VeigarPrimordialBurst",
                    DisplayName = "Veigar R",
                    Source = "Veigar",
                    BaseDelay = 0,
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion VeigarR

            #region MalzaharR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "AlZaharNetherGrasp",
                    DisplayName = "Malzahar R",
                    Source = "Malzahar",
                    BaseDelay = 0,
                    SSDelay = 0,
                    Zhonyable = false, //well true but he gets reset so no point
                    SpellShieldable = true
                });

            #endregion MalzaharR

            #region VladR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "VladimirHemoplague",
                    DisplayName = "Vlad R",
                    Source = "Vladimir",
                    BaseDelay = 2.9, //Zhonya lasts 2.5 seconds 
                    SSDelay = 5,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion VladR

            #region CaitlynR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "CaitlynAceintheHole",
                    DisplayName = "Cait R",
                    Source = "Caitlyn",
                    BaseDelay = 0.3, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0.9,
                    Zhonyable = true,
                    SpellShieldable = true
                });

            #endregion CaitlynR

            #region VelkozR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "VelkozR",
                    DisplayName = "Velkoz R",
                    Source = "VelKoz",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false
                });

            #endregion VelkozR

            #region AniviaR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "GlacialStorm",
                    DisplayName = "Anivia R",
                    Source = "Anivia",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = true,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion AniviaR

            #region BrandR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "BrandWildfire",
                    DisplayName = "Brand R",
                    Source = "Brand",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = true,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion BrandR

            #region NunuR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "AbsoluteZero",
                    DisplayName = "Nunu R",
                    Source = "Nunu",
                    BaseDelay = 2.5, //Zhonya lasts 2.5 seconds 
                    SSDelay = 2.8,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = true,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion NunuR

            #region ZyraR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "ZyraBrambleZone",
                    DisplayName = "Zyra R",
                    Source = "Zyra",
                    BaseDelay = 1.85, //Zhonya lasts 2.5 seconds 
                    SSDelay = 1.9,
                    Zhonyable = true,
                    SpellShieldable = true,
                    Buffrequirement = true,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion ZyraR

            #region RumbleR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "RumbleCarpetBomb",
                    DisplayName = "Rumble R",
                    Source = "Rumble",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = true,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion RumbleR

            #region LuxR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "LuxMaliceCannon",
                    DisplayName = "Lux R",
                    Source = "Lux",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0.5,
                    Zhonyable = true,
                    SpellShieldable = true,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion LuxR

            #region LissandraR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "LissandraR",
                    DisplayName = "Lissandra R",
                    Source = "Lissandra",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 1,
                    Zhonyable = true,
                    SpellShieldable = true,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion LissandraR

            #region KennenR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "KennenShurikenStorm",
                    DisplayName = "Kennen R",
                    Source = "Kennen",
                    BaseDelay = 1, //Zhonya lasts 2.5 seconds 
                    SSDelay = 2,
                    Zhonyable = true,
                    SpellShieldable = true,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion KennenR

            #region FiddleR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "Crowstorm",
                    DisplayName = "Fiddle R",
                    Source = "Fiddlesticks",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion FiddleR

            #region FioraR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "FioraDanceStrike",
                    DisplayName = "Fiora R",
                    Source = "Fiora",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion FioraR

            #region FioraR2

            AvoidableSpells.Add(
                new Data
                {
                    Name = "FioraDance",
                    DisplayName = "Fiora R2",
                    Source = "Fiora",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion FioraR2

            #region WukongR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "JarvanIVCataclysm",
                    DisplayName = "Jarvan R",
                    Source = "JarvanIV",
                    BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion WukongR

            #region ZedR

            AvoidableSpells.Add(
                new Data
                {
                    Name = "zedult",
                    DisplayName = "Zed R",
                    Source = "Zed",
                    BaseDelay = 1.5, //Zed untargettable for 0.75 seconds
                    SSDelay = 2.8,
                    Zhonyable = true,
                    SpellShieldable = true,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion ZedR

            #region Jarvan R

            AvoidableSpells.Add(
                new Data
                {
                    Name = "JarvanIVCataclysm",
                    DisplayName = "Jarvan R",
                    Source = "JarvanIV",
                    BaseDelay = 0, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion Jarvan R


            #region Sejuani R

            AvoidableSpells.Add(
                new Data
                {
                    Name = "SejuaniGlacialPrisonStart",
                    DisplayName = "Sejuani R",
                    Source = "Sejuani",
                    BaseDelay = 0.2, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = true,
                    Buffrequirement = false,
                    RequiredBuff = "aniviaslowdebuffnamehere"
                });

            #endregion Sejuani R

            #region Katarina R

            AvoidableSpells.Add(
                new Data
                {
                    Name = "katarianr",
                    DisplayName = "Katarina R",
                    Source = "Katarina",
                    BaseDelay = 0, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "katarinarsound"
                });

            #endregion Katarina R

            #region Urgot R

            AvoidableSpells.Add(
                new Data
                {
                    Name = "urgotswap2",
                    DisplayName = "Urgot R",
                    Source = "Urgot",
                    BaseDelay = 0, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "" //urgot ult debuff name
                });

            #endregion Urgot R

            #region Nocturne R

            AvoidableSpells.Add(
                new Data
                {
                    Name = "NocturneParanoia",
                    DisplayName = "Nocturne R",
                    Source = "Nocturne",
                    BaseDelay = 0.5,
                    SSDelay = 1.0,
                    Zhonyable = true,
                    SpellShieldable = false,
                    Buffrequirement = false,
                    RequiredBuff = "" //maybe nocturne shroud debuffname
                });

            #endregion Nocturne R



        }

        public static Data GetByName(string spellName)
        {
            spellName = spellName.ToLower();
            foreach (var Data in AvoidableSpells)
            {
                if (Data.Name.ToLower() == spellName)
                {
                    return Data;
                }
            }
            return null;
        }


        public static Data GetByName2(string spellName)
        {
            return AvoidableSpells.FirstOrDefault(spell => spell.Name.ToLower() == spellName);
        }
    }
}









