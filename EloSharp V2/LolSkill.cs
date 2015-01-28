using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using LeagueSharp;
using Color2 = SharpDX.Color;

namespace EloSharp_V2
{
    class LolSkill
    {
        public static List<heros> Playerz { get; set; }
        public class heros
        {
            public String Name { get; set; }
            public string Htmlname { get; set; }
        }

        public static List<Infoloading> Ranksloading { get; set; }

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
            public String performance { get; set; }
            public String soloqlp { get; set; }
            public String champwins { get; set; }
            public String champwinrate { get; set; }
            public String winloss { get; set; }
            public String kills { get; set; }
            public String deaths { get; set; }
            public String assists { get; set; }
            public Bitmap champsprite { get; set; }

    

  


        }


        public static void lolskilllookup(string name)
        {
            Console.WriteLine("[EloSharp] LOLSkill FOR: " + name);
            Ranksloading = new List<Infoloading>();
            Playerz = new List<heros>();
            //Get raw information 
            string regiontag = Misc.RegionTag;
            try
            {
                string htmlcode =
                new WebClient().DownloadString("http://www.lolskill.net/game/" + regiontag + "/" + name);

                //System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\lolnexus.txt", htmlcode); // Testing purposes
                //Extract information 
                foreach (
                    Match playerregex in
                        new Regex("<div class=\"summonername\"><a href=\"summoner/" + regiontag + "/(.*?)\">(.*?)</a></div>")
                            .Matches(htmlcode))
                {
                    heros p = new heros();
                    Match playername = new Regex(playerregex.Groups[2].ToString()).Matches(htmlcode)[0];
                    Match playernamehtml = new Regex(playerregex.Groups[1].ToString()).Matches(htmlcode)[0];
                    p.Name = playername.ToString();
                    p.Htmlname = playernamehtml.ToString();
                    Playerz.Add(p);
                }

                foreach (heros p in Playerz)
                {
                    Match htmlmatchinfo = new Regex("<div class=\"summonername\"><a href=\"summoner/" + regiontag + "/" + p.Htmlname + "\">" + p.Name + "</a></div>").Matches(htmlcode)[0];

                    string foundheroinfo = htmlmatchinfo.ToString();
                    string endofheroinfo = "</table>";
                    string inbetween = Misc.ExtractString(htmlcode, foundheroinfo, endofheroinfo);
                    string rankedwins = Misc.ExtractString(inbetween, "<b>Ranked Wins:", "</b><br>");
                    string soloqrank = Misc.StripHTML(Misc.ExtractString(inbetween, "is currently ranked <b>", "</b> in SoloQueue"));
                    string soloqlp = Misc.StripHTML(Misc.ExtractString(inbetween, "and has <b>", "</b> League Points"));
                    //   string checkseries = EloSharp.StripHTML(Misc.ExtractString(inbetween, " <span class=\\\"series\\\">Series", "<td class=\\\"normal-wins\\\">"));
                    // string fixedcheckseries = checkseries.Replace("\\r\\n", string.Empty).Replace(" ", string.Empty);
                    // string fixedseries = fixseries(fixedcheckseries.ToLower());

                    string runesfixed = Misc.ExtractString(inbetween, "<b>Runes:</b><br>", "<br><br><br><b>");
                    char[] delimiters = { '+', '(', '-' };
                    string[] runes = runesfixed.Split(delimiters);

                    Match namechamp = new Regex("title=\"&raquo;" + p.Name + "&laquo; has a LolSkillScore of <b>(.*?)</b> with (.*?)<br>").Matches(htmlcode)[0];
                    string champname = namechamp.ToString();
                    string champnamefix = champname.Replace("&#x27;", string.Empty);
                    string currentmasteries = Misc.StripHTML(Misc.ExtractString(inbetween, "<b>Masteries:</b><br>", "<br><br><i>"));
                    string kda = Misc.StripHTML(Misc.ExtractString(inbetween, "<td class=\\\"champion-kda\\\">\\r\\n    \\r\\n", "</span></td>\\r\\n"));
                    string champtotal = Misc.StripHTML(Misc.ExtractString(inbetween, "</b> out of <b>", "</b> games with"));
                    string performance = Misc.StripHTML(Misc.ExtractString(inbetween, "has performed <b>", "</b>than the average"));
                    string champwins = Misc.ExtractString(inbetween, "title=\"&raquo;" + p.Name + "&laquo; has won <b>", "</b> out of");
                    string champwinrate = Misc.ExtractString(inbetween, "That's a winrate of <b>", "</b>");
                    string winloss = Misc.ExtractString(inbetween, "<td class=\"stat green\">", "<span class=\"small\">");
                    string kills = Misc.ExtractString(inbetween, "has killed <b>", "</b> enemy champions per game");
                    string deaths = Misc.ExtractString(inbetween, "has died <b>", "</b> times per game");
                    string assists = Misc.ExtractString(inbetween, "has had <b>", "</b> assists per game");
                    string champnameforbitmap = champnamefix.ToLower().Replace(" ", "");
                    //Console.WriteLine(champnameforbitmap);
                    Bitmap spritechamp = Misc.champbitmap(champnameforbitmap);

                    Infoloading infoloading = new Infoloading();
                    infoloading.Name = p.Name;
                    infoloading.rankedwins = rankedwins;
                    infoloading.soloqrank = soloqrank;
                    infoloading.soloqlp = soloqlp; // LolSkill exclusive
                    infoloading.currentrunes = runes;
                    infoloading.currentmasteries = currentmasteries;
                    infoloading.champtotal = champtotal;
                    infoloading.kda = kda;
                    infoloading.champsprite = spritechamp;
                    infoloading.champname = champnamefix;
                    // infoloading.seriescheck = fixedseries;
                    infoloading.performance = performance; // LolSkill exclusive
                    infoloading.champwins = champwins; // LolSkill exclusive 
                    infoloading.champwinrate = champwinrate; // LolSkill exclusive
                    infoloading.winloss = winloss; // LolSkill exclusive
                    infoloading.kills = kills; // LolSkill exclusive
                    infoloading.deaths = deaths; // LolSkill exclusive
                    infoloading.assists = assists; // LolSkill exclusive
                    Ranksloading.Add(infoloading);
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





