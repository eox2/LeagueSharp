#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 CloneTracker.cs is part of SFXUtility.
 
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
    using System.Drawing;
    using System.Linq;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class CloneTracker : Base
    {
        #region Fields

        private readonly string[] _cloneHeroes = {"Shaco", "LeBlanc", "MonkeyKing", "Yorick"};
        private Trackers _trackers;

        #endregion

        #region Constructors

        public CloneTracker(IContainer container)
            : base(container)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get
            {
                return _trackers != null && _trackers.Menu != null &&
                       _trackers.Menu.Item(_trackers.Name + "Enabled").GetValue<bool>() && Menu != null &&
                       Menu.Item(Name + "Enabled").GetValue<bool>();
            }
        }

        public override string Name
        {
            get { return "Clone"; }
        }

        #endregion

        #region Methods

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                var circleColor = Menu.Item(Name + "DrawingCircleColor").GetValue<Color>();
                var radius = Menu.Item(Name + "DrawingCircleRadius").GetValue<Slider>().Value;

                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(hero => hero.IsValid && hero.IsEnemy && !hero.IsDead && hero.IsVisible)
                            .Where(hero => _cloneHeroes.Contains(hero.ChampionName)))
                {
                    Utility.DrawCircle(hero.ServerPosition, hero.BoundingRadius + radius, circleColor);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);

                if (IoC.IsRegistered<Trackers>() && IoC.Resolve<Trackers>().Initialized)
                {
                    TrackersLoaded(IoC.Resolve<Trackers>());
                }
                else
                {
                    if (IoC.IsRegistered<Mediator>())
                    {
                        IoC.Resolve<Mediator>().Register("Trackers_initialized", TrackersLoaded);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void TrackersLoaded(object o)
        {
            try
            {
                if (o is Trackers && (o as Trackers).Menu != null)
                {
                    _trackers = (o as Trackers);

                    Menu = new Menu(Name, Name);

                    var drawingMenu = new Menu("Drawing", Name + "Drawing");
                    drawingMenu.AddItem(
                        new MenuItem(Name + "DrawingCircleColor", "Circle Color").SetValue(Color.YellowGreen));
                    drawingMenu.AddItem(
                        new MenuItem(Name + "DrawingCircleRadius", "Circle Radius").SetValue(new Slider(30)));

                    Menu.AddSubMenu(drawingMenu);

                    Menu.AddItem(new MenuItem(Name + "Enabled", "Enabled").SetValue(false));

                    _trackers.Menu.AddSubMenu(Menu);

                    Drawing.OnDraw += OnDraw;

                    Initialized = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        #endregion
    }
}