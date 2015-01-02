using System;
using System.Text;
using System.Web;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using System.Drawing;
using ReplaySharp.Properties;


namespace ReplaySharp
{
    internal class ReplaySharp
    {
        private static readonly Vector2 Scale = new Vector2(1.0f, 1.0f);
        private static Render.Sprite _recordicon;
        private static Vector2 _posrecord = new Vector2(Drawing.Width / 2f - 100, 500);
        private static string editedgamereg = "";
        private static bool recordingbool;
        private static bool _recordable;
        private static string Gameregion = "";
        private static string gameidstring = "";
        public static ReplaySharp replaysharp;
        public string htmlsource { get; set; }
        private static Menu Config;
        public static string recordstatus = "Initializing";
        public static string stage = "";
        private static string playerNameEnc;
        private static string _region;
        public static System.Timers.Timer Timer = new System.Timers.Timer();
        public static CookieContainer container = new CookieContainer();


        private static void Main(string[] args)
        {
            Drawing.OnDraw += OnDraw;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonrecord())
            {
                Thread thread = new System.Threading.Thread(() => recordthis(playerNameEnc.ToLower(), Gameregion));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            recordingbool = false;
            _recordicon = loadrecordicon();
            Gameregion = GetGameRegion();
            Game.PrintChat("OP.GG Replay Helper By Seph");
            Config = new LeagueSharp.Common.Menu("ReplaySharp", "replaysharp", true);
            Config.AddItem(new LeagueSharp.Common.MenuItem("checker", "Only Check Once").SetValue(false));
            Config.AddItem(new LeagueSharp.Common.MenuItem("disable", "Disable Record Button").SetValue(false));
            Config.AddItem(new LeagueSharp.Common.MenuItem("enabledrawings", "Show Drawings").SetValue(true));
            Config.AddToMainMenu();

            playerNameEnc = HttpUtility.UrlEncode(ObjectManager.Player.Name);
            ObjectManager.Player.ChampionName.ToLower();

            _region = GetGameRegion();
            Thread thread = new Thread(() =>
            {

                replaysharp = new ReplaySharp();

            });
         //   thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            Game.OnWndProc += Game_OnWndProc;




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

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("enabledrawings").GetValue<bool>())
            {
                Font font = new Font("Calibri", 12.5F);

                if (recordstatus == null)
                {
                    Drawing.DrawText(Drawing.Width / 2f - TextWidth("null", font) / 2f, 50, System.Drawing.Color.White,
                        "null");
                }

                Drawing.DrawText(Drawing.Width / 2f - TextWidth(recordstatus, font) / 2f - 485, 17, getcolor(), recordstatus);

            }
        }

        public static System.Drawing.Color getcolor()
        {
            //if (recordstatus.ToLower().Contains("recording") || recordstatus.ToLower().Contains("record"))
            if (recordingbool)
            {
                return System.Drawing.Color.Red;
            }
            else
            {
                return System.Drawing.Color.White;
            }

        }




        public static string GetGameRegion()
        {
            if (Game.Region.ToLower().Contains("na"))
            {
                return "na";
            }
            if (Game.Region.ToLower().Contains("euw"))
            {
                return "euw";
            }
            if (Game.Region.ToLower().Contains("eun"))
            {
                return "eun";
            }
            if (Game.Region.ToLower().Contains("la1"))
            {
                return "euw";
            }
            if (Game.Region.ToLower().Contains("la2"))
            {
                return "las";
            }
            if (Game.Region.ToLower().Contains("tr"))
            {
                return "tr";
            }
            if (Game.Region.ToLower().Contains("br"))
            {
                return "br";
            }
            if (Game.Region.ToLower().Contains("ru"))
            {
                return "ru";
            }
            if (Game.Region.ToLower().Contains("oc1"))
            {
                return "oce";
            }
            if (Game.Region.ToLower().Contains("kr"))
            {
                return "";
            }

            return "";

        }


