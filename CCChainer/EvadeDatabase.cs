﻿// Copyright 2014 - 2014 Esk0r
// SpellDatabase.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace CCChainer
{
    public static class SpellDatabase
    {
        public static List<SpellDatabase.SpellData> Spells = new List<SpellData>();

        public class SpellData
        {
            public string ChampionName { get; set; }
            public string SpellName { get; set; }
            public SpellSlot Slot { get; set; }
            public SkillshotType Type { get; set; }
            public float Delay { get; set; }
            public float Range { get; set; }
            public float Radius { get; set; }
            public float MissileSpeed { get; set; }
            public string MissileSpellName { get; set; }
            public string[] ExtraSpellNames { get; set; }
            public string[] ExtraMissileNames { get; set; }
            public int ExtraDuration { get; set; }
            public string ToggleParticleName { get; set; }
            public bool DontCross { get; set; }
            public int ExtraRange { get; set; }
            public string FromObject { get; set; }
            public bool MissileDelayed { get; set; }
        }
        static SpellDatabase()
        {
            //Add spells to the database 

            #region Aatrox

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Aatrox",
                    SpellName = "AatroxQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 600,
                    Range = 650,
                    Radius = 250,
                    MissileSpeed = 2000,
                    MissileSpellName = "",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Aatrox",
                    SpellName = "AatroxE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1075,
                    Radius = 35,
                    MissileSpeed = 1250,
                    MissileSpellName = "AatroxEConeMissile",
                });

            #endregion Aatrox

            #region Ahri

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ahri",
                    SpellName = "AhriOrbofDeception",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 100,
                    MissileSpeed = 2500,
                    
                   
                    
            
                    MissileSpellName = "AhriOrbMissile",
                   
                    
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ahri",
                    SpellName = "AhriOrbReturn",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 100,
                    MissileSpeed = 60,

                    
                   
                    
            
                   
                   
                    
                    MissileSpellName = "AhriOrbReturn",
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ahri",
                    SpellName = "AhriSeduce",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 60,
                    MissileSpeed = 1550,
                    
                   
                    
                    
                    MissileSpellName = "AhriSeduceMissile",
                   
                   
                        
                });

            #endregion Ahri

            #region Amumu

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Amumu",
                    SpellName = "BandageToss",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 90,
                    MissileSpeed = 2000,
                    
                   
                    
                    
                    MissileSpellName = "SadMummyBandageToss",
                   
                   
                        
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Amumu",
                    SpellName = "CurseoftheSadMummy",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 550,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "",
                });

            #endregion Amumu

            #region Anivia

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Anivia",
                    SpellName = "FlashFrost",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 110,
                    MissileSpeed = 850,
                    
                   
                    
                    
                    MissileSpellName = "FlashFrostSpell",
                   
               
                });

            #endregion Anivia

            #region Annie

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Annie",
                    SpellName = "Incinerate",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCone,
                    Delay = 250,
                    Range = 825,
                    Radius = 80,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Annie",
                    SpellName = "InfernalGuardian",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 600,
                    Radius = 251,
                    MissileSpeed = int.MaxValue,
                   
                    MissileSpellName = "",
                });

            #endregion Annie

            #region Ashe

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ashe",
                    SpellName = "VolleyAttack",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 60,
                    MissileSpeed = 1500,
                    MissileSpellName = "VolleyAttack",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ashe",
                    SpellName = "EnchantedCrystalArrow",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 20000,
                    Radius = 130,
                    MissileSpeed = 1600,
                    MissileSpellName = "EnchantedCrystalArrow",
                   
               
                });

            #endregion Ashe

            #region Blitzcrank

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Blitzcrank",
                    SpellName = "RocketGrab",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 70,
                    MissileSpeed = 1800,
                    
                   
               
                    
                    MissileSpellName = "RocketGrabMissile",
                   
                   
                        
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Blitzcrank",
                    SpellName = "StaticField",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 600,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "",
                });

            #endregion Blitzcrank

            #region Brand

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Brand",
                    SpellName = "BrandBlaze",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 60,
                    MissileSpeed = 1600,
                    
                   
                    
                    
                    MissileSpellName = "BrandBlazeMissile",
                   
                   
                        
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Brand",
                    SpellName = "BrandFissure",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 850,
                    Range = 900,
                    Radius = 240,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "",
                });

            #endregion Brand

            #region Braum

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Braum",
                    SpellName = "BraumQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 60,
                    MissileSpeed = 1700,
                    
                   
                    
                    
                    MissileSpellName = "BraumQMissile",
                   
                   
                        
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Braum",
                    SpellName = "BraumRWrapper",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1200,
                    Radius = 115,
                    MissileSpeed = 1400,
                    
                   
               
                    
                    MissileSpellName = "braumrmissile",
               
                });

            #endregion Braum

            #region Caitlyn

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Caitlyn",
                    SpellName = "CaitlynPiltoverPeacemaker",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 625,
                    Range = 1300,
                    Radius = 90,
                    MissileSpeed = 2200,
                    
                   
                    
            
                    MissileSpellName = "CaitlynPiltoverPeacemaker",
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Caitlyn",
                    SpellName = "CaitlynEntrapment",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 125,
                    Range = 1000,
                    Radius = 80,
                    MissileSpeed = 2000,
                    
                   
                    
            
                    MissileSpellName = "CaitlynEntrapmentMissile",
                   
                   
                        
                });

            #endregion Caitlyn

            #region Cassiopeia

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Cassiopeia",
                    SpellName = "CassiopeiaNoxiousBlast",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 600,
                    Range = 850,
                    Radius = 150,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "CassiopeiaNoxiousBlast",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Cassiopeia",
                    SpellName = "CassiopeiaPetrifyingGaze",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCone,
                    Delay = 600,
                    Range = 825,
                    Radius = 80,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "CassiopeiaPetrifyingGaze",
                });

            #endregion Cassiopeia

            #region Chogath

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Chogath",
                    SpellName = "Rupture",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1200,
                    Range = 950,
                    Radius = 250,
                    MissileSpeed = int.MaxValue,
                    MissileSpellName = "Rupture",
                });

            #endregion Chogath

            #region Corki

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Corki",
                    SpellName = "PhosphorusBomb",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 300,
                    Range = 825,
                    Radius = 250,
                    MissileSpeed = 1000,
                    
                   
                    
            
                    MissileSpellName = "PhosphorusBombMissile",
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Corki",
                    SpellName = "MissileBarrage",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 200,
                    Range = 1300,
                    Radius = 40,
                    MissileSpeed = 2000,
                    MissileSpellName = "MissileBarrageMissile",
                   
                   
                        
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Corki",
                    SpellName = "MissileBarrage2",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 200,
                    Range = 1500,
                    Radius = 40,
                    MissileSpeed = 2000,
                    
                   
                    
            
                    MissileSpellName = "MissileBarrageMissile2",
                   
                   
                        
                });

            #endregion Corki

            #region Darius

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Darius",
                    SpellName = "DariusAxeGrabCone",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCone,
                    Delay = 300,
                    Range = 550,
                    Radius = 80,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "DariusAxeGrabCone",
                });

            #endregion Darius

            #region DrMundo

            Spells.Add(
                new SpellData
                {
                    ChampionName = "DrMundo",
                    SpellName = "InfectedCleaverMissileCast",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 60,
                    MissileSpeed = 2000,
                    
                   
                    
            
                    MissileSpellName = "InfectedCleaverMissile",
                   
                   
                        
                });

            #endregion DrMundo

            #region Draven

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Draven",
                    SpellName = "DravenDoubleShot",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 130,
                    MissileSpeed = 1400,
                    
                   
                    
                    
                    MissileSpellName = "DravenDoubleShotMissile",
                   
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Draven",
                    SpellName = "DravenRCast",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 400,
                    Range = 20000,
                    Radius = 160,
                    MissileSpeed = 2000,
                    
                   
                    
                    
                    MissileSpellName = "DravenR",
               
                });

            #endregion Draven

            #region Elise

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Elise",
                    SpellName = "EliseHumanE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 55,
                    MissileSpeed = 1450,
                    
                   
               
                    
                    MissileSpellName = "EliseHumanE",
                   
                   
                        
                });

            #endregion Elise

            #region Evelynn

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Evelynn",
                    SpellName = "EvelynnR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 650,
                    Radius = 350,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "EvelynnR",
                });

            #endregion Evelynn

            #region Ezreal

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ezreal",
                    SpellName = "EzrealMysticShot",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 60,
                    MissileSpeed = 2000,
                    MissileSpellName = "EzrealMysticShotMissile",

                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ezreal",
                    SpellName = "EzrealEssenceFlux",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 80,
                    MissileSpeed = 1600,
                    MissileSpellName = "EzrealEssenceFluxMissile",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ezreal",
                    SpellName = "EzrealTrueshotBarrage",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 1000,
                    Range = 20000,
                    Radius = 160,
                    MissileSpeed = 2000,
                    MissileSpellName = "EzrealTrueshotBarrage",

                });

            #endregion Ezreal

            #region Fizz

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Fizz",
                    SpellName = "FizzMarinerDoom",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 120,
                    MissileSpeed = 1350,
                  
                    MissileSpellName = "FizzMarinerDoomMissile",
              
                   
                });

            #endregion Fizz

            #region Galio

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Galio",
                    SpellName = "GalioResoluteSmite",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 900,
                    Radius = 200,
                    MissileSpeed = 1300,
                    MissileSpellName = "GalioResoluteSmite",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Galio",
                    SpellName = "GalioRighteousGust",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 120,
                    MissileSpeed = 1200,
                    MissileSpellName = "GalioRighteousGust",
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Galio",
                    SpellName = "GalioIdolOfDurand",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 550,
                    MissileSpeed = int.MaxValue,
                    MissileSpellName = "",
                });

            #endregion Galio

            #region Gnar

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1125,
                    Radius = 60,
                    MissileSpeed = 2500,
                    MissileSpellName = "gnarqmissile",
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarQReturn",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 2500,
                    Radius = 75,
                    MissileSpeed = 60,
                    MissileSpellName = "GnarQMissileReturn",
                  

                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarBigQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1150,
                    Radius = 90,
                    MissileSpeed = 2100,
                    MissileSpellName = "GnarBigQMissile",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarBigW",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 600,
                    Range = 600,
                    Radius = 80,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "GnarBigW",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 473,
                    Radius = 150,
                    MissileSpeed = 903,
                    
                   
                    
            
                    MissileSpellName = "GnarE",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarBigE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 475,
                    Radius = 200,
                    MissileSpeed = 1000,
                    
                   
                    
            
                    MissileSpellName = "GnarBigE",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 500,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "",
                });

            #endregion

            #region Gragas

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    SpellName = "GragasQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 1100,
                    Radius = 275,
                    MissileSpeed = 1300,
                    MissileSpellName = "GragasQMissile",
   
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    SpellName = "GragasE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 950,
                    Radius = 200,
                    MissileSpeed = 1200,
                    
                   
                    
            
                    MissileSpellName = "GragasE",
                   
                 
                  
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    SpellName = "GragasR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 1050,
                    Radius = 375,
                    MissileSpeed = 1800,
                    
                   
                    
                    
                    MissileSpellName = "GragasRBoom",
              
                });

            #endregion Gragas

            #region Graves

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Graves",
                    SpellName = "GravesClusterShot",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 50,
                    MissileSpeed = 2000,
                    
                   
                    
            
                    MissileSpellName = "GravesClusterShotAttack",

                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Graves",
                    SpellName = "GravesChargeShot",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 100,
                    MissileSpeed = 2100,
                    
                   
                    
                    
                    MissileSpellName = "GravesChargeShotShot",
                   
                       
                });

            #endregion Graves

            #region Heimerdinger

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Heimerdinger",
                    SpellName = "Heimerdingerwm",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1500,
                    Radius = 70,
                    MissileSpeed = 1800,
                    
                   
                    
            
                    MissileSpellName = "HeimerdingerWAttack2",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Heimerdinger",
                    SpellName = "HeimerdingerE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 925,
                    Radius = 100,
                    MissileSpeed = 1200,
                    
                   
                    
            
                    MissileSpellName = "heimerdingerespell",
              
                });

            #endregion Heimerdinger

            #region Irelia

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Irelia",
                    SpellName = "IreliaTranscendentBlades",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 1200,
                    Radius = 65,
                    MissileSpeed = 1600,
                    
                   
                    
            
                    MissileSpellName = "IreliaTranscendentBlades",
              
                });

            #endregion Irelia

            #region Janna

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Janna",
                    SpellName = "JannaQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1700,
                    Radius = 120,
                    MissileSpeed = 900,
                    
                   
                    
            
                    MissileSpellName = "HowlingGaleSpell",
              
                });

            #endregion Janna

            #region JarvanIV

            Spells.Add(
                new SpellData
                {
                    ChampionName = "JarvanIV",
                    SpellName = "JarvanIVDragonStrike",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 880,
                    Radius = 70,
                    MissileSpeed = 1450,
                    
                   
                    
                    
                    MissileSpellName = "JarvanIVDragonStrike",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "JarvanIV",
                    SpellName = "JarvanIVDemacianStandard",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 860,
                    Radius = 175,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "JarvanIVDemacianStandard",
                });

            #endregion JarvanIV

            #region Jayce

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jayce",
                    SpellName = "jayceshockblast",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 70,
                    MissileSpeed = 1450,
                    
                   
                    
            
                    MissileSpellName = "JayceShockBlastMis",
                   
                   
                       
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jayce",
                    SpellName = "JayceQAccel",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 70,
                    MissileSpeed = 2350,
                    
                   
                    
            
                    MissileSpellName = "JayceShockBlastWallMis",
                   
                   
                       
                });

            #endregion Jayce

            #region Jinx

            //TODO: Detect the animation from fow instead of the missile.
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jinx",
                    SpellName = "JinxW",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 600,
                    Range = 1500,
                    Radius = 60,
                    MissileSpeed = 3300,
                    
                   
                    
                    
                    MissileSpellName = "JinxWMissile",
                   
                   
                       
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Jinx",
                    SpellName = "JinxR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 600,
                    Range = 20000,
                    Radius = 140,
                    MissileSpeed = 1700,
                    
                   
                    
                    
                    MissileSpellName = "JinxR",
                   
              
                });

            #endregion Jinx

            #region Kalista

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kalista",
                    SpellName = "KalistaMysticShot",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 40,
                    MissileSpeed = 1700,
                   
                    MissileSpellName = "kalistamysticshotmis",

                   
                   
                       
                });

            #endregion Kalista

            #region Karma

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Karma",
                    SpellName = "KarmaQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 60,
                    MissileSpeed = 1700,
                    
                   
                    
            
                    MissileSpellName = "KarmaQMissile",
                   
                   
                       
                });

            //TODO: add the circle at the end.
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Karma",
                    SpellName = "KarmaQMantra",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 950,
                    Radius = 80,
                    MissileSpeed = 1700,
                    
                   
                    
            
                    MissileSpellName = "KarmaQMissileMantra",
                   
                   
                       
                });

            #endregion Karma

            #region Karthus

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Karthus",
                    SpellName = "KarthusLayWasteA2",

                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 625,
                    Range = 875,
                    Radius = 160,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "",
                });

            #endregion Karthus

            #region Kassadin

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kassadin",
                    SpellName = "RiftWalk",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 700,
                    Radius = 270,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "RiftWalk",
                });

            #endregion Kassadin

            #region Kennen

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kennen",
                    SpellName = "KennenShurikenHurlMissile1",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 125,
                    Range = 1050,
                    Radius = 50,
                    MissileSpeed = 1700,
                    
                   
                    
            
                    MissileSpellName = "KennenShurikenHurlMissile1",
                   
                   
                       
                });

            #endregion Kennen

            #region Khazix

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Khazix",
                    SpellName = "KhazixW",
                    ExtraSpellNames = new[] { "khazixwlong" },
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1025,
                    Radius = 73,
                    MissileSpeed = 1700,
                    MissileSpellName = "KhazixWMissile",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Khazix",
                    SpellName = "KhazixE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 600,
                    Radius = 300,
                    MissileSpeed = 1500,
                    MissileSpellName = "KhazixE",
                });

            #endregion Khazix

            #region Kogmaw

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kogmaw",
                    SpellName = "KogMawQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 70,
                    MissileSpeed = 1650,
                    
                   
                    
            
                    MissileSpellName = "KogMawQMis",
                   
                   
                       
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kogmaw",
                    SpellName = "KogMawVoidOoze",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1360,
                    Radius = 120,
                    MissileSpeed = 1400,
                    
                   
                    
            
                    MissileSpellName = "KogMawVoidOozeMissile",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Kogmaw",
                    SpellName = "KogMawLivingArtillery",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1200,
                    Range = 1800,
                    Radius = 150,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "KogMawLivingArtillery",
                });

            #endregion Kogmaw

            #region Leblanc

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leblanc",
                    SpellName = "LeblancSlide",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 600,
                    Radius = 220,
                    MissileSpeed = 1500,
                    
                   
                    
            
                    MissileSpellName = "LeblancSlide",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leblanc",
                    SpellName = "LeblancSlideM",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 600,
                    Radius = 220,
                    MissileSpeed = 1500,
                    
                   
                    
            
                    MissileSpellName = "LeblancSlideM",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leblanc",
                    SpellName = "LeblancSoulShackle",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 950,
                    Radius = 70,
                    MissileSpeed = 1600,
                    
                   
                    
                    
                    MissileSpellName = "LeblancSoulShackle",
                   
                   
                       
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leblanc",
                    SpellName = "LeblancSoulShackleM",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 950,
                    Radius = 70,
                    MissileSpeed = 1600,
                    
                   
                    
                    
                    MissileSpellName = "LeblancSoulShackleM",
                   
                   
                       
                });

            #endregion Leblanc

            #region LeeSin

            Spells.Add(
                new SpellData
                {
                    ChampionName = "LeeSin",
                    SpellName = "BlindMonkQOne",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 65,
                    MissileSpeed = 1800,
                    
                   
                    
                    
                    MissileSpellName = "BlindMonkQOne",
                   
                   
                       
                });

            #endregion LeeSin

            #region Leona

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leona",
                    SpellName = "LeonaZenithBlade",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 905,
                    Radius = 100,
                    MissileSpeed = 2000,
                    
                   
                    
                    
                    MissileSpellName = "LeonaZenithBladeMissile",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Leona",
                    SpellName = "LeonaSolarFlare",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1000,
                    Range = 1200,
                    Radius = 300,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "LeonaSolarFlare",
                });

            #endregion Leona

            #region Lissandra

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lissandra",
                    SpellName = "LissandraQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 700,
                    Radius = 75,
                    MissileSpeed = 2200,
                    
                   
                    
            
                    MissileSpellName = "LissandraQMissile",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lissandra",
                    SpellName = "LissandraQShards",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 700,
                    Radius = 90,
                    MissileSpeed = 2200,
                    
                   
                    
            
                    MissileSpellName = "lissandraqshards",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lissandra",
                    SpellName = "LissandraE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1025,
                    Radius = 125,
                    MissileSpeed = 850,
                    
                   
                    
            
                    MissileSpellName = "LissandraEMissile",
              
                });
            #endregion Lulu

            #region Lucian

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lucian",
                    SpellName = "LucianQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1300,
                    Radius = 65,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "LucianQ",
                });

            #endregion Lucian

            #region Lulu

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lulu",
                    SpellName = "LuluQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 950,
                    Radius = 60,
                    MissileSpeed = 1450,
                    
                   
                    
            
                    MissileSpellName = "LuluQMissile",
                    ExtraMissileNames = new[] { "LuluQMissileTwo" },
              
                });

            #endregion Lulu

            #region Lux

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lux",
                    SpellName = "LuxLightBinding",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 70,
                    MissileSpeed = 1200,
                    
                   
                    
                    
                    MissileSpellName = "LuxLightBindingMis",
                    //CanBeRemoved = true,
                    //CollisionObjects = new[] { CollisionObjectTypes.Champions, CollisionObjectTypes.Minion, CollisionObjectTypes.YasuoWall, },
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lux",
                    SpellName = "LuxLightStrikeKugel",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 1100,
                    Radius = 275,
                    MissileSpeed = 1300,
                    
                   
                    
            
                    MissileSpellName = "LuxLightStrikeKugel",
                    ExtraDuration = 5500,
                    ToggleParticleName = "LuxLightstrike_tar",
                    DontCross = true,
                   
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Lux",
                    SpellName = "LuxMaliceCannon",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 1000,
                    Range = 3500,
                    Radius = 190,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "LuxMaliceCannon",
                });

            #endregion Lux

            #region Malphite

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Malphite",
                    SpellName = "UFSlash",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 1000,
                    Radius = 270,
                    MissileSpeed = 1500,
                    
                   
                    
                    
                    MissileSpellName = "UFSlash",
                });

            #endregion Malphite

            #region Malzahar

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Malzahar",
                    SpellName = "AlZaharCalloftheVoid",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 1000,
                    Range = 900,
                    Radius = 85,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    DontCross = true,
                    MissileSpellName = "AlZaharCalloftheVoid",
                });

            #endregion Malzahar

            #region Morgana

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Morgana",
                    SpellName = "DarkBindingMissile",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 80,
                    MissileSpeed = 1200,
                    
                   
                    
                    
                    MissileSpellName = "DarkBindingMissile",
                   
                   
                       
                });

            #endregion Morgana

            #region Nami

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nami",
                    SpellName = "NamiQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1000,
                    Range = 1625,
                    Radius = 150,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "namiqmissile",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nami",
                    SpellName = "NamiR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 2750,
                    Radius = 260,
                    MissileSpeed = 850,
                    
                   
                    
            
                    MissileSpellName = "NamiRMissile",
              
                });

            #endregion Nami

            #region Nautilus

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nautilus",
                    SpellName = "NautilusAnchorDrag",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 90,
                    MissileSpeed = 2000,
                    
                   
                    
                    
                    MissileSpellName = "NautilusAnchorDragMissile",
                   
                   
                       
                    //walls?
                });

            #endregion Nautilus

            #region Nidalee

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Nidalee",
                    SpellName = "JavelinToss",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 125,
                    Range = 1500,
                    Radius = 40,
                    MissileSpeed = 1300,
                    
                   
                    
                    
                    MissileSpellName = "JavelinToss",
                   
                   
                       
                });

            #endregion Nidalee

            #region Olaf

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Olaf",
                    SpellName = "OlafAxeThrowCast",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    ExtraRange = 150,
                    Radius = 105,
                    MissileSpeed = 1600,
                    
                   
                    
            
                    MissileSpellName = "olafaxethrow",
                   
                   
                       
                });

            #endregion Olaf

            #region Orianna

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Orianna",
                    SpellName = "OriannasQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 1500,
                    Radius = 80,
                    MissileSpeed = 1200,
                    
                   
                    
            
                    MissileSpellName = "orianaizuna",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Orianna",
                    SpellName = "OriannaQend",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 1500,
                    Radius = 90,
                    MissileSpeed = 1200,
                    
                   
                    
            
                    MissileSpellName = "",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Orianna",
                    SpellName = "OrianaDissonanceCommand",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 255,
                    MissileSpeed = int.MaxValue,
                    
            
                    MissileSpellName = "OrianaDissonanceCommand",
                    FromObject = "yomu_ring_",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Orianna",
                    SpellName = "OriannasE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 1500,
                    Radius = 85,
                    MissileSpeed = 1850,
                    
                   
                    
            
                    MissileSpellName = "orianaredact",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Orianna",
                    SpellName = "OrianaDetonateCommand",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 700,
                    Range = 0,
                    Radius = 410,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "OrianaDetonateCommand",
                    FromObject = "yomu_ring_",
                });

            #endregion Orianna

            #region Quinn

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Quinn",
                    SpellName = "QuinnQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 80,
                    MissileSpeed = 1550,
                    
                   
                    
            
                    MissileSpellName = "QuinnQMissile",
                   
                   
                       
                });

            #endregion Quinn

            #region Rengar

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Rengar",
                    SpellName = "RengarE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 70,
                    MissileSpeed = 1500,
                    
                   
                    
                    
                    MissileSpellName = "RengarEFinal",
                   
                   
                       
                });

            #endregion Rengar

            #region RekSai

            Spells.Add(
                new SpellData
                {
                    ChampionName = "RekSai",
                    SpellName = "reksaiqburrowed",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1625,
                    Radius = 60,
                    MissileSpeed = 1950,
                    
                   
                    
            
                    MissileSpellName = "RekSaiQBurrowedMis",
                   
                       
                });

            #endregion RekSai

            #region Riven

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Riven",
                    SpellName = "rivenizunablade",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 125,
                    MissileSpeed = 2200,
                    
                   
                    MissileSpellName = "RivenLightsaberMissile",
                    ExtraMissileNames = new[] { "RivenLightsaberMissileSide" }
                });

            #endregion Riven

            #region Rumble

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Rumble",
                    SpellName = "RumbleGrenade",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 950,
                    Radius = 60,
                    MissileSpeed = 2000,
                    
                   
                    
            
                    MissileSpellName = "RumbleGrenade",
                   
                   
                       
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Rumble",
                    SpellName = "RumbleCarpetBombM",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 400,
                    MissileDelayed = true,
                    Range = 1200,
                    Radius = 200,
                    MissileSpeed = 1600,
                    
                   
               
            
                    MissileSpellName = "RumbleCarpetBombMissile",
               

                });

            #endregion Rumble

            #region Sejuani

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Sejuani",
                    SpellName = "SejuaniArcticAssault",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 900,
                    Radius = 70,
                    MissileSpeed = 1600,
                    
                   
                    
                    
                    MissileSpellName = "",
                    ExtraRange = 200,
                   
                       
                });
            //TODO: fix?
            Spells.Add(
                new SpellData
                {
                    ChampionName = "Sejuani",
                    SpellName = "SejuaniGlacialPrisonStart",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 110,
                    MissileSpeed = 1600,
                    
                   
                    
                    
                    MissileSpellName = "sejuaniglacialprison",
                   
              
                });

            #endregion Sejuani

            #region Sion

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Sion",
                    SpellName = "SionE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 800,
                    Radius = 80,
                    MissileSpeed = 1800,
                    
                   
                    
                    
                    MissileSpellName = "SionEMissile",
                   
                       
                });

            #endregion Sion

            #region Soraka

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Soraka",
                    SpellName = "SorakaQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 950,
                    Radius = 300,
                    MissileSpeed = 1750,
                    
                   
                    
            
                    MissileSpellName = "",
              
                });

            #endregion Soraka

            #region Shen

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Shen",
                    SpellName = "ShenShadowDash",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 650,
                    Radius = 50,
                    MissileSpeed = 1600,
                    
                   
                    
                    
                    MissileSpellName = "ShenShadowDash",
                    ExtraRange = 200,
                   
                      
                });

            #endregion Shen

            #region Shyvana

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Shyvana",
                    SpellName = "ShyvanaFireball",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 950,
                    Radius = 60,
                    MissileSpeed = 1700,
                    
                   
                    
            
                    MissileSpellName = "ShyvanaFireballMissile",
                   
                      
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Shyvana",
                    SpellName = "ShyvanaTransformCast",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 150,
                    MissileSpeed = 1500,
                    
                   
                    
                    
                    MissileSpellName = "ShyvanaTransformCast",
                    ExtraRange = 200,
                });

            #endregion Shyvana

            #region Sivir

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Sivir",
                    SpellName = "SivirQReturn",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 1250,
                    Radius = 100,
                    MissileSpeed = 1350,
                    
                   
                    
            
                    MissileSpellName = "SivirQMissileReturn",
                  
                   
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Sivir",
                    SpellName = "SivirQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1250,
                    Radius = 90,
                    MissileSpeed = 1350,
                    
                   
                    
            
                    MissileSpellName = "SivirQMissile",
              
                });

            #endregion Sivir

            #region Skarner

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Skarner",
                    SpellName = "SkarnerFracture",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 70,
                    MissileSpeed = 1500,
                    
                   
                    
            
                    MissileSpellName = "SkarnerFractureMissile",
              
                });

            #endregion Skarner

            #region Sona

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Sona",
                    SpellName = "SonaR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 140,
                    MissileSpeed = 2400,
                    
                   
                    
                    
                    MissileSpellName = "SonaR",
              
                });

            #endregion Sona

            #region Swain

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Swain",
                    SpellName = "SwainShadowGrasp",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1100,
                    Range = 900,
                    Radius = 180,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "SwainShadowGrasp",
                });

            #endregion Swain

            #region Syndra

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "SyndraQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 600,
                    Range = 800,
                    Radius = 150,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "SyndraQ",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "syndrawcast",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 950,
                    Radius = 210,
                    MissileSpeed = 1450,
                    
                   
                    
            
                    MissileSpellName = "syndrawcast",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "syndrae5",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 300,
                    Range = 950,
                    Radius = 90,
                    MissileSpeed = 1601,
                    
                   
                    
            
                    MissileSpellName = "syndrae5",
                  
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "SyndraE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 300,
                    Range = 950,
                    Radius = 90,
                    MissileSpeed = 1601,
                    
                   
                    
            
                  
                    MissileSpellName = "SyndraE",
                });

            #endregion Syndra

            #region Talon

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Talon",
                    SpellName = "TalonRake",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 800,
                    Radius = 80,
                    MissileSpeed = 2300,
                    MissileSpellName = "talonrakemissileone",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Talon",
                    SpellName = "TalonRakeReturn",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 800,
                    Radius = 80,
                    MissileSpeed = 1850,
                    
                   
                    
                    
                 
                    
                    MissileSpellName = "talonrakemissiletwo",
                });

            #endregion Riven

            #region Thresh

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Thresh",
                    SpellName = "ThreshQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1100,
                    Radius = 70,
                    MissileSpeed = 1900,
                    
                   
                    
                    
                    MissileSpellName = "ThreshQMissile",
                   
                   
                      
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Thresh",
                    SpellName = "ThreshEFlay",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 125,
                    Range = 1075,
                    Radius = 110,
                    MissileSpeed = 2000,
                    MissileSpellName = "ThreshEMissile1",
                });

            #endregion Thresh

            #region Tristana

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Tristana",
                    SpellName = "RocketJump",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 900,
                    Radius = 270,
                    MissileSpeed = 1500,
                    
                   
                    
            
                    MissileSpellName = "RocketJump",
                });

            #endregion Tristana

            #region Tryndamere

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Tryndamere",
                    SpellName = "slashCast",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 660,
                    Radius = 93,
                    MissileSpeed = 1300,
                    
                   
                    
            
                    MissileSpellName = "slashCast",
                });

            #endregion Tryndamere

            #region TwistedFate

            Spells.Add(
                new SpellData
                {
                    ChampionName = "TwistedFate",
                    SpellName = "WildCards",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1450,
                    Radius = 40,
                    MissileSpeed = 1000,
                    
                    MissileSpellName = "SealFateMissile",
                 
              
                });

            #endregion TwistedFate

            #region Twitch

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Twitch",
                    SpellName = "TwitchVenomCask",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 900,
                    Radius = 275,
                    MissileSpeed = 1400,
                    
                   
                    
            
                    MissileSpellName = "TwitchVenomCaskMissile",
              
                });

            #endregion Twitch

            #region Urgot

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Urgot",
                    SpellName = "UrgotHeatseekingLineMissile",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 125,
                    Range = 1000,
                    Radius = 60,
                    MissileSpeed = 1600,
                    
                   
                    
            
                    MissileSpellName = "UrgotHeatseekingLineMissile",
                   
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Urgot",
                    SpellName = "UrgotPlasmaGrenade",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 1100,
                    Radius = 210,
                    MissileSpeed = 1500,
                    
                   
                    
            
                    MissileSpellName = "UrgotPlasmaGrenadeBoom",
                });

            #endregion Urgot

            #region Varus

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Varus",
                    SpellName = "VarusQMissilee",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1800,
                    Radius = 70,
                    MissileSpeed = 1900,
                    
                   
                    
            
                    MissileSpellName = "VarusQMissile",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Varus",
                    SpellName = "VarusE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1000,
                    Range = 925,
                    Radius = 235,
                    MissileSpeed = 1500,
                    
                   
                    
            
                    MissileSpellName = "VarusE",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Varus",
                    SpellName = "VarusR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 100,
                    MissileSpeed = 1950,
                    
                   
                    
                    
                    MissileSpellName = "VarusRMissile",
                   
              
                });

            #endregion Varus

            #region Veigar

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Veigar",
                    SpellName = "VeigarDarkMatter",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1350,
                    Range = 900,
                    Radius = 225,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Veigar",
                    SpellName = "VeigarEventHorizon",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 600,
                    Radius = 80,
                    MissileSpeed = int.MaxValue,
                    ExtraDuration = 3000,
                    DontCross = true,
                    MissileSpellName = "",
                });

            #endregion Veigar

            #region Velkoz

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Velkoz",
                    SpellName = "VelkozQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 50,
                    MissileSpeed = 1300,
                    
                   
                    
            
                    MissileSpellName = "VelkozQMissile",
                   
                   
                      
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Velkoz",
                    SpellName = "VelkozQSplit",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 900,
                    Radius = 55,
                    MissileSpeed = 2100,
                    
                   
                    
            
                    MissileSpellName = "VelkozQMissileSplit",
                   
                   
                      
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Velkoz",
                    SpellName = "VelkozW",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 88,
                    MissileSpeed = 1700,
                    
                   
                    
            
                    MissileSpellName = "VelkozWMissile",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Velkoz",
                    SpellName = "VelkozE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 800,
                    Radius = 225,
                    MissileSpeed = 1500,
                    
                   
                    
            
                    MissileSpellName = "VelkozEMissile",
                });

            #endregion Velkoz

            #region Vi

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Vi",
                    SpellName = "Vi-q",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 90,
                    MissileSpeed = 1500,
                    
                   
                    
                    
                    MissileSpellName = "ViQMissile",
                });

            #endregion Vi

            #region Viktor

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Viktor",
                    SpellName = "Laser",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1500,
                    Radius = 80,
                    MissileSpeed = 780,
                    
                   
                    
            
                    MissileSpellName = "ViktorDeathRayFixMissile",
              
                });

            #endregion Viktor

            #region Xerath

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Xerath",
                    SpellName = "xeratharcanopulse2",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 600,
                    Range = 1600,
                    Radius = 100,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "xeratharcanopulse2",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Xerath",
                    SpellName = "XerathArcaneBarrage2",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 700,
                    Range = 1000,
                    Radius = 200,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "XerathArcaneBarrage2",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Xerath",
                    SpellName = "XerathMageSpear",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 200,
                    Range = 1150,
                    Radius = 60,
                    MissileSpeed = 1400,
                    
                   
                    
                    
                    MissileSpellName = "XerathMageSpearMissile",
                   
                   
                      
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Xerath",
                    SpellName = "xerathrmissilewrapper",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 700,
                    Range = 5600,
                    Radius = 120,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
                    
                    MissileSpellName = "xerathrmissilewrapper",
                });

            #endregion Xerath

            #region Yasuo

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Yasuo",
                    SpellName = "yasuoq2",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 400,
                    Range = 550,
                    Radius = 20,
                    MissileSpeed = int.MaxValue,
                    MissileSpellName = "yasuoq2",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Yasuo",
                    SpellName = "yasuoq3w",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1150,
                    Radius = 90,
                    MissileSpeed = 1500,
                    
                   
                    
                    
                    MissileSpellName = "yasuoq3w",
               
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Yasuo",
                    SpellName = "yasuoq",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 400,
                    Range = 550,
                    Radius = 20,
                    MissileSpeed = int.MaxValue,
                    MissileSpellName = "yasuoq",
                });

            #endregion Yasuo

            #region Zac

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Zac",
                    SpellName = "ZacQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 550,
                    Radius = 120,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "ZacQ",
                });

            #endregion Zac

            #region Zed

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Zed",
                    SpellName = "ZedShuriken",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 925,
                    Radius = 50,
                    MissileSpeed = 1700,
                    
                    MissileSpellName = "zedshurikenmisone",
                    ExtraMissileNames = new[] { "zedshurikenmistwo", "zedshurikenmisthree" },
              
                });

            #endregion Zed

            #region Ziggs

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ziggs",
                    SpellName = "ZiggsQ",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 850,
                    Radius = 140,
                    MissileSpeed = 1700,
                    
                   
                    
            
                    MissileSpellName = "ZiggsQSpell",
               
                  
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ziggs",
                    SpellName = "ZiggsQBounce1",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 850,
                    Radius = 140,
                    MissileSpeed = 1700,
                    
                    MissileSpellName = "ZiggsQSpell2",
                    ExtraMissileNames = new[] { "ZiggsQSpell2" },
               
                  
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ziggs",
                    SpellName = "ZiggsQBounce2",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 850,
                    Radius = 160,
                    MissileSpeed = 1700,
                    
                   
                    
            
                    MissileSpellName = "ZiggsQSpell3",
                    ExtraMissileNames = new[] { "ZiggsQSpell3" },
               
                  
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ziggs",
                    SpellName = "ZiggsW",
                    Slot = SpellSlot.W,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 1000,
                    Radius = 275,
                    MissileSpeed = 1750,
                    
                   
                    
            
                    MissileSpellName = "ZiggsW",
                  
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ziggs",
                    SpellName = "ZiggsE",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 900,
                    Radius = 235,
                    MissileSpeed = 1750,
                    
                   
                    
            
                    MissileSpellName = "ZiggsE",
                  
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Ziggs",
                    SpellName = "ZiggsR",
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 5300,
                    Radius = 500,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "ZiggsR",
                  
                });

            #endregion Ziggs

            #region Zyra

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Zyra",
                    SpellName = "ZyraQFissure",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 600,
                    Range = 800,
                    Radius = 220,
                    MissileSpeed = int.MaxValue,
                    
                   
                    
            
                    MissileSpellName = "ZyraQFissure",
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Zyra",
                    SpellName = "ZyraGraspingRoots",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1150,
                    Radius = 70,
                    MissileSpeed = 1150,
                    
                   
                    
                    
                    MissileSpellName = "ZyraGraspingRoots",
              
                });

            Spells.Add(
                new SpellData
                {
                    ChampionName = "Zyra",
                    SpellName = "zyrapassivedeathmanager",
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 700,
                    Range = 1474,
                    Radius = 70,
                    MissileSpeed = 2000,
                   
                    MissileSpellName = "zyrapassivedeathmanager",
              
                });

            #endregion Zyra

            //Game.PrintChat("Added " + Spells.Count + " spells.");
        }

        public static List<SpellData> GetByChampName(string Champname)
        {
            Champname = Champname.ToLower();
            List<SpellData> AbilityList = new List<SpellData>();
            foreach (var spellData in Spells)
            {
                if (spellData.ChampionName == Champname)
                {
                    AbilityList.Add(spellData);
                }
            }
            return AbilityList;
        }

        public static SpellData GetByName(string spellName)
        {
            spellName = spellName.ToLower();
            foreach (var spellData in Spells)
            {
                if (spellData.SpellName.ToLower() == spellName || spellData.ExtraSpellNames.Contains(spellName))
                {
                    return spellData;
                }
            }
            return null;
        }

        public static SpellData GetByMissileName(string missileSpellName)
        {
            missileSpellName = missileSpellName.ToLower();
            foreach (var spellData in Spells)
            {
                if (spellData.MissileSpellName != null && spellData.MissileSpellName.ToLower() == missileSpellName ||
                    spellData.ExtraMissileNames.Contains(missileSpellName))
                {
                    return spellData;
                }
            }

            return null;
        }

    }
}
