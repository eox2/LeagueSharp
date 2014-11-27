#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Recall.cs is part of SFXUtility.
 
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

namespace SFXUtility.Class
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal static class Recall
    {
        #region Fields

        private static readonly Dictionary<int, int> T = new Dictionary<int, int>();

        #endregion

        #region Methods

        public static Packet.S2C.Recall.Struct Decode(byte[] data)
        {
            var recall = new Packet.S2C.Recall.Struct {Status = Packet.S2C.Recall.RecallStatus.Unknown};
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                reader.ReadByte();
                reader.ReadInt32();
                recall.UnitNetworkId = reader.ReadInt32();
                reader.ReadBytes(66);

                if (BitConverter.ToString(reader.ReadBytes(6)) != "00-00-00-00-00-00")
                {
                    recall.Status = BitConverter.ToString(reader.ReadBytes(3)) != "00-00-00"
                        ? Packet.S2C.Recall.RecallStatus.TeleportStart
                        : Packet.S2C.Recall.RecallStatus.RecallStarted;
                }
            }
            var hero =
                ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsValid && h.NetworkId == recall.UnitNetworkId);
            if (!Equals(hero, default(Obj_AI_Hero)))
            {
                recall.Duration = recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart
                    ? 3500
                    : Utility.GetRecallTime(hero);
                if (!T.ContainsKey(recall.UnitNetworkId))
                    T.Add(recall.UnitNetworkId, Environment.TickCount);
                else
                {
                    if (T[recall.UnitNetworkId] == 0)
                    {
                        T[recall.UnitNetworkId] = Environment.TickCount;
                    }
                    else
                    {
                        if (Environment.TickCount - T[recall.UnitNetworkId] > recall.Duration - 150)
                        {
                            recall.Status = recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart
                                ? Packet.S2C.Recall.RecallStatus.TeleportEnd
                                : Packet.S2C.Recall.RecallStatus.RecallFinished;
                        }
                        else
                        {
                            recall.Status = recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart
                                ? Packet.S2C.Recall.RecallStatus.TeleportAbort
                                : Packet.S2C.Recall.RecallStatus.RecallAborted;
                        }

                        T[recall.UnitNetworkId] = 0;
                    }
                }
            }
            return recall;
        }

        #endregion
    }
}