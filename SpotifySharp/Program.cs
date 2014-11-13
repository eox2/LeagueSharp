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
using SpotifySharp.Properties;
using System.Windows;
using System.Windows.Input;


//new

using System.Speech.Synthesis;
using System.Speech.Recognition;

//new

namespace SpotSharp
{


    internal class SpotifySharp
    {
        static SpeechSynthesizer sSynth = new SpeechSynthesizer();
        static SpeechRecognitionEngine sRecognize = new SpeechRecognitionEngine();
        static Choices speechList = new Choices();
        static Grammar gr;

        private static readonly Vector2 _scale = new Vector2(1.25f, 1.25f);
        private static Vector2 _posplay = new Vector2(Drawing.Width / 2f - 286.5f, 15);
        private static Vector2 _posprev = new Vector2(Drawing.Width / 2f - 250.5f, 15);
        private static Vector2 _posnext = new Vector2(Drawing.Width / 2f - 214.5f, 15);

        private static Vector2 _posvolu = new Vector2(Drawing.Width / 2f - 250.5f, 15);
        private static Vector2 _posvold = new Vector2(Drawing.Width / 2f - 214.5f, 15);

        private static Vector2 _posspotify = new Vector2(Drawing.Width / 2f - 500, 15);

        private static Render.Sprite play;
        private static Render.Sprite spotifyicon;
         private static Render.Sprite prev;
         private static Render.Sprite next;
         private static Render.Sprite volup1;
         private static Render.Sprite voldown1;

        private static Menu Config;
        public static string songname = "Not initialized - Press Play";

        //  private static int currvol = 0;


        //   private static Obj_AI_Hero Player;




        // Credit goes to http://code.google.com/p/spotifycontrol/source/browse/trunk/SpotifyControl/Controllers/ControllerSpotify.vb && SpotifyLib
        // for some of the key message commands, etc and Trees for helping me (used some of his sprite code too)
        [DllImport("user32.dll")]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);


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










        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        public static void Game_OnGameLoad(EventArgs args)
        {

               spotifyicon = loadspotify();
               play = loadplay();
               prev = loadprev();
                next = loadnext();
          
            
                volup1 = loadvolup();
                voldown1 = loadvoldown();



                speechList.Add(new string[] { "next", "skip", "previous", "rewind", "back", "play", "pause" });
                gr = new Grammar(new GrammarBuilder(speechList));

            //Menu
            Config = new Menu("Spotify Controller", "Spotify", true);


            Config.AddSubMenu(new Menu("Spotify Settings", "settings"));
            Config.SubMenu("settings").AddItem(new MenuItem("showtrack", "Show Track Name").SetValue(true));
          //  Config.SubMenu("settings").AddItem(new MenuItem("hidespritestab", "AutoHide").SetValue(false)); //- fixed
          //  Config.AddItem(new MenuItem("hidespritestab", "Hide Sprites on Tab").SetValue(new KeyBind(9, KeyBindType.Press)));

           // Config.SubMenu("settings").AddItem(new MenuItem("showsvprites", "Show Volume Sprites").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("enablekeysvol", "Enable Volume Keys").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("enablekeysctrl", "Enable Control Keys").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("enablespeech", "Enable Speech").SetValue(true));

            // Config.SubMenu("settings").AddItem(new MenuItem("hotkey", "Modifier").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Config.SubMenu("settings").AddItem(new MenuItem("showcontrls", "Show Controls").SetValue(true));

            //  Config.AddSubMenu(new Menu("Drawings", "Drawings"));

            //   Config.SubMenu("settings").AddItem(new MenuItem("volume", "Volume %").SetValue(new Slider(40)));

            // var volpct = Config.Item("volume").GetValue<Slider>().Value / 100f;
           // var volpct = Config.AddItem(new MenuItem("vol%", "Volume %").SetValue(new Slider(50))); - cant control from L# menu maybe sprites in future
         //    var volpct2 = Config.Item("vol%").GetValue<Slider>().Value / 100f;


            // bool enablevolkeys = Config.Item("enablekeysvol").GetValue<bool>();
            //  bool enablectlkeys = Config.Item("enablekeysctl").GetValue<bool>();


            var ChangeVolumeUp = Config.AddItem(new MenuItem("Volume", "Vol +").SetValue(new KeyBind(106, KeyBindType.Press)));
            var ChangeVolumeDown = Config.AddItem(new MenuItem("Volume2", "Vol -").SetValue(new KeyBind(108, KeyBindType.Press)));
            var ChangeSkipTrack = Config.AddItem(new MenuItem("skip", "Next --->").SetValue(new KeyBind(102, KeyBindType.Press)));
            var ChangePrev = Config.AddItem(new MenuItem("prev", "Prev  <---").SetValue(new KeyBind(104, KeyBindType.Press)));
            var shosprites = Config.AddItem(new MenuItem("showhide", "Hide key").SetValue(new KeyBind(9, KeyBindType.Press)));
            var speechkeyset = Config.AddItem(new MenuItem("speechkey", "Speech Key").SetValue(new KeyBind(9, KeyBindType.Press)));

           // Config.AddItem(new MenuItem("spriteshow", "Show Sprites").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));



            Config.AddToMainMenu();

          

            if (isSpotifyOpen())
            {
                Game.PrintChat("::: Spotify has been detected :::");
            }
                // Game.PrintChat("swag" + volpct2);
                //volpct.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                //  {
                // Game.PrintChat("swag" + volpct2 + " n" + Config.Item("vol%").GetValue<Slider>().Value / 100f);

                //  if ((Config.Item("vol%").GetValue<Slider>().Value / 100f) > 0.5f) { volumeUp(); } -- wont work because shift is also modifier key and you need it for L# menu
                //   if ((Config.Item("vol%").GetValue<Slider>().Value / 100f) < 0.5f) { volumeDown(); }
                //currvol++;

                //  volumeUpkeys();
                //  };

          //   shosprites.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
           // {
                //currvol++;
               // if shosprites.

               // volumeUpkeys();
          //  };
                ChangeVolumeUp.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    //currvol++;

                    volumeUpkeys();
                };




