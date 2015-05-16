using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SpotifySharp.Properties;
using Color = System.Drawing.Color;

namespace SpotifySharp
{
    //Credits to SpotifyLib && https://code.google.com/p/spotifycontrol/source/browse/trunk/SpotifyControl/Controllers/ControllerSpotify.vb & Trees

    internal class SpotifySharp
    {
        private const int KeyMessage = 0x319;
        private const int ControlKey = 0x11;

        private const long PlaypauseKey = 0xE0000L;
        private const long NexttrackKey = 0xB0000L;
        private const long PreviousKey = 0xC0000L;
        private const long VolumeUpKey = 0xa0000L;
        private const long VolumeDownKey = 0x90000L;


        private static readonly Vector2 Scale = new Vector2(1.25f, 1.25f);
        private static Vector2 _posplay = new Vector2(Drawing.Width / 2f - 286.5f, 15);
        private static Vector2 _posprev = new Vector2(Drawing.Width / 2f - 250.5f, 15);
        private static Vector2 _posnext = new Vector2(Drawing.Width / 2f - 214.5f, 15);

        private static Vector2 _posvolu = new Vector2(Drawing.Width / 2f - 250.5f, 15);
        private static Vector2 _posvold = new Vector2(Drawing.Width / 2f - 214.5f, 15);

        private static Vector2 _posspotify = new Vector2(Drawing.Width / 2f - 500, 15);

        private static Render.Sprite _play;
        private static Render.Sprite _spotifyicon;
        private static Render.Sprite _prev;
        private static Render.Sprite _next;
        private static Render.Sprite _volup1;
        private static Render.Sprite _voldown1;

        private static Menu _config;
        public static string Songname = "Not initialized - Press Play";



        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            _spotifyicon = Loadspotify();
            _play = Loadplay();
            _prev = Loadprev();
            _next = Loadnext();
            _volup1 = Loadvolup();
            _voldown1 = Loadvoldown();


            //Menu
            _config = new Menu("Spotify Controller", "Spotify", true);


            _config.AddSubMenu(new Menu("Spotify Settings", "settings"));
            _config.SubMenu("settings").AddItem(new MenuItem("showtrack", "Show Track Name").SetValue(true));
            _config.SubMenu("settings").AddItem(new MenuItem("enablekeysvol", "Enable Volume Keys").SetValue(true));
            _config.SubMenu("settings").AddItem(new MenuItem("enablekeysctrl", "Enable Control Keys").SetValue(true));


            MenuItem changeVolumeUp =
                _config.AddItem(new MenuItem("Volume", "Vol +").SetValue(new KeyBind(106, KeyBindType.Press)));
            MenuItem changeVolumeDown =
                _config.AddItem(new MenuItem("Volume2", "Vol -").SetValue(new KeyBind(108, KeyBindType.Press)));
            MenuItem changeSkipTrack =
                _config.AddItem(new MenuItem("skip", "Next --->").SetValue(new KeyBind(102, KeyBindType.Press)));
            MenuItem changePrev =
                _config.AddItem(new MenuItem("prev", "Prev  <---").SetValue(new KeyBind(104, KeyBindType.Press)));
            _config.AddItem(new MenuItem("showhide", "Hide key").SetValue(new KeyBind(9, KeyBindType.Press)));

            // Config.AddItem(new MenuItem("spriteshow", "Show Sprites").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));


            _config.AddToMainMenu();


            if (SpotifyOpen())
            {
                Game.PrintChat("::: Spotify has been detected :::");
            }


            changeVolumeUp.ValueChanged += delegate { VolumeUpkeys(); };


            changeVolumeDown.ValueChanged += delegate { VolumeDownkeys(); };

            changeSkipTrack.ValueChanged += delegate { NextTrackkeys(); };

            changePrev.ValueChanged += delegate { PreviousTrackkeys(); };

            Game.PrintChat("Loaded Spotify Controller by Seph");
            if (!SpotifyOpen())
            {
                Game.PrintChat("Spotify isn't running");
            }

            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
        }

