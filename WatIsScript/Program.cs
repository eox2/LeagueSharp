using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace WatIsScript
{

    // Simple utility assembly that supports delaying spells and setting a minimum time before you can attack a new target.
   
    class Program
    {
        private static Obj_AI_Hero Player = null;
        private static float timecast = 0;
        private static Menu Config;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            Config = DoConfig();
            Player = ObjectManager.Player;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        private static Menu DoConfig()
        {
            var menu = new Menu("WatIsScript", "WatIsScript", true);
            var humanizerenabled = menu.AddItem(new MenuItem("humanizerenabled", "Humanizer").SetValue(true));
            var antiswitchenabled = menu.AddItem(new MenuItem("antiswitchenabled", "Anti target switch").SetValue(true));

            humanizerenabled.Permashow();
            antiswitchenabled.Permashow();


            menu.AddItem(new MenuItem("mindelay", "Minimum Delay between switching targets").SetValue(new Slider(500, 0, 3000)));

            var spellssubmenu = new Menu("Enabled Spells", "spells");
            spellssubmenu.AddItem(new MenuItem("Spells.Q", "Q").SetValue(true));
            menu.AddItem(new MenuItem("Qdelay", "Q delay").SetValue(new Slider(500, 0, 1500)));
            spellssubmenu.AddItem(new MenuItem("Spells.W", "W").SetValue(true));
            menu.AddItem(new MenuItem("Wdelay", "W delay").SetValue(new Slider(500, 0, 1500)));
            spellssubmenu.AddItem(new MenuItem("Spells.E", "E").SetValue(true));
            menu.AddItem(new MenuItem("Edelay", "E delay").SetValue(new Slider(500, 0, 1500)));
            spellssubmenu.AddItem(new MenuItem("Spells.R", "R").SetValue(true));
            menu.AddItem(new MenuItem("Rdelay", "R delay").SetValue(new Slider(500, 0, 1500)));

            menu.AddSubMenu(spellssubmenu);

            menu.AddToMainMenu();

            return menu;
        }

        static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender == null || !sender.Owner.IsMe) { 
                return; 
            }
            var antiswitch = Config.Item("antiswitchenabled").GetValue<bool>();
            var useforthis = Config.Item("Spells." + args.Slot).GetValue<bool>();
            var target = args.Target as Obj_AI_Hero;
            var mindelay = Config.Item("mindelay").GetValue<Slider>().Value;

            if (antiswitch && target != null)
            {
                if (target != Player.LastCastedSpellTarget() && Utils.TickCount - Player.LastCastedSpellT() <= mindelay)
                {
                    args.Process = false;
                    return;
                }
            }

            var humanizerenabled = Config.Item("humanizerenabled").GetValue<bool>();
            var delayforthis = Config.Item(args.Slot + "delay").GetValue<Slider>().Value;
            if (humanizerenabled && useforthis) 
            {
                if (Utils.TickCount - Player.LastCastedSpellT() <= delayforthis)
                {
                    args.Process = false;
                } 
            }
        }
    }
}
