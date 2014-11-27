#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 ObjectTimer.cs is part of SFXUtility.
 
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
    using System.Globalization;
    using System.Linq;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using Color = System.Drawing.Color;
    using Utilities = Class.Utilities;

    #endregion

    internal class ObjectTimer : Base
    {
        #region Fields

        private const float CheckInterval = 25f;
        private readonly List<ObjectBarrack> _objectBarracks = new List<ObjectBarrack>();
        private readonly List<ObjectMinion> _objectMinions = new List<ObjectMinion>();
        private float _lastCheck = Environment.TickCount;

        private Timers _timers;

        #endregion

        #region Constructors

        public ObjectTimer(IContainer container)
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
                return _timers != null && _timers.Menu != null &&
                       _timers.Menu.Item(_timers.Name + "Enabled").GetValue<bool>() && Menu != null &&
                       Menu.Item(Name + "Enabled").GetValue<bool>();
            }
        }

        public override string Name
        {
            get { return "Object"; }
        }

        #endregion

        #region Methods

        private void AddObjects()
        {
            foreach (Obj_AI_Minion obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.IsValid && !obj.IsDead))
            {
                // Health
                if (obj.Name == "OdinShieldRelic")
                {
                    if (_objectMinions.All(h => h.Object.NetworkId != obj.NetworkId))
                    {
                        _objectMinions.Add(new ObjectMinion(obj, 32f));
                    }
                }
                // Health
                if (obj.Name == "TT_Relic7.1.1")
                {
                    if (_objectMinions.All(h => h.Object.NetworkId != obj.NetworkId))
                    {
                        _objectMinions.Add(new ObjectMinion(obj, 87f));
                    }
                }
                // Health
                if (obj.Name.Contains("HealthRelic"))
                {
                    if (_objectMinions.All(h => h.Object.NetworkId != obj.NetworkId))
                    {
                        _objectMinions.Add(new ObjectMinion(obj, 37f));
                    }
                }
                // Stormshield
                if (obj.Name == "OdinCenterRelic")
                {
                    if (_objectMinions.All(h => h.Object.NetworkId != obj.NetworkId))
                    {
                        _objectMinions.Add(new ObjectMinion(obj, 180f));
                    }
                }
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                foreach (ObjectMinion objectMinion in _objectMinions.Where(camp => !(camp.NextRespawnTime <= 0f)))
                {
                    Utilities.DrawTextCentered(objectMinion.MinimapPosition,
                        Menu.Item(Name + "DrawingColor").GetValue<Color>(),
                        ((int) (objectMinion.NextRespawnTime - Game.Time)).ToString(CultureInfo.InvariantCulture));
                }

                foreach (ObjectBarrack objectBarracks in _objectBarracks.Where(camp => !(camp.NextRespawnTime <= 0f)))
                {
                    Utilities.DrawTextCentered(objectBarracks.MinimapPosition,
                        Menu.Item(Name + "DrawingColor").GetValue<Color>(),
                        ((int) (objectBarracks.NextRespawnTime - Game.Time)).ToString(CultureInfo.InvariantCulture));
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

                if (IoC.IsRegistered<Timers>() && IoC.Resolve<Timers>().Initialized)
                {
                    TimersLoaded(IoC.Resolve<Timers>());
                }
                else
                {
                    if (IoC.IsRegistered<Mediator>())
                    {
                        IoC.Resolve<Mediator>().Register("Timers_initialized", TimersLoaded);
                    }
                }
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
                if (!Enabled || _lastCheck + CheckInterval > Environment.TickCount)
                    return;

                _lastCheck = Environment.TickCount;

                foreach (ObjectBarrack objectBarrack in _objectBarracks)
                {
                    if (objectBarrack.Object.Health > 0f)
                    {
                        objectBarrack.Taken = false;
                        objectBarrack.NextRespawnTime = 0f;
                    }
                    else if (!objectBarrack.Taken && objectBarrack.Object.Health < 1f)
                    {
                        objectBarrack.Taken = true;
                        objectBarrack.NextRespawnTime = objectBarrack.RespawnTime + Game.Time;
                    }
                }
                if (Utility.Map.GetMap()._MapType != Utility.Map.MapType.SummonersRift)
                {
                    foreach (ObjectMinion objectMinion in _objectMinions.ToList())
                    {
                        if (!objectMinion.Taken &&
                            (objectMinion.Object == null || !objectMinion.Object.IsValid || objectMinion.Object.IsDead) &&
                            objectMinion.NextRespawnTime <= 0f)
                        {
                            objectMinion.Taken = true;
                            objectMinion.NextRespawnTime = objectMinion.RespawnTime + Game.Time;
                        }
                        if (objectMinion.Taken && objectMinion.NextRespawnTime < Game.Time)
                        {
                            _objectMinions.Remove(objectMinion);
                        }
                    }

                    foreach (
                        ObjectMinion objectMinion in
                            _objectMinions.Where(
                                objectMinion => objectMinion.Object != null && objectMinion.Object.IsValid))
                    {
                        var buff = objectMinion.Object.Buffs.FirstOrDefault(b => b.Name == "treelinelanternlock");
                        if (Equals(buff, default(BuffInstance)))
                        {
                            objectMinion.Taken = false;
                            objectMinion.NextRespawnTime = 0f;
                        }
                        else if (!objectMinion.Taken && buff.IsActive)
                        {
                            objectMinion.Taken = true;
                            objectMinion.NextRespawnTime = buff.EndTime;
                        }
                    }

                    AddObjects();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void TimersLoaded(object o)
        {
            try
            {
                if (o is Timers && (o as Timers).Menu != null)
                {
                    _timers = (o as Timers);

                    Menu = new Menu(Name, Name);

                    var drawingMenu = new Menu("Drawing", Name + "Drawing");
                    drawingMenu.AddItem(new MenuItem(Name + "DrawingColor", "Color").SetValue(Color.Yellow));

                    Menu.AddSubMenu(drawingMenu);

                    Menu.AddItem(new MenuItem(Name + "Enabled", "Enabled").SetValue(true));

                    _timers.Menu.AddSubMenu(Menu);

                    if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline)
                    {
                        foreach (
                            Obj_AI_Minion obj in
                                ObjectManager.Get<Obj_AI_Minion>()
                                    .Where(obj => obj.IsValid && obj.Name.Contains("Buffplat")))
                        {
                            _objectMinions.Add(new ObjectMinion(obj, 90f));
                        }
                    }

                    if (Utility.Map.GetMap()._MapType != Utility.Map.MapType.CrystalScar)
                    {
                        foreach (Obj_BarracksDampener inhibitor in ObjectManager.Get<Obj_BarracksDampener>())
                        {
                            _objectBarracks.Add(new ObjectBarrack(inhibitor, 240f, inhibitor.Health < 1f));
                        }
                    }

                    Game.OnGameUpdate += OnGameUpdate;
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

        #region Nested Types

        private class ObjectBarrack : ObjectStruct
        {
            #region Fields

            public readonly Obj_BarracksDampener Object;

            #endregion

            #region Constructors

            public ObjectBarrack(Obj_BarracksDampener obj, float respawnTime, bool taken = false)
            {
                Object = obj;
                MinimapPosition = Drawing.WorldToMinimap(obj.Position);
                RespawnTime = respawnTime;
                Taken = taken;
            }

            #endregion
        }

        private class ObjectMinion : ObjectStruct
        {
            #region Fields

            public readonly Obj_AI_Minion Object;

            #endregion

            #region Constructors

            public ObjectMinion(Obj_AI_Minion obj, float respawnTime, bool taken = false)
            {
                Object = obj;
                MinimapPosition = Drawing.WorldToMinimap(obj.Position);
                RespawnTime = respawnTime;
                Taken = taken;
            }

            #endregion
        }

        private class ObjectStruct
        {
            #region Fields

            public Vector2 MinimapPosition;
            public float NextRespawnTime;
            public float RespawnTime;
            public bool Taken;

            #endregion
        }

        #endregion
    }
}