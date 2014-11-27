#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 CooldownTracker.cs is part of SFXUtility.
 
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
    using Color = SharpDX.Color;
    using Rectangle = SharpDX.Rectangle;
    using Utilities = Class.Utilities;

    #endregion

    internal class CooldownTracker : Base
    {
        #region Fields

        private readonly List<Cooldown> _cooldowns = new List<Cooldown>();
        private Trackers _trackers;

        #endregion

        #region Constructors

        public CooldownTracker(IContainer container)
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
            get { return "Cooldown"; }
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
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
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

                    var eEMenuItem = new MenuItem(Name + "EnemyEnabled", "Track Enemy").SetValue(true);
                    var aAMenuItem = new MenuItem(Name + "AllyEnabled", "Track Ally").SetValue(true);
                    var eMenuItem = new MenuItem(Name + "Enabled", "Enabled").SetValue(true);

                    Menu.AddItem(eEMenuItem);
                    Menu.AddItem(aAMenuItem);
                    Menu.AddItem(eMenuItem);

                    eEMenuItem.ValueChanged +=
                        (sender, args) =>
                            _cooldowns.ForEach(
                                cd =>
                                    cd.Active =
                                        Menu.Item(Name + "Enabled").GetValue<bool>() &&
                                        (cd.Hero.IsEnemy && args.GetNewValue<bool>() ||
                                         cd.Hero.IsAlly && Menu.Item(Name + "AllyEnabled").GetValue<bool>()));

                    aAMenuItem.ValueChanged +=
                        (sender, args) =>
                            _cooldowns.ForEach(
                                cd =>
                                    cd.Active =
                                        Menu.Item(Name + "Enabled").GetValue<bool>() &&
                                        (cd.Hero.IsEnemy && Menu.Item(Name + "EnemyEnabled").GetValue<bool>() ||
                                         cd.Hero.IsAlly && args.GetNewValue<bool>()));

                    eMenuItem.ValueChanged +=
                        (sender, args) =>
                            _cooldowns.ForEach(
                                cd =>
                                    cd.Active =
                                        args.GetNewValue<bool>() &&
                                        (cd.Hero.IsEnemy && Menu.Item(Name + "EnemyEnabled").GetValue<bool>() ||
                                         cd.Hero.IsAlly && Menu.Item(Name + "AllyEnabled").GetValue<bool>()));

                    _trackers.Menu.AddSubMenu(Menu);

                    foreach (
                        Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && !hero.IsMe))
                    {
                        try
                        {
                            _cooldowns.Add(new Cooldown(hero)
                            {
                                Active =
                                    Enabled &&
                                    (hero.IsEnemy && Menu.Item(Name + "EnemyEnabled").GetValue<bool>() ||
                                     hero.IsAlly && Menu.Item(Name + "AllyEnabled").GetValue<bool>())
                            });
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

        private class Cooldown
        {
            #region Fields

            public readonly Obj_AI_Hero Hero;
            private readonly Render.Sprite _hudSprite;
            private readonly List<Render.Line> _spellLines = new List<Render.Line>();
            private readonly SpellSlot[] _spellSlots = {SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R};
            private readonly List<Render.Text> _spellTexts = new List<Render.Text>();

           // private readonly SpellSlot[] _summonerSpellSlots = {SpellSlot.Q, SpellSlot.W};
             private readonly SpellSlot[] _summonerSpellSlots = { ((SpellSlot) 4), ((SpellSlot) 5) };
            private readonly List<Render.Text> _summonerSpellTexts = new List<Render.Text>();
            private readonly List<Render.Sprite> _summonerSprites = new List<Render.Sprite>();

            private bool _active;
            private bool _added;

            #endregion

            #region Constructors

            public Cooldown(Obj_AI_Hero hero)
            {
                Hero = hero;
                _hudSprite =
                    new Render.Sprite(Resources.CD_Hud, default(Vector2))
                    {
                        VisibleCondition = delegate
                        {
                            try
                            {
                                return Visible;
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
                                return HpBarPostion;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return default(Vector2);
                            }
                        }
                    };

             //  Game.PrintChat(_summonerSpellSlots.Length.ToString());
                for (int i = 0; i < _summonerSpellSlots.Length; i++) 
                {
              // foreach (var sSlot in _summonerSpellSlots) {
                   //for (int i = 0; i < _summonerSpellSlots.Length; i++) 
                    var index = i;
                    var spell = Hero.SummonerSpellbook.GetSpell(_summonerSpellSlots[index]);
                   // Game.PrintChat(spell.Name);
                    var summoner = Resources.ResourceManager.GetObject(string.Format("CD_{0}", spell.Name.ToLower())) ??
                                   Resources.CD_summonerbarrier;
                    var sprite = new Render.Sprite((Bitmap) summoner, default(Vector2))
                    {
                        VisibleCondition = delegate
                        {
                            try
                            {
                                return Visible;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return false;
                            }
                        }
                    };
                    sprite.PositionUpdate = delegate
                    {
                        try
                        {
                            sprite.Crop(new Rectangle(0,
                                12*
                                ((spell.CooldownExpires - Game.Time > 0)
                                    ? (int)
                                        (19*
                                         (1f -
                                          ((Math.Abs(spell.Cooldown) > float.Epsilon)
                                              ? (spell.CooldownExpires - Game.Time)/spell.Cooldown
                                              : 1f)))
                                    : 19), 12, 12));
                            return new Vector2(HpBarPostion.X + 3, HpBarPostion.Y + 1 + index*13);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return default(Vector2);
                        }
                    };
                    var text = new Render.Text(default(Vector2), string.Empty, index*13, Color.White)
                    {
                        VisibleCondition = delegate
                        {
                            try
                            {
                                return Visible;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return false;
                            }
                        }
                    };
                    text.PositionUpdate =
                        delegate
                        {
                            try
                            {
                                return new Vector2(HpBarPostion.X - 5 - text.text.Length*5,
                                    HpBarPostion.Y + 1 + 13*index);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return default(Vector2);
                            }
                        };
                    text.TextUpdate = delegate
                    {
                        try
                        {
                            return spell.CooldownExpires - Game.Time > 0f
                                ? string.Format(spell.CooldownExpires - Game.Time < 1f ? "{0:0.0}" : "{0:0}",
                                    spell.CooldownExpires - Game.Time)
                                : string.Empty;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return string.Empty;
                        }
                    };
                    _summonerSprites.Add(sprite);
                    _summonerSpellTexts.Add(text);
                }

                for (int i = 0; i < _spellSlots.Length; i++)
                {
                    var index = i;
                    var spell = Hero.Spellbook.GetSpell(_spellSlots[index]);
                    var line = new Render.Line(default(Vector2), default(Vector2), 4, Color.Green)
                    {
                        VisibleCondition = delegate
                        {
                            try
                            {
                                return Visible &&
                                       Hero.Spellbook.CanUseSpell(_spellSlots[index]) != SpellState.NotLearned;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return false;
                            }
                        },
                        StartPositionUpdate = delegate
                        {
                            try
                            {
                                return new Vector2(HpBarPostion.X + 18f + index*27f, HpBarPostion.Y + 20f);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return default(Vector2);
                            }
                        }
                    };
                    line.EndPositionUpdate =
                        delegate
                        {
                            try
                            {
                                line.Color = spell.CooldownExpires - Game.Time <= 0f ? Color.Green : Color.DeepSkyBlue;
                                return
                                    new Vector2(
                                        line.Start.X +
                                        ((spell.CooldownExpires - Game.Time > 0f &&
                                          Math.Abs(spell.Cooldown) > float.Epsilon)
                                            ? 1f - ((spell.CooldownExpires - Game.Time)/spell.Cooldown)
                                            : 1f)*23f, line.Start.Y);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return default(Vector2);
                            }
                        };
                    var text = new Render.Text(default(Vector2), string.Empty, 13, Color.White)
                    {
                        VisibleCondition = delegate
                        {
                            try
                            {
                                return Visible;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return false;
                            }
                        }
                    };
                    text.PositionUpdate =
                        delegate
                        {
                            try
                            {
                                return new Vector2(line.Start.X + (23f - text.text.Length*4)/2, line.Start.Y + 7f);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                return default(Vector2);
                            }
                        };
                    text.TextUpdate = delegate
                    {
                        try
                        {
                            return spell.CooldownExpires - Game.Time > 0f
                                ? string.Format(spell.CooldownExpires - Game.Time < 1f ? "{0:0.0}" : "{0:0}",
                                    spell.CooldownExpires - Game.Time)
                                : string.Empty;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return string.Empty;
                        }
                    };
                    _spellLines.Add(line);
                    _spellTexts.Add(text);
                }
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

            private Vector2 HpBarPostion
            {
                get { return new Vector2(Hero.HPBarPosition.X + -9, Hero.HPBarPosition.Y + (Hero.IsEnemy ? 17 : 14)); }
            }

            private bool Visible
            {
                get
                {
                    return Active && Hero.IsVisible && !Hero.IsDead && Hero.IsHPBarRendered &&
                           Utilities.IsOnScreen(HpBarPostion, new Vector2(HpBarPostion.X + 150f, HpBarPostion.Y + 30f));
                }
            }

            #endregion

            #region Methods

            private void Update()
            {
                if (_active && !_added)
                {
                    _hudSprite.Add(0);
                    _summonerSprites.ForEach(sp => sp.Add(1));
                    _spellLines.ForEach(sp => sp.Add(2));
                    _spellTexts.ForEach(sp => sp.Add(3));
                    _summonerSpellTexts.ForEach(sp => sp.Add(3));
                    _added = true;
                }
                else if (!_active && _added)
                {
                    _hudSprite.Remove();
                    _summonerSprites.ForEach(sp => sp.Remove());
                    _spellLines.ForEach(sp => sp.Remove());
                    _spellTexts.ForEach(sp => sp.Remove());
                    _summonerSpellTexts.ForEach(sp => sp.Remove());
                    _added = false;
                }
            }

            #endregion
        }

        #endregion
    }
}
