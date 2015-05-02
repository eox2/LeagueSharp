using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SwitchSkin
{
    class Program
    {

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameLoad;
        }

        static void GameLoad(EventArgs argss)
        {
            Game.PrintChat("SwitchSkins Loaded");

            var Menu = new Menu("SwitchSkins", "Skinswitcher", true);
            foreach (var hero in HeroManager.AllHeroes)
            {
                var currenthero = hero;
                var selectskin = Menu.AddItem(new MenuItem("skin." + hero.Name, hero.ChampionName + " (" + hero.Name + ")").SetValue(new Slider(0, 0, 10)));
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
            Menu.AddToMainMenu();
        }
    }
}
