#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 AutoLeveler.cs is part of SFXUtility.
 
 SFXUtility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 SFXUtility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with SFXUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace SFXUtility.Feature
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class AutoLeveler : Base
    {
        #region Constructors

        public AutoLeveler(IContainer container) : base(container)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get { return Menu != null && Menu.Item(Name + "Enabled").GetValue<bool>(); }
        }

        public override string Name
        {
            get { return "Auto Leveler"; }
        }

        #endregion

        #region Methods

        private MenuInfo GetMenuInfoByPriority(int priority)
        {
            return new List<MenuInfo>
            {
                new MenuInfo
                {
                    Slot = SpellSlot.Q,
                    Value = Menu.Item(Name + "PatternQ").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.W,
                    Value = Menu.Item(Name + "PatternW").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.E,
                    Value = Menu.Item(Name + "PatternE").GetValue<Slider>().Value
                }
            }.OrderBy(x => x.Value).Reverse().First(s => s.Value == priority);
        }

        private IEnumerable<MenuInfo> GetOrderedList()
        {
            return new List<MenuInfo>
            {
                new MenuInfo
                {
                    Slot = SpellSlot.Q,
                    Value = Menu.Item(Name + "PatternQ").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.W,
                    Value = Menu.Item(Name + "PatternW").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.E,
                    Value = Menu.Item(Name + "PatternE").GetValue<Slider>().Value
                }
            }.OrderBy(x => x.Value).Reverse().ToList();
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);

                Menu = new Menu(Name, Name);

                var patternMenu = new Menu("Pattern", Name + "Pattern");
                patternMenu.AddItem(new MenuItem(Name + "PatternEarly", "Early Pattern").SetValue(new StringList(new[]
                {
                    "x 2 3 1",
                    "x 2 1",
                    "x 1 3",
                    "x 1 2"
                })));
                patternMenu.AddItem(new MenuItem(Name + "PatternQ", "Q").SetValue(new Slider(3, 3, 1)));
                patternMenu.AddItem(new MenuItem(Name + "PatternW", "W").SetValue(new Slider(1, 3, 1)));
                patternMenu.AddItem(new MenuItem(Name + "PatternE", "E").SetValue(new Slider(2, 3, 1)));

                Menu.AddSubMenu(patternMenu);

                Menu.AddItem(new MenuItem(Name + "OnlyR", "Only R").SetValue(false));
                Menu.AddItem(new MenuItem(Name + "Enabled", "Enabled").SetValue(false));

                BaseMenu.AddSubMenu(Menu);

                CustomEvents.Unit.OnLevelUp += OnLevelUp;

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                if (!sender.IsValid || !sender.IsMe)
                    return;

                Utility.Map.MapType map = Utility.Map.GetMap()._MapType;
                var points = args.RemainingPoints;

                if ((map == Utility.Map.MapType.SummonersRift || map == Utility.Map.MapType.TwistedTreeline) &&
                    args.NewLevel <= 1)
                    return;

                if ((map == Utility.Map.MapType.CrystalScar || map == Utility.Map.MapType.HowlingAbyss) &&
                    args.NewLevel <= 3)
                    return;

                if (args.NewLevel == 6 || args.NewLevel == 11 || args.NewLevel == 16)
                {
                    ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.R);
                    points--;
                }

                if (Menu.Item(Name + "OnlyR").GetValue<bool>())
                    return;

                var patternIndex = Menu.Item(Name + "PatternEarly").GetValue<StringList>().SelectedIndex;
                MenuInfo mf = null;
                switch (args.NewLevel)
                {
                    case 2:
                        switch (patternIndex)
                        {
                            case 0:
                            case 1:
                                mf = GetMenuInfoByPriority(2);
                                break;
                            case 2:
                            case 3:
                                mf = GetMenuInfoByPriority(1);
                                break;
                        }
                        break;
                    case 3:
                        switch (patternIndex)
                        {
                            case 0:
                            case 2:
                                mf = GetMenuInfoByPriority(3);
                                break;
                            case 1:
                                mf = GetMenuInfoByPriority(1);
                                break;
                            case 3:
                                mf = GetMenuInfoByPriority(2);
                                break;
                        }
                        break;
                    case 4:
                        switch (patternIndex)
                        {
                            case 0:
                                mf = GetMenuInfoByPriority(1);
                                break;
                        }
                        break;
                }
                if (mf != null && points > 0)
                {
                    ObjectManager.Player.Spellbook.LevelUpSpell(mf.Slot);
                    points--;
                }
                foreach (MenuInfo mi in GetOrderedList())
                {
                    if (points > 0)
                    {
                        ObjectManager.Player.Spellbook.LevelUpSpell(mi.Slot);
                        points--;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        #endregion

        #region Nested Types

        private class MenuInfo
        {
            #region Properties

            public SpellSlot Slot { get; set; }

            public int Value { get; set; }

            #endregion
        }

        #endregion
    }
}