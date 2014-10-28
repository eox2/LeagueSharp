using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Reflection;
using System.IO;



namespace SpotSharp
{

    internal class SpotifySharp
    {



        private static Menu Config;
        //  private static int currvol = 100;


        //   private static Obj_AI_Hero Player;




        // Credit goes to http://code.google.com/p/spotifycontrol/source/browse/trunk/SpotifyControl/Controllers/ControllerSpotify.vb 
        // for some of the key message commands, etc

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
           int dwExtraInfo);

        const int KEY_MESSAGE = 0x319;
        const int CONTROL_KEY = 0x11;

        const long PLAYPAUSE_KEY = 0xE0000L;
        const long NEXTTRACK_KEY = 0xB0000L;
        const long PREVIOUS_KEY = 0xC0000L;











        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {


            //Menu
            Config = new Menu("Spotify Controller", "Spotify", true);


            Config.AddSubMenu(new Menu("Spotify Settings", "settings"));
            Config.SubMenu("settings").AddItem(new MenuItem("showtrack", "Show Track Name").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("showcontrls", "Show Controls").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));

            //   Config.SubMenu("settings").AddItem(new MenuItem("volume", "Volume %").SetValue(new Slider(40)));

            // var volpct = Config.Item("volume").GetValue<Slider>().Value / 100f;
            // var volpct = Config.AddItem(new MenuItem("vol%", "Volume %").SetValue(new Slider(40)));




            Config.AddToMainMenu();



            Game.PrintChat("Loaded Spotify Controller by Seph");
            if (!isSpotifyOpen()) { Game.PrintChat("Spotify isn't running"); return; }
            if (isSpotifyOpen()) { Game.PrintChat("::: Spotify has been detected :::"); }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;

        }
        private static void OnGameUpdate(EventArgs args)
        {

            if (isSpotifyOpen())
            {
                var ChangeVolumeUp = Config.AddItem(new MenuItem("Volume", "Vol+").SetValue(new KeyBind(106, KeyBindType.Press)));
                var ChangeVolumeDown = Config.AddItem(new MenuItem("Volume2", "Vol-").SetValue(new KeyBind(108, KeyBindType.Press)));
                var ChangeSkipTrack = Config.AddItem(new MenuItem("skip", "Next").SetValue(new KeyBind(102, KeyBindType.Press)));
                var ChangePrev = Config.AddItem(new MenuItem("prev", "Prev").SetValue(new KeyBind(104, KeyBindType.Press)));


                ChangeVolumeUp.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    //currvol++;
                    volumeUp();
                };

                ChangeVolumeDown.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    // currvol--;
                    volumeDown();
                };

                ChangeSkipTrack.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    //currvol++;
                    nextTrack();
                };

                ChangePrev.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    // currvol--;
                    previousTrack();
                };


            }
        }





        private static String getTitle()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            if (spotifyWindow == new IntPtr(0))
            {
                return "";
            }
            // Allocate correct string length first
            int length = GetWindowTextLength(spotifyWindow);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(spotifyWindow, sb, sb.Capacity);
            return sb.ToString();
        }

        static public bool isSpotifyOpen()
        {
            if (getTitle() == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        static public String getSongName()
        {
            // Use character '–' to split song name/artist name
            String[] title = getTitle().Split('–');
            if (title.Count() > 1)
            {
                return title[1].Trim();
            }
            else
            {
                return "";
            }
        }



        static public String getArtistName()
        {
            // Use character '–' to split song name/artist name
            String[] title = getTitle().Split('–');
            if (title.Count() > 1)
            {
                return title[0].Split('-')[1].Trim();
            }
            else
            {
                return "";
            }
        }

        static public void pausePlay()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KEY_MESSAGE, IntPtr.Zero, new IntPtr(PLAYPAUSE_KEY));
        }
        static public void nextTrack()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KEY_MESSAGE, IntPtr.Zero, new IntPtr(NEXTTRACK_KEY));
        }
        static public void previousTrack()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KEY_MESSAGE, IntPtr.Zero, new IntPtr(PREVIOUS_KEY));
        }
        static public void volumeUp()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(CONTROL_KEY, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x26), IntPtr.Zero);
            System.Threading.Thread.Sleep(100);
            keybd_event(CONTROL_KEY, 0x1D, 0x2, 0);
        }
        static public void volumeDown()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(CONTROL_KEY, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x28), IntPtr.Zero);
            System.Threading.Thread.Sleep(100);
            keybd_event(CONTROL_KEY, 0x1D, 0x2, 0);
        }




        public static void OnDraw(EventArgs args)
        {

            Drawing.DrawText(((Drawing.Width - 150) / 2), 30, Color.White, getSongName() + " - " + getArtistName());

        }



    }

}