        public ReplaySharp()
        {
            if (Config.Item("disable").GetValue<bool>())
            {
                return;
            }

            if (Gameregion == "")
            {
                Game.PrintChat("There was a problem with your region. Make sure you are supported on OP.GG");
                recordstatus = "Unsupported Region";

            }
            if (Gameregion != "")
            {
                isgamebeingrecorded(playerNameEnc.ToLower(), Gameregion);
                Timer.Elapsed += new ElapsedEventHandler(checkifrecording);
                Timer.Interval = 30000;
                Timer.Enabled = true;


                // recorder(ObjectManager.Player.Name, Gameregion);
                // recorder("JAYOB Eryon", "br");
                //recorder("crs piglet", "na");
                //  isgamebeingrecorded("krepo", "euw");

            }

        }




        public static void checkifrecording(object sender, EventArgs e)
        {
            if (Game.ClockTime > 600 || recordingbool) { Timer.Dispose(); Console.WriteLine("Timer disabled because the game is already recording or past 10 mins"); }
            //Game.PrintChat(Game.ClockTime.ToString());
            if (Game.ClockTime <= 600)
            {
                isgamebeingrecorded(playerNameEnc.ToLower(), GetGameRegion());
            }
        }



        public static void isgamebeingrecorded(string user, string region)
        {
            // Game.PrintChat("LEL " + Game.Id); // use Game.ID instead 

            if (region != "kr")
            {
                editedgamereg = region + ".";
            }
            if (region == "kr")
            {
                editedgamereg = "";
            }
          //  Game.PrintChat("Looking up: " + user + " " + region);
            //Send
            try
            {
                // CookieContainer container = new CookieContainer();
                string referer = "http://" + editedgamereg + "op.gg/summoner/userName=" + user;
                string useragent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
                byte[] infobytes = new UTF8Encoding().GetBytes("userName=" + user + "&force=true");
                Uri specuri =
                    new Uri("http://" + editedgamereg + "op.gg/summoner/ajax/spectator/userName=" + user + "&force=true");
                HttpWebRequest inforequest = (HttpWebRequest)WebRequest.Create(specuri);
                inforequest.UserAgent = useragent;
                inforequest.KeepAlive = true;
                inforequest.CookieContainer = container;
                inforequest.Method = "POST";
                inforequest.Referer = referer;
                inforequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                //receive
                HttpWebResponse opggresponse = (HttpWebResponse)inforequest.GetResponse();
                container.Add(opggresponse.Cookies);
                string response = new StreamReader(opggresponse.GetResponseStream()).ReadToEnd();
                //   System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\response1st.txt", response); 
                if (response.Contains("게임중이"))
                {
                    Console.WriteLine("Game not detected by OP.GG");
                    recordstatus = "OP.GG cant detect this game";
                    //  Game.PrintChat("OP.GG is having problems and unable to detect our game");
                }
                if (!response.Contains("<div class=\"Spectate\"") && !response.Contains("게임중이")) // 
                {
                    Console.WriteLine("Unexpected Error. OP.gg may be down. Error code #1");
                    recordstatus = "Not in a game/OP.GG error";
                    _recordable = false;
                    //    System.IO.File.WriteAllText(@"C:\Users\Laptop\Desktop\htmlnospec.txt", response);
                }
                if (response.Contains("NowRecording"))
                {
                   // Game.PrintChat("This game is already being recorded.");
                    recordstatus = "This game is being recorded already (someone else)";
                    recordingbool = true;
                    _recordable = false;
                }

                if (response.Contains("requestRecording.json"))
                {
                   // Game.PrintChat("This game is recordable");
                    recordstatus = "Game found, can try to record this game!";
                    Console.WriteLine("We can attempt to record this game!");
                    _recordable = true;
                    recordingbool = false;
                    Match regex = new Regex("/match/observer/id=(.*?)\"").Matches(response)[0];
                    Match gameid = new Regex(regex.Groups[1].ToString()).Matches(response)[0];
                    gameidstring = gameid.ToString();
                    recordstatus = "Finding Game ID: Success.";
                    Console.WriteLine("Game ID: " + gameidstring);

                }
            }
            catch (Exception e)
            {
                Console.Write("Exception: " + e);
                recordstatus = "Exception in checking recording status";
            }

        }

