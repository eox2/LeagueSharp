using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace EloSharp
{
    public class EloSharp
    {

        public static List<Info> Ranks { get; set; }

        public class Info
        {
            public String Name { get; set; }
            public String Ranking { get; set; }
            public String lpamount { get; set; }
            public String winratio { get; set; }
            public String kdaratio { get; set; }
            public Color winratiocolor { get; set; }
            public Color kdaratiocolor { get; set; }
            public Obj_AI_Hero herohandle { get; set; }
        }


        public static string rank = "";
        public static Menu Config;
        public static EloSharp elosharp;

        public EloSharp()
        {
            Ranks = new List<Info>();

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {

                Info info = new Info();
                string playerNameEnc = HttpUtility.UrlEncode(hero.Name);
                if (getregionurl() != "Not Supported" && getregionurl().Contains("op.gg"))
                {
                    //String htmlcode = new WebClient().DownloadString(getregionurl() + hero.Name);
                    string htmlcode = "";
                    var request =
                        (HttpWebRequest) WebRequest.Create(getregionurl() + "summoner/userName=" + playerNameEnc);
                    // HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getregionurl() + "summoner/userName=Chief%20Raydere");
                    var response = (HttpWebResponse) request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream receiveStream = response.GetResponseStream();
                        StreamReader readStream = null;

                        if (response.CharacterSet == null)
                        {
                            readStream = new StreamReader(receiveStream);
                        }
                        else
                        {
                            readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                        }

                        htmlcode = readStream.ReadToEnd();
                        response.Close();
                        readStream.Close();
                        Console.WriteLine("Response OK FOR: " + playerNameEnc);
                        //  System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\" + hero.Name + ".txt", htmlcode);
                    }
                    else if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Game.PrintChat("ERROR"); //Console.WriteLine("ERROR: " + playerNameEnc);
                        htmlcode = "ErrorHTTPSTATUS: " + playerNameEnc;
                    }
                    if (htmlcode.Contains("ErrorHTTPSTATUS"))
                    {
                        Console.WriteLine("Http Error: " + htmlcode);
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "HTTPERROR";
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                    if (htmlcode.Contains("tierRank") && htmlcode.Contains("leaguePoints"))
                    {
                        try
                        {
                            Match htmlmatchrank =
                                new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                            Match htmlmatchlp =
                                new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                            Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                            Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                            rank = playerrank.ToString();
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly)
                            {
                                Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy)
                            {
                                Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }
                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = rank;
                            info.lpamount = playerlp.ToString();
                            Ranks.Add(info);
                            Console.WriteLine("Added info" + hero.Name + info.Ranking);
                        }
                        catch (Exception ex)
                        {
                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = "Exception";
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Exception in 1st");
                        }
                    }
                    if (!htmlcode.Contains("ChampionBox Unranked") &&
                        (htmlcode.Contains("tierRank")) && !htmlcode.Contains("leaguePoints"))
                    {
                        try
                        {
                            rank = "Unranked";
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly)
                            {
                                Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy)
                            {
                                Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }

                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = rank;
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Added info" + hero.Name + info.Ranking);
                        }
                        catch (Exception ex)
                        {
                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = "Exception";
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Exception in 4th");
                        }
                    }
                    if (htmlcode.Contains("tierRank") && !htmlcode.Contains("leaguePoints") &&
                        (htmlcode.Contains("ChampionBox Unranked")))
                    {
                        try
                        {
                            Match htmlmatchrank =
                                new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                            Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];

                            rank = "Unranked (L-30)";
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly)
                            {
                                Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy)
                            {
                                Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }
                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = rank;
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Added info" + hero.Name + info.Ranking);
                        }
                        catch (Exception ex)
                        {
                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = "Exception";
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Exception in 2nd");
                        }
                    }
                    if ((htmlcode.Contains("ChampionBox Unranked") &&
                         !htmlcode.Contains("leaguePoints") && (!htmlcode.Contains("tierRank"))))
                    {
                        try
                        {
                            rank = "Unranked";
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly)
                            {
                                Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy)
                            {
                                Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }


                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = "Unranked";
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Added info" + hero.Name + info.Ranking);
                        }
                        catch (Exception ex)
                        {
                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = "Exception";
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Exception in 3rd");
                        }
                    }



                    if (!htmlcode.Contains("ChampionBox Unranked") &&
                        (!htmlcode.Contains("tierRank")))
                    {
                        try
                        {
                            rank = "Error";
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly)
                            {
                                Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy)
                            {
                                Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank);
                            }

                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = rank;
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Added info" + hero.Name + info.Ranking);
                        }
                        catch (Exception ex)
                        {
                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = "Exception";
                            info.lpamount = "";
                            Ranks.Add(info);
                            Console.WriteLine("Exception in 4th");
                        }
                    }


                    if ((Config.Item("enablekdaratio").GetValue<bool>()) ||
                        (Config.Item("enablewinratio").GetValue<bool>()))
                    {

                        //Console.WriteLine("Starting Debug");
                        string data = "";
                        request =
                            (HttpWebRequest)
                                WebRequest.Create(getregionurl() + "summoner/champions/userName=" + playerNameEnc);
                        // request = (HttpWebRequest)WebRequest.Create(getregionurl() + "summoner/champions/userName=Chief Raydere");
                        response = (HttpWebResponse) request.GetResponse();

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Stream receiveStream = response.GetResponseStream();
                            StreamReader readStream = null;

                            if (response.CharacterSet == null)
                            {
                                readStream = new StreamReader(receiveStream);
                            }
                            else
                            {
                                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                            }

                            data = readStream.ReadToEnd();

                            response.Close();
                            readStream.Close();
                        }


                        if (data.Contains("__spc24 __spc24-" + championid(hero) + "\""))
                        {
                
                            int index = data.IndexOf("__spc24 __spc24-" + championid(hero) + "\"");
                            String htmlstats = data.Remove(0, index);
                            index = htmlstats.IndexOf("gold");
                            htmlstats = htmlstats.Substring(0, index);

                            Match htmlmatchwinRatio =
                                new Regex(@"\<span class=\""winRatio\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchkill =
                                new Regex(@"\<span class=\""kill\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchdeath =
                                new Regex(@"\<span class=\""death\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchassist =
                                new Regex(@"\<span class=\""assist\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchkdaratio =
                                new Regex(@"\<span class=\""kdaratio\"">(.*?)</span>").Matches(htmlstats)[0];

                            Match winRatio = new Regex(htmlmatchwinRatio.Groups[1].ToString()).Matches(htmlstats)[0];
                            Match kill = new Regex(htmlmatchkill.Groups[1].ToString()).Matches(htmlstats)[0];
                            Match death = new Regex(htmlmatchdeath.Groups[1].ToString()).Matches(htmlstats)[0];
                            Match assist = new Regex(htmlmatchassist.Groups[1].ToString()).Matches(htmlstats)[0];
                            Match kdaratio = new Regex(htmlmatchkdaratio.Groups[1].ToString()).Matches(htmlstats)[0];

                            index = htmlstats.IndexOf("wins");
                            htmlstats = htmlstats.Remove(0, (index + 6));
                            index = htmlstats.IndexOf("</span>");
                            String wins = htmlstats.Substring(0, index);
                            wins = wins.Trim();
                            int Length = wins.Length;
                            wins = wins.Substring(0, Length - 1);

                            index = htmlstats.IndexOf("losses");
                            htmlstats = htmlstats.Remove(0, (index + 8));
                            index = htmlstats.IndexOf("</span>");
                            String losses = htmlstats.Substring(0, index);
                            losses = losses.Trim();
                            Length = losses.Length;
                            losses = losses.Substring(0, Length - 1);

                            String winratioString = ("Win Ratio (champ) = " + winRatio + " (" + wins + "/" + losses +
                                                     ")");

                            String kdaString = ("KDA = " + kdaratio + " (" + kill + "K + " + assist + "A / " + death +
                                                "D)");

              
                            if (kdaratio.ToString().Contains("."))
                            {
                                index = kdaratio.ToString().IndexOf(".");
                            }
                                 
                            else
                            {
                                index = kdaratio.ToString().IndexOf(":");
                            }
                            String kdaratio2 = kdaratio.ToString().Substring(0, index);
                            int kdaratiocolor = 0;

                            if (Int32.TryParse(kdaratio2, out kdaratiocolor))
                            {
                                //Console.WriteLine(kdaratiocolor);
                                if (kdaratiocolor > 8)
                                {
                                    info.kdaratiocolor = Color.Orange; //Challenger
                                }
                                if (kdaratiocolor >= 6 && kdaratiocolor < 8)
                                {
                                    info.kdaratiocolor = Color.LimeGreen; //Master
                                }
                                if (kdaratiocolor >= 4 && kdaratiocolor < 6)
                                {
                                    info.kdaratiocolor = Color.Cyan; //Diamond
                                }
                                if (kdaratiocolor >= 3 && kdaratiocolor < 4)
                                {
                                    info.kdaratiocolor = Color.DeepSkyBlue; //Platinum
                                }
                                if (kdaratiocolor >= 2 && kdaratiocolor < 3)
                                {
                                    info.kdaratiocolor = Color.Gold; //Gold
                                }
                                if (kdaratiocolor >= 1 && kdaratiocolor < 2)
                                {
                                    info.kdaratiocolor = Color.Silver; //Silver
                                }
                                if (kdaratiocolor < 1)
                                {
                                    info.kdaratiocolor = Color.SandyBrown; //Bronze
                                }
                            }

                            string stringwinratio = winRatio.ToString();
                            string winrateremovepercent = stringwinratio.Replace("%", "");
                            double winratio = Convert.ToInt32(winrateremovepercent);
                            info.winratiocolor = colorwinratio(winratio);
                            info.winratio = winratioString;
                            info.kdaratio = kdaString;
                            Ranks.Add(info);
              
                        }
                        else
                        {
                            info.winratio = "error";
                            info.kdaratio = "error";
                            info.winratiocolor = Color.White;
                            Ranks.Add(info);
            
                        }
                    }
                    //  Ranks.Add(info);
                }
            

            if (getregionurl() != "Not Supported" && getregionurl().Contains("quickfind")) // Garena lookups
                {
                   
                        try
                        {
                            var container = new CookieContainer();
                            playerNameEnc = "Tsu of Cat";
                            var inforequest1 = (HttpWebRequest) WebRequest.Create(getregionurl() + playerNameEnc + "/");
                            inforequest1.UserAgent =
                                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
                            inforequest1.KeepAlive = true;
                            inforequest1.Accept = "*/*";
                            inforequest1.CookieContainer = container;
                            inforequest1.Method = "POST";
                            inforequest1.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                            var respo = (HttpWebResponse) inforequest1.GetResponse();
                            container.Add(respo.Cookies);
                            string responser = new StreamReader(respo.GetResponseStream()).ReadToEnd();
                            string zmqid = ExtractString(responser, "QF.player =", ";").Replace("\"", "");
                            string memcache = ExtractString(responser, "memcache: \"", "\"");
                            string csrfToken = ExtractString(responser, "csrfToken:", "};").Replace("\"", "");
                            string datatobeposted = "zeromq_key=" + zmqid + "&memcache=" + memcache + "&csrfToken=" + csrfToken;
                            string referer = getregionurl() + playerNameEnc + "/";
                            string useragent =
                                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
                            var requri = new Uri("http://quickfind.kassad.in/ahnlab/" + getregioncode() + "/AcquisitionServiceGate/LSP.aspx");
                            var inforequest = (HttpWebRequest) WebRequest.Create(requri);
                            inforequest.UserAgent = useragent;
                            byte[] byteArray = Encoding.UTF8.GetBytes(datatobeposted);
                            inforequest.KeepAlive = true;
                            inforequest.CookieContainer = container;
                            inforequest.Method = "POST";
                            inforequest.ContentLength = byteArray.Length;
                            inforequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                            inforequest.Accept = "application/json, text/javascript, */*; q=0.01";
                            inforequest.Referer = referer;
                            WebHeaderCollection WebHeaders = inforequest.Headers;
                            WebHeaders.Add("Origin: http://quickfind.kassad.in");
                          //  WebHeaders.Add("Accept-Encoding: gzip, deflate");
                            WebHeaders.Add("X-Requested-With: XMLHttpRequest");
                            WebHeaders.Add("Accept-Language: en-US,en;q=0.8");
                            Stream dataStream = inforequest.GetRequestStream();
                            dataStream.Write(byteArray, 0, byteArray.Length);
                            dataStream.Close();
                            var response = (HttpWebResponse) inforequest.GetResponse();
                            Console.WriteLine(response.StatusDescription);
                            dataStream = response.GetResponseStream();
                            var reader = new StreamReader(dataStream);
                            string responseFromServer = reader.ReadToEnd();
                            

                       //     System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\responseswag.txt", responseFromServer);

                            string winrate = ExtractString(responseFromServer, "\"rate\":", ",");
                            string wins = ExtractString(responseFromServer, ",\"w\":",",");
                            string losses = ExtractString(responseFromServer, ",\"l\":", ",");
                            string totalranked = ExtractString(responseFromServer, ",\"total\":", ",");
                            string tier = ExtractString(responseFromServer, ",\"tier\":\"", "\"}");
                            //Game.PrintChat("WR: " + winrate + " Wins: " + wins + " Losses: " + losses + " Total: " + totalranked + " Tier: " + tier);
                            string lp = ExtractString(responseFromServer,
                                                           "<a href='\\/profile\\/" + getregioncode() + "\\/" + hero.Name + "\\/'>" + hero.Name +
                                                           "<\\/a><\\/td><td class='hide-sm'><\\/td><td>", "<\\/td><td>");
                            String winratioString = ("Win Ratio = " + winrate + "%% (" + wins + "/" + losses + ")");

                            double wr;
                            double.TryParse(winrate, out wr);
                            /* Old sht
                           string getsumminfo = ExtractString(responseFromServer,
                                "<a href='\\/profile\\/" + getregioncode() + "\\/" + hero.Name + "\\/'>" + hero.Name +
                                "<\\/a><\\/td><td class='hide-sm'><\\/td><td>", "<a href=");
                            string lp = ExtractString(responseFromServer,
                                "<a href='\\/profile\\/" + getregioncode() + "\\/" + hero.Name + "\\/'>" + hero.Name +
                                "<\\/a><\\/td><td class='hide-sm'><\\/td><td>", "<\\/td><td>");
                            string rankedwins = ExtractString(responseFromServer,
                                "<a href='\\/profile\\/" + getregioncode() + "\\/" + hero.Name + "\\/'>" + hero.Name +
                                "<\\/a><\\/td><td class='hide-sm'><\\/td><td>" + lp + "<\\/td><td>", "<\\/td><\\/tr>");
                            string ranky = ExtractString(getsumminfo, "<tr class='rR '><td class='hide-sm'>",
                                "<\\/td><td>");
                            string rankinfosecondary = ExtractString(responseFromServer,
                                "<span>Ranked Solo 5v5<\\/span><strong>", "<\\/strong><\\/div>");
                            string winslosses = ExtractString(responseFromServer,
                                "<u style='text-decoration:none;color:#444;font-weight:bold'>", "<\\/u>");
                             


                              int winss = Convert.ToInt32(wins);
                            int lossess = Convert.ToInt32(losses);
                            double winRatio = Math.Round((double) winss/(winss + lossess)*100, 0);
                            String winratioString = ("Win Ratio = " + winRatio + "%% (" + wins + "/" + losses + ")");
                           String winratioString = winslosses;


                             double.TryParse(wins, out winss);
                              double.TryParse(losses, out lossess);
                              double winRatiopercent = (winss / (winss + lossess)) * 100;
          
                             just % causes bugsplat
                              */
                            reader.Close();
                            dataStream.Close();
                            response.Close();
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly)
                            {
                                Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + tier);
                            }
                            if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy)
                            {
                               Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName +
                                               "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + tier);
                            }
                           

                            info.Name = hero.Name;
                            info.herohandle = hero;
                            info.Ranking = tier;
                            info.kdaratio = "Not Supported";
                            info.lpamount = lp;
                            info.winratio = winratioString;
                            info.winratiocolor = colorwinratio(wr);
                            Ranks.Add(info);
                            Random random = new Random();
                            int delay = random.Next(8000, 13000);
                          //  Game.PrintChat("Last time " + playerNameEnc + " Time: " + Game.Time);
                            System.Threading.Thread.Sleep(delay);
                
                        }

                        catch (Exception ex)
                        {
                            Console.Write("Error " + ex);
                        }
                                      
                        
         
                 
                }
               
            }
        }


        public static Color rankcolor(string rank)
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

        

        public static void OnDraw(EventArgs args)
        {
            if (!Config.Item("enabledrawings").GetValue<bool>())
            {
                return;
            }
         
            foreach (Info info in Ranks)
            {
              //  System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\" + info.Name + ".txt", info.Ranking);
                if ((!info.herohandle.IsDead) && (info.herohandle.IsVisible))
                {
                    // var wts = Drawing.WorldToScreen(info.herohandle.Position);
                    var indicator = new HpBarIndicator { Unit = info.herohandle };
                    int Xee = (int) indicator.Position.X + 90;
                    int Yee = (int) indicator.Position.Y + 5;
                    var font = new Font("Calibri", 13.5F);

                    if (Config.Item("enabledebug").GetValue<bool>())
                    {
                     
                          //  Console.WriteLine("Drawing: " + info.Name);
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking, font)/2), Yee - 60, Color.Yellow,
                                info.Ranking);
                        


                    }

                    
                    if (Config.Item("enablekdaratio").GetValue<bool>())
                    {
                        if (info.kdaratio.Contains("KDA")) //checking if its valid
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.kdaratio, font)/2), Yee - 50, info.kdaratiocolor,
                                info.kdaratio);
                            Yee = Yee - 20;
                        }
                     
                    }
                    
                
                    if (Config.Item("enablewinratio").GetValue<bool>())
                    {
                        if (info.winratio.Contains("Win Ratio")) //checking if its valid
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.winratio, font)/2), Yee - 50, info.winratiocolor,
                                info.winratio);
                            Yee = Yee - 20;
                        }
                    }

                    // Drawing.DrawText(wts.X, wts.Y, Color.Brown, "x");
                    // Unneccesary at the moment
                    if (Config.Item("enablerank").GetValue<bool>())
                    {
                        if (info.Ranking.ToLower().Contains("not found") && Config.Item("showunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking, font)/2), Yee - 50, Color.Yellow,
                                info.Ranking);
                        }
                        if (info.Ranking.ToLower().Contains("unknown") && Config.Item("showunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth("Unknown)", font)/2), Yee - 50, Color.Yellow, "Unknown");
                        }
                        if (info.Ranking.Contains("Unranked (L-30)") && Config.Item("showunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth("Unranked (L-30)", font)/2), Yee - 50, Color.Yellow,
                                "Unranked (L-30)");
                        }
                        if (info.Ranking.ToLower().Contains("error") && Config.Item("showunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking, font)/2), Yee - 50, Color.Red, info.Ranking);
                        }
                        if (info.Ranking.ToLower().Equals("unranked"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking, font)/2), Yee - 50, Color.White, "Unranked");
                        }
                        if (info.Ranking.ToLower().Contains("bronze") || info.Ranking.ToLower().Contains("silver") ||
                            info.Ranking.ToLower().Contains("gold") || info.Ranking.ToLower().Contains("platinum") ||
                            info.Ranking.ToLower().Contains("diamond") || info.Ranking.ToLower().Contains("master") ||
                            info.Ranking.ToLower().Contains("challenger"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font)/2),
                                Yee - 50, rankincolor(info.Ranking), info.Ranking + " (" + info.lpamount + ")");
                        }
                        //else { Game.PrintChat }
                    }
                }
            }
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

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Drawing.OnDraw += OnDraw;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Loaded EloSharp by Seph");
            Game.PrintChat("Your Region is: " + Game.Region +
                           " ; Please post this on the topic if it is not working properly for your region");

            //Menu
            Config = new Menu("EloSharp", "elosharp", true);
            Config.AddItem(new MenuItem("enabledrawings", "Enable Drawings").SetValue(true));
            Config.AddItem(new MenuItem("enablerank", "Draw Rank").SetValue(true));
            Config.AddItem(new MenuItem("enablewinratio", "Draw Win Ratio").SetValue(false));
            Config.AddItem(new MenuItem("enablekdaratio", "Draw KDA Ratio").SetValue(false));
            Config.AddItem(new MenuItem("showunknown", "Show Unknown").SetValue(true));
          
            Config.AddItem(new MenuItem("printranks", "Print at the beginning").SetValue(true));
            Config.AddItem(new MenuItem("enabledebug", "Enable Debug").SetValue(false));
            Config.AddToMainMenu();
           
            if (getregionurl() != "Not Supported" && !getregionurl().Contains(("Disabled")))
            {
                var thread = new Thread(() => { elosharp = new EloSharp(); });
                //  thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
           
        }


        public static string championid(Obj_AI_Hero hero)
        {
            if (hero.ChampionName.Contains("Aatrox"))
            {
                return "117";
            }
            if (hero.ChampionName.Contains("Ahri"))
            {
                return "86";
            }
            if (hero.ChampionName.Contains("Akali"))
            {
                return "74";
            }
            if (hero.ChampionName.Contains("Alistar"))
            {
                return "11";
            }
            if (hero.ChampionName.Contains("Amumu"))
            {
                return "31";
            }
            if (hero.ChampionName.Contains("Anivia"))
            {
                return "33";
            }
            if (hero.ChampionName.Contains("Annie"))
            {
                return "0";
            }
            if (hero.ChampionName.Contains("Ashe"))
            {
                return "21";
            }
            if (hero.ChampionName.Contains("Azir"))
            {
                return "119";
            }
            if (hero.ChampionName.Contains("Blitzcrank"))
            {
                return "48";
            }
            if (hero.ChampionName.Contains("Brand"))
            {
                return "58";
            }
            if (hero.ChampionName.Contains("Braum"))
            {
                return "112";
            }
            if (hero.ChampionName.Contains("Caitlyn"))
            {
                return "47";
            }
            if (hero.ChampionName.Contains("Cassiopeia"))
            {
                return "62";
            }
            if (hero.ChampionName.Contains("Cho'Gath"))
            {
                return "30";
            }
            if (hero.ChampionName.Contains("Corki"))
            {
                return "41";
            }
            if (hero.ChampionName.Contains("Darius"))
            {
                return "101";
            }
            if (hero.ChampionName.Contains("Diana"))
            {
                return "104";
            }
            if (hero.ChampionName.Contains("Dr. Mundo"))
            {
                return "35";
            }
            if (hero.ChampionName.Contains("Draven"))
            {
                return "98";
            }
            if (hero.ChampionName.Contains("Elise"))
            {
                return "55";
            }
            if (hero.ChampionName.Contains("Evelynn"))
            {
                return "27";
            }
            if (hero.ChampionName.Contains("Ezreal"))
            {
                return "71";
            }
            if (hero.ChampionName.Contains("Fiddlesticks"))
            {
                return "8";
            }
            if (hero.ChampionName.Contains("Fiora"))
            {
                return "95";
            }
            if (hero.ChampionName.Contains("Fizz"))
            {
                return "88";
            }
            if (hero.ChampionName.Contains("Galio"))
            {
                return "2";
            }
            if (hero.ChampionName.Contains("Gangplank"))
            {
                return "40";
            }
            if (hero.ChampionName.Contains("Garen"))
            {
                return "76";
            }
            if (hero.ChampionName.Contains("Gnar"))
            {
                return "108";
            }
            if (hero.ChampionName.Contains("Gragas"))
            {
                return "69";
            }
            if (hero.ChampionName.Contains("Graves"))
            {
                return "87";
            }
            if (hero.ChampionName.Contains("Hecarim"))
            {
                return "99";
            }
            if (hero.ChampionName.Contains("Heimerdinger"))
            {
                return "64";
            }
            if (hero.ChampionName.Contains("Irelia"))
            {
                return "38";
            }
            if (hero.ChampionName.Contains("Janna"))
            {
                return "39";
            }
            if (hero.ChampionName.Contains("Jarvan IV"))
            {
                return "54";
            }
            if (hero.ChampionName.Contains("Jax"))
            {
                return "23";
            }
            if (hero.ChampionName.Contains("Jayce"))
            {
                return "102";
            }
            if (hero.ChampionName.Contains("Jinx"))
            {
                return "113";
            }
            if (hero.ChampionName.Contains("Kalista"))
            {
                //return "121";
            }
            if (hero.ChampionName.Contains("Karma"))
            {
                return "42";
            }
            if (hero.ChampionName.Contains("Karthus"))
            {
                return "29";
            }
            if (hero.ChampionName.Contains("Kassadin"))
            {
                return "37";
            }
            if (hero.ChampionName.Contains("Katarina"))
            {
                return "50";
            }
            if (hero.ChampionName.Contains("Kayle"))
            {
                return "9";
            }
            if (hero.ChampionName.Contains("Kennen"))
            {
                return "75";
            }
            if (hero.ChampionName.Contains("Kha'Zix"))
            {
                return "100";
            }
            if (hero.ChampionName.Contains("Kog'Maw"))
            {
                return "81";
            }
            if (hero.ChampionName.Contains("LeBlanc"))
            {
                return "6";
            }
            if (hero.ChampionName.Contains("Lee Sin"))
            {
                return "59";
            }
            if (hero.ChampionName.Contains("Leona"))
            {
                return "77";
            }
            if (hero.ChampionName.Contains("Lissandra"))
            {
                return "103";
            }
            if (hero.ChampionName.Contains("Lucian"))
            {
                return "114";
            }
            if (hero.ChampionName.Contains("Lulu"))
            {
                return "97";
            }
            if (hero.ChampionName.Contains("Lux"))
            {
                return "83";
            }
            if (hero.ChampionName.Contains("Malphite"))
            {
                return "49";
            }
            if (hero.ChampionName.Contains("Malzahar"))
            {
                return "78";
            }
            if (hero.ChampionName.Contains("Maokai"))
            {
                return "52";
            }
            if (hero.ChampionName.Contains("Master Yi"))
            {
                return "10";
            }
            if (hero.ChampionName.Contains("Miss Fortune"))
            {
                return "20";
            }
            if (hero.ChampionName.Contains("Mordekaiser"))
            {
                return "72";
            }
            if (hero.ChampionName.Contains("Morgana"))
            {
                return "24";
            }
            if (hero.ChampionName.Contains("Nami"))
            {
                return "118";
            }
            if (hero.ChampionName.Contains("Nasus"))
            {
                return "65";
            }
            if (hero.ChampionName.Contains("Nautilus"))
            {
                return "92";
            }
            if (hero.ChampionName.Contains("Nidalee"))
            {
                return "66";
            }
            if (hero.ChampionName.Contains("Nocturne"))
            {
                return "51";
            }
            if (hero.ChampionName.Contains("Nunu"))
            {
                return "19";
            }
            if (hero.ChampionName.Contains("Olaf"))
            {
                return "1";
            }
            if (hero.ChampionName.Contains("Orianna"))
            {
                return "56";
            }
            if (hero.ChampionName.Contains("Pantheon"))
            {
                return "70";
            }
            if (hero.ChampionName.Contains("Poppy"))
            {
                return "68";
            }
            if (hero.ChampionName.Contains("Quinn"))
            {
                return "105";
            }
            if (hero.ChampionName.Contains("Rammus"))
            {
                return "32";
            }
            if (hero.ChampionName.Contains("Renekton"))
            {
                return "53";
            }
            if (hero.ChampionName.Contains("Rengar"))
            {
                return "90";
            }
            if (hero.ChampionName.Contains("Riven"))
            {
                return "80";
            }
            if (hero.ChampionName.Contains("Rumble"))
            {
                return "61";
            }
            if (hero.ChampionName.Contains("Ryze"))
            {
                return "12";
            }
            if (hero.ChampionName.Contains("Sejuani"))
            {
                return "94";
            }
            if (hero.ChampionName.Contains("Shaco"))
            {
                return "34";
            }
            if (hero.ChampionName.Contains("Shen"))
            {
                return "82";
            }
            if (hero.ChampionName.Contains("Shyvana"))
            {
                return "85";
            }
            if (hero.ChampionName.Contains("Singed"))
            {
                return "26";
            }
            if (hero.ChampionName.Contains("Sion"))
            {
                return "13";
            }
            if (hero.ChampionName.Contains("Sivir"))
            {
                return "14";
            }
            if (hero.ChampionName.Contains("Skarner"))
            {
                return "63";
            }
            if (hero.ChampionName.Contains("Sona"))
            {
                return "36";
            }
            if (hero.ChampionName.Contains("Soraka"))
            {
                return "15";
            }
            if (hero.ChampionName.Contains("Swain"))
            {
                return "46";
            }
            if (hero.ChampionName.Contains("Syndra"))
            {
                return "106";
            }
            if (hero.ChampionName.Contains("Talon"))
            {
                return "79";
            }
            if (hero.ChampionName.Contains("Taric"))
            {
                return "43";
            }
            if (hero.ChampionName.Contains("Teemo"))
            {
                return "16";
            }
            if (hero.ChampionName.Contains("Thresh"))
            {
                return "120";
            }
            if (hero.ChampionName.Contains("Tristana"))
            {
                return "17";
            }
            if (hero.ChampionName.Contains("Trundle"))
            {
                return "45";
            }
            if (hero.ChampionName.Contains("Tryndamere"))
            {
                return "22";
            }
            if (hero.ChampionName.Contains("Twisted Fate"))
            {
                return "3";
            }
            if (hero.ChampionName.Contains("Twitch"))
            {
                return "28";
            }
            if (hero.ChampionName.Contains("Udyr"))
            {
                return "67";
            }
            if (hero.ChampionName.Contains("Urgot"))
            {
                return "5";
            }
            if (hero.ChampionName.Contains("Varus"))
            {
                return "91";
            }
            if (hero.ChampionName.Contains("Vayne"))
            {
                return "60";
            }
            if (hero.ChampionName.Contains("Veigar"))
            {
                return "44";
            }
            if (hero.ChampionName.Contains("Vel'Koz"))
            {
                return "111";
            }
            if (hero.ChampionName.Contains("Vi"))
            {
                return "116";
            }
            if (hero.ChampionName.Contains("Viktor"))
            {
                return "93";
            }
            if (hero.ChampionName.Contains("Vladimir"))
            {
                return "7";
            }
            if (hero.ChampionName.Contains("Volibear"))
            {
                return "89";
            }
            if (hero.ChampionName.Contains("Warwick"))
            {
                return "18";
            }
            if (hero.ChampionName.Contains("Wukong"))
            {
                return "57";
            }
            if (hero.ChampionName.Contains("Xerath"))
            {
                return "84";
            }
            if (hero.ChampionName.Contains("Xin Zhao"))
            {
                return "4";
            }
            if (hero.ChampionName.Contains("Yasuo"))
            {
                return "110";
            }
            if (hero.ChampionName.Contains("Yorick"))
            {
                return "73";
            }
            if (hero.ChampionName.Contains("Zac"))
            {
                return "109";
            }
            if (hero.ChampionName.Contains("Zed"))
            {
                return "115";
            }
            if (hero.ChampionName.Contains("Ziggs"))
            {
                return "96";
            }
            if (hero.ChampionName.Contains("Zilean"))
            {
                return "25";
            }
            if (hero.ChampionName.Contains("Zyra"))
            {
                return "107";
            }
            return "";
        }

        public static string getregioncode()
        {
            //Only testing
            /*  
            if (Game.Region.Contains("NA"))
            {
                return "vn";
            }
             */
            if (Game.Region.Contains("SG"))
            {
                return "sg";
            }
            if (Game.Region.Contains("VN"))
            {
                return "vn";
            }
            if (Game.Region.Contains("PH"))
            {
                return "ph";
            }
            if (Game.Region.Contains("TW"))
            {
                return "tw";
            }
            if (Game.Region.Contains("TH"))
            {
                return "th";
            }
            if (Game.Region.Contains("ID"))
            {
                return "id";
            }
            return "";
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
            
            //Garena lookups
            /*  Testing purposes
            if (Game.Region.Contains("NA"))
            {
                return "http://quickfind.kassad.in/profile/vn/";
        
            }
             */
            if (Game.Region.Contains("SG"))
            {
                return "http://quickfind.kassad.in/profile/sg/";
 
            }
            if (Game.Region.Contains("VN"))
            {
                return "http://quickfind.kassad.in/profile/vn/";
     
                
            }
            
            if (Game.Region.Contains("PH"))
            {
               return "http://quickfind.kassad.in/profile/ph/";
            }
             
            if (Game.Region.Contains("TW"))
            {
               return "http://quickfind.kassad.in/profile/tw/";
            }
            if (Game.Region.Contains("TH"))
            {
                return "http://quickfind.kassad.in/profile/th/";
            }
            if (Game.Region.Contains("ID"))

            {
   
                return "http://quickfind.kassad.in/profile/id/";
            }

            return "Not Supported";

            // testing stuff
            //return "http://kassad.in";
            // return "http://na.op.gg/";
        }

        private string ExtractString(string s, string start, string end)
        {
            if (s.Contains(start) && s.Contains(end))
            {
                // You should check for errors in real-world code, omitted for brevity

                int startIndex = s.IndexOf(start) + start.Length;
                int endIndex = s.IndexOf(end, startIndex);

                return s.Substring(startIndex, endIndex - startIndex);
            }

            return "";
        }

        public static Color rankincolor(string rank)
        {
            if (rank.ToLower().Contains("error"))
            {
                return Color.Red;
            }
            if (rank.ToLower().Contains("unranked"))
            {
                return Color.SandyBrown;
            }
            if (rank.ToLower().Contains("bronze"))
            {
                return Color.Brown;
            }
            if (rank.ToLower().Contains("silver"))
            {
                return Color.Silver;
            }
            if (rank.ToLower().Contains("gold"))
            {
                return Color.Gold;
            }
            if (rank.ToLower().Contains("platinum"))
            {
                // other codes to try: #06828E, #06828E, #33D146, #33D146, #55AC82
                return Color.DeepSkyBlue;
            }
            if (rank.ToLower().Contains("diamond"))
            {
                //other codes: #38B0D5, #38B0D5, #2389B1, #3A7FBA
                return Color.Cyan;
            }
            if (rank.ToLower().Contains("master"))
            {
                // other codes: #B6F3EC, #B6F3EC, #A8E0D5, #73847E, #E5BF50
                return Color.LimeGreen;
            }
            if (rank.ToLower().Contains("challenger"))
            {
                //other codes: #DB910D, #DF9B42, #12607E
                return Color.Orange;
            }
            return Color.Red;
        }

        public static Color colorwinratio(double winratiocolor)
        {
            //  int winratiocolor = 0;

            if (winratiocolor >= 90 && winratiocolor <= 100)
            {
                return Color.Orange; //Challenger
            }
            if (winratiocolor >= 80 && winratiocolor < 90)
            {
                return Color.LimeGreen; //Master
            }
            if (winratiocolor >= 70 && winratiocolor < 80)
            {
                return Color.Cyan; //Diamond
            }
            if (winratiocolor >= 60 && winratiocolor < 70)
            {
                return Color.DeepSkyBlue; //Platinum
            }
            if (winratiocolor >= 50 && winratiocolor < 60)
            {
                return Color.Gold; //Gold
            }
            if (winratiocolor >= 40 && winratiocolor < 50)
            {
                return Color.Silver; //Silver
            }
            if (winratiocolor >= 30 && winratiocolor < 40)
            {
                return Color.SandyBrown; //Bronze
            }
            if (winratiocolor < 30)
            {
                return Color.Red; //Red
            }
            return Color.White;
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
        // End Tc Crew
      
    }
}