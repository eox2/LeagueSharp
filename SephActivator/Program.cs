using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
//using Color = System.Drawing.Color;
namespace SephElise
{
    class Program
    {
        private const string ChampionName = "";

        private static Menu Config;
        public static Items.Item DFG; //Deathfire Grasp
        private static Items.Item HDR; // hydra
        private static Items.Item BRK; // bork
        private static Items.Item RNO; // randuins
        private static Items.Item BWC; // Bilgewater
        private static Items.Item ZNY; // Zhonyas
        private static Items.Item BOC; // Banner of Command
        private static Items.Item FOM; // Face of Mountain
        private static Items.Item FCQ; // Frost Queen's Claim
        private static Items.Item HXG; // Hextech gunblade
        private static Items.Item LIS; // Locket of Iron Solari
        private static Items.Item MRS; // Merc Scim
        private static Items.Item QSS; // Quicksilver sash
        private static Items.Item SRE; // Seraph's Embrace
        private static Items.Item YMS; // Youmu
        private static Items.Item SOTD; // Sword of Divine
        private static Items.Item HPot; // Health Potion
        private static Items.Item MPot; // Mana Potion
        private static Items.Item ElixB; // Brilliance Elixir
        private static Items.Item ElixF; // Fortitude Elixir
        private static Items.Item Cryst; // Crystalline flask
        private static Items.Item PinkW; // Pink Ward
        private static Items.Item Sweeper; // Sweeper





        private static SpellSlot IgniteSlot;

        private static Items.Item YOU;

        private static Obj_AI_Hero Player;
        private static bool HumanForm;
        private static bool SpiderForm;

        public static Menu SEPH;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {


            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 950f);
            E = new Spell(SpellSlot.E, 1075f);
            QS = new Spell(SpellSlot.Q, 475f);
            WS = new Spell(SpellSlot.W, 0);
            ES = new Spell(SpellSlot.E, 750f);
            R = new Spell(SpellSlot.R, 0);
            W.SetSkillshot(0.25f, 100f, 1000, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 55f, 1300, true, SkillshotType.SkillshotLine);

            HDR = new Items.Item(3074, 175f);
            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 185f);
            DFG = new Items.Item(3128, 750f);
        }
    }
}
