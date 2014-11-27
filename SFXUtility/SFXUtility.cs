#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 SFXUtility.cs is part of SFXUtility.
 
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
    using Class;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class SFXUtility
    {
        #region Constructors

        public SFXUtility()
        {
            try
            {
                Menu = new Menu(Name, Name, true);

                var miscMenu = new Menu("Misc", "Misc");

                var infoMenu = new Menu("Info", "Info");
                infoMenu.AddItem(new MenuItem("InfoVersion", string.Format("Version: {0}", Version)));
                infoMenu.AddItem(new MenuItem("InfoIRC", "IRC: Appril"));
                infoMenu.AddItem(new MenuItem("InfoGithub", "Github:").SetValue(new StringList(new[]
                {
                    "Smokyfox",
                    "Lizzaran",
                    "SephLeague"
                })));

                miscMenu.AddSubMenu(infoMenu);

                miscMenu.AddItem(new MenuItem("MiscCircleThickness", "Circles Thickness").SetValue(new Slider(3, 10, 1)));

                Menu.AddSubMenu(miscMenu);
                AppDomain.CurrentDomain.DomainUnload += OnExit;
                AppDomain.CurrentDomain.ProcessExit += OnExit;
                CustomEvents.Game.OnGameEnd += OnGameEnd;
                Game.OnGameEnd += OnGameEnd;
                CustomEvents.Game.OnGameLoad += OnGameLoad;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region Events

        public event EventHandler OnUnload;

        #endregion

        #region Properties

        public Menu Menu { get; private set; }

        public string Name
        {
            get { return "SFXUtility"; }
        }

        public System.Version Version
        {
            get { return new System.Version(0, 7, 6, 2); }
        }

        #endregion

        #region Methods

        private void OnExit(object sender, EventArgs e)
        {
            try
            {
                EventHandler handler = OnUnload;
                if (null != handler) handler(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameEnd(EventArgs args)
        {
            try
            {
                OnExit(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Chat.Print(string.Format("{0} v{1}.{2}.{3} loaded.", Name, Version.Major, Version.Minor, Version.Build));
                Menu.AddToMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion
    }
}
