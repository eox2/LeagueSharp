using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using EloSharp_V2.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color2 = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
using System.Timers;
using System.Security.Permissions;
using System.Web;

namespace EloSharp_V2
{

    /*
     * //Credits to TC Crew for using Tracker HP bar indicator
     * To do: Test and Update Assembly once Loader adds ability to allow websites through sandbox // Lolnexus gets whitelisted
     * DW about Lolskill = unreliable af
     */

    public class EloSharp
    {

        public static Timer Timer;


        public static bool disabletext;
        public static Sprite Sprite;
        public static Font Text;
        public static int X = 0;
        public static int Y = 0;

        //Loading Screen shit


        private static Render.Sprite buttonicon;
        private static Vector2 _posbutton = new Vector2(Drawing.Width / 2f - 286.5f, 15);
        private static Render.Sprite background;
        private static Vector2 _posbackground = new Vector2(Drawing.Width / 2f, 0);
        private static readonly Vector2 _scalebackground = new Vector2(1f, 1f);
        private static readonly Vector2 _scaleicon = new Vector2(1.0f, 1.0f);
        private static readonly Vector2 _scalesprites = new Vector2(0.2f, 0.2f);
        public static bool ready;
        public static List<Obj_AI_Hero> champlist = new List<Obj_AI_Hero>();
        private static string nameofplayer = "";
        private static bool delaying;


