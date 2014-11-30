using System;
using System.Runtime.InteropServices;
using AssemblySelector.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace AssemblySelector
{
    internal class Selector
    {
        private static readonly Vector2 _scale = new Vector2(1.25f, 1.25f);
        private static Render.Sprite Leaguesharpicon;
        private static Vector2 _posLsharp = new Vector2(Drawing.Width/2f - 286.5f, 15);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [return: MarshalAs(UnmanagedType.Bool)]
        private static void Main(string[] args)
        {
            Leaguesharpicon = loadleaguesharp();
            CustomEvents.Game.OnGameLoad += onGameLoad;
            Game.OnWndProc += Game_OnWndProc;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if ((args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN) && mouseonlsharp())
            {
                bringtofront();
            }
        }

        private static void onGameLoad(EventArgs args)
        {
            Game.PrintChat("Loaded AssemblySelector by Seph");
        }

        private static IntPtr FindLeagueSharp()
        {
            return FindWindow(null, "LeagueSharp");
        }


        private static void bringtofront()
        {
            ShowWindow(FindLeagueSharp(), 1);
            SetForegroundWindow(FindLeagueSharp());
            SetFocus(FindLeagueSharp());
        }

        private static Vector2 GetPosition(int width)
        {
            return new Vector2(Drawing.Width/2f - width/2f, 10);
        }

        private static Vector2 GetScaledVector(Vector2 vector)
        {
            return Vector2.Modulate(_scale, vector);
        }

        private static Render.Sprite loadleaguesharp()
        {
            _posLsharp = GetScaledVector(_posLsharp);

            var loadlsharp = new Render.Sprite(Resources.lsharpicon, _posLsharp)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadlsharp.Position = GetPosition(loadlsharp.Width - 700);

            loadlsharp.Show();
            loadlsharp.Add(0);

            return loadlsharp;
        }

        private static bool mouseonlsharp()
        {
            _posLsharp = GetScaledVector(_posLsharp);

            var loadlsharp = new Render.Sprite(Resources.lsharpicon, _posLsharp)
            {
                Scale = _scale,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };

            Vector2 pos = Utils.GetCursorPos();
            Vector2 lsharpbutton = GetPosition(loadlsharp.Width - 700);

            return ((pos.X >= lsharpbutton.X) && pos.X <= (lsharpbutton.X + loadlsharp.Width) && pos.Y >= lsharpbutton.Y &&
                    pos.Y <= (lsharpbutton.Y + loadlsharp.Height));
        }
    }
}