        public static void recordthis(string user, string region)
        {
            try
            {
                if (region != "kr")
                {
                    editedgamereg = region + ".";
                }
                if (region == "kr")
                {
                    editedgamereg = "";
                }

                isgamebeingrecorded(user, Gameregion);
                string useragent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
                // CookieContainer container = new CookieContainer();
                Uri recorduri =
                    new Uri("http://" + editedgamereg + "op.gg/summoner/ajax/requestRecording.json/gameId=" +
                            gameidstring);
                HttpWebRequest recordjson = (HttpWebRequest)WebRequest.Create(recorduri);
                recordjson.KeepAlive = true;
                recordjson.UserAgent = useragent;
                recordjson.CookieContainer = container;
                recordjson.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                recordjson.Referer = "http://" + editedgamereg + "op.gg/summoner/userName=" + user;

                WebHeaderCollection WebHeaders = recordjson.Headers;
                WebHeaders.Add("Accept-Language: en-US,en;q=0.8");
                WebHeaders.Add("Cache-Control: max-age=0");
                recordjson.ContentType = "application/json";
                HttpWebResponse recordresponse = (HttpWebResponse)recordjson.GetResponse();
                container.Add(recordresponse.Cookies);
                StreamReader newstreamreader = new StreamReader(recordjson.GetResponse().GetResponseStream());
                string response = newstreamreader.ReadToEnd();
                newstreamreader.Close();
                if (!response.Contains("error\":true") && response.Contains("success\":true"))
                {
                    recordstatus = "Started Recording the Game";
                    Game.PrintChat("Game Recording Started");
                    Console.WriteLine("Successfully started recording the Game");
                    recordingbool = true;

                }
                if (response.Contains("error\":true"))
                {
                    Console.WriteLine("Error, more details below possibly");
                    Console.Write(response);
                    if (response.Contains("possible"))
                    {
                        Console.Write("Error - Too late to record this game. Error code #2");
                        recordstatus = "Error - Too late to record the game";
                        Game.PrintChat("There was an error, most likely too late to record the game");
                    }
                    if (!response.Contains("possible"))
                    {
                        recordstatus = "OP.GG returned some type of unexpected error";
                    }

                }

            }
            catch (WebException ex)
            {
                Console.Write("WebException in trying to record: " + ex);
                recordstatus = "WebException trying to record";
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.GetBaseException().ToString());
            }
        }
        private static Vector2 GetPosition(int width)
        {
            return new Vector2(Drawing.Width / 2f - width / 2f, 1f);
        }

        private static Vector2 GetScaledVector(Vector2 vector)
        {
            return Vector2.Modulate(Scale, vector);
        }
        private static Render.Sprite loadrecordicon()
        {
            _posrecord = GetScaledVector(_posrecord);

            var loadrecord = new Render.Sprite(Resources.record, _posrecord)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadrecord.Position = GetPosition(loadrecord.Width - 826);
            loadrecord.Show();
            loadrecord.VisibleCondition += s => recordingbool;
            loadrecord.Add(0);


            _posrecord = GetScaledVector(_posrecord);

            var loadrecord2 = new Render.Sprite(Resources.disabled2, _posrecord)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadrecord2.Position = GetPosition(loadrecord2.Width - 826);
            loadrecord2.Show();
            loadrecord2.VisibleCondition += s => !recordingbool;
            loadrecord2.Add(0);


            return loadrecord;
        }


        private static bool mouseonrecord()
        {
            _posrecord = GetScaledVector(_posrecord);

            var loadrecord = new Render.Sprite(Resources.record, _posrecord)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            Vector2 pos = Utils.GetCursorPos();
            Vector2 recordbutton = GetPosition(loadrecord.Width - 830);

            return ((pos.X >= recordbutton.X) && pos.X <= (recordbutton.X + loadrecord.Width) && pos.Y >= recordbutton.Y &&
                    pos.Y <= (recordbutton.Y + loadrecord.Height));
        }
    }



}









