using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using LeagueSharp;
using LeagueSharp.Common;

namespace LolBuilder
{
    internal class EventProcessing
    {
        public static Menu Config;

        public static void GameLoad(EventArgs args)
        {
            Game.PrintChat("<font color=\"#43C6DB\"><b>LolBuilder Loaded - By Seph</font></b>");
            String championname = ObjectManager.Player.ChampionName.Replace(" ", "").Replace("'", "");
            var main = new System.Threading.Thread(() =>
            {
                ProBuilds(championname);
                CreateMenu(Config);
            });

            main.Start();
   
            if (AutoLevOn())
            {
                var sequence = BuildData.SkillSequence;
                new AutoLevel(sequence);
            }
            
        }

        public static void ProBuilds(string cname)
        {
            BuildData.BuildsList = new List<BuildData.BuildInfo>();
            WebClient pbClient = new WebClient();
            String Data = pbClient.DownloadString("http://lolbuilder.net/" + cname);

            String SkillSeq = ExtractString(Data, "window.skillOrder[0] = [", "];");
            string[] seqinstringarray = SkillSeq.Split(new string[] {","}, StringSplitOptions.None);
            int[] OrderedSequence = new int[seqinstringarray.Length];
                    for (int i = 0; i < seqinstringarray.Length; i++)
                    {
                        try
                        {
                            OrderedSequence[i] = int.Parse(seqinstringarray[i]);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e);
                        }

                        BuildData.SkillSequence = OrderedSequence;
                    }
            MatchCollection Builds = Regex.Matches(Data, "<div class=\"build-body\"[\\S\\s]*?id=\"build-content-");
            foreach (var b in Builds)
            {
                List<String> Starting = new List<string>();
                List<String> Buildorders = new List<string>();
                List<String> Final = new List<string>();
                List<String> BuildSummary = new List<string>();
                BuildData.BuildInfo BuildInfo = new BuildData.BuildInfo();

                //Specific Build info
                string buildinfo = b.ToString();

                //Extraction 
                String sitemsect = ExtractString(buildinfo, "<div class=\"shortcut-area starting-item-sets row",
                    "</section>");
                MatchCollection StartItems = Regex.Matches(sitemsect, "<small class=\"t-overflow[\\S\\s]*?</small>");
                foreach (var si in StartItems)
                {
                    String ItemNameFixed = HTMLStrip(si.ToString());
                    Starting.Add(ItemNameFixed);

                }
                String BOItemsect = ExtractString(buildinfo, "<h4 class=\"block-title\">Build Order", "</section>");
                MatchCollection BuildOrder = Regex.Matches(BOItemsect, "<small class=\"t-overflow\">[\\S\\s]*?</small>");
                foreach (var item in BuildOrder)
                {
                    String ItemNameFixed = HTMLStrip(item.ToString());
                    Buildorders.Add(ItemNameFixed);

                }
                String FinalItemsect = ExtractString(buildinfo, "<section class=\"final-items\">", "</section>");
                MatchCollection FinalBuild = Regex.Matches(FinalItemsect,
                    "<small class=\"t-overflow\">[\\S\\s]*?</small>");
                foreach (var item in FinalBuild)
                {
                    String ItemNameFixed = HTMLStrip(item.ToString());
                    Final.Add(ItemNameFixed);
                    //Console.WriteLine(ItemNameFixed);
                }

                String BuildSummarysect = ExtractString(buildinfo, "<div class=\"shortcut-area build-summary\">", "</section>");
                MatchCollection Summary = Regex.Matches(BuildSummarysect,
                    "<small class=\"t-overflow\">[\\S\\s]*?</small>");
                foreach (var item in Summary)
                {
                    String ItemNameFixed = HTMLStrip(item.ToString());
                    BuildSummary.Add(ItemNameFixed);
                }


                // Add to Lists
                BuildInfo.startingitems = Starting;
                BuildInfo.buildorder = Buildorders;
                BuildInfo.finalitems = Final;
                BuildInfo.buildsummary = BuildSummary;
                BuildData.BuildsList.Add(BuildInfo);
            }

        }

        private static bool NotifOn()
        {
            //return false;
            return Config.Item("notif").GetValue<bool>();
        }