        private static void Bringtofront()
        {
            if (SpotifyOpen())
            {
                WinAPI.ShowWindow(FindSpotify(), 1);
                SetForegroundWindow(FindSpotify());
                SetFocus(FindSpotify());
            }
        }


        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!_config.Item("showhide").GetValue<KeyBind>().Active)
            {
                _spotifyicon.Show();
                _play.Show();
                _prev.Show();
                _next.Show();

                _volup1.Show();
                _voldown1.Show();
            }

            if (_config.Item("showhide").GetValue<KeyBind>().Active)
            {
                _spotifyicon.Hide();
                _play.Hide();
                _prev.Hide();
                _next.Hide();

                _volup1.Hide();
                _voldown1.Hide();
            }


            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && Mouseonnext())
            {
                NextTrack();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && Mouseonprev())
            {
                PreviousTrack();
            }

            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && Mouseonplay())
            {
                Songname = GetSongName() + " - " + GetArtistName() + " - Paused... ";
                PausePlay();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && Mouseonspotify())
            {
                Bringtofront();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && Mouseonvolup())
            {
                VolumeUp();
            }
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && Mouseonvoldown())
            {
                VolumeDown();
            }
        }


        private static Render.Sprite Loadspotify()
        {
            _posspotify = GetScaledVector(_posspotify);

            var loadspotify = new Render.Sprite(Resources.spotify, _posspotify)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadspotify.Position = GetPosition(loadspotify.Width - 300);

            loadspotify.Show();
            loadspotify.Add(0);

            return loadspotify;
        }


        private static Render.Sprite Loadnext()
        {
            _posnext = GetScaledVector(_posnext);

            var loadnext = new Render.Sprite(Resources.forward, _posnext)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadnext.Position = GetPosition(loadnext.Width - 150);

            loadnext.Show();
            loadnext.Add(0);

            return loadnext;
        }

        private static Render.Sprite Loadprev()
        {
            _posprev = GetScaledVector(_posprev);


            var loadprev = new Render.Sprite(Resources.rewind, _posprev)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadprev.Position = GetPosition(loadprev.Width + 150);

            loadprev.Show();
            loadprev.Add(0);


            return loadprev;
        }

        private static Render.Sprite Loadvolup()
        {
            _posvolu = GetScaledVector(_posvolu);


            var loadvolup = new Render.Sprite(Resources.volup, _posvolu)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadvolup.Position = GetPosition(loadvolup.Width + 300);

            loadvolup.Show();
            loadvolup.Add(0);


            return loadvolup;
        }

        private static Render.Sprite Loadvoldown()
        {
            _posvold = GetScaledVector(_posvold);


            var loadvoldown = new Render.Sprite(Resources.voldown, _posvold)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadvoldown.Position = GetPosition(loadvoldown.Width + 400);

            loadvoldown.Show();
            loadvoldown.Add(0);


            return loadvoldown;
        }


        private static Render.Sprite Loadplay()
        {
            _posplay = GetScaledVector(_posplay);

            var loadplay = new Render.Sprite(Resources.play, _posplay)
            {
                Scale = Scale,
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
            return Vector2.Multiply(Scale, vector);
        }


        private static IntPtr FindSpotify()
        {
            return FindWindow("SpotifyMainWindow", null);
        }


        private static String GetTitle()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            if (spotifyWindow == new IntPtr(0))
            {
                return "";
            }
            int length = GetWindowTextLength(spotifyWindow);
            var sb = new StringBuilder(length + 1);
            GetWindowText(spotifyWindow, sb, sb.Capacity);
            return sb.ToString();
        }



        public static bool SpotifyOpen()
        {
            IntPtr Spotify = FindWindow("SpotifyMainWindow", null);
            return Spotify != null;
        }


        public static String GetSongName()
        {
            String[] title = GetTitle().Split('–');
            if (title.Count() > 1)
            {
                return title[1].Trim();
            }
            return "";
        }


        public static String GetArtistName()
        {
            String[] title = GetTitle().Split('–');
            if (title.Count() > 1)
            {
                return title[0].Split('-')[1].Trim();
            }
            return "";
        }

        public static void PausePlay()
        {
            var spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KeyMessage, IntPtr.Zero, new IntPtr(PlaypauseKey));
        }

        public static void NextTrackkeys()
        {
            if (!_config.Item("enablekeysctrl").GetValue<bool>())
            {
                return;
            }
            var spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KeyMessage, IntPtr.Zero, new IntPtr(NexttrackKey));
        }

        public static void PreviousTrackkeys()
        {
            if (!_config.Item("enablekeysctrl").GetValue<bool>())
            {
                return;
            }
            var spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KeyMessage, IntPtr.Zero, new IntPtr(PreviousKey));
        }

        public static void VolumeUpkeys()
        {
            if (!_config.Item("enablekeysvol").GetValue<bool>())
            {
                return;
            }
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(ControlKey, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x26), IntPtr.Zero);
            Thread.Sleep(1);
            keybd_event(ControlKey, 0x1D, 0x2, 0);
        }

        public static void VolumeDownkeys()
        {
            if (!_config.Item("enablekeysvol").GetValue<bool>())
            {
                return;
            }
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(ControlKey, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x28), IntPtr.Zero);
            Thread.Sleep(1);
            keybd_event(ControlKey, 0x1D, 0x2, 0);
        }

        public static void VollUp()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KeyMessage, IntPtr.Zero, new IntPtr(VolumeUpKey));
        }
        public static void VollDown()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KeyMessage, IntPtr.Zero, new IntPtr(VolumeDownKey));
        }


        public static void NextTrack()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KeyMessage, IntPtr.Zero, new IntPtr(NexttrackKey));
        }

        public static void PreviousTrack()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            PostMessage(spotifyWindow, KeyMessage, IntPtr.Zero, new IntPtr(PreviousKey));
        }

        public static void VolumeUp()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(ControlKey, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x26), IntPtr.Zero);
            Thread.Sleep(1);
            keybd_event(ControlKey, 0x1D, 0x2, 0);
        }

        public static void VolumeDown()
        {
            IntPtr spotifyWindow = FindWindow("SpotifyMainWindow", null);
            keybd_event(ControlKey, 0x1D, 0, 0);
            PostMessage(spotifyWindow, 0x100, new IntPtr(0x28), IntPtr.Zero);
            Thread.Sleep(1);
            keybd_event(ControlKey, 0x1D, 0x2, 0);
        }


        public static float TextWidth(string text, Font f)
        {
            float textWidth;

            using (var bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    textWidth = g.MeasureString(text, f).Width;
                }
            }

            return textWidth;
        }

        private static bool Mouseonspotify()
        {
            _posspotify = GetScaledVector(_posspotify);
            var loadspotify = new Render.Sprite(Resources.spotify, _posspotify)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadspotify.Position = GetPosition(loadspotify.Width - 300);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 spotbuttonpos = GetPosition(loadspotify.Width - 300);

            return ((pos.X >= spotbuttonpos.X) && pos.X <= (spotbuttonpos.X + loadspotify.Width) &&
                    pos.Y >= spotbuttonpos.Y && pos.Y <= (spotbuttonpos.Y + loadspotify.Height));
        }


        private static bool Mouseonplay()
        {
            var loadplay = new Render.Sprite(Resources.play, _posplay)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadplay.Position = GetPosition(loadplay.Width);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 playbuttonpos = GetPosition(loadplay.Width);

            return ((pos.X >= playbuttonpos.X) && pos.X <= (playbuttonpos.X + loadplay.Width) &&
                    pos.Y >= playbuttonpos.Y && pos.Y <= (playbuttonpos.Y + loadplay.Height));
        }


        private static bool Mouseonprev()
        {
            var loadprev = new Render.Sprite(Resources.rewind, _posprev)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadprev.Position = GetPosition(loadprev.Width);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 prevbuttonpos = GetPosition(loadprev.Width + 150);

            return ((pos.X >= prevbuttonpos.X) && pos.X <= (prevbuttonpos.X + loadprev.Width) &&
                    pos.Y >= prevbuttonpos.Y && pos.Y <= (prevbuttonpos.Y + loadprev.Height));
        }


        private static bool Mouseonnext()
        {
            var loadnext = new Render.Sprite(Resources.forward, _posnext)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadnext.Position = GetPosition(loadnext.Width);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 nextbuttonpos = GetPosition(loadnext.Width - 150);

            return ((pos.X >= nextbuttonpos.X) && pos.X <= (nextbuttonpos.X + loadnext.Width) &&
                    pos.Y >= nextbuttonpos.Y && pos.Y <= (nextbuttonpos.Y + loadnext.Height));
        }

        private static bool Mouseonvolup()
        {
            _posvolu = GetScaledVector(_posvolu);


            var loadvolup = new Render.Sprite(Resources.volup, _posvold)
            {
                Scale = Scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };

            loadvolup.Position = GetPosition(loadvolup.Width);

            Vector2 pos = Utils.GetCursorPos();
            Vector2 volubuttonpos = GetPosition(loadvolup.Width + 300);

            return ((pos.X >= volubuttonpos.X) && pos.X <= (volubuttonpos.X + loadvolup.Width) &&
                    pos.Y >= volubuttonpos.Y && pos.Y <= (volubuttonpos.Y + loadvolup.Height));
        }


        private static bool Mouseonvoldown()
        {
            _posvold = GetScaledVector(_posvold);


            var loadvoldown = new Render.Sprite(Resources.voldown, _posvold)
            {
                Scale = Scale,
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
            if (_config.Item("showtrack").GetValue<bool>())
            {
                var font = new Font("Calibri", 12.5F);

                if (GetSongName() == null)
                {
                    Drawing.DrawText(Drawing.Width / 2f - TextWidth("null", font) / 2f, 50, Color.White, "null");
                }

                if (GetSongName() == "") // note bugsplat if songname undefined 
                {
                    Drawing.DrawText(Drawing.Width / 2f - TextWidth(Songname, font) / 2f, 50, Color.White, Songname);
                }

                if (GetSongName() != "")
                {
                    Drawing.DrawText(
                        Drawing.Width / 2f - TextWidth(GetSongName() + " - " + GetArtistName(), font) / 2f, 50,
                        Color.White, GetSongName() + " - " + GetArtistName());
                }
            }
        }

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
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }

}