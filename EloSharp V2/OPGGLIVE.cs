using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;



namespace EloSharp_V2
{
    class OPGGLIVE
    {
        public static bool disabletext;
        public static List<Info> Ranks { get; set; }
        public class Info
        {
            public String Name { get; set; }
            public String Ranking { get; set; }
            public String winratio { get; set; }
            public String gamesplayed { get; set; }
            public String champgamesplayed { get; set; }
            public String lastseason { get; set; }
            public String champwinratio { get; set; }
            public String kdaratio { get; set; }
            public Color winratiocolor { get; set; }
            public Color champwinratiocolor { get; set; }
            public Color kdaratiocolor { get; set; }
            public Bitmap champsprite { get; set; }
            public Obj_AI_Hero herohandle { get; set; }
        }


        public static string rank = "";
        public static Menu Config;


        public OPGGLIVE()
        {
            Ranks = new List<Info>();
            WebClient Getinfo = new WebClient();
            WebHeaderCollection WHC = new WebHeaderCollection();
            Getinfo.Headers = WHC;
            WHC.Add("Accept-Language: en-US,en;q=0.8"); // Don't want any korean shit
            //Getinfo.Encoding = System.Text.Encoding.UTF8;
            String username = Misc.GetRandomHero().Name;
            Game.PrintChat(username);
            byte[] databytes = Getinfo.DownloadData("http://" + Misc.sortedregion() + ".op.gg/summoner/ajax/spectator/userName=" + ObjectManager.Player.Name + "&force=true");
            string data = System.Text.Encoding.UTF8.GetString(databytes, 0, databytes.Length);
            System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\inbetween.txt", data); // Testing purposes
            ExtractInformation(data);
        }



        public static void ExtractInformation(String html)
        {
            var list = ObjectManager.Get<Obj_AI_Hero>();
            foreach (var hero in list)
            {
                Info info = new Info();
                string startinfo = "<a href=\"/summoner/userName=" + hero.Name + "\" class=\"summonerName\" target=\"_blank\">" + hero.Name + "</a>"; 
                string endofinfo = "<td class=\"SummonerName\">";
                string inbetween = Misc.ExtractString(html, startinfo, endofinfo);
                string tierandlp = Misc.ExtractString(inbetween, "<div class=\"TierRank\">", "</div>");
                string winratio = Misc.ExtractString(inbetween, "<td class=\"WinRatio\">\">", "</div>");
                Console.WriteLine(winratio);
                string gamesplayed = Misc.ExtractString(inbetween, "<span class=\"title\">", "</span>");
                Console.WriteLine(gamesplayed);

                //Champ info
                string champsection = Misc.ExtractString(inbetween, "<td class=\"ChampionInfo\">", "<td class=\"ChampionSpell\">");
                Console.Write(champsection);
                string champkda = Misc.ExtractString(champsection, "<div class=\"KDA\">", "</span>");
                Console.WriteLine(champkda);
                string champwinratio = Misc.ExtractString(champsection, "<div class=\"WinRatio\">", "");
                Console.WriteLine(champwinratio);
                string champplayed = Misc.ExtractString(champsection, "<span class=\"title\">", "</span>");
                Console.WriteLine(champplayed);
                string lastseason = Misc.ExtractString(champsection, "<div class=\"TierRankImage tip\" title=\"", "\">");
                Console.WriteLine(lastseason);

                //Add it to list
                info.Name = hero.Name;
                info.Ranking = tierandlp;
                info.winratio = winratio;
                info.gamesplayed = gamesplayed;

                info.champgamesplayed = champplayed;
                info.kdaratio = champkda;
                info.champwinratio = champwinratio;
                info.lastseason = lastseason;
                info.herohandle = hero;
                Bitmap spritechamp = Misc.champbitmap(hero.Name);
                info.champsprite = spritechamp;
                Ranks.Add(info);
                System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\inbetween.txt", inbetween); // Testing purposes
            }
        }


    }
}
