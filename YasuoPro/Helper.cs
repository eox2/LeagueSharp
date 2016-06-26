using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace YasuoPro
{
    class Helper
    {

        internal static Obj_AI_Hero Yasuo;

        internal float QRadius = 170;

        internal static Obj_Shop shop;

        internal static bool DontDash = false;

        internal static int Q = 1, Q2 = 2, W = 3, E = 4, R = 5, Ignite = 6;

        internal const float LaneClearWaitTimeMod = 2f;

        internal static float WCLastE = 0f;

        internal static ItemManager.Item Hydra, Titanic, Tiamat, Blade, Bilgewater, Youmu;

        internal float LastTornadoClearTick;

        internal float LastDashTick;

        public static int TickCount
        {
            get { return (int)(Game.Time * 1000f); }
        }

        internal Orbwalking.Orbwalker Orbwalker
        {
            get { return YasuoMenu.Orbwalker; }
        }

        internal bool InDash
        {
            get { return TickCount - LastDashTick < 500; }
        }

        /* Credits to Brian for Q Skillshot values */
        internal static Dictionary<int, Spell> Spells;


        internal void InitSpells()
        {
            Spells =  new Dictionary<int, Spell> {
            { 1, new Spell(SpellSlot.Q, 450f) },
            { 2, new Spell(SpellSlot.Q, 1100f) },
            { 3, new Spell(SpellSlot.W, 450f) },
            { 4, new Spell(SpellSlot.E, 475f) },
            { 5, new Spell(SpellSlot.R, 1250f) },
            { 6, new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600) }
            };

            Spells[Q].SetSkillshot(GetQ1Delay, 20f, float.MaxValue, false, SkillshotType.SkillshotLine);
           
            Spells[Q2].SetSkillshot(GetQ2Delay, 90, 1500, false, SkillshotType.SkillshotLine);
            Spells[E].SetTargetted(0.075f, 1025);
        }

        private static float GetQDelay { get { return 1 - Math.Min((Yasuo.AttackSpeedMod - 1) * 0.0058552631578947f, 0.6675f); } }

        private static float GetQ1Delay { get { return 0.4f * GetQDelay; }  }

        private static float GetQ2Delay { get { return 0.5f * GetQDelay; } }


        internal float Qrange
        {
            get { return TornadoReady ? Spells[Q2].Range : Spells[Q].Range; }
        }

        internal float Qdelay
        {
            get
            {
                return 0.250f - (Math.Min(BonusAttackSpeed, 0.66f) * 0.250f);
            }
        }


        internal float BonusAttackSpeed
        {
            get
            {
                return (1 / Yasuo.AttackDelay) - 0.658f;
            }
        }

        internal float Erange
        {
            get { return Spells[E].Range; }
        }

        internal float Rrange
        {
            get { return Spells[R].Range; }
        }

        internal bool TornadoReady
        {
            get { return Yasuo.HasBuff("yasuoq3w"); }
        }

        internal static int DashCount
        {
            get
            {
                var bc = Yasuo.GetBuffCount("yasuodashscalar");
                return bc;
            }
        }

        internal float QLeftPCT
        {
            get
            {
                var buff = Yasuo.GetBuff("yasuoq3w");
                if (buff != null)
                {
                    var timeLeft = buff.EndTime - Game.Time;
                    var totalDuration = buff.EndTime - buff.StartTime;
                    var pctRemain = timeLeft/totalDuration;
                    return pctRemain;
                    
                }
                return 0;
            }
        }



        internal bool ShouldNormalQ(Obj_AI_Hero target)
        {
            var pos = GetDashPos(target);
            return QLeftPCT <= 30 || !TowerCheck(pos, true) || !target.IsDashable(Spells[E].Range * 3) || !targInKnockupRadius(pos.To3D());
        }


        internal bool UseQ(Obj_AI_Hero target, HitChance minhc = HitChance.Medium, bool UseQ1 = true, bool UseQ2 = true)
        {
            if (target == null)
            {
                return false;
            }

            if (Yasuo.IsDashing() || InDash)
            {
                return false;
            }

            var tready = TornadoReady;

            if ((tready && !UseQ2) || !tready && !UseQ1)
            {
                return false;
            }

            /*
            if (!ShouldNormalQ(target))
            {
                if (tready && GetBool("Combo.UseEQ"))
                {
                    if (Spells[E].IsReady() && target.IsDashable(500))
                    {
                        var dashPos = GetDashPos(target);
                        if (dashPos.To3D().CountEnemiesInRange(QRadius) >= 1)
                        {
                            //Cast E to trigger EQ 
                            if (GetBool("Misc.saveQ4QE") && isHealthy && GetBool("Combo.UseE") &&
                                (GetBool("Combo.ETower") || GetKeyBind("Misc.TowerDive") ||
                                 !GetDashPos(target).PointUnderEnemyTurret()))
                            {
                                return Spells[E].CastOnUnit(target);
                            }
                        }
                    }
                }
            }
            */

            

                Spell sp = tready ? Spells[Q2] : Spells[Q];
                PredictionOutput pred = sp.GetPrediction(target);

                if (pred.Hitchance >= minhc)
                {
                    return sp.Cast(pred.CastPosition);
                }

            return false;
        }

        internal IEnumerable<Obj_AI_Hero> KnockedUp
        {
            get
            {
                List<Obj_AI_Hero> KnockedUpEnemies = new List<Obj_AI_Hero>();
                foreach (var hero in HeroManager.Enemies)
                {
                    if (hero.IsValidEnemy(Spells[R].Range)) {
                        var knockup = hero.Buffs.Find(x => (x.Type == BuffType.Knockup && (x.EndTime - Game.Time) <= (GetSliderFloat("Combo.knockupremainingpct") / 100) * (x.EndTime - x.StartTime)) || x.Type == BuffType.Knockback);
                        if (knockup != null)
                        {
                            KnockedUpEnemies.Add(hero);
                        }
                    }
                }
                return KnockedUpEnemies;
            }
        }

        internal static bool isHealthy
        {
            get { return Yasuo.IsInvulnerable || Yasuo.HasBuffOfType(BuffType.Invulnerability) || Yasuo.HasBuffOfType(BuffType.SpellShield) || Yasuo.HasBuffOfType(BuffType.SpellImmunity) || Yasuo.HealthPercent > GetSliderFloat("Misc.Healthy") || Yasuo.HasBuff("yasuopassivemovementshield") && Yasuo.HealthPercent > 30; }
        }

        internal static bool GetBool(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<bool>();
        }

        internal static bool GetKeyBind(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<KeyBind>().Active;
        }

        internal static int GetSliderInt(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<Slider>().Value;
        }

        internal static float GetSliderFloat(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<Slider>().Value;
        }


        internal static int GetSL(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<StringList>().SelectedIndex;
        }

        internal static Circle GetCircle(string name)
        {
            return YasuoMenu.Config.Item(name).GetValue<Circle>();
        }

        internal static Vector2 DashPosition;

        internal static Vector2 GetDashPos(Obj_AI_Base @base)
        {
            var predictedposition = Yasuo.ServerPosition.Extend(@base.Position, Yasuo.Distance(@base) + 475 - Yasuo.Distance(@base)).To2D();
            DashPosition = predictedposition;
            return predictedposition;
        }

        internal static Vector2 GetDashPosFrom(Vector2 startPos, Obj_AI_Base @base)
        {
            var startPos3D = startPos.To3D();
            var predictedposition = startPos3D.Extend(@base.Position, startPos.Distance(@base) + 475 - startPos.Distance(@base)).To2D();
            DashPosition = predictedposition;
            return predictedposition;
        }

        internal static double GetProperEDamage(Obj_AI_Base target)
        {
            double dmg = Yasuo.GetSpellDamage(target, SpellSlot.E);
            float amplifier = 0;
            if (DashCount == 0)
            {
                amplifier = 0;
            }
            else if (DashCount == 1)
            {
                amplifier = 0.25f;
            }
            else if (DashCount == 2)
            {
                amplifier = 0.50f;
            }
            dmg += dmg * amplifier;
            return dmg;
        }

        internal static bool Debug
        {
            get { return GetBool("Misc.Debug"); }
        }

        internal static HitChance GetHitChance(String search)
        {
            var hitchance = YasuoMenu.Config.Item(search).GetValue<StringList>();
            switch (hitchance.SList[hitchance.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "VeryHigh":
                    return HitChance.VeryHigh;
            }
            return HitChance.Medium;
        }


        internal FleeType FleeMode
        {
            get
            {
                var GetFM = GetSL("Flee.Mode");
                if (GetFM == 0)
                {
                    return FleeType.ToNexus;
                }
                if (GetFM == 1)
                {
                    return FleeType.ToAllies;
                }
                return FleeType.ToCursor;
            }
        } 

        internal enum FleeType
        {
            ToNexus,
            ToAllies,
            ToCursor,
        }

        internal enum UltMode
        {
            Health,
            Priority, 
            EnemiesHit
        }

        internal UltMode GetUltMode()
        {
            switch (GetSL("Combo.UltMode"))
            {
                case 0:
                    return UltMode.Health;
                case 1:
                    return UltMode.Priority;
                case 2:
                    return UltMode.EnemiesHit;
            }
            return UltMode.Priority;
        }

        internal void InitItems()
        {
            Hydra = new ItemManager.Item(3074, 225f, ItemManager.ItemCastType.RangeCast, 1, 2);
            Tiamat = new ItemManager.Item(3077, 225f, ItemManager.ItemCastType.RangeCast, 1, 2);
            Titanic = new ItemManager.Item(3053, 225f, ItemManager.ItemCastType.RangeCast, 1, 2);
            Blade = new ItemManager.Item(3153, 450f, ItemManager.ItemCastType.TargettedCast, 1);
            Bilgewater = new ItemManager.Item(3144, 450f, ItemManager.ItemCastType.TargettedCast, 1);
            Youmu = new ItemManager.Item(3142, 185f, ItemManager.ItemCastType.SelfCast, 1, 3);
        }

        internal bool TowerCheck(Obj_AI_Base unit, bool isCombo = false)
        {
            return (isCombo && Helper.GetBool("Combo.ETower") || Helper.GetKeyBind("Misc.TowerDive") ||
                    !GetDashPos(unit).PointUnderEnemyTurret());
        }

        internal bool ShouldDive(Obj_AI_Base unit)
        {
            return Helper.GetKeyBind("Misc.TowerDive") || !Helper.GetDashPos(unit).PointUnderEnemyTurret();
        }

        internal bool TowerCheck(Vector2 pos, bool isCombo = false)
        {
            return (isCombo && Helper.GetBool("Combo.ETower") || Helper.GetKeyBind("Misc.TowerDive") ||
                    pos.PointUnderEnemyTurret());
        }

        internal bool targInKnockupRadius(Obj_AI_Hero targ)
        {
            var dpos = GetDashPos(targ).To3D();
            return dpos.CountEnemiesInRange(QRadius) > 0;
        }

        internal bool targInKnockupRadius(Vector3 dpos)
        {
            return dpos.CountEnemiesInRange(QRadius) > 0;
        }




    }
}
