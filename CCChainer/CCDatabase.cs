using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace CCChainer
{
   public static class CCDatabase
   {

       public static List<CCData> CCList;

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


           #region Fiddlesticks
           CCList.Add(new CCData { CCname = "Fiddle Q", CCsource = "Fiddlesticks", Skillshot = false, CCSlot = SpellSlot.Q, range = 575f });
           CCList.Add(new CCData { CCname = "Fiddle E", CCsource = "Fiddlesticks", Skillshot = false, CCSlot = SpellSlot.E, range = 750f });
           #endregion Fiddlesticks

           #region Rammus
           CCList.Add(new CCData { CCname = "Rammus E", CCsource = "Rammus", Skillshot = false, CCSlot = SpellSlot.E });
           #endregion Rammus






       }

   }
}
