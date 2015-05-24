using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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


        public static String RemoveSpaces(string s)
        {
            return Regex.Replace(s, @"\s+", " ");
        }
    


        //todo completely redo menu to customize for each website

        public static void MenuAttach(Menu menu)
        {
            Config = new Menu("EloSharp", "elosharp", true);
            Config.AddSubMenu(new Menu("General", "General"));
            Config.SubMenu("General").AddItem(new MenuItem("enabledrawings", "Enable Drawings").SetValue(true));
            Config.SubMenu("General").AddItem(new MenuItem("enablerank", "Draw Rank").SetValue(true));
            Config.SubMenu("General").AddItem(new MenuItem("enablewinratio", "Draw Win Ratio").SetValue(false));
            Config.SubMenu("General").AddItem(new MenuItem("enablekdaratio", "Draw KDA Ratio").SetValue(false));
            Config.SubMenu("General").AddItem(new MenuItem("drawicons", "Draw Icons").SetValue(false));
            Config.SubMenu("General").AddItem(new MenuItem("printranks", "Print at the beginning").SetValue(true));


            Config.AddItem(new MenuItem("showunknown", "Show Unknown").SetValue(true));

            Config.AddItem(new MenuItem("enabledebug", "Enable Debug").SetValue(false));
            Config.AddItem(new MenuItem("autoupdate", "Auto change name").SetValue(true));
            Config.AddItem(new MenuItem("choosewebsite", "Choose Website").SetValue(new StringList(new[] { "LolNexus", "LolSkill - NOT WORKING", "OPGG Live", "Old EloSharp" }, 1)));
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

      

        public static string FormatString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static Obj_AI_Hero GetRandomHero()
        {
            var herolist = ObjectManager.Get<Obj_AI_Hero>().ToList();
            Random r = new Random();
            Obj_AI_Hero randomhero = herolist[r.Next(herolist.Count)];
            return randomhero;
        }


        public static bool Validregion()
        {
            string lxtag = sortedregion();
            if (lxtag == "na" || lxtag == "euw" || lxtag == "euw" || lxtag == "eune" || lxtag == "oce" || lxtag == "las" ||
                lxtag == "lan" || lxtag == "ru" || lxtag == "br" || lxtag == "tr" || lxtag == "kr")
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
            if (RegionTag.ToLower().Contains("kr"))
            {
                return "kr";
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
            if (champicon != null)
            {
                return champicon;
            }
            return (Bitmap)Resources.ResourceManager.GetObject("aatrox_square_0");
        }

        public static string getsetwebsite()
        {
            if (SetWebsite == 0) { return "lolnexus"; }
            if (SetWebsite == 1) { return "lolskill"; }
            if (SetWebsite == 2) { return "opgg"; }
            if (SetWebsite == 3) { return "opgg2"; }
            return "opgg2";
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


        public static float TextWidth(string text, Font f)
        {
            float textWidth = 0;

            using (var bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                textWidth = g.MeasureString(text, f).Width;
            }

            return textWidth;
        }

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


        public static string getregionurl()
        {
            if (Game.Region.ToLower().Contains("na"))
            {
                return "http://na.op.gg/";
            }

            if (Game.Region.ToLower().Contains("euw"))
            {
                return "http://euw.op.gg/";
            }
            if (Game.Region.ToLower().Contains("eun"))
            {
                return "http://eune.op.gg/";
            }
            if (Game.Region.ToLower().Contains("la1"))
            {
                return "http://lan.op.gg/";
            }
            if (Game.Region.ToLower().Contains("la2"))
            {
                return "http://las.op.gg/";
            }
            if (Game.Region.ToLower().Contains("tr"))
            {
                return "http://tr.op.gg/";
            }
            if (Game.Region.ToLower().Contains("ru"))
            {
                return "http://ru.op.gg/";
            }
            if (Game.Region.ToLower().Contains("oc1"))
            {
                return "http://oce.op.gg/";
            }
            if (Game.Region.ToLower().Contains("br"))
            {
                return "http://br.op.gg/";
            }
            if (Game.Region.ToLower().Contains("kr"))
            {
                return "http://op.gg/";
            }
            return null;
        }


        public static byte[] Decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}


