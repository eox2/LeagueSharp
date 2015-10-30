using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using LeagueSharp.Common;
using LeagueSharp;
using YTSharp;


namespace YTSharp
{

    class Program
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand,
                                         StringBuilder strReturn,
                                         int iReturnLength,
                                         IntPtr hwndCallback);


        static System.Media.SoundPlayer soundPlayer;

        private static bool donedowloading;

        static void Main(string[] args)
        {
            Console.WriteLine("xd");
            new Search("kappa");
            Console.Read();
            //CustomEvents.Game.OnGameLoad += OnLoad;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        static void OnLoad(EventArgs args)
        {
            Game.OnInput += OnInput;
        }

        public static System.Timers.Timer Timer = new System.Timers.Timer();

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void OnInput(GameInputEventArgs args)
        {
            var input = args.Input;
            string path = null;
            if (input.StartsWith(".yt"))
            {
                string url = input.Substring(input.IndexOf(" ") + 1);
                Game.PrintChat("Attempting : " + url);
                args.Process = false;

                Worker workerObject = new Worker();
                workerObject.url = "https://www.youtube.com/watch?v=DwyABA9OiHY";
                Thread workerThread = new Thread(workerObject.DoWork);

                // Start the worker thread.
                workerThread.Start();
                Timer.Elapsed += delegate
                {
                    if (workerObject._done)
                    {
                        play(workerObject.path);
                        Timer.Dispose();
                        Timer.Stop();
                    }

                };
                Timer.Interval = 3000;
                Timer.Enabled = true;
            }
  
            if (input.StartsWith(".stop"))
            {
                stop();
                args.Process = false;
            }
        }

        public static void play(string playpath)
        {
            Console.WriteLine("playing " + playpath);
            mciSendString("open \"" + playpath + "\" type mpegvideo alias MediaFile", null, 0, IntPtr.Zero);
            mciSendString("play MediaFile", null, 0, IntPtr.Zero);
        }

        public static void stop()
        {
            string command = "stop MyMp3";
            mciSendString(command, null, 0, IntPtr.Zero);

            command = "close MyMp3";
            mciSendString(command, null, 0, IntPtr.Zero);

            mciSendString("close MediaFile", null, 0, IntPtr.Zero);
        }

        public static string ProcessUrl(string yt)
        {
            var path = Processing.DLAudio(yt);
            return path;
        }
    }
}

public class Worker
{
    public void DoWork()
    {
        path = Program.ProcessUrl(url);
        _done = true;
    }
    private volatile bool _shouldStop;
    public volatile bool _done;
    public volatile string url;
    public volatile string path;
}
