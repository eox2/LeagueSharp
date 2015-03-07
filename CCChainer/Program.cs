using System;
using System.Collections.Generic;
using System.Linq;
using CCChainer.Data;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

//Credits to Kortatu for the Evade Spell Database 

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
            Game.PrintChat("CC Chainer Loaded - Beta");
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
                spells.AddSubMenu(SpellMenu);
            }
            Config.AddSubMenu(spells);

            misc.AddItem(new MenuItem("slowpct", "Slowed % to consider CC").SetValue(new Slider(40, 0, 100)));
            misc.AddItem(new MenuItem("hpct", "Min targ health %").SetValue(new Slider(0, 0, 100)));
            Config.AddSubMenu(misc);
            Config.AddToMainMenu();

        }

        private static bool CustomDelays()
        {
            return Config.Item("customdelays").GetValue<bool>();
        }

        private static bool CCenabled(string ccname)
        {
            return Config.Item("Enabled" + ccname).GetValue<bool>();
        }

        private static void Debug()
        {
            foreach (var buff in Player.Buffs)
            {
                Console.Write(buff.Name);
                if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt ||
                    buff.Type == BuffType.Charm ||
                    buff.Type == BuffType.Fear || buff.Type == BuffType.Suppression)
                {
                    Console.WriteLine("CC Duration " + buff.Name + " " + (buff.EndTime - buff.StartTime));

                }

            }
        }


        private static void OnGameUpdate(EventArgs args)
        {
            // Debug();
            CCChain();
        }


        private static void CCChain()
        {
           
            foreach (var skill in PlayerCCs.Where(c => CCenabled(c.CCname)))
            {
                var ability = skill;
                var slot = skill.CCSlot;
                if (!slot.IsReady())
                {
                    return;
                }
                foreach (
                    var hero in
                        ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy && Player.Distance(h) <= ability.range && Player.HealthPercent >= Config.Item("hpct").GetValue<Slider>().Value))
                {
                    var target = hero;
                    var tenacity = hero.PercentCCReduction;

                    foreach (var buff in hero.Buffs)
                    {
                        var buffType = buff.Type;
                        var buffEndTime = buff.EndTime - (tenacity*(buff.EndTime - buff.StartTime)); //actual
                        if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt ||
                            buff.Type == BuffType.Charm ||
                            buff.Type == BuffType.Fear || buff.Type == BuffType.Knockup ||
                            buff.Type == BuffType.Polymorph || buff.Type == BuffType.Snare ||
                            buff.Type == BuffType.Suppression || buff.Type == BuffType.Flee || buff.Type == BuffType.Slow && target.MoveSpeed <= ((Config.Item("slowpct").GetValue<Slider>().Value / 100f) * target.MoveSpeed))
                        {
                            //Game.PrintChat(buff.Name + " The buff end time is on  " + hero.ChampionName + " is" + buff.EndTime + "and the current time is" + Game.Time);
                            /*
                                var EvadeData2 = SpellDatabase.GetByName(ability.Skillshotname);
                                var lastpossibletime2 = buffEndTime - (R.Delay) - (Player.Distance(target) / EvadeData2.MissileSpeed) -
                                                             Game.Ping;
                                Game.PrintChat(buff.Name + " " + buffEndTime + " cc red " + target.PercentCCReduction + " actual duration " + (buffEndTime - Game.Time) + " apparent duration " + (buff.EndTime - Game.Time) + " last poss " + lastpossibletime2);
                                */
                            var totalcctime = (buffEndTime - buff.StartTime);
                            var cctimeleft = buffEndTime - Game.Time;

                            var casttime = 25; // IdK delays for targetted spells maybe implement in future

                            if (!ability.Skillshot)
                            {
                                if (cctimeleft >= casttime)
                                {
                                    var lastpossibletime = buffEndTime - casttime;
                                    var delayby = lastpossibletime - Game.Time;
                                    if (ability.CCSlot == SpellSlot.Q && !delayingq)
                                    {
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

                                    }

                                    if (ability.CCSlot == SpellSlot.W && !delayingw)
                                    {
                                        delayingw = true;
                                        Utility.DelayAction.Add((int) delayby, () =>
                                        {
                                            delayingw = false;
                                            W.CastOnUnit(target);

                                        });
                    
                                    }
                                    if (ability.CCSlot == SpellSlot.E && !delayinge)
                                    {
                                        delayinge = true;
                                        Utility.DelayAction.Add((int) delayby, () =>
                                        {
                                            delayinge = false;
                                            E.CastOnUnit(target);
                                        });
                    
                                    }
                                    if (ability.CCSlot == SpellSlot.R && !delayingr)
                                    {
                                        delayingr = true;
                                        Utility.DelayAction.Add((int) delayby, () =>
                                        {
                                            delayingr = false;
                                            R.CastOnUnit(target);

                                        });
          
                                    }
                                }
                            }


                            if (ability.Skillshot)
                            {
                                var EvadeData = SpellDatabase.GetByName(ability.Skillshotname);
                                var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);

                                if (ability.CCSlot == SpellSlot.Q && !delayingq)
                                {
                                    var lastpossibletime = buffEndTime - (dist/EvadeData.MissileSpeed + Q.Delay) - (Game.Ping / 1000f);
                                    var delayby = lastpossibletime - Game.Time;
                                    if (Game.Time <= lastpossibletime)
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
                                            CastSpell(Q, buffType, target, lastpossibletime);

                                        });
                                    }
                                }

                                if (ability.CCSlot == SpellSlot.W && !delayingw)
                                {
                                    var lastpossibletime = buff.EndTime - (dist / EvadeData.MissileSpeed + W.Delay) - (Game.Ping / 1000f);
                                    var delayby = lastpossibletime - Game.Time;
                                  //  Console.WriteLine(buff.DisplayName + " W - last possible " + lastpossibletime + " delay amt" + delayby + " current " + Game.Time);
                                    if (Game.Time <= lastpossibletime)
                                    {
                                        delayingw = true;
                                        Utility.DelayAction.Add((int)delayby, () =>
                                        {
                                            CastSpell(W, buffType, target, lastpossibletime);
                                            delayingw = false;
                                        });
                                    }
                                }
                                if (ability.CCSlot == SpellSlot.E && !delayinge)
                                {
                                    var lastpossibletime = buff.EndTime - (dist/EvadeData.MissileSpeed + E.Delay) - (Game.Ping / 1000f);
                                    var delayby = lastpossibletime - Game.Time;
                                   // Console.WriteLine(buff.DisplayName + " last possible " + lastpossibletime + " delay amt" + delayby + " current " + Game.Time);
                                    if (Game.Time <= lastpossibletime)
                                    {
                                        delayinge = true;
                                        Utility.DelayAction.Add((int)delayby, () =>
                                        {
                                            CastSpell(E, buffType, target, lastpossibletime);
                                            delayinge = false;
                                        });
                                    }
                                }
                                if (ability.CCSlot == SpellSlot.R && !delayingr)
                                {
                                    var lastpossibletime = buff.EndTime - (dist/EvadeData.MissileSpeed + R.Delay) - (Game.Ping / 1000f);
                                    var delayby = lastpossibletime - Game.Time;
                                    // Console.WriteLine(buff.DisplayName + " Bufftype " + buffType + " R - last possible " + lastpossibletime + " delay amt" + delayby + " current " + Game.Time);
                                    if (Game.Time <= lastpossibletime)
                                    {
                                        delayingr = true;
                                        Utility.DelayAction.Add((int) delayby, () =>
                                        {
                                            CastSpell(R, buffType, target, lastpossibletime);
                                            delayingr = false;
                                        });
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        private static void CastSpell(Spell spell, BuffType x, Obj_AI_Hero target, float lastpostime)
        {
            if (x == BuffType.Stun || x == BuffType.Snare || x == BuffType.Knockup || x == BuffType.Fear)
            {
                if (Game.Time <= lastpostime)
                {
                    spell.Cast(target.ServerPosition);
                }
            }
            else
            {
                
                var pred = spell.GetPrediction(target);
                if (pred != null && Game.Time <= lastpostime)
                {
                    spell.Cast(pred.CastPosition);
                }
            }
        }


        static Spell GetByslot(SpellSlot slot)
        {
            Spell spell = null;
            switch (slot)
            {
                case SpellSlot.Q:
                    spell = Q;
                    break;
                case SpellSlot.W:
                    spell = Q;
                    break;
                case SpellSlot.E:
                    spell = Q;
                    break;
                case SpellSlot.R:
                    spell = Q;
                    break;
            }
            return spell;
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


