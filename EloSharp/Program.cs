﻿using System;
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


namespace EloSharp
{
    public class EloSharp
    {

        public static string rank = "unknown";


        public static List<Info> Ranks { get; set; }

        public class Info
        {
            public String Name { get; set; }
            public String Ranking { get; set; }
            public String lpamount { get; set; }
            public Obj_AI_Hero herohandle { get; set; }
        }

        public static EloSharp elosharp;

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



            foreach (Info info in Ranks)
            {


                if ((!info.herohandle.IsDead) && (info.herohandle.IsVisible))
                {
                   // var wts = Drawing.WorldToScreen(info.herohandle.Position);
                    var indicator = new HpBarIndicator { Unit = info.herohandle };
                    var Xee = (int)indicator.Position.X + 80;
                    var Yee = (int)indicator.Position.Y;


                   // Drawing.DrawText(wts.X, wts.Y, Color.Brown, "x");
                    /* Unneccesary at the moment
                    if (info.Ranking.ToLower().Contains("unknown"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth("Not Registered", font) / 2), Yee - 50, Color.White, "Not Registered");
                    }
                     * */
                    if (info.Ranking.ToLower().Contains("unranked"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking, font) / 2), Yee - 50, Color.White, "Unranked");
                    }
                    if (info.Ranking.Contains("Bronze"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.SandyBrown, info.Ranking + " (" + info.lpamount + ")");
                    }
                    if (info.Ranking.Contains("Silver"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Silver, info.Ranking + " (" + info.lpamount + ")");
                    }
                    if (info.Ranking.Contains("Gold"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Gold, info.Ranking + " (" + info.lpamount + ")");
                    }
                    if (info.Ranking.Contains("Platinum"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        //   Drawing.DrawText(wts.X - (TextWidth(info.Ranking, font) / 2), wts.Y - 160, Color.Cyan, info.Ranking);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Cyan, info.Ranking + " (" + info.lpamount + ")");
                    }
                    if (info.Ranking.Contains("Diamond"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.DeepSkyBlue, info.Ranking + " (" + info.lpamount + ")");
                    }
                    if (info.Ranking.Contains("Master"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.LimeGreen, info.Ranking + " (" + info.lpamount + ")");
                    }
                    if (info.Ranking.Contains("Challenger"))
                    {
                        Font font = new Font("Calibri", 13.5F);
                        Drawing.DrawText(Xee - (TextWidth(info.Ranking + " (" + info.lpamount + ")", font) / 2), Yee - 50, Color.Orange, info.Ranking + " (" + info.lpamount + ")");
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

            new System.Threading.Thread(() =>
            {
                elosharp = new EloSharp();

            }).Start();
            Drawing.OnDraw += OnDraw;
        }





        public EloSharp()
        {

            Ranks = new List<Info>();
            // foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                // Game.PrintChat(hero.Name);
                //Game.PrintChat(LeagueSharp.Game.Region);

                Info info = new Info();
                //
                if (Game.Region.ToLower().Contains("na"))
                {
                    String htmlcode = new WebClient().DownloadString("http://na.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
               
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }

                if (Game.Region.ToLower().Contains("euw"))
                {
                    String htmlcode = new WebClient().DownloadString("http://euw.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }

                if (Game.Region.ToLower().Contains("eun"))
                {
                    String htmlcode = new WebClient().DownloadString("http://eune.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }
                if (Game.Region.ToLower().Contains("la1"))
                {
                    String htmlcode = new WebClient().DownloadString("http://lan.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }

                if (Game.Region.ToLower().Contains("tr"))
                {
                    String htmlcode = new WebClient().DownloadString("http://tr.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }
                if (Game.Region.ToLower().Contains("oc1"))
                {
                    String htmlcode = new WebClient().DownloadString("http://oce.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }
                if (Game.Region.ToLower().Contains("br"))
                {
                    String htmlcode = new WebClient().DownloadString("http://br.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }
                if (Game.Region.ToLower().Contains("ru"))
                {
                    String htmlcode = new WebClient().DownloadString("http://ru.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }
                if (Game.Region.ToLower().Contains("la2"))
                {
                    String htmlcode = new WebClient().DownloadString("http://las.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = "";
                        Ranks.Add(info);
                    }
                }
                if (Game.Region.ToLower().Contains("kr"))
                {
                    String htmlcode = new WebClient().DownloadString("http://kr.op.gg/summoner/userName=" + hero.Name);


                    if (htmlcode.ToString().Contains("tierRank") && htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = rank;
                        info.lpamount = playerlp.ToString();
                        Ranks.Add(info);
                    }
                    if (htmlcode.ToString().Contains("unranked") && !htmlcode.ToString().Contains("leaguePoints"))
                    {
                        Match htmlmatchrank = new Regex(@"\<span class=\""tierRank\"">(.*?)</span>").Matches(htmlcode)[0];
                        //Match htmlmatchlp = new Regex(@"\<span class=\""leaguePoints\"">(.*?)</span>").Matches(htmlcode)[0];
                        Match playerrank = new Regex(htmlmatchrank.Groups[1].ToString()).Matches(htmlcode)[0];
                        // Match playerlp = new Regex(htmlmatchlp.Groups[1].ToString()).Matches(htmlcode)[0];
                        rank = playerrank.ToString();
                        info.Name = hero.Name;
                        info.herohandle = hero;
                        info.Ranking = "Unranked";
                        info.lpamount = "0";
                        Ranks.Add(info);
                    }
                    if (!htmlcode.ToString().Contains("tierRank"))
                    {
                        //Game.PrintChat("This hero is not registered");

                        rank = "unknown";
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


