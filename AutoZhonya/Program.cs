using System;
using System.Linq;
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
        private static readonly Items.Item Seraph = new Items.Item(3040, 0);
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
            miscmenu.AddItem(new MenuItem("enableseraph", "Use Seraph").SetValue(true));
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
            Game.OnUpdate += GameUpdate;
        }

        static void GameUpdate(EventArgs args)
        {
            if (Menu.Item("enablehpzhonya").GetValue<bool>() && zhonyaready())
            {
                BuffDetector();
                if (Player.Health < Player.MaxHealth * 0.35 && Player.CountEnemiesInRange(300) >= 1 && (!SpellSlot.Q.IsReady() || !SpellSlot.W.IsReady() || !SpellSlot.E.IsReady() || !SpellSlot.R.IsReady()) && Player.Mana < Player.MaxMana * 80) {
                    Zhonya.Cast();
                }
            }
        }

        static void SpellDetector(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            // return if ally or non hero spell
        
            if (Player.IsDead || (!zhonyaready() && !seraphready()) || !(sender is Obj_AI_Hero) || sender.IsAlly || !args.Target.IsMe || args.SData.IsAutoAttack() || sender.IsMe)
            {
                return;
            }
            DangerousSpells.Data Spellinfo = null;
            try
            {
                Spellinfo = DangerousSpells.GetByName(args.SData.Name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e + e.StackTrace);
            }
          
      
            if (Spellinfo != null)
            {
                Console.WriteLine(Spellinfo.DisplayName);
                if (Menu.Item("Enabled" + Spellinfo.DisplayName).GetValue<bool>())
                {
                    Game.PrintChat("Attempting to Zhonya: " + args.SData.Name);
                    var delay = Spellinfo.BaseDelay * 1000;
                    if (zhonyaready())
                    {
                        Utility.DelayAction.Add((int) delay, () => Zhonya.Cast());
                        return;
                    }
                    if (seraphready() && Menu.Item("enableseraph").GetValue<bool>())
                    {
                        Utility.DelayAction.Add((int) delay, () => Seraph.Cast());
                    }
                    return;
                }
            }

            if (Menu.Item("enablehpzhonya").GetValue<bool>() && (zhonyaready() || seraphready()))
                {
                    var calcdmg = sender.GetSpellDamage(Player, args.SData.Name);
                    var remaininghealth = Player.Health - calcdmg;
                    var slidervalue = Menu.Item("minspelldmg").GetValue<Slider>().Value / 100f;
                    var hptozhonya = Menu.Item("hptozhonya").GetValue<Slider>().Value;
                    var remaininghealthslider = Menu.Item("remaininghealth").GetValue<Slider>().Value / 100f;
                    if ((calcdmg / Player.Health) >= slidervalue || Player.HealthPercent <= hptozhonya || remaininghealth <= remaininghealthslider * Player.Health)
                    {
                        Console.WriteLine("Attempting to Zhonya because incoming spell costs " + calcdmg / Player.Health
                            + " of our health.");
                        if (zhonyaready())
                        {
                            Zhonya.Cast();
                            return;
                        }
                        if (seraphready() && Menu.Item("enableseraph").GetValue<bool>())
                        {
                            Seraph.Cast();
                        }
                    }
                }
        }

        private static bool delayingzhonya;

        static void BuffDetector()
        {
            foreach (var buff in Player.Buffs)
            {
                var isbadbuff = DangerousBuffs.ScaryBuffs.ContainsKey(buff.Name);

                if (isbadbuff)
                {
                     var bufftime = DangerousBuffs.ScaryBuffs[buff.Name];
                    if (zhonyaready())
                    {
                        if (bufftime.Equals(0))
                        {
                            Zhonya.Cast();
                            return;
                        }
                        delayingzhonya = true;
                        Utility.DelayAction.Add(
                            (int) bufftime, () =>
                            {
                                Zhonya.Cast();
                                delayingzhonya = false;
                            });
                            return;
                     }

                    if (seraphready() && Menu.Item("enableseraph").GetValue<bool>() && !delayingzhonya)
                    {
                        if (bufftime.Equals(0))
                        {
                            Seraph.Cast();
                            return;
                        }
                        Utility.DelayAction.Add((int) bufftime, () => Seraph.Cast());
                    }
                }
            }
        }


        public static bool zhonyaready()
        {
            return Zhonya.IsReady();
        }

        public static bool seraphready()
        {
            return Seraph.IsReady();
        }
    }



}

