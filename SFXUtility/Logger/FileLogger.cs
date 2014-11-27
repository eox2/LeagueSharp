#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 FileLogger.cs is part of SFXUtility.
 
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
    #region

    using System.IO;
    using System.Text;

    #endregion

    internal class FileLogger : ILogger
    {
        #region Fields

        private readonly StreamWriter _streamWriter;

        #endregion

        #region Constructors

        public FileLogger(string file)
        {
            File = file;
            _streamWriter = new StreamWriter(file, true, Encoding.UTF8);
        }

        #endregion

        #region Properties

        public string File { get; set; }
        public string Prefix { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            _streamWriter.Dispose();
        }

        public void Write(string message)
        {
            _streamWriter.Write(string.IsNullOrWhiteSpace(Prefix) ? message : string.Format("{0}: {1}", Prefix, message));
        }

        public void WriteBlock(string header, string message)
        {
            _streamWriter.WriteLine(string.IsNullOrWhiteSpace(Prefix)
                ? header
                : string.Format("{0}: {1}", Prefix, header));
            _streamWriter.WriteLine("--------------------");
            _streamWriter.WriteLine(message);
            _streamWriter.WriteLine("--------------------");
            _streamWriter.WriteLine(string.Empty);
        }

        public void WriteLine(string message)
        {
            _streamWriter.WriteLine(string.IsNullOrWhiteSpace(Prefix)
                ? message
                : string.Format("{0}: {1}", Prefix, message));
        }

        #endregion
    }
}