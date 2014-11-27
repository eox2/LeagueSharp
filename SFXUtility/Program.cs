#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Program.cs is part of SFXUtility.
 
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

namespace SFXUtility
{
    #region

    using System;
    using System.Linq;
    using System.Reflection;
    using Class;
    using IoCContainer;
    using Logger;

    #endregion

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            var container = new Container();
            container.Register<ILogger, ConsoleLogger>();
            container.Register<Mediator, Mediator>(true);

            container.Register<SFXUtility, SFXUtility>(true, true);

            var bType = typeof (Base);
            foreach (
                var type in
                    Assembly.GetAssembly(bType)
                        .GetTypes()
                        .OrderBy(type => type.Name)
                        .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(bType)))
            {
                try
                {
                    var tmpType = type;
                    container.Register(type, () => Activator.CreateInstance(tmpType, new object[] {container}), true,
                        true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            //container.Register(typeof (Analytics),
            //    () => Activator.CreateInstance(typeof (Analytics), new object[] {container}), true, true);
        }

        #endregion
    }
}