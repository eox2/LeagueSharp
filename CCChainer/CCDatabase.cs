using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace CCChainer
{
   public static class CCDatabase
   {

       public static List<CCData> CCList = new List<CCData>();

       public class CCData
       {
           public string CCname { get; set; }
           public string CCsource { get; set; }
           public float range { get; set; }
           public bool Skillshot { get; set; }
           public bool GoesThroughMinions { get; set; }
           public string BuffName { get; set; } //Annie Stun 
           public string Skillshotname { get; set; } //match evade
           public float Skillshotspeed { get; set; }
           public float Skillshotwidth { get; set; }
           public float skillshotdelay { get; set; } 
           public SkillshotType skillshottype { get; set; }
           public SpellSlot CCSlot { get; set; }
       }

       //To do add ranges for everything
       static CCDatabase()
       {
           #region Aatrox
           CCList.Add(new CCData { CCname = "Aatrox Q", CCsource = "Aatrox", Skillshot = true, Skillshotname = "AatroxQ", CCSlot = SpellSlot.Q, range = 650f });
           #endregion Aatrox

           #region Ahri
           CCList.Add(new CCData { CCname = "Ahri E", CCsource = "Ahri", Skillshot = true, Skillshotname = "AhriSeduce", CCSlot = SpellSlot.E, range = 1000f });
           #endregion Ahri

           #region Alistar
           CCList.Add(new CCData { CCname = "Alistar Q", CCsource = "Alistar", Skillshot = false, CCSlot = SpellSlot.Q, range = 365f });
           #endregion Alistar

           #region Amumu
           CCList.Add(new CCData { CCname = "Ammumu Q", CCsource = "Amumu", Skillshot = true, Skillshotname = "BandageToss", CCSlot = SpellSlot.Q, range = 1080f });
           CCList.Add(new CCData { CCname = "Ammumu R", CCsource = "Amumu", Skillshot = true, Skillshotname = "CurseoftheSadMummy", CCSlot = SpellSlot.R, range = 550f });
           #endregion Amumu

           #region Anivia
           CCList.Add(new CCData { CCname = "Anivia Q", CCsource = "Anivia", Skillshot = true, Skillshotname = "FlashFrost", CCSlot = SpellSlot.Q, range = 1000f });
           #endregion Anivia

           /* No evade info on the skillshot cbf atm
           #region Azir
           CCList.Add(new CCData { CCname = "Azir E", CCsource = "Azir", Skillshot = true, Skillshotname = "FlashFrost",  CCSlot = SpellSlot.E, range = 875f});
           #endregion Azir
           */

           #region Annie
           CCList.Add(new CCData { CCname = "Annie Q", CCsource = "Annie", Skillshot = false, BuffName = "pyromania_particle", CCSlot = SpellSlot.Q, range = 625f });
           CCList.Add(new CCData { CCname = "Annie W", CCsource = "Annie", Skillshot = true, Skillshotname = "Incinerate", BuffName = "pyromania_particle", CCSlot = SpellSlot.W, GoesThroughMinions = true, range = 625f });
           CCList.Add(new CCData { CCname = "Annie R", CCsource = "Annie", Skillshot = true, Skillshotname = "InfernalGuardian", BuffName = "pyromania_particle", CCSlot = SpellSlot.R, range = 600f });
           #endregion Annie

    
           #region Ashe
           CCList.Add(new CCData { CCname = "Ashe W", CCsource = "Ashe", Skillshot = true, Skillshotname = "VolleyAttack", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 1200f });
           CCList.Add(new CCData { CCname = "Ashe R", CCsource = "Ashe", Skillshot = true, Skillshotname = "EnchantedCrystalArrow", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 20000f });
           #endregion Ashe

           #region Blitzcrank
           CCList.Add(new CCData { CCname = "Blitzcrank Q", CCsource = "Blitzcrank", Skillshot = true, Skillshotname = "RocketGrab", CCSlot = SpellSlot.Q, GoesThroughMinions = false, range = 1050f });
           CCList.Add(new CCData { CCname = "Blitzcrank E", CCsource = "Blitzcrank", Skillshot = false, CCSlot = SpellSlot.E, range = 575f });
           CCList.Add(new CCData { CCname = "Blitzcrank R", CCsource = "Blitzcrank", Skillshot = true, Skillshotname = "StaticField", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 600f });
           #endregion BlitzCrank

           #region Braum
           CCList.Add(new CCData { CCname = "Braum Q", CCsource = "Braum", Skillshot = true, Skillshotname = "BraumQ", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 1050f });
           CCList.Add(new CCData { CCname = "Braum R", CCsource = "Braum", Skillshot = true, Skillshotname = "BraumRWrapper", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 1200f });
           #endregion Braum

           #region Cassio
           CCList.Add(new CCData { CCname = "Cassiopeia R", CCsource = "Cassiopeia", Skillshot = true, Skillshotname = "CassiopeiaPetrifyingGaze", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 825f });
           #endregion Cassio

           #region Chogath
           CCList.Add(new CCData { CCname = "Chogath Q", CCsource = "Chogath", Skillshot = true, Skillshotname = "Rupture", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 950f });
           #endregion Chogath

           #region Elise
           CCList.Add(new CCData { CCname = "Elise E", CCsource = "Elise", Skillshot = true, Skillshotname = "EliseHumanE", CCSlot = SpellSlot.E, GoesThroughMinions = true, range = 1100f });
           #endregion Elise

           #region Galio
           CCList.Add(new CCData { CCname = "Galio R", CCsource = "Galio", Skillshot = false, CCSlot = SpellSlot.R, range = 600 });
           #endregion Galio
           #region Garen
           CCList.Add(new CCData { CCname = "Garen Q", CCsource = "Garen", Skillshot = false, CCSlot = SpellSlot.Q }); //Garen buff name add
           #endregion Garen

           #region Gnar
           CCList.Add(new CCData { CCname = "Gnar Q", CCsource = "Gnar", Skillshot = true, Skillshotname = "GnarQ", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 1125f });
           CCList.Add(new CCData { CCname = "Gnar Q2", CCsource = "Gnar", Skillshot = true, Skillshotname = "GnarBigQ", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 1150f });
           #endregion Gnar

           #region Heimerdinger
           CCList.Add(new CCData { CCname = "Heimerdinger E", CCsource = "Heimerdinger", Skillshot = true, Skillshotname = "HeimerdingerE", CCSlot = SpellSlot.E, GoesThroughMinions = true, range = 925f });
           #endregion Heimerdinger

           #region Irelia
           CCList.Add(new CCData { CCname = "Irelia E", CCsource = "Irelia", Skillshot = false, CCSlot = SpellSlot.E, range = 425 });
           #endregion Irelia

           #region Janna
           CCList.Add(new CCData { CCname = "Janna Q", CCsource = "Janna", Skillshot = true, Skillshotname = "JannaQ", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 1700f });
           #endregion Janna

           #region Leona
           CCList.Add(new CCData { CCname = "Leona Q", CCsource = "Leona", Skillshot = false, CCSlot = SpellSlot.Q, range = ObjectManager.Player.AttackRange }); //Add as buff
           CCList.Add(new CCData { CCname = "Leona E", CCsource = "Leona", Skillshot = true, Skillshotname = "LeonaZenithBlade", CCSlot = SpellSlot.E, GoesThroughMinions = true, range = 905f });
           CCList.Add(new CCData { CCname = "Leona R", CCsource = "Leona", Skillshot = true, Skillshotname = "LeonaSolarFlare", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 1200f });
           #endregion Leona

           #region Lissandra
           CCList.Add(new CCData { CCname = "Lissandra R", CCsource = "Lissandra", Skillshot = false, CCSlot = SpellSlot.R, range = 700f });
           #endregion Lissandra

           #region Lulu
           CCList.Add(new CCData { CCname = "Lulu W", CCsource = "Lulu", Skillshot = false, CCSlot = SpellSlot.W, range = 650f });
           #endregion Lulu

           #region Malphite
           CCList.Add(new CCData { CCname = "Malphite Q", CCsource = "Malphite", Skillshot = false, CCSlot = SpellSlot.Q, range = 625 });
           CCList.Add(new CCData { CCname = "Malphite R", CCsource = "Malphite", Skillshot = true, Skillshotname = "UFSlash", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 1000f });
           #endregion Malphite

           #region Malzahar
           CCList.Add(new CCData { CCname = "Malzahar Q", CCsource = "Malzahar", Skillshot = false, CCSlot = SpellSlot.R, range = 700 });
           #endregion Malzahar

           #region Morgana
           CCList.Add(new CCData { CCname = "Morgana Q", CCsource = "Morgana", Skillshot = true, Skillshotname = "DarkBindingMissile", CCSlot = SpellSlot.Q, GoesThroughMinions = false, range = 1300f });
           #endregion Morgana

           #region Nami
           CCList.Add(new CCData { CCname = "Nami Q", CCsource = "Nami", Skillshot = true, Skillshotname = "NamiQ", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 1625f });
           CCList.Add(new CCData { CCname = "Nami R", CCsource = "Nami", Skillshot = true, Skillshotname = "NamiR", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 2750f });
           #endregion Nami

           #region Nautilus
           CCList.Add(new CCData { CCname = "Nautilus R", CCsource = "Nautilus", Skillshot = false, CCSlot = SpellSlot.R, range = 825f });
           #endregion Nautilus

           #region Pantheon
           CCList.Add(new CCData { CCname = "Pantheon E", CCsource = "Pantheon", Skillshot = false, CCSlot = SpellSlot.E, range = 600f });
           #endregion Pantheon

           #region Renekton
          // CCList.Add(new CCData { CCname = "Renekton W", CCsource = "Renekton", Skillshot = false, CCSlot = SpellSlot.W, range = 600f }); 
           #endregion Renekton

           #region Riven
           CCList.Add(new CCData { CCname = "Riven W", CCsource = "Riven", Skillshot = false, CCSlot = SpellSlot.W, range = 250f });
           #endregion Riven

           #region Sejuani
           CCList.Add(new CCData { CCname = "Sejuani Q", CCsource = "Sejuani", Skillshot = true, Skillshotname = "SejuaniArcticAssault", CCSlot = SpellSlot.Q, GoesThroughMinions = true, range = 900f });
           CCList.Add(new CCData { CCname = "Sejuani R", CCsource = "Sejuani", Skillshot = true, Skillshotname = "SejuaniGlacialPrisonStart", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 1100f });
           #endregion Sejuani

           #region Shen
           CCList.Add(new CCData { CCname = "Shen E", CCsource = "Shen", Skillshot = true, Skillshotname = "ShenShadowDash", CCSlot = SpellSlot.E, GoesThroughMinions = true, range = 650f });
           #endregion Shen

           #region Skarner
           CCList.Add(new CCData { CCname = "Skarner E", CCsource = "Skarner", Skillshot = true, Skillshotname = "SkarnerFracture", CCSlot = SpellSlot.E, GoesThroughMinions = true, range = 1000f });
           CCList.Add(new CCData { CCname = "Skarner R", CCsource = "Skarner", Skillshot = false, CCSlot = SpellSlot.R, range = 350f });
           #endregion Skarner

           #region Sona
           CCList.Add(new CCData { CCname = "Sona R", CCsource = "Sona", Skillshot = true, Skillshotname = "SonaR", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 1000f });
           #endregion Sona

           #region Taric
           CCList.Add(new CCData { CCname = "Taric E", CCsource = "Taric", Skillshot = false, CCSlot = SpellSlot.E, range = 750f });
           #endregion Taric

           #region Urgot
           CCList.Add(new CCData { CCname = "Urgot R", CCsource = "Urgot", Skillshot = false, CCSlot = SpellSlot.R, range = 550f });
           #endregion Urgot

           #region Veigar
           CCList.Add(new CCData { CCname = "Veigar E", CCsource = "Veigar", Skillshot = true, Skillshotname = "VeigarEventHorizon", CCSlot = SpellSlot.E, GoesThroughMinions = true, range = 600f });
           #endregion Veigar

           #region Vi
           CCList.Add(new CCData { CCname = "Vi R", CCsource = "Vi", Skillshot = false, CCSlot = SpellSlot.R, range = 800f });
           #endregion Vi

           #region Warwick
           CCList.Add(new CCData { CCname = "Warwick R", CCsource = "Warwick", Skillshot = false, CCSlot = SpellSlot.R, range = 700f });
           #endregion Warwick

           #region Zyra
           CCList.Add(new CCData { CCname = "Zyra E", CCsource = "Zyra", Skillshot = true, Skillshotname = "ZyraGraspingRoots", CCSlot = SpellSlot.E, GoesThroughMinions = true, range = 800f });
           CCList.Add(new CCData { CCname = "Zyra R", CCsource = "Zyra", Skillshot = true, Skillshotname = "ZyraBrambleZone", CCSlot = SpellSlot.R, GoesThroughMinions = true, range = 700f });
           #endregion Zyra
           
           #region Fiddlesticks
           CCList.Add(new CCData { CCname = "Fiddle Q", CCsource = "Fiddlesticks", Skillshot = false, CCSlot = SpellSlot.Q, range = 575f });
           CCList.Add(new CCData { CCname = "Fiddle E", CCsource = "Fiddlesticks", Skillshot = false, CCSlot = SpellSlot.E, range = 750f });
           #endregion Fiddlesticks

           #region Rammus
           CCList.Add(new CCData { CCname = "Rammus E", CCsource = "Rammus", Skillshot = false, CCSlot = SpellSlot.E, range = 325f });
           #endregion Rammus

       }

       public static List<CCData> GetByChampName(string Champname)
       {
           List<CCData> AbilityList = new List<CCData>();
           foreach (var Data in CCList)
           {
               if (Data.CCsource == Champname)
               {
           
                   AbilityList.Add(Data);
               }
           }
           return AbilityList;
       }

   }


}