        private static bool AutoLevOn()
        {
            //return false;
            return Config.Item("leveler").GetValue<bool>();
        }
        public static void CreateMenu(Menu Menu)
        {
            Config = new Menu("ProBuilds", "ProBuilds", true);
            var settings = new Menu("Misc", "Misc");
            MenuItem levelersetting = Config.AddItem(new MenuItem("leveler", "ProLeveler").SetValue(true));
            levelersetting.ValueChanged += delegate { AutoLevel.Enabled(Config.Item("leveler").GetValue<bool>()); };
            
            settings.AddItem(new MenuItem("notif", "Enable Notifications")).SetValue(true);
            Config.AddSubMenu(settings);
            foreach (var build in BuildData.BuildsList)
            {

                Random Random = new Random();
                var BuildName = "Build " + BuildData.BuildsList.IndexOf(build);
                var BuildMenu = new Menu(BuildName, BuildName);
                var starting = BuildMenu.AddSubMenu(new Menu("Starting", "Starting"));
                var Buildorder = BuildMenu.AddSubMenu(new Menu("Order", "Build Order"));
                var Final = BuildMenu.AddSubMenu(new Menu("Final Items", "Final"));
                var Summary = BuildMenu.AddSubMenu(new Menu("Build Summary", "Summary"));

                if (BuildData.BuildsList.IndexOf(build) == 0 && NotifOn())
                {
                    INotification start = new Notification("Starting Items", 60000);
                    Notifications.AddNotification(start);
                }
                foreach (var si in build.startingitems)
                {
                    starting.AddItem(new MenuItem(si + Random.Next(), si));
                    if (BuildData.BuildsList.IndexOf(build) == 0 && NotifOn())
                    {
                        INotification buildnotif = new Notification(si, 100000);
                        Notifications.AddNotification(buildnotif);
                    }
                }
                if (BuildData.BuildsList.IndexOf(build) == 0 && NotifOn())
                {
                    INotification start = new Notification("Build Order", 100000);
                    Notifications.AddNotification(start);
                }

                foreach (var bo in build.buildorder)
                {
                    Buildorder.AddItem(new MenuItem(bo + Random.Next(), bo));
                    if (BuildData.BuildsList.IndexOf(build) == 0 && NotifOn())
                    {
                        INotification buildnotif = new Notification(bo, 100000);
                        Notifications.AddNotification(buildnotif);
                    }

                }
                if (BuildData.BuildsList.IndexOf(build) == 0 && NotifOn())
                {
                    INotification start = new Notification("Final Items", 100000);
                    Notifications.AddNotification(start);
                }
                foreach (var finalitem in build.finalitems)
                {
                    Final.AddItem(new MenuItem(finalitem + Random.Next(), finalitem));
                    if (BuildData.BuildsList.IndexOf(build) == 0 && NotifOn())
                    {
                        INotification buildnotif = new Notification(finalitem, 100000);
                        Notifications.AddNotification(buildnotif);
                    }

                }

                foreach (var summitem in build.buildsummary)
                {
                    Summary.AddItem(new MenuItem(summitem + Random.Next(), summitem));
                }
    
                Config.AddSubMenu(BuildMenu);
            }
            Config.AddToMainMenu();
        }


        public static string HTMLStrip(string htmlString)
        {
            string pattern = @"<(.|\n)*?>";
            string removeus = htmlString.Replace("&", string.Empty).Replace(";", string.Empty);
            return Regex.Replace(removeus, pattern, string.Empty);
        }

        private static string ConstructPattern(string start, string end)
        {
            string rstart = Regex.Escape(start);
            string rend = Regex.Escape(end);
            string regexpattern = rstart + @"(.*?)" + rend;
            return regexpattern;
        }

        private static Match RegexExtract(string bundleoftext, string start, string end)
        {
            string rstart = Regex.Escape(start);
            string rend = Regex.Escape(end);
            string matchedstring = rstart + @"(.*?)" + rend;
            Match match = Regex.Match(bundleoftext, @matchedstring);
            return match;
        }

        private static string ExtractString(string s, string start, string end)
        {
            if (s.Contains(start) && s.Contains(end))
            {
                int startIndex = s.IndexOf(start) + start.Length;
                int endIndex = s.IndexOf(end, startIndex);

                return s.Substring(startIndex, endIndex - startIndex);
            }

            return "";
        }
       
    }


}
