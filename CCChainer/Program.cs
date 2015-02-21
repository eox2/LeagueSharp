using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using CCChainer.Data;
using LeagueSharp;
using LeagueSharp.Common;

//Credits to Kortatu for the Evade Spell Database 
//TO DO: Add/Fix Autoattack based cc's like Leona and Renekton Stun + Test script for the first time ingame

namespace CCChainer
{
    internal class Program
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;
        private static List<CCDatabase.CCData> PlayerCCs = new List<CCDatabase.CCData>();
        private static bool delayingq, delayingw, delayinge, delayingr;
        public static Spell Q, W, E, R;
        private static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            Game.PrintChat("CC Chainer Loaded - WIP INCOMPLETE BY SEPH WTF");
            DefineSpells();
            CreateMenu();
            Game.OnGameUpdate += OnGameUpdate;
        }


        private static void CreateMenu()
        {
            Config = new Menu("CC Chainer", "ccc", true);
            var spells = new Menu("Spells", "spells");
            var misc = new Menu("Misc", "misc");
            foreach (var x in PlayerCCs)
            {
                var SpellMenu = new Menu(x.CCname, x.CCname);
                SpellMenu.AddItem(new MenuItem("Enabled" + x.CCname, "Enabled").SetValue(true));
                SpellMenu.AddItem(new MenuItem("percent" + x.CCname, "CC duration past (%)").SetValue(new Slider(30, 0, 100)));
                spells.AddSubMenu(SpellMenu);
            }
            misc.AddItem(new MenuItem("customdelays", "Use % CC duration").SetValue(false));
            Config.AddSubMenu(spells);
            Config.AddSubMenu(misc);
            Config.AddToMainMenu();
     
        }

        private static float CCdurationpast(string ccname)
        {
            return Config.Item("percent" + ccname).GetValue<Slider>().Value / 100f;
        }

        private static bool CustomDelays()
        {
            return Config.Item("customdelays").GetValue<bool>();
        }

       

        private static void OnGameUpdate(EventArgs args)
        {
            CCChain();
        }


        private static void CCChain()
        {
            foreach (var skill in PlayerCCs)
            {
                var ability = skill;
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
                {
                    var target = hero;
                    if (hero.Distance(Player) <= ability.range)
                    { 
                        foreach (var buff in hero.Buffs)
                        {
                           // Game.PrintChat(buff.Name + " " + buff.StartTime);
                            if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt || buff.Type == BuffType.Charm ||
                                buff.Type == BuffType.Fear)
                            { 
                              //  Game.PrintChat("The buff end time is on  " + hero.ChampionName + " is" + buff.EndTime + "and the current time is" + Game.Time);

                                var totalcctime = buff.EndTime - buff.StartTime;
                                var cctimeleft = buff.EndTime - Game.Time;
                                var percentcc = cctimeleft/totalcctime;
                                var casttime = 25; // IdK delays for targetted spells maybe implement in future

                                if (!ability.Skillshot)
                                {
                                    if ((Game.Time - buff.EndTime) <= casttime)
                                    {
                                        var lastpossibletime = buff.EndTime - casttime - 100;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (percentcc >= CCdurationpast(ability.CCname))
                                        {
                                            if (ability.CCSlot == SpellSlot.Q && !delayingq)
                                            {
                                                if (ability.SelfthenAuto)
                                                {
                                                    SpellLogic.CastSpellSelfAuto(Q, target);
                                                    return;
                                                }
                                                delayingq = true;
                                                Utility.DelayAction.Add((int) delayby, () =>
                                                {
                                                    if (ability.SelfthenAuto)
                                                    {
                                                        SpellLogic.CastSpellSelfAuto(Q, target);
                                                        return;
                                                    }
                                                    delayingq = false;
                                                    Q.CastOnUnit(target);
                                                    
                                                });
                                              //  Q.Cast(hero);
                                            }

                                            if (ability.CCSlot == SpellSlot.W && !delayingw)
                                            {
                                                delayingw = true;
                                                Utility.DelayAction.Add((int)delayby, () =>
                                                {
                                                    delayingw = false;
                                                    W.CastOnUnit(target);

                                                });
                                               // W.Cast(hero);
                                            }
                                            if (ability.CCSlot == SpellSlot.E && !delayinge)
                                            {
                                                delayinge = true;
                                                Utility.DelayAction.Add((int)delayby, () =>
                                                {
                                                    delayinge = false;
                                                    E.CastOnUnit(target);
                                                });
                                               // E.CastOnUnit(hero);
                                            }
                                            if (ability.CCSlot == SpellSlot.R && !delayingr)
                                            {
                                                delayingr = true;
                                                Utility.DelayAction.Add((int)delayby, () =>
                                                {
                                                    delayingr = false;
                                                    R.CastOnUnit(target);

                                                });
                                               // R.Cast(hero);
                                            }
                                        }
                                    }
                                }

                                if (ability.Skillshot)
                                {
                                    if (ability.CCSlot == SpellSlot.Q && (Game.Time - buff.EndTime) <= Q.Delay && !delayingq)
                                    {
                                        var lastpossibletime = buff.EndTime - Q.Delay - 100;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (CustomDelays())
                                        {
                                            if (percentcc >= CCdurationpast(ability.CCname))
                                            {
                                                if (ability.Skillshotname == "JannaQ")
                                                {
                                                    var JannaQPred = Q.GetPrediction(target);
                                                    SpellLogic.JannaQ(JannaQPred.CastPosition);
                                                    return;
                                                }
                                                var predpos = Q.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    Q.Cast(predpos.CastPosition);
                                                }
                                            }
                                        }

                                        if (!CustomDelays())
                                        {
                                            delayingq = true;
                                            Utility.DelayAction.Add((int) delayby, () =>
                                            {
                                                if (ability.Skillshotname == "JannaQ")
                                                {
                                                    var JannaQPred = Q.GetPrediction(target);
                                                    SpellLogic.JannaQ(JannaQPred.CastPosition);
                                                    return;
                                                }
                                                delayingq = false;
                                                var predpos = Q.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    Q.Cast(predpos.CastPosition);
                                                }

                                            });
                                        }
                                    }

                                    if (ability.CCSlot == SpellSlot.W && (Game.Time - buff.EndTime) <= W.Delay && !delayingw)
                                    {
                                        var lastpossibletime = buff.EndTime - W.Delay - 100;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (CustomDelays())
                                        {
                                            if (percentcc >= CCdurationpast(ability.CCname))
                                            {
                                                var predpos = W.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    W.Cast(predpos.CastPosition);
                                                }
                                            }
                                        }

                                        if (!CustomDelays())
                                        {
                                            delayingw = true;
                                            Utility.DelayAction.Add((int)delayby, () =>
                                            {
                                                delayingw = false;
                                                var predpos = W.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    W.Cast(predpos.CastPosition);
                                                }

                                            });
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.E && (Game.Time - buff.EndTime) <= E.Delay && !delayinge)
                                    {
                                        var lastpossibletime = buff.EndTime - E.Delay - 100;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (CustomDelays())
                                        {
                                            if (percentcc >= CCdurationpast(ability.CCname))
                                            {
                                                var predpos = E.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    E.Cast(predpos.CastPosition);
                                                }
                                            }
                                        }

                                        if (!CustomDelays())
                                        {
                                            delayinge = true;
                                            Utility.DelayAction.Add((int)delayby, () =>
                                            {
                                                delayinge = false;
                                                var predpos = E.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    E.Cast(predpos.CastPosition);
                                                }

                                            });
                                        }
                                    }
                                    var data = SpellDatabase.GetByName(ability.CCname);
                                    if (ability.CCSlot == SpellSlot.R && (Game.Time - buff.EndTime) <= R.Delay && !delayingr)
                                    {
                                        var lastpossibletime = buff.EndTime - R.Delay - 100;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (CustomDelays())
                                        {
                                            if (percentcc >= CCdurationpast(ability.CCname))
                                            {
                                                var predpos = R.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    R.Cast(predpos.CastPosition);
                                                }
                                            }
                                        }

                                        if (!CustomDelays())
                                        {
                                            delayingr = true;
                                            Utility.DelayAction.Add((int)delayby, () =>
                                            {
                                                delayingr = false;
                                                var predpos = R.GetPrediction(target);
                                                if (predpos != null)
                                                {
                                                    R.Cast(predpos.CastPosition);
                                                }

                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static LeagueSharp.Common.SkillshotType ConvertSkillShotType(CCChainer.Data.SkillShotType Type)
        {
            var result = new SkillshotType();
            switch (Type)
            {
                case SkillShotType.SkillshotCircle:
                    result = SkillshotType.SkillshotCircle;
                    break;

                case SkillShotType.SkillshotMissileLine:
                case SkillShotType.SkillshotLine:
                    result = SkillshotType.SkillshotLine;
                    break;

                case SkillShotType.SkillshotMissileCone:
                case SkillShotType.SkillshotCone:
                    result = SkillshotType.SkillshotCone;
                    break;
            }
            return result;
        }

        private static void DefineSpells()
        {
            PlayerCCs = CCDatabase.GetByChampName(ObjectManager.Player.ChampionName);
            foreach (var x in PlayerCCs)
            {
                if (x.CCSlot == SpellSlot.Q)
                {
                    Q = new Spell(x.CCSlot, x.range);
                    if (x.Skillshot)
                    {
                        var EvadeData = SpellDatabase.GetByName(x.Skillshotname);
                        Q.SetSkillshot(EvadeData.Delay / 1000f, EvadeData.Radius, EvadeData.MissileSpeed, EvadeData.CollisionObjects.Contains(CollisionObjectTypes.Champions), ConvertSkillShotType(EvadeData.Type));
                    }
                }
                if (x.CCSlot == SpellSlot.W)
                {
                    W = new Spell(x.CCSlot, x.range);
                    if (x.Skillshot)
                    {
                        var EvadeData = SpellDatabase.GetByName(x.Skillshotname);
                        W.SetSkillshot(EvadeData.Delay / 1000f, EvadeData.Radius, EvadeData.MissileSpeed, EvadeData.CollisionObjects.Contains(CollisionObjectTypes.Champions), ConvertSkillShotType(EvadeData.Type));
                    }
                }
                if (x.CCSlot == SpellSlot.E)
                {
                    E = new Spell(x.CCSlot, x.range);
                    if (x.Skillshot)
                    {
                        var EvadeData = SpellDatabase.GetByName(x.Skillshotname);
                        E.SetSkillshot(EvadeData.Delay / 1000f, EvadeData.Radius, EvadeData.MissileSpeed, EvadeData.CollisionObjects.Contains(CollisionObjectTypes.Champions), ConvertSkillShotType(EvadeData.Type));
                    }

                }
                if (x.CCSlot == SpellSlot.R)
                {
                    R = new Spell(x.CCSlot, x.range);
                    if (x.Skillshot)
                    {
                        var EvadeData = SpellDatabase.GetByName(x.Skillshotname);
                        R.SetSkillshot(EvadeData.Delay / 1000f, EvadeData.Radius, EvadeData.MissileSpeed, EvadeData.CollisionObjects.Contains(CollisionObjectTypes.Champions), ConvertSkillShotType(EvadeData.Type));
                    }
                }
            }

        }
    }
}


