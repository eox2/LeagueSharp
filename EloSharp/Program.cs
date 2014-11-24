using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using Color = System.Drawing.Color;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;


namespace EloSharp
{
    public class EloSharp
    {
        const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        const int SET_FEATURE_ON_THREAD = 0x00000001;
        const int SET_FEATURE_ON_PROCESS = 0x00000002;
        const int SET_FEATURE_IN_REGISTRY = 0x00000004;
        const int SET_FEATURE_ON_THREAD_LOCALMACHINE = 0x00000008;
        const int SET_FEATURE_ON_THREAD_INTRANET = 0x00000010;
        const int SET_FEATURE_ON_THREAD_TRUSTED = 0x00000020;
        const int SET_FEATURE_ON_THREAD_INTERNET = 0x00000040;
        const int SET_FEATURE_ON_THREAD_RESTRICTED = 0x00000080;
        public static string rank = "";
        public string GeneratedSource { get; set; }
        public string htmlforgarena { get; set; }
        //   public Uri Url2 { get; set; }
        private string URL { get; set; }
        private string URL2 { get; set; }
        private static LeagueSharp.Common.Menu Config;
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

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int FeatureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable);



        public static EloSharp elosharp;


        static void DisableClickSounds()
        {

            CoInternetSetFeatureEnabled(
                FEATURE_DISABLE_NAVIGATION_SOUNDS,
                SET_FEATURE_ON_PROCESS,
                true);
        }
        // TC-Crew Tracker code
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
        public static void OnDraw(EventArgs args)
        {

            if (!Config.Item("enabledrawings").GetValue<bool>()) { return; }

            foreach (Info info in Ranks)
            {


                if ((!info.herohandle.IsDead) && (info.herohandle.IsVisible))
                {
                    // var wts = Drawing.WorldToScreen(info.herohandle.Position);
                    var indicator = new HpBarIndicator { Unit = info.herohandle };
                    var Xee = (int)indicator.Position.X + 90;
                    var Yee = (int)indicator.Position.Y + 5;
                    Font font = new Font("Calibri", 13.5F);

                    if (Config.Item("enablekdaratio").GetValue<bool>()) 
                    {
                        if (info.kdaratio.Contains("KDA")) //checking if its valid
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.kdaratio, font) / 2), Yee - 50, info.kdaratiocolor, info.kdaratio);
                            Yee = Yee - 20;
                        }
                    }

                    if (Config.Item("enablewinratio").GetValue<bool>())
                    {
                        if (info.winratio.Contains("Win Ratio")) //checking if its valid
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.winratio, font) / 2), Yee - 50, info.winratiocolor, info.winratio);
                            Yee = Yee - 20;
                        }
                    }

                    // Drawing.DrawText(wts.X, wts.Y, Color.Brown, "x");
                    // Unneccesary at the moment
                    if (Config.Item("enablerank").GetValue<bool>())
                    {
                        if (info.Ranking.ToLower().Contains("not found") && Config.Item("disableunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking, font) / 2), Yee - 50, Color.Yellow, info.Ranking);
                        }
                        if (info.Ranking.ToLower().Contains("unknown") && Config.Item("disableunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth("Unknown)", font) / 2), Yee - 50, Color.Yellow, "Unknown");
                        }
                        if (info.Ranking.ToLower().Contains("Unranked (L-30)") && Config.Item("disableunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth("Unranked (L-30)", font) / 2), Yee - 50, Color.Yellow, "Unranked (L-30)");
                        }
                        if (info.Ranking.ToLower().Contains("error") && Config.Item("disableunknown").GetValue<bool>())
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking, font) / 2), Yee - 50, Color.Red, info.Ranking);
                        }
                        if (info.Ranking.ToLower().Equals("unranked"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking, font) / 2), Yee - 50, Color.White, "Unranked");
                        }
                        if (info.Ranking.Contains("Bronze"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.SandyBrown, info.Ranking + " (" + info.lpamount + ")");
                        }
                        if (info.Ranking.Contains("Silver"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Silver, info.Ranking + " (" + info.lpamount + ")");
                        }
                        if (info.Ranking.Contains("Gold"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Gold, info.Ranking + " (" + info.lpamount + ")");
                        }
                        if (info.Ranking.Contains("Platinum"))
                        {
                            //   Drawing.DrawText(wts.X - (TextWidth(info.Ranking, font) / 2), wts.Y - 160, Color.Cyan, info.Ranking);
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.DeepSkyBlue, info.Ranking + " (" + info.lpamount + ")");
                        }
                        if (info.Ranking.Contains("Diamond"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Cyan, info.Ranking + " (" + info.lpamount + ")");
                        }
                        if (info.Ranking.Contains("Master"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.LimeGreen, info.Ranking + " (" + info.lpamount + ")");
                        }
                        if (info.Ranking.Contains("Challenger"))
                        {
                            Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Orange, info.Ranking + " (" + info.lpamount + ")");
                        }
                    }
                }
            }
        }




        public static float TextWidth(string text, Font f)
        {
            float textWidth = 0;

            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                textWidth = g.MeasureString(text, f).Width;
            }

            return textWidth;
        }

        public static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;



        }

        public static void Game_OnGameLoad(EventArgs args)
        {

            Game.PrintChat("Loaded EloSharp by Seph");
            Game.PrintChat("Your Region is: " + Game.Region + " ; Please post this on the topic if it is not working properly for your region");

            //Menu
            Config = new LeagueSharp.Common.Menu("EloSharp", "elosharp", true);
            Config.AddItem(new LeagueSharp.Common.MenuItem("enabledrawings", "Enable Drawings").SetValue(true));
            Config.AddItem(new LeagueSharp.Common.MenuItem("enablerank", "Draw Rank").SetValue(true));
            Config.AddItem(new LeagueSharp.Common.MenuItem("enablewinratio", "Draw Win Ratio").SetValue(false));
            Config.AddItem(new LeagueSharp.Common.MenuItem("enablekdaratio", "Draw KDA Ratio").SetValue(false));
            Config.AddItem(new LeagueSharp.Common.MenuItem("disableunknown", "Show Unknown").SetValue(true));
            Config.AddItem(new LeagueSharp.Common.MenuItem("printranks", "Print at the beginning").SetValue(true));
            Config.AddToMainMenu();
            //
            DisableClickSounds();
            //Thread t = new Thread(new ThreadStart(WBThread));
            //   t.SetApartmentState(ApartmentState.STA);
            //  t.Start();
            //   t.Join();

            //new System.Threading.Thread(() =>
            //    {
            //    elosharp = new EloSharp();

            //  }).Start();

            if (Game.Region.Contains("SG") || Game.Region.Contains("VN") || Game.Region.Contains("PH") || Game.Region.Contains("TW") || Game.Region.Contains("TH") || Game.Region.Contains("ID"))
            {
                Game.PrintChat("Garena Detected");
                Thread thread = new System.Threading.Thread(() =>
                {
                    elosharp = new EloSharp();

                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
            {

                new System.Threading.Thread(() =>
                {
                    elosharp = new EloSharp();

                }).Start();
                //Game.PrintChat("other server detected");
            }



            Drawing.OnDraw += OnDraw;


        }


        public string GetGenHTML(string url)
        {

            URL = url;

            //Game.PrintChat("NOR");
            Thread t = new Thread(new ThreadStart(WBThread));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            return "";
            //    return GeneratedSource;
        }

        private void WBThread()
        {
            WebBrowser wb = new WebBrowser();

            DisableClickSounds();
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;
            wb.Navigate(URL);


            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wb_DocumentCompleted);

            while (wb.ReadyState != WebBrowserReadyState.Complete)


                Application.DoEvents();


            //Added this line, because the final HTML takes a while to show up
            // GeneratedSource = wb.Document.Body.InnerHtml;
            htmlforgarena = wb.Document.Body.InnerHtml;
            //System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\innit.txt", GeneratedSource);
            wb.Stop();



            //  wb.Dispose(true);
            Application.ExitThread();
            Application.Exit();
            wb.Dispose();
            Console.WriteLine("Disposed");
        }

        private void wb_DocumentCompleted(object sender,
            WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser wb = (WebBrowser)sender;
            URL2 = wb.Url.ToString();
            // Game.PrintChat(URL);
            //   GeneratedSource = wb.Document.Body.InnerHtml;
            htmlforgarena = wb.Document.Body.InnerHtml;
            //  Console.Write(GeneratedSource);
            //   System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\swag2.txt", GeneratedSource);
        }

        public String championID;

        public EloSharp()
        {
            Ranks = new List<Info>();
            // foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                
                if (hero.ChampionName.Contains("Aatrox"))
                {
                    championID = "117";
                }
                else if (hero.ChampionName.Contains("Ahri"))
                {
                    championID = "86";
                }
                else if (hero.ChampionName.Contains("Akali"))
                {
                    championID = "74";
                }
                else if (hero.ChampionName.Contains("Alistar"))
                {
                    championID = "11";
                }
                else if (hero.ChampionName.Contains("Amumu"))
                {
                    championID = "31";
                }
                else if (hero.ChampionName.Contains("Anivia"))
                {
                    championID = "33";
                }
                else if (hero.ChampionName.Contains("Annie"))
                {
                    championID = "0";
                }
                else if (hero.ChampionName.Contains("Ashe"))
                {
                    championID = "21";
                }
                else if (hero.ChampionName.Contains("Azir"))
                {
                    championID = "119";
                }
                else if (hero.ChampionName.Contains("Blitzcrank"))
                {
                    championID = "48";
                }
                else if (hero.ChampionName.Contains("Brand"))
                {
                    championID = "58";
                }
                else if (hero.ChampionName.Contains("Braum"))
                {
                    championID = "112";
                }
                else if (hero.ChampionName.Contains("Caitlyn"))
                {
                    championID = "47";
                }
                else if (hero.ChampionName.Contains("Cassiopeia"))
                {
                    championID = "62";
                }
                else if (hero.ChampionName.Contains("Cho'Gath"))
                {
                    championID = "30";
                }
                else if (hero.ChampionName.Contains("Corki"))
                {
                    championID = "41";
                }
                else if (hero.ChampionName.Contains("Darius"))
                {
                    championID = "101";
                }
                else if (hero.ChampionName.Contains("Diana"))
                {
                    championID = "104";
                }
                else if (hero.ChampionName.Contains("Dr. Mundo"))
                {
                    championID = "35";
                }
                else if (hero.ChampionName.Contains("Draven"))
                {
                    championID = "98";
                }
                else if (hero.ChampionName.Contains("Elise"))
                {
                    championID = "55";
                }
                else if (hero.ChampionName.Contains("Evelynn"))
                {
                    championID = "27";
                }
                else if (hero.ChampionName.Contains("Ezreal"))
                {
                    championID = "71";
                }
                else if (hero.ChampionName.Contains("Fiddlesticks"))
                {
                    championID = "8";
                }
                else if (hero.ChampionName.Contains("Fiora"))
                {
                    championID = "95";
                }
                else if (hero.ChampionName.Contains("Fizz"))
                {
                    championID = "88";
                }
                else if (hero.ChampionName.Contains("Galio"))
                {
                    championID = "2";
                }
                else if (hero.ChampionName.Contains("Gangplank"))
                {
                    championID = "40";
                }
                else if (hero.ChampionName.Contains("Garen"))
                {
                    championID = "76";
                }
                else if (hero.ChampionName.Contains("Gnar"))
                {
                    championID = "108";
                }
                else if (hero.ChampionName.Contains("Gragas"))
                {
                    championID = "69";
                }
                else if (hero.ChampionName.Contains("Graves"))
                {
                    championID = "87";
                }
                else if (hero.ChampionName.Contains("Hecarim"))
                {
                    championID = "99";
                }
                else if (hero.ChampionName.Contains("Heimerdinger"))
                {
                    championID = "64";
                }
                else if (hero.ChampionName.Contains("Irelia"))
                {
                    championID = "38";
                }
                else if (hero.ChampionName.Contains("Janna"))
                {
                    championID = "39";
                }
                else if (hero.ChampionName.Contains("Jarvan IV"))
                {
                    championID = "54";
                }
                else if (hero.ChampionName.Contains("Jax"))
                {
                    championID = "23";
                }
                else if (hero.ChampionName.Contains("Jayce"))
                {
                    championID = "102";
                }
                else if (hero.ChampionName.Contains("Jinx"))
                {
                    championID = "113";
                }
                else if (hero.ChampionName.Contains("Kalista"))
                {
                    //championID = "121";
                }
                else if (hero.ChampionName.Contains("Karma"))
                {
                    championID = "42";
                }
                else if (hero.ChampionName.Contains("Karthus"))
                {
                    championID = "29";
                }
                else if (hero.ChampionName.Contains("Kassadin"))
                {
                    championID = "37";
                }
                else if (hero.ChampionName.Contains("Katarina"))
                {
                    championID = "50";
                }
                else if (hero.ChampionName.Contains("Kayle"))
                {
                    championID = "9";
                }
                else if (hero.ChampionName.Contains("Kennen"))
                {
                    championID = "75";
                }
                else if (hero.ChampionName.Contains("Kha'Zix"))
                {
                    championID = "100";
                }
                else if (hero.ChampionName.Contains("Kog'Maw"))
                {
                    championID = "81";
                }
                else if (hero.ChampionName.Contains("LeBlanc"))
                {
                    championID = "6";
                }
                else if (hero.ChampionName.Contains("Lee Sin"))
                {
                    championID = "59";
                }
                else if (hero.ChampionName.Contains("Leona"))
                {
                    championID = "77";
                }
                else if (hero.ChampionName.Contains("Lissandra"))
                {
                    championID = "103";
                }
                else if (hero.ChampionName.Contains("Lucian"))
                {
                    championID = "114";
                }
                else if (hero.ChampionName.Contains("Lulu"))
                {
                    championID = "97";
                }
                else if (hero.ChampionName.Contains("Lux"))
                {
                    championID = "83";
                }
                else if (hero.ChampionName.Contains("Malphite"))
                {
                    championID = "49";
                }
                else if (hero.ChampionName.Contains("Malzahar"))
                {
                    championID = "78";
                }
                else if (hero.ChampionName.Contains("Maokai"))
                {
                    championID = "52";
                }
                else if (hero.ChampionName.Contains("Master Yi"))
                {
                    championID = "10";
                }
                else if (hero.ChampionName.Contains("Miss Fortune"))
                {
                    championID = "20";
                }
                else if (hero.ChampionName.Contains("Mordekaiser"))
                {
                    championID = "72";
                }
                else if (hero.ChampionName.Contains("Morgana"))
                {
                    championID = "24";
                }
                else if (hero.ChampionName.Contains("Nami"))
                {
                    championID = "118";
                }
                else if (hero.ChampionName.Contains("Nasus"))
                {
                    championID = "65";
                }
                else if (hero.ChampionName.Contains("Nautilus"))
                {
                    championID = "92";
                }
                else if (hero.ChampionName.Contains("Nidalee"))
                {
                    championID = "66";
                }
                else if (hero.ChampionName.Contains("Nocturne"))
                {
                    championID = "51";
                }
                else if (hero.ChampionName.Contains("Nunu"))
                {
                    championID = "19";
                }
                else if (hero.ChampionName.Contains("Olaf"))
                {
                    championID = "1";
                }
                else if (hero.ChampionName.Contains("Orianna"))
                {
                    championID = "56";
                }
                else if (hero.ChampionName.Contains("Pantheon"))
                {
                    championID = "70";
                }
                else if (hero.ChampionName.Contains("Poppy"))
                {
                    championID = "68";
                }
                else if (hero.ChampionName.Contains("Quinn"))
                {
                    championID = "105";
                }
                else if (hero.ChampionName.Contains("Rammus"))
                {
                    championID = "32";
                }
                else if (hero.ChampionName.Contains("Renekton"))
                {
                    championID = "53";
                }
                else if (hero.ChampionName.Contains("Rengar"))
                {
                    championID = "90";
                }
                else if (hero.ChampionName.Contains("Riven"))
                {
                    championID = "80";
                }
                else if (hero.ChampionName.Contains("Rumble"))
                {
                    championID = "61";
                }
                else if (hero.ChampionName.Contains("Ryze"))
                {
                    championID = "12";
                }
                else if (hero.ChampionName.Contains("Sejuani"))
                {
                    championID = "94";
                }
                else if (hero.ChampionName.Contains("Shaco"))
                {
                    championID = "34";
                }
                else if (hero.ChampionName.Contains("Shen"))
                {
                    championID = "82";
                }
                else if (hero.ChampionName.Contains("Shyvana"))
                {
                    championID = "85";
                }
                else if (hero.ChampionName.Contains("Singed"))
                {
                    championID = "26";
                }
                else if (hero.ChampionName.Contains("Sion"))
                {
                    championID = "13";
                }
                else if (hero.ChampionName.Contains("Sivir"))
                {
                    championID = "14";
                }
                else if (hero.ChampionName.Contains("Skarner"))
                {
                    championID = "63";
                }
                else if (hero.ChampionName.Contains("Sona"))
                {
                    championID = "36";
                }
                else if (hero.ChampionName.Contains("Soraka"))
                {
                    championID = "15";
                }
                else if (hero.ChampionName.Contains("Swain"))
                {
                    championID = "46";
                }
                else if (hero.ChampionName.Contains("Syndra"))
                {
                    championID = "106";
                }
                else if (hero.ChampionName.Contains("Talon"))
                {
                    championID = "79";
                }
                else if (hero.ChampionName.Contains("Taric"))
                {
                    championID = "43";
                }
                else if (hero.ChampionName.Contains("Teemo"))
                {
                    championID = "16";
                }
                else if (hero.ChampionName.Contains("Thresh"))
                {
                    championID = "120";
                }
                else if (hero.ChampionName.Contains("Tristana"))
                {
                    championID = "17";
                }
                else if (hero.ChampionName.Contains("Trundle"))
                {
                    championID = "45";
                }
                else if (hero.ChampionName.Contains("Tryndamere"))
                {
                    championID = "22";
                }
                else if (hero.ChampionName.Contains("Twisted Fate"))
                {
                    championID = "3";
                }
                else if (hero.ChampionName.Contains("Twitch"))
                {
                    championID = "28";
                }
                else if (hero.ChampionName.Contains("Udyr"))
                {
                    championID = "67";
                }
                else if (hero.ChampionName.Contains("Urgot"))
                {
                    championID = "5";
                }
                else if (hero.ChampionName.Contains("Varus"))
                {
                    championID = "91";
                }
                else if (hero.ChampionName.Contains("Vayne"))
                {
                    championID = "60";
                }
                else if (hero.ChampionName.Contains("Veigar"))
                {
                    championID = "44";
                }
                else if (hero.ChampionName.Contains("Vel'Koz"))
                {
                    championID = "111";
                }
                else if (hero.ChampionName.Contains("Vi"))
                {
                    championID = "116";
                }
                else if (hero.ChampionName.Contains("Viktor"))
                {
                    championID = "93";
                }
                else if (hero.ChampionName.Contains("Vladimir"))
                {
                    championID = "7";
                }
                else if (hero.ChampionName.Contains("Volibear"))
                {
                    championID = "89";
                }
                else if (hero.ChampionName.Contains("Warwick"))
                {
                    championID = "18";
                }
                else if (hero.ChampionName.Contains("Wukong"))
                {
                    championID = "57";
                }
                else if (hero.ChampionName.Contains("Xerath"))
                {
                    championID = "84";
                }
                else if (hero.ChampionName.Contains("Xin Zhao"))
                {
                    championID = "4";
                }
                else if (hero.ChampionName.Contains("Yasuo"))
                {
                    championID = "110";
                }
                else if (hero.ChampionName.Contains("Yorick"))
                {
                    championID = "73";
                }
                else if (hero.ChampionName.Contains("Zac"))
                {
                    championID = "109";
                }
                else if (hero.ChampionName.Contains("Zed"))
                {
                    championID = "115";
                }
                else if (hero.ChampionName.Contains("Ziggs"))
                {
                    championID = "96";
                }
                else if (hero.ChampionName.Contains("Zilean"))
                {
                    championID = "25";
                }
                else if (hero.ChampionName.Contains("Zyra"))
                {
                    championID = "107";
                }

                // Game.PrintChat(hero.Name);
                //Game.PrintChat(LeagueSharp.Game.Region);

                Info info = new Info();
                //
                String regionUrl = "";

                if (Game.Region.ToLower().Contains("na"))
                {
                    regionUrl = "http://na.op.gg/";
                }

                if (Game.Region.ToLower().Contains("euw"))
                {
                    regionUrl = "http://euw.op.gg/";
                }

                if (Game.Region.ToLower().Contains("eun"))
                {
                    regionUrl = "http://eune.op.gg/";
                }

                if (Game.Region.ToLower().Contains("la1"))
                {
                    regionUrl = "http://lan.op.gg/";
                }

                if (Game.Region.ToLower().Contains("tr"))
                {
                    regionUrl = "http://tr.op.gg/";
                }

                if (Game.Region.ToLower().Contains("oc1"))
                {
                    regionUrl = "http://oce.op.gg/";
                }

                if (Game.Region.ToLower().Contains("br"))
                {
                    regionUrl = "http://br.op.gg/";
                }

                if (Game.Region.ToLower().Contains("ru"))
                {
                    regionUrl = "http://ru.op.gg/";
                }

                if (Game.Region.ToLower().Contains("la2"))
                {
                    regionUrl = "http://las.op.gg/";
                }

                if (Game.Region.ToLower().Contains("kr"))
                {
                    regionUrl = "http://kr.op.gg/";
                }

                if (regionUrl.Contains("http"))
                {

                    string htmlcode = "";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(regionUrl + "summoner/userName=" + hero.Name);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

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
                    }


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {

                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];


                        rank = playerrank.ToString();
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("tierRank") && !htmlcode.ToString().Contains("leaguePoints"))
                    {

                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //   Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        //   Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];


                        rank = "Unranked (L-30)";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if ((htmlcode.ToString().Contains("ChampionBox Unranked") && !htmlcode.ToString().Contains("leaguePoints") && (!htmlcode.ToString().Contains("tierRank"))))
                    {

                        // Game.PrintChat("unranked found");
                        // Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        //  Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = "Unranked";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        //rank = playerrank.ToString();

                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "";
                        Ranks.Add(info);
                    }

                    // if (!htmlcode.ToString().Contains("tierRank") && !htmlcode.ToString().Contains("ChampionBox Unranked") && (htmlcode.ToString().Contains("spelling")))
                    if (!htmlcode.ToString().Contains("ChampionBox Unranked") && (!htmlcode.ToString().Contains("tierRank")))
                    {

                        //Game.PrintChat("not found");
                        //Game.PrintChat("didnt find ranks");
                        //Game.PrintChat("This hero is not registered");
                        rank = "Error";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }

                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);

                    }

                    if ((Config.Item("enablekdaratio").GetValue<bool>()) || (Config.Item("enablewinratio").GetValue<bool>()))
                    {
                        //GetGenHTML("http://br.op.gg/summoner/champions/userName=" + hero.Name);

                        //Console.WriteLine("Starting Debug");
                        string data = "";
                        request = (HttpWebRequest)WebRequest.Create(regionUrl + "summoner/champions/userName=" + hero.Name);
                        response = (HttpWebResponse)request.GetResponse();

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


                        if (data.Contains("__spc24 __spc24-" + championID + "\""))
                        {
                            //Console.WriteLine(hero.ChampionName);
                            int index = data.IndexOf("__spc24 __spc24-" + championID + "\"");
                            String htmlstats = data.Remove(0, index);
                            index = htmlstats.IndexOf("gold");
                            htmlstats = htmlstats.Substring(0, index);

                            Match htmlmatchwinRatio = new Regex(@"\<span class=\""winRatio\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchkill = new Regex(@"\<span class=\""kill\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchdeath = new Regex(@"\<span class=\""death\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchassist = new Regex(@"\<span class=\""assist\"">(.*?)</span>").Matches(htmlstats)[0];
                            Match htmlmatchkdaratio = new Regex(@"\<span class=\""kdaratio\"">(.*?)</span>").Matches(htmlstats)[0];

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
                            int Lenght = wins.Length;
                            wins = wins.Substring(0, Lenght - 1);

                            index = htmlstats.IndexOf("losses");
                            htmlstats = htmlstats.Remove(0, (index + 8));
                            index = htmlstats.IndexOf("</span>");
                            String losses = htmlstats.Substring(0, index);
                            losses = losses.Trim();
                            Lenght = losses.Length;
                            losses = losses.Substring(0, Lenght - 1);

                            String winratioString = ("Win Ratio = " + winRatio + " (" + wins + "/" + losses + ")");
                            String kdaString = ("KDA = " + kdaratio + " (" + kill + "K + " + assist + "A / " + death + "D)");

                            //Console.WriteLine(winratioString);
                            //Console.WriteLine(kdaString);

                            index = winRatio.ToString().IndexOf("%");
                            String winratio2 = winRatio.ToString().Substring(0, index);
                            int winratiocolor = 0;
                            if (Int32.TryParse(winratio2, out winratiocolor))
                            {
                                if (winratiocolor >= 90 && winratiocolor <= 100)
                                {
                                    info.winratiocolor = Color.Orange; //Challenger
                                }
                                if (winratiocolor >= 80 && winratiocolor < 90)
                                {
                                    info.winratiocolor = Color.LimeGreen; //Master
                                }
                                if (winratiocolor >= 70 && winratiocolor < 80)
                                {
                                    info.winratiocolor = Color.Cyan; //Diamond
                                }
                                if (winratiocolor >= 60 && winratiocolor < 70)
                                {
                                    info.winratiocolor = Color.DeepSkyBlue; //Platinum
                                }
                                if (winratiocolor >= 50 && winratiocolor < 60)
                                {
                                    info.winratiocolor = Color.Gold; //Gold
                                }
                                if (winratiocolor >= 40 && winratiocolor < 50)
                                {
                                    info.winratiocolor = Color.Silver; //Silver
                                }
                                if (winratiocolor >= 30 && winratiocolor < 40)
                                {
                                    info.winratiocolor = Color.SandyBrown; //Bronze
                                }
                                if (winratiocolor < 30)
                                {
                                    info.winratiocolor = Color.Red; //Red
                                }
                            }
                            //Console.WriteLine(kdaratio.ToString());
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


                            info.winratio = winratioString.ToString();
                            info.kdaratio = kdaString.ToString();
                            Ranks.Add(info);
                        }
                        else
                        {
                            info.winratio = "";
                            info.kdaratio = "";
                            Ranks.Add(info);
                        }
                    }
                }






                if (Game.Region.ToLower().Contains("sg"))
                {
                    GetGenHTML("http://www.legendsasia.com/summoner/sg/" + hero.Name + "/");
                    if (htmlforgarena.Contains("<!-- TierRank"))
                    {
                        Match htmlmatchrank = new Regex(@"\<!-- TierRank   --><h4><b>(.*?)</b></h4>").Matches(htmlforgarena)[0];
                        Match htmlmatchlp = new Regex(@"\<!-- LP         --><h5><b>(.*?)</b> <br>").Matches(htmlforgarena)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        rank = playerrank.ToString();
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }

                    if (!htmlforgarena.Contains("<!-- TierRank") && (URL2.Contains("?nf")))
                    {
                        rank = "Not found";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }

                }
                if (Game.Region.ToLower().Contains("vn"))
                {
                    GetGenHTML("http://www.legendsasia.com/summoner/vn/" + hero.Name + "/");
                    if (htmlforgarena.Contains("<!-- TierRank"))
                    {
                        Match htmlmatchrank = new Regex(@"\<!-- TierRank   --><h4><b>(.*?)</b></h4>").Matches(htmlforgarena)[0];
                        Match htmlmatchlp = new Regex(@"\<!-- LP         --><h5><b>(.*?)</b> <br>").Matches(htmlforgarena)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        rank = playerrank.ToString();
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }

                    if (!htmlforgarena.Contains("<!-- TierRank") || (URL2.Contains("?nf")))
                    {
                        rank = "Not found";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }

                }

                if (Game.Region.ToLower().Contains("ph"))
                {
                    GetGenHTML("http://www.legendsasia.com/summoner/ph/" + hero.Name + "/");
                    if (htmlforgarena.Contains("<!-- TierRank"))
                    {
                        Match htmlmatchrank = new Regex(@"\<!-- TierRank   --><h4><b>(.*?)</b></h4>").Matches(htmlforgarena)[0];
                        Match htmlmatchlp = new Regex(@"\<!-- LP         --><h5><b>(.*?)</b> <br>").Matches(htmlforgarena)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        rank = playerrank.ToString();
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }

                    if (!htmlforgarena.Contains("<!-- TierRank") || (URL2.Contains("?nf")))
                    {
                        rank = "Not found";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }

                }

                if (Game.Region.ToLower().Contains("th"))
                {
                    GetGenHTML("http://www.legendsasia.com/summoner/th/" + hero.Name + "/");

                    if (htmlforgarena.Contains("<!-- TierRank"))
                    {
                        Match htmlmatchrank = new Regex(@"\<!-- TierRank   --><h4><b>(.*?)</b></h4>").Matches(htmlforgarena)[0];
                        Match htmlmatchlp = new Regex(@"\<!-- LP         --><h5><b>(.*?)</b> <br>").Matches(htmlforgarena)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        rank = playerrank.ToString();
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }

                    if (!htmlforgarena.Contains("<!-- TierRank") || (URL2.Contains("?nf")))
                    {
                        rank = "Not found";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }

                }
                if (Game.Region.ToLower().Contains("tw"))
                {
                    GetGenHTML("http://www.legendsasia.com/summoner/tw/" + hero.Name + "/");

                    if (htmlforgarena.Contains("<!-- TierRank"))
                    {
                        Match htmlmatchrank = new Regex(@"\<!-- TierRank   --><h4><b>(.*?)</b></h4>").Matches(htmlforgarena)[0];
                        Match htmlmatchlp = new Regex(@"\<!-- LP         --><h5><b>(.*?)</b> <br>").Matches(htmlforgarena)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        rank = playerrank.ToString();
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }

                    if (!htmlforgarena.Contains("<!-- TierRank") || (URL2.Contains("?nf")))
                    {
                        rank = "Not found";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }

                }
                if (Game.Region.ToLower().Contains("id"))
                {
                    GetGenHTML("http://www.legendsasia.com/summoner/id/" + hero.Name + "/");
                    if (htmlforgarena.Contains("<!-- TierRank"))
                    {
                        Match htmlmatchrank = new Regex(@"\<!-- TierRank   --><h4><b>(.*?)</b></h4>").Matches(htmlforgarena)[0];
                        Match htmlmatchlp = new Regex(@"\<!-- LP         --><h5><b>(.*?)</b> <br>").Matches(htmlforgarena)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlforgarena)[0];
                        rank = playerrank.ToString();
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }

                    if (!htmlforgarena.Contains("<!-- TierRank") || (URL2.Contains("?nf")))
                    {
                        rank = "Not found";
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsAlly) { Game.PrintChat("<font color=\"#FF000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        if (Config.Item("printranks").GetValue<bool>() && hero.IsEnemy) { Game.PrintChat("<font color=\"#FF0000\"><b>" + hero.ChampionName + "</font> <font color=\"#FFFFFF\">(" + hero.Name + ")" + " : " + rank); }
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }
            }
        }
    }
}
