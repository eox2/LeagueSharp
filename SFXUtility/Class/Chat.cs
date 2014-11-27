#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Chat.cs is part of SFXUtility.
 
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

    using LeagueSharp;

    #endregion

    public class Chat
    {
        #region Fields

        public const string DefaultColor = "#F7A100";

        #endregion

        #region Methods

        public static void Print(string message, string hexColor = DefaultColor)
        {
            Game.PrintChat(string.Format("<font color='{0}'>{1}</font>", hexColor, message));
        }

        #endregion
    }
}