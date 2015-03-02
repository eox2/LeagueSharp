using System;
using System.Collections.Generic;
using System.Linq;
using CCChainer.Data;
using LeagueSharp;
using LeagueSharp.Common;

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
                SpellMenu.AddItem(
                    new MenuItem("percent" + x.CCname, "CC duration past (%)").SetValue(new Slider(30, 0, 100)));
                spells.AddSubMenu(SpellMenu);
            }
            misc.AddItem(new MenuItem("customdelays", "Use % CC duration (only for testing)").SetValue(false));
            Config.AddSubMenu(spells);
            Config.AddSubMenu(misc);
            Config.AddToMainMenu();

        }

        private static float CCdurationpast(string ccname)
        {
            return Config.Item("percent" + ccname).GetValue<Slider>().Value/100f;
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
     
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy && Player.Distance(h) <= ability.range))
                {
                        var target = hero;
                        var tenacity = hero.PercentCCReduction;
                   
                        foreach (var buff in hero.Buffs)
                        {
                            var buffEndTime = buff.EndTime - (tenacity * (buff.EndTime - buff.StartTime)); //actual
                            var bufftype = buff.Type;
                            // Game.PrintChat(buff.Name + " " + buff.StartTime);
                            if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt ||
                                 buff.Type == BuffType.Charm ||
                                 buff.Type == BuffType.Fear || buff.Type == BuffType.Suppression)
                            {
                                //Game.PrintChat(buff.Name + " The buff end time is on  " + hero.ChampionName + " is" + buff.EndTime + "and the current time is" + Game.Time);

                                var totalcctime = (buffEndTime - buff.StartTime);
                                var cctimeleft = buffEndTime - Game.Time;
                                var percentcc = cctimeleft/totalcctime;

                                var casttime = 25; // IdK delays for targetted spells maybe implement in future

                                if (!ability.Skillshot)
                                {
                                    if ((buffEndTime - Game.Time) >= casttime)
                                    {
                                        var lastpossibletime = buffEndTime - casttime - 200;
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
                                                Utility.DelayAction.Add((int) delayby, () =>
                                                {
                                                    delayingw = false;
                                                    W.CastOnUnit(target);

                                                });
                                                // W.Cast(hero);
                                            }
                                            if (ability.CCSlot == SpellSlot.E && !delayinge)
                                            {
                                                delayinge = true;
                                                Utility.DelayAction.Add((int) delayby, () =>
                                                {
                                                    delayinge = false;
                                                    E.CastOnUnit(target);
                                                });
                                                // E.CastOnUnit(hero);
                                            }
                                            if (ability.CCSlot == SpellSlot.R && !delayingr)
                                            {
                                                delayingr = true;
                                                Utility.DelayAction.Add((int) delayby, () =>
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
                                    if (bufftype == BuffType.Fear)
                                    {
                                        //Work on this
                                        return;
                                    }

                                    var EvadeData = SpellDatabase.GetByName(ability.Skillshotname);
                                    var dist = Player.Distance(target.ServerPosition.To2D());

                                    if (ability.CCSlot == SpellSlot.Q && !delayingq)
                                    {
                                        var lastpossibletime = buffEndTime - (Q.Delay / 1000f) - (dist/EvadeData.MissileSpeed) -
                                                               Game.Ping;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (Game.Time <= lastpossibletime)
                                        {
                                            if (CustomDelays())
                                            {
                                                if (percentcc >= CCdurationpast(ability.CCname))
                                                {
                                                    /*
                                                    if (bufftype == BuffType.Fear)
                                                    {
                                                        var pred = Q.GetPrediction(target);
                                                        Q.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                     * */
                                                    if (ability.Skillshotname == "JannaQ")
                                                    {
                                                        var JannaQPred = Q.GetPrediction(target);
                                                        SpellLogic.JannaQ(JannaQPred.CastPosition);
                                                        return;
                                                    }
                                                    var pred = Q.GetPrediction(target);
                                                    if (pred != null)
                                                    {
                                                        Q.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                    Q.Cast(target.ServerPosition);
                                                    return;
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
                                                    var pred = Q.GetPrediction(target);
                                                    if (pred != null)
                                                    {
                                                        Q.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                    Q.Cast(target.ServerPosition);
       
                                                });
                                            }
                                        }
                                    }

                                    if (ability.CCSlot == SpellSlot.W && !delayingw)
                                    {
                                        var lastpossibletime = buffEndTime - (W.Delay / 1000f) - (dist / EvadeData.MissileSpeed) -
                                                               Game.Ping - 200;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (Game.Time <= lastpossibletime)
                                        {
                                            if (CustomDelays())
                                            {
                                                if (percentcc >= CCdurationpast(ability.CCname))
                                                {
                                                    var pred = W.GetPrediction(target);
                                                    if (pred != null)
                                                    {
                                                        W.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                    W.Cast(target.ServerPosition);
                                                    return;
                                                }
                                            }

                                            if (!CustomDelays())
                                            {
                                                delayingw = true;
                                                Utility.DelayAction.Add((int) delayby, () =>
                                                {
                                                    delayingw = false;
                                                    var pred = W.GetPrediction(target);
                                                    if (pred != null)
                                                    {
                                                        W.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                    W.Cast(target.ServerPosition);
                                                    return;

                                                });
                                            }
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.E && !delayinge)
                                    {
                                        var lastpossibletime = buffEndTime - (E.Delay / 1000f) - (dist / EvadeData.MissileSpeed) -
                                                               Game.Ping;
                                        var delayby = lastpossibletime - Game.Time;
                                        if (Game.Time <= lastpossibletime)
                                        {
                                            if (CustomDelays())
                                            {
                                                if (percentcc >= CCdurationpast(ability.CCname))
                                                {
                                                    var pred = E.GetPrediction(target);
                                                    if (pred != null)
                                                    {
                                                        E.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                    E.Cast(target.ServerPosition);
                                                    return;
                                                }
                                            }

                                            if (!CustomDelays())
                                            {
                                                delayinge = true;
                                                Utility.DelayAction.Add((int) delayby, () =>
                                                {
                                                    delayinge = false;
                                                    var pred = E.GetPrediction(target);
                                                    if (pred != null)
                                                    {
                                                        E.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                    E.Cast(target.ServerPosition);
                                                    return;

                                                });
                                            }
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.R && !delayingr)
                                    {
                                        var lastpossibletime = buffEndTime - (R.Delay / 1000f) - (dist / EvadeData.MissileSpeed) -
                                                               Game.Ping - 200; 
                                        var delayby = lastpossibletime - Game.Time;
                                        if (Game.Time <= lastpossibletime)
                                        {
                                            if (CustomDelays())
                                            {
                                                if (percentcc >= CCdurationpast(ability.CCname))
                                                {
                                                    var pred = R.GetPrediction(target);
                                                    if (pred != null)
                                                    {
                                                        R.Cast(pred.CastPosition);
                                                        return;
                                                    }
                                                    R.Cast(target.ServerPosition);
                                                    return;

                                                }
                                            }
                                        }

                                        if (!CustomDelays())
                                        {
                                            delayingr = true;
                                            Utility.DelayAction.Add((int) delayby, () =>
                                            {
                                                delayingr = false;
                                                var pred = R.GetPrediction(target);
                                                if (pred != null)
                                                {
                                                    R.Cast(pred.CastPosition);
                                                    return;
                                                }
                                                R.Cast(target.ServerPosition);
                                                return;

                                            });
                                        }
                                    }
                                }
                            }
                        
                    }
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


