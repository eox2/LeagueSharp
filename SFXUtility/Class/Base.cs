#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Base.cs is part of SFXUtility.
 
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
    using IoCContainer;
    using LeagueSharp.Common;
    using Logger;

    #endregion

    internal abstract class Base
    {
        #region Constructors

        protected Base(IContainer container)
        {
            if (!container.IsRegistered<SFXUtility>())
                throw new InvalidOperationException("SFXUtility");
            if (!container.IsRegistered<ILogger>())
                throw new InvalidOperationException("ILogger");

            IoC = container;

            var sfx = IoC.Resolve<SFXUtility>();

            Logger = IoC.Resolve<ILogger>();
            BaseMenu = sfx.Menu;
            BaseName = sfx.Name;
        }

        #endregion

        #region Properties

        public abstract bool Enabled { get; }
        public abstract string Name { get; }
        public bool Initialized { get; protected set; }

        public Menu Menu { get; set; }

        protected Menu BaseMenu { get; private set; }

        protected string BaseName { get; private set; }
        protected IContainer IoC { get; private set; }

        protected ILogger Logger { get; set; }

        #endregion
    }
}