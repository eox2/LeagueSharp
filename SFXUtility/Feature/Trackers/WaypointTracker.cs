#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 WaypointTracker.cs is part of SFXUtility.
 
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
    using System.Linq;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using Color = System.Drawing.Color;
    using Utilities = Class.Utilities;

    #endregion

    internal class WaypointTracker : Base
    {
        #region Fields

        private Trackers _trackers;

        #endregion

        #region Constructors

        public WaypointTracker(IContainer container)
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
            get { return "Waypoint"; }
        }

        #endregion

        #region Methods

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                var crossColor = Menu.Item(Name + "DrawingCrossColor").GetValue<Color>();
                var lineColor = Menu.Item(Name + "DrawingLineColor").GetValue<Color>();

                foreach (Obj_AI_Hero hero in from hero in ObjectManager.Get<Obj_AI_Hero>()
                    where hero.IsValid && !hero.IsDead
                    where Menu.Item(Name + "DrawAlly").GetValue<Boolean>() || !hero.IsAlly
                    where Menu.Item(Name + "DrawEnemy").GetValue<Boolean>() || !hero.IsEnemy
                    select hero)
                {
                    var arrivalTime = 0.0f;
                    var waypoints = hero.GetWaypoints();
                    for (int i = 0, l = waypoints.Count - 1; i < l; i++)
                    {
                        if (waypoints[i].IsValid() && waypoints[i + 1].IsValid())
                        {
                            var current = Drawing.WorldToScreen(waypoints[i].To3D());
                            var next = Drawing.WorldToScreen(waypoints[i + 1].To3D());

                            if (Utilities.IsOnScreen(current, next))
                            {
                                arrivalTime += (Vector3.Distance(waypoints[i].To3D(), waypoints[i + 1].To3D())/
                                                (ObjectManager.Player.MoveSpeed/1000))/1000;
                                Drawing.DrawLine(current.X, current.Y, next.X, next.Y, 1, lineColor);
                                if (i == l - 1 && arrivalTime > 0.1f)
                                {
                                    Utilities.DrawCross(next, 10f, 2f, crossColor);
                                    Utilities.DrawTextCentered(new Vector2(next.X - 5, next.Y + 15), crossColor,
                                        arrivalTime.ToString("0.0"));
                                }
                            }
                        }
                    }
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
                    drawingMenu.AddItem(new MenuItem(Name + "DrawingCrossColor", "Cross Color").SetValue(Color.DarkRed));
                    drawingMenu.AddItem(new MenuItem(Name + "DrawingLineColor", "Line Color").SetValue(Color.White));

                    Menu.AddSubMenu(drawingMenu);

                    Menu.AddItem(new MenuItem(Name + "DrawAlly", "Ally").SetValue(false));
                    Menu.AddItem(new MenuItem(Name + "DrawEnemy", "Enemy").SetValue(true));
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