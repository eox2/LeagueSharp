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
                var newselect = menu.AddItem(new MenuItem("skin." + hero.Name, hero.ChampionName + " (" + hero.Name + ")").SetValue(new StringList(new[] { "Skin 0", "Skin 1", "Skin 2", "Skin 3", "Skin 4", "Skin 5", "Skin 6", "Skin 7", "Skin 8", "Skin 9", "Skin 10" }, 0)));

                if (currenthero.Name == ObjectManager.Player.Name)
                {
                    currenthero.SetSkin(currenthero.ChampionName, GetSkinNumber(currenthero));
                }

                newselect.ValueChanged += delegate
                {
                   currenthero.SetSkin(currenthero.ChampionName, GetSkinNumber(currenthero));
                };
            }
            menu.AddToMainMenu();
        }

        public static int GetSkinNumber(Obj_AI_Hero hero)
        {
            var strings = menu.Item("skin." + hero.Name).GetValue<StringList>();
            switch (strings.SList[strings.SelectedIndex])
            {
                case "Skin 0":
                    return 0;
                case "Skin 1":
                    return 1;
                case "Skin 2":
                    return 2;
                case "Skin 3":
                    return 3;
                case "Skin 4":
                    return 4;
                case "Skin 5":
                    return 5;
                case "Skin 6":
                    return 6;
                case "Skin 7":
                    return 7;
                case "Skin 8":
                    return 8;
                case "Skin 9":
                    return 9;
                case "Skin 10":
                    return 10;
                default:
                    Console.WriteLine(("def"));
                    return 0;
            }
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
                hero.SetSkin(hero.ChampionName, GetSkinNumber(hero));
            }
        }
    }
}