       [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        public static void Main(string[] args)
        {
            Console.WriteLine("Elosharp V2 injected");

            CustomEvents.Game.OnGameLoad += Setup;

            PrepareDrawing();

            Misc.MenuAttach(Misc.Config);

            var website = Misc.Config.Item("choosewebsite");
            website.ValueChanged += delegate { Misc.SetWebsite = Misc.Config.Item("choosewebsite").GetValue<StringList>().SelectedIndex; };

            Misc.RegionTag = Game.Region;

            if (File.Exists(Config.AppDataDirectory + "\\elosharp.txt"))
            {
                nameofplayer = File.ReadAllText(Config.AppDataDirectory + "\\elosharp.txt").ToLower();
                nameofplayer = HttpUtility.UrlEncode(nameofplayer);
            }
            else
            {
                Console.WriteLine("Elosharp.txt not found. Waiting for game to load to setup.");
            }

            if (Misc.Validregion() && (nameofplayer != ""))
            {
                Console.WriteLine("[EloSharp] Configuration set for:  " + nameofplayer);

                if (Game.Mode == GameMode.Running)
                {
                    try
                    {
                        Console.WriteLine("performing instant lookup");
                            Performlookup();
                            delaying = false;
                            Game_OnGameLoad(new EventArgs());
        
         
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine("performing delayed lookup");
                    delaying = true;
                    CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
                    Timer = new Timer(5000);
                    Timer.Elapsed += new ElapsedEventHandler(TriggerLookup);
                    Timer.Enabled = true;
                }
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        static void TriggerLookup(object sender, ElapsedEventArgs e) 
        {
            Performlookup();
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void Performlookup()
        {
            try
            {
                string setwebsite = Misc.getsetwebsite().ToLower();
                if (setwebsite == "lolnexus")
                {
                    Console.WriteLine("Looking up using Lolnexus");
                    Lolnexus.lolnexuslookup(nameofplayer, Misc.sortedregion());
                    if (Lolnexus.Ranksloading.Any())
                    {
                        DoDrawings();
                        Game.OnWndProc += Game_OnWndProc;
                        if (Timer != null)
                        {
                            Timer.Elapsed -= new ElapsedEventHandler(TriggerLookup);
                            Timer.Enabled = false;
                        }
                    }
                    return;
                }
                if (setwebsite == "lolskill")
                {
                    Console.WriteLine("Looking up using Lolskill");
                    LolSkill.lolskilllookup(nameofplayer);
                    if (LolSkill.Ranksloading.Any())
                    {
                        DoDrawings();
                        Game.OnWndProc += Game_OnWndProc;
                        if (Timer != null)
                        {
                            Timer.Elapsed -= new ElapsedEventHandler(TriggerLookup);
                            Timer.Enabled = false;
                        }
                    }
                    return;
                }
                if (setwebsite == "opgg")
                {
                    Console.WriteLine("Looking up using OPGG Live");
                    OPGGLIVE.PerformLookup(nameofplayer);
                    if (OPGGLIVE.Ranks.Any())
                    {
                        DoDrawings();
                        Game.OnWndProc += Game_OnWndProc;
                        if (Timer != null)
                        {
                            Timer.Elapsed -= new ElapsedEventHandler(TriggerLookup);
                            Timer.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e + e.StackTrace);
            }
        }


        public static void Game_OnGameLoad(EventArgs args)
        {

            Console.WriteLine("<<EloSharp V2 Loaded>> by Seph.");

            if (Timer != null)
            {
                Timer.Elapsed -= new ElapsedEventHandler(TriggerLookup);
                Timer.Enabled = false;
            }

            string setwebsite = Misc.getsetwebsite().ToLower();

            
            if (setwebsite == "opgg2")
            {
                new System.Threading.Thread(() =>
                {
                    new OPGG();
                    Drawing.OnDraw += OPGG.OnDraw;
                }).Start();

                return;
            }

            if (Lolnexus.Ranksloading.Any() || LolSkill.Ranksloading.Any() || OPGGLIVE.Ranks.Any())
            {
                SubEvents();
                GetHeroHandles(setwebsite);
            }
        }


        private static void Setup(EventArgs args)
        {
            if (File.Exists(Config.AppDataDirectory + "\\elosharp.txt") &&
                Misc.Config.Item("autoupdate").GetValue<bool>())
            {
                string getplayername = File.ReadAllText(Config.AppDataDirectory + "\\elosharp.txt");
                if (getplayername != ObjectManager.Player.Name)
                {
                    Console.WriteLine("[EloSharp] has set the default name as the current user for faster lookups!");
                    File.WriteAllText(
                        LeagueSharp.Common.Config.AppDataDirectory + "\\elosharp.txt", ObjectManager.Player.Name);
                    nameofplayer = HttpUtility.UrlEncode(ObjectManager.Player.Name);
                    Performlookup();
                }
                return;
            }
            if (!File.Exists(LeagueSharp.Common.Config.AppDataDirectory + "\\elosharp.txt"))
            {
                Console.WriteLine("Creating elosharp.txt because first use");
                File.WriteAllText(LeagueSharp.Common.Config.AppDataDirectory + "\\elosharp.txt", ObjectManager.Player.Name);
                nameofplayer = HttpUtility.UrlEncode(ObjectManager.Player.Name);
                Performlookup();
            }
        }


        private static void SubEvents()
        {
            Drawing.OnPreReset += DrawingOnOnPreReset;
            Drawing.OnDraw += Drawing_OnEndScene;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnDomainUnload;
        }

        private static void GetHeroHandles(string setwebsite)
        {
            if (setwebsite == "opgg")
            {
                foreach (OPGGLIVE.Info infoloading in OPGGLIVE.Ranks)
                {
                    Obj_AI_Hero herohandle = HeroManager.AllHeroes.Find(o => o.Name.ToLower().Equals(infoloading.Name.ToLower()));
                    if (herohandle != null)
                    {
                        infoloading.herohandle = herohandle;
                    }
                }
                return;
            }

            if (setwebsite == "lolnexus")
            {
                foreach (Lolnexus.Infoloading infoloading in Lolnexus.Ranksloading)
                {
                    infoloading.herohandle = HeroManager.AllHeroes.Find(o => o.Name.ToLower().Equals(infoloading.Name.ToLower()));
                }
                return;
            }
            if (setwebsite == "lolskill")
            {
                foreach (LolSkill.Infoloading infoloading in LolSkill.Ranksloading)
                {
                    infoloading.herohandle = HeroManager.AllHeroes.Find(o => o.Name.ToLower().Equals(infoloading.Name.ToLower()));
                    LolSkill.Ranksloading.Add(infoloading);
                }
                return;
            }

        }


        private static void Drawsprite(Bitmap bitmap, Vector2 position, Vector2 position2)
        {
            _posbutton = GetScaledVector(_posbutton, _scalesprites);

            var loadnewsprite = new Render.Sprite(bitmap, position)
            {
                Scale = _scalesprites,
                Color = new ColorBGRA(255f, 255f, 255f, 0.7f),
                Position = position2,
                VisibleCondition = sender => !disabletext
            };

            loadnewsprite.Show();
            loadnewsprite.Add(0);
        }

        private static Vector2 Newspriteposition(int x, int y)
        {
            return new Vector2(x, y);
        }

        private static Vector2 GetPosition(int width, int height)
        {
            return new Vector2(Drawing.Width / 2f - width / 2f, height);
        }

        private static Vector2 GetScaledVector(Vector2 vector, Vector2 scale)
        {
            return Vector2.Multiply(scale, vector);
        }

        private static Render.Sprite loadbutton()
        {
            _posbutton = GetScaledVector(_posbutton, _scaleicon);

            var loadbutton = new Render.Sprite(Resources.elosharp, _posbutton)
            {
                Scale = _scaleicon,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            loadbutton.Position = GetPosition(loadbutton.Width - 600, 13);

            loadbutton.Show();
            loadbutton.Add(0);

            return loadbutton;
        }

        private static bool mouseonload()
        {
            _posbutton = GetScaledVector(_posbutton, _scaleicon);

            var loadbutton = new Render.Sprite(Resources.elosharp, _posbutton)
            {
                Scale = _scaleicon,
                Color = new ColorBGRA(255f, 255f, 255f, 20f)
            };
            Vector2 pos = Utils.GetCursorPos();
            Vector2 lsharpbutton = GetPosition(loadbutton.Width - 600, 13);

            return ((pos.X >= lsharpbutton.X) && pos.X <= (lsharpbutton.X + loadbutton.Width) && pos.Y >= lsharpbutton.Y &&
                    pos.Y <= (lsharpbutton.Y + loadbutton.Height));
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if ((args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN) && mouseonload() && !Misc.Config.Item("OnlyKeyShow").GetValue<bool>())
            {
                if (!disabletext)
                {
                    // Game.PrintChat("Disabling");
                    disabletext = true;
                }

                else if (disabletext)
                {
                    // Game.PrintChat("Showing now");
                    disabletext = false;
                }

            }
        }



        private static Render.Sprite loadbackground()
        {
            _posbackground = GetScaledVector(_posbackground, _scalebackground);
            float visibility = ((Misc.Config.Item("opacity").GetValue<Slider>().Value) / 100f);

            var loadbackground = new Render.Sprite(Resources.blacky, _posbackground)
            {
                Scale = _scalebackground,
                Color = new ColorBGRA(255f, 255f, 255f, visibility)
            };

            loadbackground.Position = GetPosition(loadbackground.Width, 20);

            loadbackground.VisibleCondition = sender => (!disabletext && !Misc.Config.Item("OnlyKeyShow").GetValue<bool>() || Misc.Config.Item("ShowKey").GetValue<KeyBind>().Active && (Game.Mode != GameMode.Running || !Misc.Config.Item("notingame").GetValue<bool>()));

            loadbackground.Add(0);
            return loadbackground;
        }


        public static void DrawLoading()
        {
            try
            {
                var screenheight = Drawing.Height;
                var screenwidth = Drawing.Width;

                if (Lolnexus.Ranksloading != null && Misc.getsetwebsite() == "lolnexus")
                {
                    foreach (Lolnexus.Infoloading infoloading in Lolnexus.Ranksloading)
                    {
                        Console.WriteLine("Width x Height" + Drawing.Width + "x" + Drawing.Height);
                        int indexof = 0;
                        indexof = Lolnexus.Ranksloading.IndexOf(infoloading);
                        bool isTop = indexof < 5;

                        /*
                        if (Misc.Config.Item("drawicons").GetValue<bool>())
                        {
                            Drawsprite(
                                infoloading.champsprite, Newspriteposition(xformula, ystart),
                                Newspriteposition(xformula - 20, ystart + 5));
                        }
                         * */

                        RenderText(
                            Misc.FormatString(infoloading.Name) + " " + infoloading.seriescheck, isTop, indexof, 15,
                            Color2.White);

                        RenderText(infoloading.soloqrank, isTop, indexof, 35, Misc.rankincolorls(infoloading.soloqrank));

                        RenderText(
                            infoloading.champname + " Games: " + infoloading.champtotal, isTop, indexof, 55, Color2.Red);

                        RenderText("Ranked wins: " + infoloading.rankedwins, isTop, indexof, 75, Color2.White);

                        RenderText("KDA: " + infoloading.kda, isTop, indexof, 95, Color2.Red);

                        RenderText("Masteries: " + infoloading.currentmasteries, isTop, indexof, 115, Color2.White);

                        int runescount = infoloading.currentrunes.Length;

                        RenderText("Runes: " + Misc.StripHTML(infoloading.currentrunes[0]), isTop, indexof, 135, Color2.Red);

                        for (int i = 1; i < runescount; i++)
                        {
                            RenderText(
                                "" + Misc.StripHTML(infoloading.currentrunes[i]), isTop, indexof, 135 + (i * 20),
                                Color2.White);
                        }
                    }

                }

                if (LolSkill.Ranksloading != null && Misc.getsetwebsite() == "lolskill")
                {
                    foreach (LolSkill.Infoloading infoloading in LolSkill.Ranksloading)
                    {
                        int indexof = 0;
                        indexof = LolSkill.Ranksloading.IndexOf(infoloading);
                        bool isTop = indexof < 5;
                        int ystart = isTop ? 15 : 411;
                        int xformula = isTop ? 210 + (indexof * 200) : 210 + ((indexof - 5) * 200);

                        Drawsprite(
                            infoloading.champsprite, Newspriteposition(xformula, ystart),
                            Newspriteposition(xformula - 20, ystart + 5));

                        RenderText(
                            Misc.FormatString(infoloading.Name) + " " + infoloading.seriescheck, isTop, indexof, 15,
                            Color2.White);

                        RenderText(infoloading.soloqrank, isTop, indexof, 35, Misc.rankincolorls(infoloading.soloqrank));

                        RenderText(
                            infoloading.champname + " Games: " + infoloading.champtotal, isTop, indexof, 55, Color2.Red);

                        RenderText("Ranked wins: " + infoloading.rankedwins, isTop, indexof, 75, Color2.White);

                        RenderText("KDA: " + infoloading.kda, isTop, indexof, 95, Color2.Red);

                        RenderText("Masteries: " + infoloading.currentmasteries, isTop, indexof, 115, Color2.White);

                        int runescount = infoloading.currentrunes.Length;

                        RenderText("Runes: " + Misc.StripHTML(infoloading.currentrunes[0]), isTop, indexof, 135, Color2.Red);

                        for (int i = 1; i < runescount; i++)
                        {
                            RenderText(
                                "" + Misc.StripHTML(infoloading.currentrunes[i]), isTop, indexof, 135 + (i * 20),
                                Color2.White);
                        }
                        return;
                    }

                }

                if (OPGGLIVE.Ranks != null && OPGGLIVE.Ranks.Any() && Misc.getsetwebsite() == "opgg")
                {
                    foreach (var hero in OPGGLIVE.Ranks)
                    {
                        int indexof = 0;
                        indexof = OPGGLIVE.Ranks.IndexOf(hero);
                        bool isTop = indexof < 5;
                        int ystart = isTop ? 15 : 411;
                        int xformula = isTop ? 210 + (indexof * 200) : 210 + ((indexof - 5) * 200);

                      //  Drawsprite(
                         //   hero.champsprite, Newspriteposition(xformula, ystart),
                           // Newspriteposition(xformula - 20, ystart + 5));


                        RenderText(
                            Misc.FormatString(hero.Name) + " ", isTop, indexof, 15,
                            Color2.White);

                        RenderText(hero.Ranking, isTop, indexof, 35, Misc.rankincolorls(hero.Ranking));

                        RenderText("Ranked Wins: " + hero.rankedwins, isTop, indexof, 55, Color2.White);

                        RenderText("Ranked Win Ratio: " + hero.rankedwinrate, isTop, indexof, 75, Color2.White);

                        RenderText(hero.champname + " Games: " + hero.champtotal, isTop, indexof, 95, Color2.Red);

                        RenderText("Champ Win Ratio: " + hero.champwinrate, isTop, indexof, 115, Color2.White);

                        RenderText("KDA: " + hero.kda, isTop, indexof, 135, Color2.Red);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                Console.Write("Exception in DrawLoading" + e);
            }
        }


        private static void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
        {
            Sprite.Dispose();
            Text.Dispose();
        }

        private static void DrawingOnOnPostReset(EventArgs args)
        {
            Sprite.OnResetDevice();
            Text.OnResetDevice();
        }

        private static void DrawingOnOnPreReset(EventArgs args)
        {
            Sprite.OnLostDevice();
            Text.OnLostDevice();
        }




        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed || Game.Mode != GameMode.Running)
            {
                return;
            }
            try
            {
                if (Sprite.IsDisposed)
                {
                    return;
                }
                if (Misc.getsetwebsite() == "lolnexus")
                {
                    foreach (Lolnexus.Infoloading infoloading in Lolnexus.Ranksloading.ToList())
                    {
                        var indicator = new Misc.HpBarIndicator { Unit = infoloading.herohandle };
                        X = (int)indicator.Position.X;
                        Y = (int)indicator.Position.Y;
                        var startX = X + 50;
                        var startY = Y - 60;
                        if (Misc.Config.Item("enablerank").GetValue<bool>())
                        {
                            Text.DrawText(null, infoloading.soloqrank, startX + (15 - infoloading.soloqrank.Length * 4) / 2, startY, Misc.ColorRank(infoloading.soloqrank));
                        }
        
                        if (Misc.Config.Item("enablekdaratio").GetValue<bool>())
                        {
                            Text.DrawText(
                                null, "KDA: " + infoloading.kda,
                                startX + (15 - infoloading.soloqrank.Length * 4) / 2, startY + 15, Misc.ColorRank(infoloading.soloqrank));
                        }

                    }
                }

                if (Misc.getsetwebsite() == "lolskill")
                {
                    foreach (LolSkill.Infoloading infoloading in LolSkill.Ranksloading.ToList())
                    {

                        var indicator = new Misc.HpBarIndicator { Unit = infoloading.herohandle };
                        X = (int)indicator.Position.X;
                        Y = (int)indicator.Position.Y;
                        var startX = X + 50;
                        var startY = Y - 60;
                        Text.DrawText(
                            null, infoloading.soloqrank, startX + (15 - infoloading.soloqrank.Length * 4) / 2,
                            startY + 6, Misc.ColorRank(infoloading.soloqrank));
                    }
                }

                if (Misc.getsetwebsite() == "opgg")
                {
                    foreach (OPGGLIVE.Info info in OPGGLIVE.Ranks)
                    {
                        var indicator = new Misc.HpBarIndicator { Unit = info.herohandle };
                        X = (int)indicator.Position.X;
                        Y = (int)indicator.Position.Y;
                        var startX = X + 50;
                        var startY = Y - 70;
                        if (Misc.Config.Item("enablerank").GetValue<bool>())
                        {
                            Text.DrawText(
                                null, info.Ranking, startX + (15 - info.Ranking.Length * 4) / 2, startY + 5,
                                Misc.ColorRank(info.Ranking));
                        }
                        if (Misc.Config.Item("enablewinratio").GetValue<bool>())
                        {
                            Text.DrawText(
                                null, "Ranked: " + info.rankedwinrate + " (" + info.rankedwins + " wins)",
                                startX + (15 - info.Ranking.Length * 4) / 2, startY + 20, Misc.ColorRank(info.Ranking));
                        }
                        if (Misc.Config.Item("enablekdaratio").GetValue<bool>())
                        {
                            Text.DrawText(
                                null, "Champ: " + info.champwinrate + " (" + info.kda + " KDA)",
                                startX + (15 - info.Ranking.Length * 4) / 2, startY + 35, Misc.ColorRank(info.Ranking));
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(@"Error drawing text overheads " + e);
            }
        }


        private static Render.Text RenderText(string text, bool isTop, int indexof, double toffset, Color2 color)
        {
            double ystart = isTop ? 15 / 768 * Drawing.Height + (toffset / 768 * Drawing.Height) : 411 / 768 * Drawing.Height + (toffset / 768 * Drawing.Height);
            const int size = 20;
            double xformula = isTop ? (210f / 1366) * Drawing.Width + ((indexof * 200 / 1366) * Drawing.Width) : 210 / 1366 * Drawing.Width + ((indexof - 5) * 200 / 1366 * Drawing.Width);
            
            var texty = new Render.Text(text, (int) xformula, (int) ystart, size, color);

            texty.VisibleCondition = sender => (!disabletext && !Misc.Config.Item("OnlyKeyShow").GetValue<bool>() || Misc.Config.Item("ShowKey").GetValue<KeyBind>().Active && (Game.Mode != GameMode.Running || !Misc.Config.Item("notingame").GetValue<bool>()));
            
            texty.Add(1);
            return texty;
        }


        public static void DoDrawings()
        {
            DrawLoading();
            background = loadbackground();
            buttonicon = loadbutton();
        }


        public static void PrepareDrawing()
        {
            Sprite = new Sprite(Drawing.Direct3DDevice);
            Text = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Calibri",
                    Height = 20,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default,
                });
        }


    }
}