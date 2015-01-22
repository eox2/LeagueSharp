using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EloSharp_V2.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;

namespace EloSharp_V2
{
    class Misc
    {
        public static Menu Config;
        public static int SetWebsite;
        public static String RegionTag;

        //Tc CREW
        internal class HpBarIndicator
        {
            internal Obj_AI_Hero Unit { get; set; }

            private Vector2 Offset
            {
                get
                {
                    if (Unit != null)
                    {
                        return Unit.IsAlly ? new Vector2(-9, 14) : new Vector2(-9, 17);
                    }

                    return new Vector2();
                }
            }

            internal Vector2 Position
            {
                get { return new Vector2(Unit.HPBarPosition.X + Offset.X, Unit.HPBarPosition.Y + Offset.Y); }
            }
        }

        // End Tc Crew

        public static void MenuAttach(Menu menu)
        {
            Config = new Menu("EloSharp", "elosharp", true);
            Config.AddItem(new MenuItem("enabledrawings", "Enable Drawings").SetValue(true));
            Config.AddItem(new MenuItem("enablerank", "Draw Rank").SetValue(true));
            Config.AddItem(new MenuItem("enablewinratio", "Draw Win Ratio").SetValue(false));
            Config.AddItem(new MenuItem("enablekdaratio", "Draw KDA Ratio").SetValue(false));
            Config.AddItem(new MenuItem("showunknown", "Show Unknown").SetValue(true));
            Config.AddItem(new MenuItem("printranks", "Print at the beginning").SetValue(true));
            Config.AddItem(new MenuItem("enabledebug", "Enable Debug").SetValue(false));
            Config.AddItem(new MenuItem("autoupdate", "Auto change name").SetValue(true));
            Config.AddItem(new MenuItem("TrackAllies", "Track allies").SetValue(true));
            Config.AddItem(new MenuItem("TrackEnemies", "Track enemies").SetValue(true));
            Config.AddItem(new MenuItem("TrackMe", "Track me").SetValue(false));
            Config.AddItem(new MenuItem("choosewebsite", "Choose Website").SetValue(new StringList(new[] { "LolNexus", "LolSkill", "OPGG"}, 0)));
            SetWebsite = Config.Item("choosewebsite").GetValue<StringList>().SelectedIndex;
            Config.AddSubMenu(new Menu("Loading Screen", "loadingscreen"));
            Config.SubMenu("loadingscreen")
                .AddItem(new MenuItem("enablechampdrawings", "Enable drawings"))
                .SetValue(true);
            Config.SubMenu("loadingscreen")
                .AddItem(new MenuItem("opacity", "Background").SetValue(new Slider(58, 0, 100)));


            Config.AddToMainMenu();
        }

        public static string StripHTML(string html)
        {
            string htmlgarbage = @"<(.|\n)*?>";
            string spaces = @"(?<=>)\s+?(?=<)";
            string removehtml = Regex.Replace(html, htmlgarbage, string.Empty);
            string removedspaces = Regex.Replace(removehtml, spaces, string.Empty).Trim();
            return removedspaces;
        }

