using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

//Credits to Kortatu for the Evade Spell Database 

namespace CCChainer
{
    internal class Program
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static List<CCDatabase.CCData> PlayerCCs = new List<CCDatabase.CCData>();
        private static Spell Q, W, E, R;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            Game.PrintChat("CC Chainer Loaded - WIP INCOMPLETE BY SEPH");
            DefineSpells();
            Game.OnGameUpdate += OnGameUpdate;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            CCChain();
        }


        private static void CCChain()
        {
            foreach (var ability in PlayerCCs)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
                {
                    if (hero.Distance(Player) <= ability.range)
                    {
                        foreach (var buff in hero.Buffs)
                        {
                            if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt || buff.Type == BuffType.Charm ||
                                buff.Type == BuffType.Fear || hero.IsMovementImpaired() || hero.MoveSpeed <= 0.60 * hero.MoveSpeed)
                            {
                                if (!ability.Skillshot)
                                {
                                    if (ability.CCSlot == SpellSlot.Q)
                                    {
                                        Q.Cast(hero);
                                    }
                                    if (ability.CCSlot == SpellSlot.W)
                                    {
                                        W.Cast(hero);
                                    }
                                    if (ability.CCSlot == SpellSlot.E)
                                    {
                                        E.Cast(hero);
                                    }
                                    if (ability.CCSlot == SpellSlot.R)
                                    {
                                        R.Cast(hero);
                                    }
                                }
                                if (ability.Skillshot)
                                {
                                    if (ability.CCSlot == SpellSlot.Q)
                                    {
                                        var predpos = Q.GetPrediction(hero);
                                        if (predpos != null)
                                        {
                                            Q.Cast(predpos.CastPosition);
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.W)
                                    {
                                        var predpos = W.GetPrediction(hero);
                                        if (predpos != null)
                                        {
                                            W.Cast(predpos.CastPosition);
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.E)
                                    {
                                        var predpos = E.GetPrediction(hero);
                                        if (predpos != null)
                                        {
                                            E.Cast(predpos.CastPosition);
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.R)
                                    {
                                        var predpos = R.GetPrediction(hero);
                                        if (predpos != null)
                                        {
                                            R.Cast(predpos.CastPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                        Q.SetSkillshot(EvadeData.Delay, EvadeData.Radius, EvadeData.MissileSpeed, x.GoesThroughMinions, EvadeData.Type);
                    }
                }
                if (x.CCSlot == SpellSlot.W)
                {
                    W = new Spell(x.CCSlot, x.range);
                    if (x.Skillshot)
                    {
                        var EvadeData = SpellDatabase.GetByName(x.Skillshotname);
                        W.SetSkillshot(EvadeData.Delay, EvadeData.Radius, EvadeData.MissileSpeed, x.GoesThroughMinions, EvadeData.Type);
                    }
                }
                if (x.CCSlot == SpellSlot.E)
                {
                    E = new Spell(x.CCSlot, x.range);
                    if (x.Skillshot)
                    {
                        var EvadeData = SpellDatabase.GetByName(x.Skillshotname);
                        E.SetSkillshot(EvadeData.Delay, EvadeData.Radius, EvadeData.MissileSpeed, x.GoesThroughMinions, EvadeData.Type);
                    }

                }
                if (x.CCSlot == SpellSlot.R)
                {
                    R = new Spell(x.CCSlot, x.range);
                    if (x.Skillshot)
                    {
                        var EvadeData = SpellDatabase.GetByName(x.Skillshotname);
                        R.SetSkillshot(EvadeData.Delay, EvadeData.Radius, EvadeData.MissileSpeed, x.GoesThroughMinions, EvadeData.Type);
                    }
                }
            }

        }
    }
}


