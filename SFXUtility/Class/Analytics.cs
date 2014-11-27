#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Analytics.cs is part of SFXUtility.
 
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
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Management;
    using System.Net;
    using System.Reflection;
    using System.Timers;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class Analytics
    {
        #region Fields

        private readonly IContainer _container;
        private readonly Uri _url = new Uri("http://www.sentryfox.com/leagueharp/analytics.php?v=1");
        private readonly WebClient _webClient = new WebClient();
        private Timer _timer;

        #endregion

        #region Constructors

        public Analytics(IContainer container)
        {
            _container = container;
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            CustomEvents.Game.OnGameEnd += OnGameEnd;
        }

        #endregion

        #region Methods

        private void OnGameEnd(EventArgs args)
        {
            try
            {
                var name = "unknown";
                var version = "unknown";
                if (_container.IsRegistered<SFXUtility>())
                {
                    var sfxutility = _container.Resolve<SFXUtility>();
                    name = sfxutility.Name;
                    version = sfxutility.Version.ToString();
                }
                var values = new NameValueCollection
                {
                    {"action", "game_end"},
                    {"app_name", name},
                    {"app_version", version},
                    {"game_id", Game.Id.ToString(CultureInfo.InvariantCulture)},
                    {"user_fingerprint", Fingerprint.Id}
                };
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
                        if (_container.IsRegistered(type))
                        {
                            var feature = _container.Resolve(type) as Base;
                            if (feature != null)
                            {
                                values.Add(string.Format("feature_{0}", type.Name),
                                    (feature.Initialized && feature.Enabled).ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                _webClient.UploadValues(_url, "POST", values);
                _timer.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                _webClient.UploadValuesAsync(_url, "POST", new NameValueCollection
                {
                    {"action", "game_load"},
                    {"game_id", Game.Id.ToString(CultureInfo.InvariantCulture)},
                    {"game_version", Game.Version},
                    {"game_region", Game.Region},
                    {"game_map", Game.MapId.ToString()},
                    {"game_champion", ObjectManager.Player.ChampionName},
                    {"user_fingerprint", Fingerprint.Id}
                });
                _timer = new Timer(60*1000*10)
                {
                    Enabled = true,
                };
                _timer.Elapsed += OnTick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnTick(object sender, ElapsedEventArgs args)
        {
            try
            {
                _webClient.UploadValuesAsync(_url, "POST", new NameValueCollection
                {
                    {"action", "game_update"},
                    {"game_id", Game.Id.ToString(CultureInfo.InvariantCulture)},
                    {"user_fingerprint", Fingerprint.Id}
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        #region Nested Types

        private static class Fingerprint
        {
            #region Fields

            private static string _id;

            #endregion

            #region Properties

            public static string Id
            {
                get
                {
                    if (string.IsNullOrEmpty(_id))
                    {
                        try
                        {
                            _id = Utilities.GetMd5(string.Concat(CpuId(), HddId()));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    return _id;
                }
            }

            #endregion

            #region Methods

            private static string CpuId()
            {
                using (var mc = new ManagementClass("win32_processor"))
                {
                    using (var moc = mc.GetInstances())
                    {
                        foreach (
                            var mo in
                                moc.Cast<ManagementBaseObject>()
                                    .Where(mo => !string.IsNullOrEmpty(mo.Properties["processorID"].Value.ToString())))
                        {
                            return mo.Properties["processorID"].Value.ToString();
                        }
                    }
                }
                return string.Empty;
            }

            private static string HddId()
            {
                using (var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""C:"""))
                {
                    dsk.Get();
                    return dsk["VolumeSerialNumber"].ToString();
                }
            }

            #endregion
        }

        #endregion
    }
}