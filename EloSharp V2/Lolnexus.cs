using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color2 = SharpDX.Color;

namespace EloSharp_V2
{
    class Lolnexus
    {
        private static Render.Sprite buttonicon;
        private static Vector2 _posbutton = new Vector2(Drawing.Width / 2f - 286.5f, 15);
        private static Render.Sprite background;
        private static Vector2 _posbackground = new Vector2(Drawing.Width / 2f, 0);
        private static readonly Vector2 _scalebackground = new Vector2(1f, 1f);
        private static readonly Vector2 _scaleicon = new Vector2(1.0f, 1.0f);
        private static readonly Vector2 _scalesprites = new Vector2(0.2f, 0.2f);
        public static String RegionTag;

        public static List<heros> Playerz { get; set; }
        public class heros
        {
            public String Name { get; set; }
        }

        public static List<Infoloading> Ranksloading { get; set; }
        public static Render.Sprite Sprite;
        public class Infoloading
        {
            public String Name { get; set; }
            public String Ranking { get; set; }
            public String lpamount { get; set; }
            public Obj_AI_Hero herohandle { get; set; }
            public String rankedwins { get; set; }
            public String soloqrank { get; set; }
            public String seriescheck { get; set; }
            public String[] currentrunes { get; set; }
            public String currentmasteries { get; set; }
            public String champtotal { get; set; }
            public String kda { get; set; }
            public String champname { get; set; }
            public Bitmap champsprite { get; set; }
        }


        public static void lolnexuslookup(string name, string region)
        {
            Ranksloading = new List<Infoloading>();
            Playerz = new List<heros>();
            string lxregion = Misc.sortedregion();
            try
            {
                //Get raw information 
                string htmlcode =
                    new WebClient().DownloadString("http://www.lolnexus.com/ajax/get-game-info/" + region + ".json?name=" + name);
               // System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\lolnexus.txt", htmlcode); // Testing purposes
                //Extract information 
                foreach (
                    Match playerregex in
                        new Regex("href=\\\\\"http://" + lxregion + ".op.gg/summoner/userName=(.*?)\\\\\" target=\\\\\"outbound\\\\\"")
                            .Matches(htmlcode))
                {
                    heros p = new heros();
                    Match playername = new Regex(playerregex.Groups[1].ToString()).Matches(htmlcode)[0];
                    p.Name = playername.ToString();


                    Playerz.Add(p);
                }

                foreach (heros p in Playerz)
                {

                    Match htmlmatchinfo =
                        new Regex("href=\\\\\"http://" + Misc.sortedregion() + ".op.gg/summoner/userName=" + p.Name +
                                  "\\\\\" target=\\\\\"outbound\\\\\"").Matches(htmlcode)[0];
                    string foundheroinfo = htmlmatchinfo.ToString();
                    string endofheroinfo = "\\n             </a>\\r\\n         </td>\\r\\n  \\r\\n</tr>";
                    string inbetween = Misc.ExtractString(htmlcode, foundheroinfo, endofheroinfo);
                    string rankedwins = Misc.ExtractString(inbetween, "<span class=\\\"ranked-wins\\\">", "</span>\\r\\n");
                    string soloqrank = Misc.StripHTML(Misc.ExtractString(inbetween, "<div class=\\\"ranking\\\">\\r\\n", "</span>\\r\\n"));
                    string checkseries = Misc.StripHTML(Misc.ExtractString(inbetween, " <span class=\\\"series\\\">Series", "<td class=\\\"normal-wins\\\">"));
                    string fixedcheckseries = checkseries.Replace("\\r\\n", string.Empty).Replace(" ", string.Empty);
                    string fixedseries = fixseries(fixedcheckseries.ToLower());
                    string runesfixed = Misc.ExtractString(inbetween, "class=\\\"tooltip-html\\\"><div><h2>", "</span>\\r\\n");

                    char[] delimiters = { '+', '(', '-' };
                    string[] runes = runesfixed.Split(delimiters);
                    string champname = Misc.StripHTML(Misc.ExtractString(inbetween, "</div>\\r\\n\\r\\n        <span>", "\\r\\n        \\r\\n"));
                    string champnamefix = champname.Replace("&#x27;", string.Empty);

                    string currentmasteries = Misc.StripHTML(Misc.ExtractString(inbetween, "<span class=\\\"offense\\\">", "</span>\\r"));
                    string kda = Misc.StripHTML(Misc.ExtractString(inbetween, "<td class=\\\"champion-kda\\\">\\r\\n    \\r\\n", "</span></td>\\r\\n"));
                    string champtotal = Misc.StripHTML(Misc.ExtractString(inbetween, "<h2>Champion Games</h2>The number of games played with this champion.\\\">", "</b>)</span>"));

                    string champnameforbitmap = champnamefix.ToLower().Replace(" ", "");
                    Game.PrintChat(champnameforbitmap);
                    //Console.WriteLine(champnameforbitmap);
                    Bitmap spritechamp = Misc.champbitmap(champnameforbitmap);

                    Infoloading infoloading = new Infoloading();
                    infoloading.Name = p.Name;
                    infoloading.rankedwins = rankedwins;
                    infoloading.soloqrank = soloqrank;
                    infoloading.currentrunes = runes;
                    infoloading.currentmasteries = currentmasteries;
                    infoloading.champtotal = champtotal;
                    infoloading.kda = kda;
                    infoloading.champsprite = spritechamp;
                    infoloading.champname = champnamefix;
                    infoloading.seriescheck = fixedseries;
                    Ranksloading.Add(infoloading);
                    Console.WriteLine("End of lookup function");
                }
            }
            catch (Exception e)
            {
                Console.Write("Exception" + e);
            }
        }

        public static string fixseries(string rawseries)
        {
            Console.WriteLine(rawseries);
            string x = rawseries.Replace("win", "✓").Replace("loss", "X").Replace("none", "-");
            return x;
        }


    }

}




