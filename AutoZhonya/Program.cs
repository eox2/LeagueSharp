using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoZhonya
{
    /*
     * To do
     * Add Buffs for not detectable spells
     */
    class Program
    {

        private static Obj_AI_Hero Player;
        private static readonly Items.Item Zhonya = new Items.Item(3157, 0);
        private static readonly string Version = "1.0";
        public static Menu Menu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            Game.PrintChat("<font color=\"#12FA54\"><b>AutoZhonya v." + Version + " by Seph</font></b>");
            Player = ObjectManager.Player;
            EventSubscriptions();
            Menu = new Menu("AutoZhonya", "AutoZhonya", true);
            var spellmenu = new Menu("Spells", "Spells");
            var miscmenu = new Menu("Misc", "Misc");
            miscmenu.AddItem(new MenuItem("enablehpzhonya", "Zhonya when low HP").SetValue(true));
            miscmenu.AddItem(new MenuItem("hptozhonya", "HP % to Zhonya")).SetValue(new Slider(25, 0, 100));
            miscmenu.AddItem(new MenuItem("minspelldmg", "Spell Damage % (non dangerous)")).SetValue(new Slider(45, 0, 100));
            miscmenu.AddItem(new MenuItem("remaininghealth", "Remaining Health %")).SetValue(new Slider(15, 0, 100));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    foreach (var spell in DangerousSpells.AvoidableSpells)
                    {
                        if (spell.Source.ToLower() == hero.ChampionName.ToLower())
                        {
                            var subMenu = new Menu(spell.DisplayName, spell.DisplayName);

                            subMenu.AddItem(new MenuItem("Enabled" + spell.DisplayName, "Enabled").SetValue(true));
                         //   subMenu.AddItem(new MenuItem("spelldelay", "Spell Delay")).SetValue(new Slider(0, 0, 2000));

                            spellmenu.AddSubMenu(subMenu);
                        }
                    }
                }

            }
            Menu.AddSubMenu(spellmenu);
            Menu.AddSubMenu(miscmenu);
            Menu.AddToMainMenu();


        }

        static void EventSubscriptions()
        {
            Obj_AI_Base.OnProcessSpellCast += SpellDetector;
            Game.OnUpdate += BuffDetector;
        }

        static void SpellDetector(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            // return if ally or non hero spell
        
            if (Player.IsDead || sender.IsAlly || !(sender is Obj_AI_Hero) || !args.Target.IsMe || args.SData.Name.ToLower().Contains("basicattack"))
            {
                return;
            }
               // Game.PrintChat(args.SData.Name + " Detected");
                var Spellinfo = DangerousSpells.GetByName2(args.SData.Name);

                if (Spellinfo != null && /*zhonyaready() && */
                    (Menu.Item("Enabled" + Spellinfo.DisplayName).GetValue<bool>()))
                {
                    Game.PrintChat("Attempting to Zhonya: " + args.SData.Name);
                    var delay = Spellinfo.BaseDelay * 1000;
                    Utility.DelayAction.Add((int) delay, () => Zhonya.Cast());
                    return;
                }

                if (Menu.Item("enablehpzhonya").GetValue<bool>() && zhonyaready())
                {
                    var incomingspelldmg = sender.GetSpellDamage(Player, args.SData.Name);
                    var calcdmg = Damage.CalcDamage(
                        sender, Player, sender.GetDamageSpell(Player, args.SData.Name).DamageType, incomingspelldmg);
                    var remaininghealth = Player.Health - calcdmg;
                    var slidervalue = Menu.Item("minspelldmg").GetValue<Slider>().Value / 100f;
                    var hptozhonya = Menu.Item("hptozhonya").GetValue<Slider>().Value;
                    var remaininghealthslider = Menu.Item("remaininghealth").GetValue<Slider>().Value / 100f;
                    if ((
                        calcdmg / Player.Health) >= slidervalue || Player.HealthPercent <= hptozhonya || remaininghealth <= remaininghealthslider * Player.Health)
                    {
                        Console.WriteLine("Attempting to Zhonya because incoming spell costs " + calcdmg / Player.Health
                            + " of our health.");
                        Zhonya.Cast();
                    }
                }
            

        }

        static void BuffDetector(EventArgs args)
        {

        }


        public static bool zhonyaready()
        {
            return Items.HasItem(3157) && Zhonya.IsReady();
        }
    }



}

