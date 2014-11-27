#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 UselessLogger.cs is part of SFXUtility.
 
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

namespace SFXUtility.Logger
{
    internal class UselessLogger : ILogger
    {
        #region Properties

        public string Prefix { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
        }

        public void Write(string message)
        {
        }

        public void WriteBlock(string header, string message)
        {
        }

        public void WriteLine(string message)
        {
        }

        #endregion
    }
}