                ChangeVolumeDown.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    // currvol--;

                    volumeDownkeys();
                };

                ChangeSkipTrack.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    //currvol++;
                    nextTrackkeys();
                };

                ChangePrev.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
                {
                    // currvol--;
                    previousTrackkeys();
                };

               

            
                Game.PrintChat("Loaded Spotify Controller by Seph");
                if (!isSpotifyOpen()) { Game.PrintChat("Spotify isn't running"); }

              //  Game.OnGameUpdate += OnGameUpdate;
                Game.OnWndProc += Game_OnWndProc;
                Drawing.OnDraw += OnDraw;


                //}

            
        }

        private static void bringtofront()
        {
            if (isSpotifyOpen())
            {
                SpotifySharp.ShowWindow(SpotifySharp.FindSpotify(), 1);
                SpotifySharp.SetForegroundWindow(SpotifySharp.FindSpotify());
                SpotifySharp.SetFocus(SpotifySharp.FindSpotify());
            }
        }

        // private static void Event(object sender, EventArgs e) { Game.PrintChat("Left mouse click!"); }

        static void sRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "next":
                case "forward":
                case "skip":
                    Game.PrintChat("Speech Recognizer: Next Song Selected -- >");
                    nextTrack();
                    break;
                case "previous":
                case "rewind":
                case "back":
                    Game.PrintChat("Speech Recognizer: Previous Song Selected < --");
                    previousTrack();
                    break;
                case "pause":
                case "play":
                    Game.PrintChat("Speech Recognizer: Play/Pause");
                    songname = getSongName() + " - " + getArtistName() + " - Paused... ";
                    pausePlay();
                    break;
   
  
                case "recall":
                    Game.PrintChat("Speech Recognizer: Recall");
                    Spell C = new Spell(SpellSlot.Recall);
                    C.Cast();   
                    break;
                case "spotify":
                case "front":
                case "bring to front":
                    bringtofront();
                    break;
            }
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {

            if (Config.Item("speechkey").GetValue<KeyBind>().Active)
            {
                try
                {
                    sRecognize.RequestRecognizerUpdate();
                    sRecognize.LoadGrammar(gr);
                    sRecognize.SpeechRecognized += sRecognize_SpeechRecognized;
                    sRecognize.SetInputToDefaultAudioDevice();
                    sRecognize.RecognizeAsync(RecognizeMode.Multiple);
                    sRecognize.Recognize();
                }
                catch
                {
                    return;
                }
            }
            /*
            if (Config.Item("spriteshow").GetValue<KeyBind>().Active && (!Config.Item("showhide").GetValue<KeyBind>().Active))
            {
                spotifyicon.Show();
                play.Show();
                prev.Show();
                next.Show();


                volup1.Show();
                voldown1.Show();
            }

            if ((!Config.Item("spriteshow").GetValue<KeyBind>().Active) || (Config.Item("showhide").GetValue<KeyBind>().Active))
            {
                spotifyicon.Hide();
                play.Hide();
                prev.Hide();
                next.Hide();


                volup1.Hide();
                voldown1.Hide();
            }
            */

            if  (!Config.Item("showhide").GetValue<KeyBind>().Active)
            {
                spotifyicon.Show();
                play.Show();
                prev.Show();
                next.Show();


                volup1.Show();
                voldown1.Show();
            }

            if (Config.Item("showhide").GetValue<KeyBind>().Active)
            {
                spotifyicon.Hide();
                play.Hide();
                prev.Hide();
                next.Hide();


                volup1.Hide();
                voldown1.Hide();
            }
           

           if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonnext()) 
            {
                nextTrack();
            }
           if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonprev())
           {
               previousTrack();
           } 
             
            if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonplay())
            {
                songname = getSongName() + " - " + getArtistName() + " - Paused... ";
                pausePlay();
            }
            if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonspotify())
            {
                bringtofront();
              
            } 
            if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonvolup())
            {
                volumeUp();
            }
            if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonvoldown())
            {
                volumeDown();
            } 


        }

        private static Render.Sprite loadspotify()
        {

            _posspotify = GetScaledVector(_posspotify);

            var loadspotify = new Render.Sprite(Resources.spotify, _posspotify)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadspotify.Position = GetPosition(loadspotify.Width - 300);

            loadspotify.Show();
            loadspotify.Add(0);

            return loadspotify;
        }
      

        private static Render.Sprite loadnext()
               {

                   _posnext = GetScaledVector(_posnext);

                   var loadnext = new Render.Sprite(Resources.forward, _posnext)
                   {
                       Scale = _scale,
                       Color = new ColorBGRA(255f, 255f, 255f, 20f)
                   };
                   loadnext.Position = GetPosition(loadnext.Width - 150);
  
                       loadnext.Show();
                       loadnext.Add(0);
     
                   return loadnext;
               }

               private static Render.Sprite loadprev()
               {
                   _posprev = GetScaledVector(_posprev);
          

                   var loadprev = new Render.Sprite(Resources.rewind, _posprev)
                   {
                       Scale = _scale,
                       Color = new ColorBGRA(255f, 255f, 255f, 20f)
                   };
                   loadprev.Position = GetPosition(loadprev.Width + 150);
      
                       loadprev.Show();
                       loadprev.Add(0);


                   return loadprev;

              }

               private static Render.Sprite loadvolup()
               {
                   _posvolu = GetScaledVector(_posvolu);


                   var loadvolup = new Render.Sprite(Resources.volup, _posvolu)
                   {
                       Scale = _scale,
                       Color = new ColorBGRA(255f, 255f, 255f, 20f)
                   };
                   loadvolup.Position = GetPosition(loadvolup.Width + 300);

                   loadvolup.Show();
                   loadvolup.Add(0);


                   return loadvolup;

               }

               private static Render.Sprite loadvoldown()
               {
                   _posvold = GetScaledVector(_posvold);


                   var loadvoldown = new Render.Sprite(Resources.voldown, _posvold)
                   {
                       Scale = _scale,
                       Color = new ColorBGRA(255f, 255f, 255f, 20f)
                   };
                   loadvoldown.Position = GetPosition(loadvoldown.Width + 400);

                   loadvoldown.Show();
                   loadvoldown.Add(0);


                   return loadvoldown;

               }
        



        private static Render.Sprite loadplay()
        {
            _posplay = GetScaledVector(_posplay);

            var loadplay = new Render.Sprite(Resources.play, _posplay)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadplay.Position = GetPosition(loadplay.Width);

            loadplay.Show();
            loadplay.Add(0);




            return loadplay;
        }

        private static Vector2 GetPosition(int width)
        {
            return new Vector2(Drawing.Width / 2f - width / 2f, 15);
        }

        private static Vector2 GetScaledVector(Vector2 vector)
        {
            return Vector2.Modulate(_scale, vector);
        }




        private static void OnGameUpdate(EventArgs args)
        {

        }


        private static IntPtr FindSpotify()
        {
            return FindWindow("SpotifyMainWindow", null);
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
             string songname = getSongName() + " - " + getArtistName();
            PostMessage(spotifyWindow, KEY_MESSAGE, IntPtr.Zero, new IntPtr(PLAYPAUSE_KEY));

        }
        static public void nextTrackkeys()
        {
            if (!Config.Item("enablekeysctrl").GetValue<bool>()) { return; }
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KEY_MESSAGE, IntPtr.Zero, new IntPtr(NEXTTRACK_KEY));
        }
        static public void previousTrackkeys()
        {
            if (!Config.Item("enablekeysctrl").GetValue<bool>()) { return; }
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KEY_MESSAGE, IntPtr.Zero, new IntPtr(PREVIOUS_KEY));
        }
        static public void volumeUpkeys()
        {
            if (!Config.Item("enablekeysvol").GetValue<bool>()) { return; }
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(CONTROL_KEY, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x26), IntPtr.Zero);
           System.Threading.Thread.Sleep(1);
            keybd_event(CONTROL_KEY, 0x1D, 0x2, 0);
        }
        static public void volumeDownkeys()
        {
            if (!Config.Item("enablekeysvol").GetValue<bool>()) { return; }
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(CONTROL_KEY, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x28), IntPtr.Zero);
            System.Threading.Thread.Sleep(1);
            keybd_event(CONTROL_KEY, 0x1D, 0x2, 0);
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
            System.Threading.Thread.Sleep(1);
            keybd_event(CONTROL_KEY, 0x1D, 0x2, 0);
        }
        static public void volumeDown()
        {

            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(CONTROL_KEY, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x28), IntPtr.Zero);
            System.Threading.Thread.Sleep(1);
            keybd_event(CONTROL_KEY, 0x1D, 0x2, 0);
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

        private static bool mouseonspotify()
        {
            _posspotify = GetScaledVector(_posspotify);
            var loadspotify = new Render.Sprite(Resources.spotify, _posspotify)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadspotify.Position = GetPosition(loadspotify.Width - 300);

            var pos = Utils.GetCursorPos();
            var spotbuttonpos = GetPosition(loadspotify.Width - 300);

            return ((pos.X >= spotbuttonpos.X) && pos.X <= (spotbuttonpos.X + loadspotify.Width) && pos.Y >= spotbuttonpos.Y && pos.Y <= (spotbuttonpos.Y + loadspotify.Height));
        }
     

        private static bool mouseonplay()
        {
            var loadplay = new Render.Sprite(Resources.play, _posplay)

              {
                  Scale = _scale,
                  Color = new ColorBGRA(255f, 255f, 255f, 20f)
              };
            loadplay.Position = GetPosition(loadplay.Width);

            var pos = Utils.GetCursorPos();
            var playbuttonpos = GetPosition(loadplay.Width);

            return ((pos.X >= playbuttonpos.X) && pos.X <= (playbuttonpos.X + loadplay.Width) && pos.Y >= playbuttonpos.Y && pos.Y <= (playbuttonpos.Y + loadplay.Height));
        }

        
        private static bool mouseonprev()
        {
            var loadprev = new Render.Sprite(Resources.rewind,_posprev)

              {
                   Scale = _scale,
                  Color = new ColorBGRA(255f, 255f, 255f, 20f)
                };
                loadprev.Position = GetPosition(loadprev.Width);

              var pos = Utils.GetCursorPos();
             var prevbuttonpos = GetPosition(loadprev.Width + 150);
          
            return ((pos.X >= prevbuttonpos.X) && pos.X <= (prevbuttonpos.X + loadprev.Width) && pos.Y >= prevbuttonpos.Y && pos.Y <= (prevbuttonpos.Y + loadprev.Height));
        }
          private static bool mouseonnext()
        {
            var loadnext = new Render.Sprite(Resources.forward, _posnext)

              {
                   Scale = _scale,
                  Color = new ColorBGRA(255f, 255f, 255f, 20f)
                };
                loadnext.Position = GetPosition(loadnext.Width);

              var pos = Utils.GetCursorPos();
             var nextbuttonpos = GetPosition(loadnext.Width - 150);
          
            return ((pos.X >= nextbuttonpos.X) && pos.X <= (nextbuttonpos.X + loadnext.Width) && pos.Y >= nextbuttonpos.Y && pos.Y <= (nextbuttonpos.Y + loadnext.Height));
        }
          private static bool mouseonvolup()
          {

              _posvolu = GetScaledVector(_posvolu);


              var loadvolup = new Render.Sprite(Resources.volup, _posvold)
              {
                  Scale = _scale,
                  Color = new ColorBGRA(255f, 255f, 255f, 20f)
              };

              loadvolup.Position = GetPosition(loadvolup.Width);

              var pos = Utils.GetCursorPos();
              var volubuttonpos = GetPosition(loadvolup.Width + 300);

              return ((pos.X >= volubuttonpos.X) && pos.X <= (volubuttonpos.X + loadvolup.Width) && pos.Y >= volubuttonpos.Y && pos.Y <= (volubuttonpos.Y + loadvolup.Height));
          }
       
          private static bool mouseonvoldown()
          {

              _posvold = GetScaledVector(_posvold);


              var loadvoldown = new Render.Sprite(Resources.voldown, _posvold)
              {
                  Scale = _scale,
                  Color = new ColorBGRA(255f, 255f, 255f, 20f)
              };

              loadvoldown.Position = GetPosition(loadvoldown.Width);

              var pos = Utils.GetCursorPos();
              var voldbuttonpos = GetPosition(loadvoldown.Width + 400);

              return ((pos.X >= voldbuttonpos.X) && pos.X <= (voldbuttonpos.X + loadvoldown.Width) && pos.Y >= voldbuttonpos.Y && pos.Y <= (voldbuttonpos.Y + loadvoldown.Height));
          }
       
        public static void OnDraw(EventArgs args)
        {
            // try {
            if (Config.Item("showtrack").GetValue<bool>())
            {

                Font font = new Font("Calibri", 12.5F);

                if (getSongName() == null)
                {
                    Drawing.DrawText(Drawing.Width / 2f - TextWidth("null", font) / 2f, 50, Color.White, "null");
                }

                if (getSongName() == "") // note bugsplat if songname undefined 
                {
                    Drawing.DrawText(Drawing.Width / 2f - TextWidth(songname, font) / 2f, 50, Color.White, songname);
                }

                if (getSongName() != "")
                {
                   Drawing.DrawText(Drawing.Width / 2f - TextWidth(getSongName() + " - " + getArtistName(), font) / 2f, 50, Color.White, getSongName() + " - " + getArtistName());
                }
                
                

            }
            //   catch {
            // Console.WriteLine("Exception in Drawing");
            // }

        }
    }


    }
   // }