        // Credits to Shalzuth for this
        public static string GetRegionInfo()
        {
            Process proc = Process.GetProcesses().First(p => p.ProcessName.Contains("League of Legends"));
            String propFile =
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Path.GetDirectoryName(Path.GetDirectoryName(proc.Modules[0].FileName))))));
            propFile += @"\projects\lol_air_client\releases\";
            DirectoryInfo di =
                new DirectoryInfo(propFile).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).First();
            propFile = di.FullName + @"\deploy\lol.properties";
            propFile = File.ReadAllText(propFile);
            RegionTag = new Regex("regionTag=(.+)\r").Match(propFile).Groups[1].Value;
            return RegionTag;
        }

        // End Shalzuth

        public static string FormatString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

 

        public static bool Validregion()
        {
            string lxtag = sortedregion();
            if (lxtag == "na" || lxtag == "euw" || lxtag == "euw" || lxtag == "eune" || lxtag == "oce" || lxtag == "las" ||
                lxtag == "lan" || lxtag == "ru" || lxtag == "br" || lxtag == "tr")
            {
                return true;
            }
            return false;
        }

        public static string sortedregion()
        {
            if (RegionTag.ToLower().Contains("na"))
            {
                return "na";
            }
            if (RegionTag.ToLower().Contains("euw"))
            {
                return "euw";
            }
            if (RegionTag.ToLower().Contains("eun"))
            {
                return "eune";
            }
            if (RegionTag.ToLower().Contains("la1"))
            {
                return "lan";
            }
            if (RegionTag.ToLower().Contains("la2"))
            {
                return "las";
            }
            if (RegionTag.ToLower().Contains("tr"))
            {
                return "tr";
            }
            if (RegionTag.ToLower().Contains("ru"))
            {
                return "ru";
            }
            if (RegionTag.ToLower().Contains("oc1"))
            {
                return "oce";
            }
            if (RegionTag.ToLower().Contains("br"))
            {
                return "br";
            }
            return "Not Supported";
        }

        public static ColorBGRA ColorRank(string rank)
        {
            if (rank.ToLower().Contains("error"))
            {
                return Color.SandyBrown;
            }
            if (rank.ToLower().Equals("unranked"))
            {
                return Color.SandyBrown;
            }
            if (rank.Contains("Bronze"))
            {
                return Color.Brown;
            }
            if (rank.Contains("Silver"))
            {
                return Color.Silver;
            }
            if (rank.Contains("Gold"))
            {
                return Color.Gold;
            }
            if (rank.Contains("Platinum"))
            {
                // other codes to try: #06828E, #06828E, #33D146, #33D146, #55AC82
                return Color.LawnGreen;
            }
            if (rank.Contains("Diamond"))
            {
                //other codes: #38B0D5, #38B0D5, #2389B1, #3A7FBA
                return Color.DeepSkyBlue;
            }
            if (rank.Contains("Master"))
            {
                // other codes: #B6F3EC, #B6F3EC, #A8E0D5, #73847E, #E5BF50
                return Color.LimeGreen;
            }
            if (rank.Contains("Challenger"))
            {
                //other codes: #DB910D, #DF9B42, #12607E
                return Color.Orange;
            }
            return Color.White;
        }

        public static string ExtractString(string s, string start, string end)
        {
            if (s.Contains(start) && s.Contains(end) && start.Length > 0)
            {
                int startIndex = s.IndexOf(start) + start.Length;
                int endIndex = s.IndexOf(end, startIndex);

                return s.Substring(startIndex, endIndex - startIndex);
            }

            return "";
        }

        public static Bitmap champbitmap(string champname)
        {
            var champicon = (Bitmap)Resources.ResourceManager.GetObject(string.Format("{0}_square_0", champname.ToLower()));
            return champicon;
        }

        public static string getsetwebsite()
        {
            if (SetWebsite == 0) { return "lolnexus"; }
            if (SetWebsite == 1) { return "lolskill"; }
            if (SetWebsite == 2) { return "opgg"; }
            return "lolnexus";
        }

        public static Color rankincolorls(string rank)
        {
            if (rank.ToLower().Contains("error"))
            {
                return Color.SandyBrown;
            }
            if (rank.ToLower().Equals("unranked"))
            {
                return Color.SandyBrown;
            }
            if (rank.Contains("Bronze"))
            {
                return Color.Brown;
            }
            if (rank.Contains("Silver"))
            {
                return Color.Silver;
            }
            if (rank.Contains("Gold"))
            {
                return Color.Gold;
            }
            if (rank.Contains("Platinum"))
            {
                // other codes to try: #06828E, #06828E, #33D146, #33D146, #55AC82
                return Color.LawnGreen;
            }
            if (rank.Contains("Diamond"))
            {
                //other codes: #38B0D5, #38B0D5, #2389B1, #3A7FBA
                return Color.DeepSkyBlue;
            }
            if (rank.Contains("Master"))
            {
                // other codes: #B6F3EC, #B6F3EC, #A8E0D5, #73847E, #E5BF50
                return Color.LimeGreen;
            }
            if (rank.Contains("Challenger"))
            {
                //other codes: #DB910D, #DF9B42, #12607E
                return Color.Orange;
            }
            return Color.White;
        }

        
    }
}
