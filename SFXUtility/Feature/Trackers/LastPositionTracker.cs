#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 lastpositiontracker.cs is part of SFXUtility.
 
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
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Properties;
    using SharpDX;

    #endregion

    internal class LastPositionTracker : Base
    {
        #region Fields

        private readonly List<LastPosition> _enemies = new List<LastPosition>();
        private Trackers _trackers;

        #endregion

        #region Constructors

        public LastPositionTracker(IContainer container)
            : base(container)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get
            {
                return _trackers != null && _trackers.Menu != null &&
                       _trackers.Menu.Item(_trackers.Name + "Enabled").GetValue<bool>() && Menu != null &&
                       Menu.Item(Name + "Enabled").GetValue<bool>();
            }
        }

        public override string Name
        {
            get { return "Last Position"; }
        }

        #endregion

        #region Methods

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);

                if (IoC.IsRegistered<Trackers>() && IoC.Resolve<Trackers>().Initialized)
                {
                    TrackersLoaded(IoC.Resolve<Trackers>());
                }
                else
                {
                    if (IoC.IsRegistered<Mediator>())
                    {
                        IoC.Resolve<Mediator>().Register("Trackers_initialized", TrackersLoaded);
                    }
                }

                if (IoC.IsRegistered<Mediator>())
                {
                    IoC.Resolve<Mediator>().Register("Recall_Finished", RecallFinished);
                    IoC.Resolve<Mediator>().Register("Recall_Started", RecallStarted);
                    IoC.Resolve<Mediator>().Register("Recall_Aborted", RecallAborted);
                    IoC.Resolve<Mediator>().Register("Recall_Unknown", RecallAborted);
                    IoC.Resolve<Mediator>().Register("Recall_Enabled", RecallEnabled);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void RecallAborted(object o)
        {
            var unitId = o is int ? (int) o : 0;
            var lastPosition = _enemies.FirstOrDefault(e => e.Hero.NetworkId == unitId);
            if (!Equals(lastPosition, default(LastPosition)))
            {
                lastPosition.IsRecalling = false;
            }
        }

        private void RecallEnabled(object o)
        {
            _enemies.ForEach(e => e.Recall = o is bool && (bool) o);
        }

        private void RecallFinished(object o)
        {
            var unitId = o is int ? (int) o : 0;
            var lastPosition = _enemies.FirstOrDefault(e => e.Hero.NetworkId == unitId);
            if (!Equals(lastPosition, default(LastPosition)))
            {
                lastPosition.Recalled = true;
                lastPosition.IsRecalling = false;
            }
        }

        private void RecallStarted(object o)
        {
            var unitId = o is int ? (int) o : 0;
            var lastPosition = _enemies.FirstOrDefault(e => e.Hero.NetworkId == unitId);
            if (!Equals(lastPosition, default(LastPosition)))
            {
                lastPosition.IsRecalling = true;
            }
        }

        private void TrackersLoaded(object o)
        {
            try
            {
                if (o is Trackers && (o as Trackers).Menu != null)
                {
                    _trackers = (o as Trackers);

                    Menu = new Menu(Name, Name);

                    var eMenuItem = new MenuItem(Name + "Enabled", "Enabled").SetValue(true);
                    eMenuItem.ValueChanged +=
                        (sender, args) => _enemies.ForEach(enemy => enemy.Active = args.GetNewValue<bool>());

                    Menu.AddItem(eMenuItem);

                    _trackers.Menu.AddSubMenu(Menu);

                    var recall = false;

                    if (IoC.IsRegistered<RecallTracker>())
                    {
                        var rt = IoC.Resolve<RecallTracker>();
                        if (rt.Initialized)
                        {
                            recall = rt.Menu.Item(rt.Name + "Enabled").GetValue<bool>();
                        }
                    }

                    foreach (
                        Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy)
                        )
                    {
                        try
                        {
                            _enemies.Add(new LastPosition(hero) {Active = Enabled, Recall = recall});
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteBlock(ex.Message, ex.ToString());
                        }
                    }

                    Initialized = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        #endregion

        #region Nested Types

        private class LastPosition
        {
            #region Fields

            public readonly Obj_AI_Hero Hero;
            public bool IsRecalling;
            public bool Recall;
            public bool Recalled;
            private readonly Render.Sprite _recallSprite;
            private readonly Render.Sprite _sprite;
            private bool _active;
            private bool _added;

            #endregion

            #region Constructors

            public LastPosition(Obj_AI_Hero hero)
            {
                Hero = hero;
                var mPos = Drawing.WorldToMinimap(hero.Position);
                var spawnPoint = ObjectManager.Get<GameObject>().FirstOrDefault(s => s is Obj_SpawnPoint && s.IsEnemy);

                _sprite =
                    new Render.Sprite(
                        (Bitmap) Resources.ResourceManager.GetObject(string.Format("LP_{0}", hero.ChampionName)) ??
                        Resources.LP_Aatrox, new Vector2(mPos.X, mPos.Y))
                    {
                        VisibleCondition = delegate
                        {
                            try
                            {
                                if (hero.IsVisible)
                                {
                                    Recalled = false;
                                }
                                return Active && !Hero.IsVisible && !Hero.IsDead;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return false;
                            }
                        },
                        PositionUpdate = delegate
                        {
                            try
                            {
                                if (Recall && Recalled)
                                {
                                    if (!Equals(spawnPoint, default(Obj_SpawnPoint)))
                                    {
                                        var p =
                                            Drawing.WorldToMinimap(spawnPoint.Position);
                                        return new Vector2(p.X - (_sprite.Size.X/2), p.Y - (_sprite.Size.Y/2));
                                    }
                                }
                                var pos = Drawing.WorldToMinimap(hero.Position);
                                return new Vector2(pos.X - (_sprite.Size.X/2), pos.Y - (_sprite.Size.Y/2));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return default(Vector2);
                            }
                        }
                    };
                _recallSprite =
                    new Render.Sprite(Resources.LP_Recall, new Vector2(mPos.X, mPos.Y))
                    {
                        VisibleCondition = delegate
                        {
                            try
                            {
                                return Active && !Hero.IsVisible && !Hero.IsDead && Recall && IsRecalling;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return false;
                            }
                        },
                        PositionUpdate = delegate
                        {
                            try
                            {
                                var pos = Drawing.WorldToMinimap(hero.Position);
                                return new Vector2(pos.X - (_recallSprite.Size.X/2), pos.Y - (_recallSprite.Size.Y/2));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return default(Vector2);
                            }
                        }
                    };
            }

            #endregion

            #region Properties

            public bool Active
            {
                private get { return _active; }
                set
                {
                    _active = value;
                    Update();
                }
            }

            #endregion

            #region Methods

            private void Update()
            {
                if (_sprite == null)
                    return;

                if (_active && !_added)
                {
                    _recallSprite.Add(0);
                    _sprite.Add(1);
                    _added = true;
                }
                else if (!_active && _added)
                {
                    _recallSprite.Remove();
                    _sprite.Remove();
                    _added = false;
                }
            }

            #endregion
        }

        #endregion
    }
}