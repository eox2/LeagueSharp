using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using iTunesLib;
using iTunesSharp.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace iTunes
{
    internal class iTunesSharp
    {
        private const int KEY_MESSAGE = 0x319;
        private const int CONTROL_KEY = 0x11;

        private const long PLAYPAUSE_KEY = 0xE0000L;
        private const long NEXTTRACK_KEY = 0xB0000L;
        private const long PREVIOUS_KEY = 0xC0000L;
        private static Timer aTimer;


        private static readonly Vector2 _scale = new Vector2(1.25f, 1.25f);
        private static Vector2 _posplay = new Vector2(Drawing.Width/2f - 286.5f, 15);
        private static Vector2 _posprev = new Vector2(Drawing.Width/2f - 250.5f, 15);
        private static Vector2 _posnext = new Vector2(Drawing.Width/2f - 214.5f, 15);

        private static Vector2 _posvolu = new Vector2(Drawing.Width/2f - 250.5f, 15);
        private static Vector2 _posvold = new Vector2(Drawing.Width/2f - 214.5f, 15);

        private static Vector2 _positunes = new Vector2(Drawing.Width/2f - 500, 15);

        private static Render.Sprite play;
        private static Render.Sprite itunesicon;
        private static Render.Sprite prev;
        private static Render.Sprite next;
        private static Render.Sprite volup1;
        private static Render.Sprite voldown1;


        private static Menu Config;
        public static string songname = "Not initialized - Press Play";

        //  private static int currvol = 0;


        //   private static Obj_AI_Hero Player;

        [DllImport("user32.dll")]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
            int dwExtraInfo);


        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static void Game_OnGameLoad(EventArgs args)
        {
            itunesicon = loaditunes();
            play = loadplay();
            prev = loadprev();
            next = loadnext();


            volup1 = loadvolup();
            voldown1 = loadvoldown();


            // songname = getsongname() + " - " + getartistname();

            //Menu
            Config = new Menu("iTunes Controller", "itunes", true);


            Config.AddSubMenu(new Menu("iTunes Settings", "settings"));
            Config.SubMenu("settings").AddItem(new MenuItem("showtrack", "Show Track Name").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("enablekeysvol", "Enable Volume Keys").SetValue(true));
            Config.SubMenu("settings").AddItem(new MenuItem("enablekeysctrl", "Enable Control Keys").SetValue(true));


            MenuItem ChangeVolumeUp =
                Config.AddItem(new MenuItem("Volume", "Vol +").SetValue(new KeyBind(106, KeyBindType.Press)));
            MenuItem ChangeVolumeDown =
                Config.AddItem(new MenuItem("Volume2", "Vol -").SetValue(new KeyBind(108, KeyBindType.Press)));
            MenuItem ChangeSkipTrack =
                Config.AddItem(new MenuItem("skip", "Next --->").SetValue(new KeyBind(102, KeyBindType.Press)));
            MenuItem ChangePrev =
                Config.AddItem(new MenuItem("prev", "Prev  <---").SetValue(new KeyBind(104, KeyBindType.Press)));
            MenuItem shosprites =
                Config.AddItem(new MenuItem("showhide", "Hide key").SetValue(new KeyBind(9, KeyBindType.Press)));

            // Config.AddItem(new MenuItem("spriteshow", "Show Sprites").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));


            Config.AddToMainMenu();


            if (isitunesOpen())
            {
                Game.PrintChat("::: iTunes has been detected :::");
            }

            ChangeVolumeUp.ValueChanged += delegate { volumeUpkeys(); };


            ChangeVolumeDown.ValueChanged += delegate { volumeDownkeys(); };

            ChangeSkipTrack.ValueChanged += delegate { nextTrackkeys(); };

            ChangePrev.ValueChanged += delegate { previousTrackkeys(); };


            Game.PrintChat("Loaded iTunes Controller by Seph");
            if (!isitunesOpen())
            {
                Game.PrintChat("iTunes isn't running");
            }

            //  Game.OnGameUpdate += OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
            aTimer = new Timer(500);
            aTimer.Elapsed += OnSongCheck;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void bringtofront()
        {
            if (isitunesOpen())
            {
                ShowWindow(Finditunes(), 1);
                SetForegroundWindow(Finditunes());
                SetFocus(Finditunes());
            }
        }


        private static void Game_OnWndProc(WndEventArgs args)
        {
            /*
            if (Config.Item("spriteshow").GetValue<KeyBind>().Active && (!Config.Item("showhide").GetValue<KeyBind>().Active))
            {
                itunesicon.Show();
                play.Show();
                prev.Show();
                next.Show();


                volup1.Show();
                voldown1.Show();
            }

            if ((!Config.Item("spriteshow").GetValue<KeyBind>().Active) || (Config.Item("showhide").GetValue<KeyBind>().Active))
            {
                itunesicon.Hide();
                play.Hide();
                prev.Hide();
                next.Hide();


                volup1.Hide();
                voldown1.Hide();
            }
            */

            if (!Config.Item("showhide").GetValue<KeyBind>().Active)
            {
                itunesicon.Show();
                play.Show();
                prev.Show();
                next.Show();


                volup1.Show();
                voldown1.Show();
            }

            if (Config.Item("showhide").GetValue<KeyBind>().Active)
            {
                itunesicon.Hide();
                play.Hide();
                prev.Hide();
                next.Hide();


                volup1.Hide();
                voldown1.Hide();
            }


            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && mouseonnext())
            {
                nextTrack();
                songname = getsongname() + " - " + getartistname();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && mouseonprev())
            {
                previousTrack();
                songname = getsongname() + " - " + getartistname();
            }

            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && mouseonplay())
            {
                pausePlay();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && mouseonitunes())
            {
                bringtofront();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && mouseonvolup())
            {
                volumeUp();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && mouseonvoldown())
            {
                volumeDown();
            }
        }

        private static Render.Sprite loaditunes()
        {
            _positunes = GetScaledVector(_positunes);

            var loaditunes = new Render.Sprite(Resources.itunes, _positunes)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loaditunes.Position = GetPosition(loaditunes.Width - 300);

            loaditunes.Show();
            loaditunes.Add(0);

            return loaditunes;
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
            return new Vector2(Drawing.Width/2f - width/2f, 15);
        }

        private static Vector2 GetScaledVector(Vector2 vector)
        {
            return Vector2.Modulate(_scale, vector);
        }


        private static void OnSongCheck(Object source, ElapsedEventArgs e)
        {
            if (isitunesOpen())
            {
                var app = new iTunesApp();
                bool musicplaying = app.PlayerState == ITPlayerState.ITPlayerStatePlaying;
                if (musicplaying)
                {
                    songname = getsongname() + " - " + getartistname();
                }
                else if (!musicplaying)
                {
                    songname = getsongname() + " - " + getartistname() + " - Paused... ";
                }
            }

            if (!isitunesOpen())
            {
                songname = "iTunes not running";
                ;
            }
        }


        private static IntPtr Finditunes()
        {
            return FindWindow("iTunes", null);
        }


        private static String getTitle()
        {
            IntPtr itunesWindow = FindWindow("iTunes", null);
            if (itunesWindow == new IntPtr(0))
            {
                return "";
            }
            int length = GetWindowTextLength(itunesWindow);
            var sb = new StringBuilder(length + 1);
            GetWindowText(itunesWindow, sb, sb.Capacity);
            return sb.ToString();
        }

        public static bool isitunesOpen()
        {
            if (getTitle() == "")
            {
                return false;
            }
            return true;
        }


        public static string getsongname()
        {
            var app = new iTunesApp();
            string track = app.CurrentTrack.Name;
            app = null;
            return track;
        }

        public static string getartistname()
        {
            var app = new iTunesApp();
            string artist = app.CurrentTrack.Artist;
            app = null;
            return artist;
        }


        public static void pausePlay()
        {
            var app = new iTunesApp();
            app.PlayPause();
            app = null;
        }


        public static void nextTrackkeys()
        {
            if (!Config.Item("enablekeysctrl").GetValue<bool>())
            {
                return;
            }
            var app = new iTunesApp();
            app.NextTrack();
            app = null;
        }

        public static void previousTrackkeys()
        {
            if (!Config.Item("enablekeysctrl").GetValue<bool>())
            {
                return;
            }
            var app = new iTunesApp();
            app.PreviousTrack();
            app = null;
        }

        public static void volumeUpkeys()
        {
            if (!Config.Item("enablekeysvol").GetValue<bool>())
            {
                return;
            }
            var app = new iTunesApp();
            app.SoundVolume += 5;
            app = null;
        }

        public static void volumeDownkeys()
        {
            if (!Config.Item("enablekeysvol").GetValue<bool>())
            {
                return;
            }
            var app = new iTunesApp();
            app.SoundVolume -= 5;
            app = null;
        }

        public static void nextTrack()
        {
            var app = new iTunesApp();
            app.NextTrack();
            app = null;
        }

        public static void previousTrack()
        {
            var app = new iTunesApp();
            app.PreviousTrack();
            app = null;
        }

        public static void volumeUp()
        {
            var app = new iTunesApp();
            app.SoundVolume += 5;
            app = null;
        }

        public static void volumeDown()
        {
            var app = new iTunesApp();
            app.SoundVolume -= 5;
            app = null;
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

        private static bool mouseonitunes()
        {
            _positunes = GetScaledVector(_positunes);
            var loaditunes = new Render.Sprite(Resources.itunes, _positunes)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loaditunes.Position = GetPosition(loaditunes.Width - 300);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 spotbuttonpos = GetPosition(loaditunes.Width - 300);

            return ((pos.X >= spotbuttonpos.X) && pos.X <= (spotbuttonpos.X + loaditunes.Width) &&
                    pos.Y >= spotbuttonpos.Y && pos.Y <= (spotbuttonpos.Y + loaditunes.Height));
        }


        private static bool mouseonplay()
        {
            var loadplay = new Render.Sprite(Resources.play, _posplay)

            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadplay.Position = GetPosition(loadplay.Width);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 playbuttonpos = GetPosition(loadplay.Width);

            return ((pos.X >= playbuttonpos.X) && pos.X <= (playbuttonpos.X + loadplay.Width) &&
                    pos.Y >= playbuttonpos.Y && pos.Y <= (playbuttonpos.Y + loadplay.Height));
        }


        private static bool mouseonprev()
        {
            var loadprev = new Render.Sprite(Resources.rewind, _posprev)

            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadprev.Position = GetPosition(loadprev.Width);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 prevbuttonpos = GetPosition(loadprev.Width + 150);

            return ((pos.X >= prevbuttonpos.X) && pos.X <= (prevbuttonpos.X + loadprev.Width) &&
                    pos.Y >= prevbuttonpos.Y && pos.Y <= (prevbuttonpos.Y + loadprev.Height));
        }

        private static bool mouseonnext()
        {
            var loadnext = new Render.Sprite(Resources.forward, _posnext)

            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadnext.Position = GetPosition(loadnext.Width);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 nextbuttonpos = GetPosition(loadnext.Width - 150);

            return ((pos.X >= nextbuttonpos.X) && pos.X <= (nextbuttonpos.X + loadnext.Width) &&
                    pos.Y >= nextbuttonpos.Y && pos.Y <= (nextbuttonpos.Y + loadnext.Height));
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

            Vector2 pos = Utils.GetCursorPos();
            Vector2 volubuttonpos = GetPosition(loadvolup.Width + 300);

            return ((pos.X >= volubuttonpos.X) && pos.X <= (volubuttonpos.X + loadvolup.Width) &&
                    pos.Y >= volubuttonpos.Y && pos.Y <= (volubuttonpos.Y + loadvolup.Height));
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

            Vector2 pos = Utils.GetCursorPos();
            Vector2 voldbuttonpos = GetPosition(loadvoldown.Width + 400);

            return ((pos.X >= voldbuttonpos.X) && pos.X <= (voldbuttonpos.X + loadvoldown.Width) &&
                    pos.Y >= voldbuttonpos.Y && pos.Y <= (voldbuttonpos.Y + loadvoldown.Height));
        }

        public static void OnDraw(EventArgs args)
        {
            // try {
            if (Config.Item("showtrack").GetValue<bool>())
            {
                var font = new Font("Calibri", 12.5F);
                Drawing.DrawText(Drawing.Width/2f - TextWidth(songname, font)/2f, 50, Color.White, songname);

                //   if (songname == null)
                //  {
                //    Drawing.DrawText(Drawing.Width / 2f - TextWidth("null", font) / 2f, 50, Color.White, "null");
                // } 
            }
            //   catch {
            // Console.WriteLine("Exception in Drawing");
            // }
        }
    }
}

// }