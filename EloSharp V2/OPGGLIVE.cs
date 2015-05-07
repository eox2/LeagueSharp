using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using System.Text.RegularExpressions;
using System.IO;

namespace EloSharp_V2
{
    class OPGGLIVE
    {
        public static List<Info> Ranks { get; set; }

        public class Info
        {
            public String Name { get; set; }
            public String Ranking { get; set; }
            public String lastseason { get; set; }
            public String lpamount { get; set; }
            public Obj_AI_Hero herohandle { get; set; }
            public String rankedwins { get; set; }
            public String rankedwinrate { get; set; }
            public String soloqrank { get; set; }
            public String seriescheck { get; set; }
            public String[] currentrunes { get; set; }
            public String currentmasteries { get; set; }
            public String kda { get; set; }
            public String champname { get; set; }
            public Bitmap champsprite { get; set; }
            public String champwinrate { get; set; }
            public String champtotal { get; set; }
        }


        public static void PerformLookup(string name)
        {
            WebClient Getinfo = new WebClient();
            WebHeaderCollection WHC = new WebHeaderCollection();
            Getinfo.Headers = WHC;
            WHC.Add("Accept-Language: en-US,en;q=0.8"); // Don't want any korean shit
            String source = Getinfo.DownloadString(Misc.getregionurl() + "summoner/ajax/spectator/userName=" + name + "&force=true");
            System.IO.File.WriteAllText(Path.Combine(LeagueSharp.Common.Config.AppDataDirectory, "webpage.txt"), Misc.RemoveSpaces(source)); // Testing purposes
            ParseIt(source);
        }


        static void ParseIt(string raw)
        {
            Ranks = new List<Info>();
            Regex re = new Regex(@"(?<=<td class=""Champion"">)(?s).*?(?=RuneMastery)");
            MatchCollection mc = re.Matches(raw);
            for (int i = 0; i < mc.Count; i++)
            {
                Match match = mc[i];
                Info info = new Info();
                string data = Misc.RemoveSpaces(match.Value);
                try
                {
                    info.Name = Misc.ExtractString(data, @"target=""_blank"">", "</a>");
                    info.champname = Misc.ExtractString(data, @"<div class=""championIcon tip"" title=""", @""">");
                    info.seriescheck = Misc.ExtractString(data, @"<div class=""Series""> Series: ", "</i> </div> </td>");
                    info.Ranking = Misc.ExtractString(data, @"<td class=""TierRank""> <div class=""TierRank""> ", "</div>");
                    info.rankedwinrate = Misc.ExtractString(data, @"<div class=""ratio normal"">", "</div>");
                    info.rankedwins = Misc.ExtractString(data, @"<span class=""title"">(", ")</span>");
                   // info.champtotal = Misc.ExtractString(data, @"<span class=""title"">", "</span>"); -- ranked wins
                    info.champwinrate = RmColor(Misc.ExtractString(data, @"<div class=""WinRatio""> <span class=""ratio", "</span>"));
                    info.champtotal = Misc.ExtractString(data, @"(<span class=""title"">", "</span>)");
                    string kdastring = Misc.ExtractString(data, @"<div class=""KDA"">", "/span>");
                    if (Regex.Match(kdastring, @"[^0-9\.]+").Success)
                    {
                        string kda = Regex.Split(kdastring, @"[^0-9\.]+")[1];
                        Double kdar;
                        if (Double.TryParse(kda, out kdar))
                        {
                            info.kda = kdar.ToString();
                        }
                    }
                    else
                    {
                        info.kda = "0";
                    }
                    info.lastseason = Misc.ExtractString(data, @"<div class=""TierRankImage tip"" title=""", @"""> <img");
                    Ranks.Add(info);
                    Console.WriteLine(info.Name + " " + info.Ranking + " " + info.rankedwins + " " + info.rankedwins + " " + info.champwinrate + " " + info.kda);
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }
            Console.WriteLine("[EloSharp] Data Collection Completed");
        }


        public static string RmColor(string input)
        {
            return Regex.Replace(input, ".*?>", String.Empty);
        }

        public static string SortSeries(string rawseries)
        {
            Console.WriteLine(rawseries);
            string x = rawseries.Replace("win", "✓").Replace("loss", "X").Replace(@"<i class=""icon-minus"">", "-");
            return x;
        }

    }
}