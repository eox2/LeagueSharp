using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

//Credits to Kortatu for the Evade Spell Database 
//TO DO: Add/Fix Autoattack based cc's like Leona and Renekton Stun + Test script for the first time ingame

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
                                var casttime = 150; // need to reimplement evade database for casttimes i deleted them all fuckkkkkkk

                                if (!ability.Skillshot)
                                {
                                    if ((Game.Time - buff.EndTime) <= casttime)
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
                                }

                                if (ability.Skillshot)
                                {
                                    if (ability.CCSlot == SpellSlot.Q && (Game.Time - buff.EndTime) <= Q.Delay)
                                    {
                                        var predpos = Q.GetPrediction(hero);
                                        if (predpos != null)
                                        {
                                            Q.Cast(predpos.CastPosition);
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.W && (Game.Time - buff.EndTime) <= W.Delay)
                                    {
                                        var predpos = W.GetPrediction(hero);
                                        if (predpos != null)
                                        {
                                            W.Cast(predpos.CastPosition);
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.E && (Game.Time - buff.EndTime) <= E.Delay)
                                    {
                                        var predpos = E.GetPrediction(hero);
                                        if (predpos != null)
                                        {
                                            E.Cast(predpos.CastPosition);
                                        }
                                    }
                                    if (ability.CCSlot == SpellSlot.R && (Game.Time - buff.EndTime) <= R.Delay)
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


