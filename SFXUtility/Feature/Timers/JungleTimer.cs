#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 JungleTimer.cs is part of SFXUtility.
 
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

    internal class JungleTimer : Base
    {
        #region Fields

        private const float CheckInterval = 25f;

        private readonly List<Camp> _camps = new List<Camp>();
        private float _lastCheck = Environment.TickCount;
        private Timers _timers;

        #endregion

        #region Constructors

        public JungleTimer(IContainer container)
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
            get { return "Jungle"; }
        }

        #endregion

        #region Methods

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                foreach (Camp camp in _camps.Where(camp => !(camp.NextRespawnTime <= 0f)))
                {
                    Utilities.DrawTextCentered(Drawing.WorldToMinimap(camp.Position),
                        Menu.Item(Name + "DrawingColor").GetValue<Color>(),
                        ((int) (camp.NextRespawnTime - Game.Time)).ToString(CultureInfo.InvariantCulture));
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

        private void OnGameProcessPacket(GamePacketEventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                if (args.PacketData[0] == Packet.S2C.EmptyJungleCamp.Header)
                {
                    var packet = Packet.S2C.EmptyJungleCamp.Decoded(args.PacketData);
                    var camp = _camps.FirstOrDefault(c => c.Id == packet.CampId);
                    if (packet.UnitNetworkId != 0 && !Equals(camp, default(Camp)))
                    {
                        if (packet.EmptyType != 3)
                        {
                            camp.NextRespawnTime = Game.Time + camp.RespawnTime;
                        }
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

                foreach (Camp camp in _camps.Where(camp => (camp.NextRespawnTime - Game.Time) < 0f))
                {
                    camp.NextRespawnTime = 0f;
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

                    if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
                    {
                        // Blue: Blue Buff
                        _camps.Add(new Camp(new Vector3(3388.2f, 7697.2f, 55.2f), 1, 300f));

                        // Blue: Wolves
                        _camps.Add(new Camp(new Vector3(3415.8f, 6269.6f, 55.6f), 2, 100f));

                        // Blue: Wraiths
                        _camps.Add(new Camp(new Vector3(6447f, 5384f, 60f), 3, 100f));

                        // Blue: Red Buff
                        _camps.Add(new Camp(new Vector3(7509.4f, 3977.1f, 56.9f), 4, 300f));

                        // Blue: Golems
                        _camps.Add(new Camp(new Vector3(8042.2f, 2274.3f, 54.3f), 5, 100f));

                        // Blue: Wight
                        _camps.Add(new Camp(new Vector3(1859.1f, 8246.3f, 54.9f), 13, 100f));

                        // Red: Blue Buff
                        _camps.Add(new Camp(new Vector3(10440f, 6717.9f, 54.9f), 7, 300f));

                        // Red: Wolves
                        _camps.Add(new Camp(new Vector3(10575f, 8083f, 65.5f), 8, 100f));

                        // Red: Wraiths
                        _camps.Add(new Camp(new Vector3(7534.3f, 9226.5f, 55.5f), 9, 100f));

                        // Red: Red Buff
                        _camps.Add(new Camp(new Vector3(6558.2f, 10524.9f, 54.6f), 10, 300f));

                        // Red: Golems
                        _camps.Add(new Camp(new Vector3(6005f, 12055f, 39.6f), 11, 100f));

                        // Red: Wight
                        _camps.Add(new Camp(new Vector3(12287f, 6205f, 54.8f), 14, 100f));
                        
                            //Neutral: Crab Red Side
                           _camps.Add(new Camp(new Vector3(10554f, 5836f, 54.8f), 15, 180f));
                           
                        //Neutral: Crab Blue Side
                           _camps.Add(new Camp(new Vector3(4545f, 9136f, 55.2f), 16, 180f));
                           
                           

                        // Neutral: Dragon
                        _camps.Add(new Camp(new Vector3(9606.8f, 4210.5f, -60.3f), 6, 360f));

                        // Neutral: Baron
                        _camps.Add(new Camp(new Vector3(4549.1f, 10126.7f, -63.1f), 12, 420f));
                    }

                    if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline)
                    {
                        // Blue: Wraiths
                        _camps.Add(new Camp(new Vector3(4414f, 5774f, 60f), 1, 50f));

                        // Blue: Golems
                        _camps.Add(new Camp(new Vector3(5088f, 8065f, 60f), 2, 50f));

                        // Blue: Wolves
                        _camps.Add(new Camp(new Vector3(6148f, 5993f, 60f), 3, 50f));

                        // Red: Wraiths
                        _camps.Add(new Camp(new Vector3(11008f, 5775f, 60f), 4, 50f));

                        // Red: Golems
                        _camps.Add(new Camp(new Vector3(10341f, 8084f, 60f), 5, 50f));

                        // Red: Wolves
                        _camps.Add(new Camp(new Vector3(9239f, 6022f, 60f), 6, 50f));

                        // Neutral: Vilemaw
                        _camps.Add(new Camp(new Vector3(7711f, 10080f, 60f), 8, 300f));
                    }

                    if (_camps.Count > 0)
                    {
                        Game.OnGameUpdate += OnGameUpdate;
                        Game.OnGameProcessPacket += OnGameProcessPacket;
                        Drawing.OnDraw += OnDraw;
                    }

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

        private class Camp
        {
            #region Fields

            public readonly int Id;
            public readonly Vector3 Position;
            public readonly float RespawnTime;
            public float NextRespawnTime;

            #endregion

            #region Constructors

            public Camp(Vector3 position, int id, float respawnTime)
            {
                Position = position;
                Id = id;
                RespawnTime = respawnTime;
            }

            #endregion
        }

        #endregion
    }
}
