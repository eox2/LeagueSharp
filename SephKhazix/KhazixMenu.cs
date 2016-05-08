using LeagueSharp;
using LeagueSharp.Common;

namespace SephKhazix
{
    class KhazixMenu
    {
        internal Menu menu;
        internal Orbwalking.Orbwalker Orbwalker;

        public KhazixMenu()
        {
            menu = new Menu("SephKhazix", "SephKhazix", true);

            var ow = menu.AddSubMenu("Orbwalking");
            Orbwalker = new Orbwalking.Orbwalker(ow);

            //Harass
            var harass = menu.AddSubMenu("Harass");
            harass.AddBool("UseQHarass", "Use Q");
            harass.AddBool("UseWHarass", "Use W");
            harass.AddBool("Harass.AutoWI", "Auto-W immobile");
            harass.AddBool("Harass.AutoWD", "Auto W");
            harass.AddKeyBind("Harass.Key", "Harass key", "H".ToCharArray()[0], KeyBindType.Toggle).Permashow();
            harass.AddBool("Harass.InMixed", "Harass in Mixed Mode", false);
            harass.AddSList("Harass.WHitchance", "W Hit Chance", new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1);


            //Combo
            var combo = menu.AddSubMenu("Combo");
            combo.AddBool("UseQCombo", "Use Q");
            combo.AddBool("UseWCombo", "Use W");
            combo.AddSList("WHitchance", "W Hit Chance", new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1);
            combo.AddBool("UseECombo", "Use E");
            combo.AddBool("UseEGapclose", "Use E To Gapclose for Q");
            combo.AddBool("UseEGapcloseW", "Use E To Gapclose For W");
            combo.AddBool("UseRGapcloseW", "Use R after long gapcloses");
            combo.AddBool("UseRCombo", "Use R");
            combo.AddBool("UseItems", "Use Items");

            //Farm
            var farm = menu.AddSubMenu("Farm");
            farm.AddBool("UseQFarm", "Use Q");
            farm.AddBool("UseEFarm", "Use E");
            farm.AddBool("UseWFarm", "Use W");
            farm.AddSlider("Farm.WHealth", "Health % to use W", 80, 0, 100);
            farm.AddBool("UseItemsFarm", "Use Items").SetValue(true);

            //Kill Steal
            var ks = menu.AddSubMenu("KillSteal");
            ks.AddBool("Kson", "Use KillSteal");
            ks.AddBool("UseQKs", "Use Q");
            ks.AddBool("UseWKs", "Use W");
            ks.AddBool("UseEKs", "Use E");
            ks.AddBool("Ksbypass", "Bypass safety checks for E KS", false);
            ks.AddBool("UseEQKs", "Use EQ in KS");
            ks.AddBool("UseEWKs", "Use EW in KS");
            ks.AddBool("UseTiamatKs", "Use items");
            ks.AddSlider("Edelay", "E Delay (ms)", 0, 0, 300);
            ks.AddBool("UseIgnite", "Use Ignite");

            var safety = menu.AddSubMenu("Safety Menu");
            safety.AddBool("Safety.Enabled", "Enable Safety Checks");
            safety.AddKeyBind("Safety.Override", "Safety Override Key", 'T', KeyBindType.Press).Permashow();
            safety.AddBool("Safety.autoescape", "Use E to get out when low");
            safety.AddBool("Safety.CountCheck", "Min Ally ratio to Enemies to jump");
            safety.AddItem(new MenuItem("Safety.Ratio", "Ally:Enemy Ratio (/5)").SetValue(new Slider(1, 0, 5)));
            safety.AddBool("Safety.TowerJump", "Avoid Tower Diving");
            safety.AddSlider("Safety.MinHealth", "Healthy %", 15, 0, 100);
            safety.AddBool("Safety.noaainult", "No Autos while Stealth", false);

            //Double Jump
            var djump = menu.AddSubMenu("Double Jumping");
            djump.AddBool("djumpenabled", "Enabled");
            djump.AddSlider("JEDelay", "Delay between jumps", 250, 250, 500);
            djump.AddSList("jumpmode", "Jump Mode", new[] { "Default (jumps towards your nexus)", "Custom - Settings below" }, 0);
            djump.AddBool("save", "Save Double Jump Abilities", false);
            djump.AddBool("noauto", "Wait for Q instead of autos", false);
            djump.AddBool("jcursor", "Jump to Cursor (true) or false for script logic");
            djump.AddBool("secondjump", "Do second Jump");
            djump.AddBool("jcursor2", "Second Jump to Cursor (true) or false for script logic");
            djump.AddBool("jumpdrawings", "Enable Jump Drawinsg");


            //Drawings
            var draw = menu.AddSubMenu("Drawings");
            draw.AddBool("Drawings.Disable", "Disable all", true);
            draw.AddCircle("DrawQ", "Draw Q", 0, System.Drawing.Color.White);
            draw.AddCircle("DrawW", "Draw W", 0, System.Drawing.Color.Red);
            draw.AddCircle("DrawE", "Draw E", 0, System.Drawing.Color.Green);


            //Debug
            var debug = menu.AddSubMenu("Debug");
            debug.AddBool("Debugon", "Enable Debugging");

            menu.AddToMainMenu();
        }

        internal bool GetBool(string name)
        {
            return menu.Item(name).GetValue<bool>();
        }

        internal bool GetKeyBind(string name)
        {
            return menu.Item(name).GetValue<KeyBind>().Active;
        }

        internal float GetSliderFloat(string name)
        {
            return menu.Item(name).GetValue<Slider>().Value;
        }

        internal int GetSlider(string name)
        {
            return menu.Item(name).GetValue<Slider>().Value;
        }

        internal int GetSL(string name)
        {
            return menu.Item(name).GetValue<StringList>().SelectedIndex;
        }

        internal string GetSLVal(string name)
        {
            return menu.Item(name).GetValue<StringList>().SelectedValue;
        }

        internal Circle GetCircle(string name)
        {
            return menu.Item(name).GetValue<Circle>();
        }

        internal HitChance GetHitChance(string search)
        {
            var hitchance = menu.Item(search).GetValue<StringList>();
            switch (hitchance.SList[hitchance.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }
    }
}
