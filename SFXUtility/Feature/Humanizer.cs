#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Humanizer.cs is part of SFXUtility.
 
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
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class Humanizer : Base
    {
        #region Fields

        private float _lastMovement;
        private float _lastSpell;

        #endregion

        #region Constructors

        public Humanizer(Container container)
            : base(container)
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
            get { return "Humanizer"; }
        }

        #endregion

        #region Methods

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);

                Menu = new Menu(Name, Name);

                var delayMenu = new Menu("Delay", Name + "Delay");
                delayMenu.AddItem(new MenuItem(Name + "DelaySpells", "Spells (ms)").SetValue(new Slider(50, 0, 250)));
                delayMenu.AddItem(new MenuItem(Name + "DelayMovement", "Movement (ms)").SetValue(new Slider(50, 0, 250)));

                Menu.AddSubMenu(delayMenu);

                var eMenuItem = new MenuItem(Name + "Enabled", "Enabled").SetValue(true);

                Menu.AddItem(eMenuItem);

                eMenuItem.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        SpellHumanizer.Enabled = eventArgs.GetNewValue<bool>();
                    };

                BaseMenu.AddSubMenu(Menu);

                Game.OnGameSendPacket += OnGameSendPacket;

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnGameSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                var spellsDelay = Menu.Item(Name + "DelaySpells").GetValue<Slider>().Value;
                var movementDelay = Menu.Item(Name + "DelayMovement").GetValue<Slider>().Value;

                if (spellsDelay > 0 && (new GamePacket(args.PacketData)).Header == Packet.C2S.Cast.Header)
                {
                    Packet.C2S.Cast.Struct castStruct = Packet.C2S.Cast.Decoded(args.PacketData);
                    if (castStruct.SourceNetworkId == ObjectManager.Player.NetworkId)
                    {
                        if (_lastSpell + spellsDelay > Environment.TickCount)
                        {
                            args.Process = false;
                        }
                        else
                        {
                            _lastSpell = Environment.TickCount;
                        }
                    }
                }

                if (movementDelay > 0 && (new GamePacket(args.PacketData)).Header == Packet.C2S.Move.Header)
                {
                    Packet.C2S.Move.Struct movementStruct = Packet.C2S.Move.Decoded(args.PacketData);
                    if (movementStruct.MoveType == 2)
                    {
                        if (movementStruct.SourceNetworkId == ObjectManager.Player.NetworkId)
                        {
                            if (_lastMovement + movementDelay > Environment.TickCount)
                            {
                                args.Process = false;
                            }
                            else
                            {
                                _lastMovement = Environment.TickCount;
                            }
                        }
                    }
                    else if (movementStruct.MoveType == 3)
                    {
                        _lastMovement = 0f;
                    }
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