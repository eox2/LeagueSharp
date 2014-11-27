#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 LasthitMarker.cs is part of SFXUtility.
 
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
    using SharpDX;
    using Color = System.Drawing.Color;

    #endregion

    internal class LasthitMarker : Base
    {
        #region Fields

        private List<Obj_AI_Minion> _minions = new List<Obj_AI_Minion>();

        #endregion

        #region Constructors

        public LasthitMarker(Container container)
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
                return Menu != null && Menu.Item(Name + "Enabled").GetValue<bool>() &&
                       (Menu.Item(Name + "DrawingHpBarEnabled").GetValue<bool>() ||
                        Menu.Item(Name + "DrawingCircleEnabled").GetValue<bool>());
            }
        }

        public override string Name
        {
            get { return "Lasthit Marker"; }
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
                var hpKillableColor = Menu.Item(Name + "DrawingHpBarKillableColor").GetValue<Color>();
                var hpUnkillableColor = Menu.Item(Name + "DrawingHpBarUnkillableColor").GetValue<Color>();
                var hpLinesThickness = Menu.Item(Name + "DrawingHpBarLinesThickness").GetValue<Slider>().Value;
                var radius = Menu.Item(Name + "DrawingCircleRadius").GetValue<Slider>().Value;
                var circleThickness = BaseMenu.Item("MiscCircleThickness").GetValue<Slider>().Value;

                foreach (Obj_AI_Minion minion in _minions.Where(minion => minion.Team != GameObjectTeam.Neutral))
                {
                    var aaDamage = ObjectManager.Player.GetAutoAttackDamage(minion, true);
                    var killable = minion.Health <= aaDamage;
                    if (Menu.Item(Name + "DrawingHpBarEnabled").GetValue<bool>() && minion.IsHPBarRendered)
                    {
                        var barPos = minion.HPBarPosition;
                        var offset = 62/(minion.MaxHealth/aaDamage);
                        offset = offset > 62 ? 62 : offset;
                        var tmpThk = (int) (62 - offset);
                        hpLinesThickness = tmpThk > hpLinesThickness ? hpLinesThickness : (tmpThk == 0 ? 1 : tmpThk);
                        Drawing.DrawLine(new Vector2(barPos.X + 45 + (float) offset, barPos.Y + 18),
                            new Vector2(barPos.X + 45 + (float) offset, barPos.Y + 23), hpLinesThickness,
                            killable ? hpKillableColor : hpUnkillableColor);
                    }
                    if (Menu.Item(Name + "DrawingCircleEnabled").GetValue<bool>() && killable)
                    {
                        Utility.DrawCircle(minion.Position, minion.BoundingRadius + radius, circleColor, circleThickness);
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

                Menu = new Menu(Name, Name);

                var drawingMenu = new Menu("Drawing", Name + "Drawing");

                var drawingHpBarMenu = new Menu("HPBar", Name + "HPBar");
                drawingHpBarMenu.AddItem(
                    new MenuItem(Name + "DrawingHpBarKillableColor", "Killable Color").SetValue(Color.Green));
                drawingHpBarMenu.AddItem(
                    new MenuItem(Name + "DrawingHpBarUnkillableColor", "Unkillable Color").SetValue(Color.White));
                drawingHpBarMenu.AddItem(
                    new MenuItem(Name + "DrawingHpBarLinesThickness", "Lines Thickness").SetValue(new Slider(1, 1, 10)));
                drawingHpBarMenu.AddItem(new MenuItem(Name + "DrawingHpBarEnabled", "Enabled").SetValue(true));

                var drawingCirclesMenu = new Menu("Circle", Name + "Circle");
                drawingCirclesMenu.AddItem(
                    new MenuItem(Name + "DrawingCircleColor", "Circle Color").SetValue(Color.Fuchsia));
                drawingCirclesMenu.AddItem(
                    new MenuItem(Name + "DrawingCircleRadius", "Circle Radius").SetValue(new Slider(30)));
                drawingCirclesMenu.AddItem(new MenuItem(Name + "DrawingCircleEnabled", "Enabled").SetValue(true));

                drawingMenu.AddSubMenu(drawingHpBarMenu);
                drawingMenu.AddSubMenu(drawingCirclesMenu);

                var distanceMenu = new Menu("Distance", Name + "Distance");
                distanceMenu.AddItem(new MenuItem(Name + "DistanceEnabled", "Limit by Distance").SetValue(true));
                distanceMenu.AddItem(
                    new MenuItem(Name + "DistanceLimit", "Distance Limit").SetValue(new Slider(1000, 500, 3000)));

                Menu.AddSubMenu(drawingMenu);
                Menu.AddSubMenu(distanceMenu);

                Menu.AddItem(new MenuItem(Name + "Enabled", "Enabled").SetValue(true));

                BaseMenu.AddSubMenu(Menu);

                Game.OnGameUpdate += OnGameUpdate;
                Drawing.OnDraw += OnDraw;

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }


        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                _minions = (from minion in ObjectManager.Get<Obj_AI_Minion>()
                    where minion.IsValidTarget() && minion.Health > 0.1f
                    where
                        !Menu.Item(Name + "DistanceEnabled").GetValue<bool>() ||
                        minion.Distance(ObjectManager.Player.Position) <=
                        Menu.Item(Name + "DistanceLimit").GetValue<Slider>().Value
                    select minion).ToList();
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        #endregion
    }
}