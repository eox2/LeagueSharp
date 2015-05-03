using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SwitchSkin
{
    class Program
    {
        private static Menu menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameLoad;
            Obj_AI_Hero.OnFloatPropertyChange += FloatPropertyChange;
        }

        static void GameLoad(EventArgs argss)
        {
             menu = new Menu("SwitchSkins", "Skinswitcher", true);

            foreach (var hero in HeroManager.AllHeroes)
            {
                var currenthero = hero;
                var selectskin = menu.AddItem(new MenuItem("skin." + hero.Name, hero.ChampionName + " (" + hero.Name + ")").SetValue(new Slider(0, 0, 10)));

                if (hero.Name == ObjectManager.Player.Name)
                {
                    currenthero.SetSkin(currenthero.ChampionName, selectskin.GetValue<Slider>().Value);
                }

                selectskin.ValueChanged += delegate
                {
                    try
                    {
                        currenthero.SetSkin(currenthero.ChampionName, selectskin.GetValue<Slider>().Value);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }
                };
            }
            menu.AddToMainMenu();
        }

        static void FloatPropertyChange(GameObject sender, GameObjectFloatPropertyChangeEventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || args.Property != "mHP")
            {
                return; 
            }
            var hero = (Obj_AI_Hero) sender;
 
            if (args.OldValue.Equals(args.NewValue) && args.NewValue.Equals(hero.MaxHealth) && !hero.IsDead)
            {
                hero.SetSkin(hero.ChampionName, menu.Item("skin." + hero.Name).GetValue<Slider>().Value);
            }
        }
    }
}


