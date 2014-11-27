#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 MappingKey.cs is part of SFXUtility.
 
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

namespace SFXUtility.IoCContainer
{
    #region

    using System;

    #endregion

    internal class MappingKey
    {
        #region Constructors

        public MappingKey(Type type, bool singleton, string instanceName)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;
            Singleton = singleton;
            InstanceName = instanceName;
        }

        #endregion

        #region Properties

        public object Instance { get; set; }

        public string InstanceName { get; protected set; }

        public bool Singleton { get; protected set; }
        public Type Type { get; protected set; }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var compareTo = obj as MappingKey;

            if (ReferenceEquals(this, compareTo))
                return true;

            if (compareTo == null)
                return false;

            return Type == compareTo.Type &&
                   string.Equals(InstanceName, compareTo.InstanceName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int multiplier = 31;
                int hash = GetType().GetHashCode();

                hash = hash*multiplier + Type.GetHashCode();
                hash = hash*multiplier + (InstanceName == null ? 0 : InstanceName.GetHashCode());

                return hash;
            }
        }


        public override string ToString()
        {
            const string format = "{0} ({1}) - hash code: {2}";

            return string.Format(format, InstanceName ?? "[null]", Type.FullName, GetHashCode());
        }

        public string ToTraceString()
        {
            const string format = "Instance Name: {0} ({1})";

            return string.Format(format, InstanceName ?? "[null]", Type.FullName);
        }

        #endregion
    }
}