using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


//ToDo: Better Evolved W calcs, figure out why OnWaveClear activates when farm keybind not active

namespace SephKhazix
{
    internal class Program
    {
        private const string ChampionName = "Khazix";
        private static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, WE;
        private const float Wangle = 22 * (float) Math.PI / 180;
        private static Menu Config;
        private static Items.Item HDR;
        private static Items.Item TIA;
        private static Items.Item BKR;
        private static Items.Item BWC;
        private static Items.Item YOU;
        private static SpellSlot IgniteSlot;
        private static List<Obj_AI_Hero> HeroList;

        private static Obj_AI_Hero Player;
        private static bool Wnorm, Wevolved;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName)
                return;
            
            Q = new Spell(SpellSlot.Q, 325f);
            W = new Spell(SpellSlot.W, 1000f);
            WE = new Spell(SpellSlot.W, 10000f);
            E = new Spell(SpellSlot.E, 600f);
            R = new Spell(SpellSlot.R, 0);

            W.SetSkillshot(0.225f, 80f, 828.5f, true, SkillshotType.SkillshotLine);
            WE.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);

            E.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);


            HDR = new Items.Item(3074, 225f);
            TIA = new Items.Item(3077, 225f);
            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 185f);


            IgniteSlot = Player.GetSpellSlot("summonerdot");



            Config = new Menu("SephKhazix", "Khazix", true);


            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //AutoHarrass
            Config.AddSubMenu(new Menu("Auto Harrass/Poke", "autopoke"));
            Config.SubMenu("autopoke").AddItem(new MenuItem("AutoHarrass", "AutoHarrass")).SetValue(true);
            Config.SubMenu("autopoke").AddItem(new MenuItem("AutoWI", "Auto-W immobile")).SetValue(true);
            Config.SubMenu("autopoke").AddItem(new MenuItem("AutoWD", "Auto W")).SetValue(true);
            Config.SubMenu("autopoke").AddItem(new MenuItem("AutoWHitchance", "W Hit Chance").SetValue(new StringList(new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));


            //Packets
            Config.AddSubMenu(new Menu("Packet Setting", "Packets"));
            Config.SubMenu("Packets").AddItem(new MenuItem("usePackets", "Enable Packets").SetValue(false));

            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEGapclose", "Use E To Gapclose for Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEGapcloseW", "Use E To Gapclose For W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRGapcloseW", "Use R after long gapcloses")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("Combo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));


            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("Harass", "Harass key").SetValue(
                        new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            Config.AddSubMenu(new Menu("Farm/Clear", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q")).SetValue(true);
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E")).SetValue(false);
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W")).SetValue(true);
            Config.SubMenu("Farm").AddItem(new MenuItem("UseItemsFarm", "Use Items")).SetValue(true);
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("WaveFarm", "Farm Key").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("JungleFarm", "Jungle Key").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Kill Steal
            Config.AddSubMenu(new Menu("KillSteal", "Ks"));
            Config.SubMenu("Ks").AddItem(new MenuItem("Kson", "Use KillSteal")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseWKs", "Use W")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEKs", "Use E")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEQKs", "Use EQ in KS")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEWKs", "Use EW in KS")).SetValue(false);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseTiaKs", "Use items")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("Edelay", "E Delay (ms)").SetValue(new Slider(0, 2, 300)));
            Config.SubMenu("Ks").AddItem(new MenuItem("autoescape", "Use E to get out when low")).SetValue(false);

            Config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            //Debug
            Config.AddSubMenu(new Menu("Debug", "Debug"));
            Config.SubMenu("Debug").AddItem(new MenuItem("Debugon", "Enable Debugging").SetValue(false));

            Config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("<font color='#1d87f2'>SephKhazix has been Loaded. Version 1.6. Lots of changes, report problems on main topic</font>");
            HeroList = ObjectManager.Get<Obj_AI_Hero>().ToList();

        }

        private static void CastWE(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0)
        {
            var usePacket = Config.Item("usePackets").GetValue<bool>();
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            Vector2 startPoint = ObjectManager.Player.ServerPosition.To2D();
            Vector2 originalDirection = W.Range * (unitPosition - startPoint).Normalized();

            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    PredictionOutput pos = W.GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int) enemy.BoundingRadius);
                    }
                }
            }


            var posiblePositions = new List<Vector2>();

            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                    posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1)
                    posiblePositions.Add(startPoint + originalDirection.Rotated(Wangle));
                if (i == 2)
                    posiblePositions.Add(startPoint + originalDirection.Rotated(-Wangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = posiblePositions[i];
                    Vector2 direction = (pos - startPoint).Normalized().Perpendicular();
                    float k = (2 / 3 * (unit.BoundingRadius + Q.Width));
                    posiblePositions.Add(startPoint - k * direction);
                    posiblePositions.Add(startPoint + k * direction);
                }
            }

            var bestPosition = new Vector2();
            int bestHit = -1;

            foreach (Vector2 position in posiblePositions)
            {
                int hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit + 1 <= minTargets)
                return;

            W.Cast(bestPosition.To3D(), usePacket);
        }


        private static int CountHits(Vector2 position, List<Vector2> points, List<int> hitBoxes)
        {
            int result = 0;

            Vector2 startPoint = ObjectManager.Player.ServerPosition.To2D();
            Vector2 originalDirection = Q.Range * (position - startPoint).Normalized();
            Vector2 originalEndPoint = startPoint + originalDirection;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                for (int k = 0; k < 3; k++)
                {
                    var endPoint = new Vector2();
                    if (k == 0)
                        endPoint = originalEndPoint;
                    if (k == 1)
                        endPoint = startPoint + originalDirection.Rotated(Wangle);
                    if (k == 2)
                        endPoint = startPoint + originalDirection.Rotated(-Wangle);

                    if (point.Distance(startPoint, endPoint, true, true) <
                        (W.Width + hitBoxes[i]) * (W.Width + hitBoxes[i]))
                    {
                        result++;
                        break;
                    }
                }
            }

            return result;
        }

        private static void OnGameUpdate(EventArgs args)
        {
           // Orbwalker.SetAttack(true);

            CheckSpells();
            if (Config.Item("Combo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("Harass").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (Config.Item("AutoHarrass").GetValue<bool>())
            {
                AutoHarrass();
            }

            if (Config.Item("WaveFarm").GetValue<KeyBind>().Active)
            {
                OnWaveClear();
            }
            if (Config.Item("JungleFarm").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (Config.Item("Kson").GetValue<bool>())
            {
                KillSteal();
            }
        }

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (target != null)
            {
                var usePacket = Config.Item("usePackets").GetValue<bool>();
                if (Player.Distance(target) <= Q.Range && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
                {
                    Orbwalker.SetAttack(false);
                    Q.Cast(target, usePacket);
                    Orbwalker.SetAttack(true);
                }

                if (Player.Distance(target) <= W.Range && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
                    Wnorm)
                {
                    W.Cast(target, usePacket);
                }
                if (Player.Distance(target) <= W.Range && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
                    Wevolved)
                {
                    W.Cast(target, usePacket);
                }
            }
        }


        private static void JungleClear()
        {
            var pos = new List<Vector2>();
            List<Obj_AI_Base> mobs = MinionManager.GetMinions(
                Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
            foreach (Obj_AI_Base minion in mobs)
            {
                if (minion != null)
                {
                    pos.Add(minion.Position.To2D());
                }
                // Orbwalker.SetAttacks(!(Q.IsReady() || W.IsReady() || E.IsReady()) || TIA.IsReady() || HDR.IsReady());
                // Normal Farms
                if (Q.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= Q.Range &&
                    Config.Item("UseQFarm").GetValue<bool>())
                {
                    Orbwalker.SetAttack(false);
                    Q.Cast(minion);
                    Orbwalker.SetAttack(true);
                }
                if (W.IsReady() && minion.IsValidTarget() && Wnorm && Player.Distance(minion) <= W.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);

                    W.Cast(pred.Position);
                }
                if (E.IsReady() && minion.IsValidTarget() && Player.Distance(minion) <= E.Range &&
                    Config.Item("UseEFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestCircularFarmLocation(pos, 300, 600);
                    E.Cast(pred.Position);
                }
                //Evolved
    
                if (W.IsReady() && minion.IsValidTarget() && Wevolved && Player.Distance(minion) <= WE.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);

                    W.Cast(pred.Position);
                }
                if (Config.Item("UseItems").GetValue<bool>())
                {
                    if (HDR.IsReady() && Player.Distance(minion) <= HDR.Range)
                    {
                        HDR.Cast();
                        // Items.UseItem(3077, ObjectManager.Player);
                    }
                    if (TIA.IsReady() && Player.Distance(minion) <= TIA.Range)
                    {
                        TIA.Cast();
                    }
                }
            }
        }


        private static void OnWaveClear()
        {
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                foreach (Obj_AI_Base minion in
                    allMinions.Where(
                        minion =>
                            minion.IsValidTarget() &&
                            HealthPrediction.GetHealthPrediction(
                                minion, (int) (ObjectManager.Player.Distance(minion) * 1000 / 1400)) <
                            0.75 * Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && Player.Distance(minion) <= Q.Range)
                    {
                        Orbwalker.SetAttack(false);
                        Q.CastOnUnit(minion, false);
                        Orbwalker.SetAttack(true);
                        return;
                    }
                }

            }
            if (Config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
            {
                MinionManager.FarmLocation farmLocation =
              MinionManager.GetBestCircularFarmLocation(
                  MinionManager.GetMinions(Player.Position, W.Range)
                      .Select(minion => minion.ServerPosition.To2D())
                      .ToList(), W.Width, W.Range);
                if (Wnorm && !Wevolved)
                {
          

                    if (Player.Distance(farmLocation.Position) <= W.Range)
                        W.Cast(farmLocation.Position);
                }
                if (Wevolved && !Wnorm)
                {

                    if (Player.Distance(farmLocation.Position) <= WE.Range)
                        W.Cast(farmLocation.Position);
                }
            }

            if (Config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {

                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Player.Position, E.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), E.Width, E.Range);

                if (Player.Distance(farmLocation.Position) <= W.Range)
                    E.Cast(farmLocation.Position);
            }


            if (Config.Item("UseItemsFarm").GetValue<bool>())
            {
                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Player.Position, HDR.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), HDR.Range, HDR.Range);

                if (HDR.IsReady() && Player.Distance(farmLocation.Position) <= HDR.Range && farmLocation.MinionsHit >= 2)
                {
                    // HDR.Cast(farmLocation.Position);
                    // HDR.Cast();
                    Items.UseItem(3074, ObjectManager.Player);
                }
                if (TIA.IsReady() && Player.Distance(farmLocation.Position) <= TIA.Range && farmLocation.MinionsHit >= 2)
                {
                    //TIA.Cast(farmLocation.Position);
                    // TIA.Cast();
                    Items.UseItem(3077, ObjectManager.Player);
                }
            }
        }

        //Detuks
        public static bool targetisisolated(Obj_AI_Base target)
        {
            var enes = ObjectManager.Get<Obj_AI_Base>()
                .Where(her => her.IsEnemy && her.NetworkId != target.NetworkId && target.Distance(her) < 500 && !her.IsMe)
                .ToArray();
            return !enes.Any();
        }
        //Detuks
  
        /*
        private static bool targetisisolated(Obj_AI_Hero Target)
        {
            return !ObjectManager.Get<Obj_AI_Base>().Where(x => x.Distance(Target) < 500 && !x.IsAlly && !x.IsMe).ToList().Any();
        }
        */
        private static double getdamages(SpellSlot X, Obj_AI_Hero target)
        {
            if (X == SpellSlot.Q) {
                if (Q.Range == 325)
                {
                    return targetisisolated(target) ? Player.GetSpellDamage(target, SpellSlot.Q, 1) : Player.GetSpellDamage(target, SpellSlot.Q);

                }
                else
                {
                    return Player.GetSpellDamage(target, SpellSlot.Q, targetisisolated(target) ? 3 : 2);
                }
            }
            return 0;
        }

        public static bool ishealthy()
        {
            return Player.HealthPercentage() > 15;
        }

        private static void KillSteal()
        {
            Obj_AI_Hero target = ObjectManager.Get<Obj_AI_Hero>()
                 .Where(x => x.IsValidTarget() && x.Distance(Player.Position) < 1000f)
                 .OrderBy(x => x.Health).FirstOrDefault();
            var usePacket = Config.Item("usePackets").GetValue<bool>();
      


            if (target != null && !target.IsInvulnerable && !target.IsDead && !target.IsZombie)
            {
                double igniteDmg = Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                double QDmg = getdamages(SpellSlot.Q, target);
                double WDmg = Player.GetSpellDamage(target, SpellSlot.W);
                double EDmg = Player.GetSpellDamage(target, SpellSlot.E);
                double hydradmg = Player.GetItemDamage(target, Damage.DamageItems.Hydra);
                double tiamatdmg = Player.GetItemDamage(target, Damage.DamageItems.Tiamat);
                if (Config.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > target.Health)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                    }
                }

                if (!ishealthy() && Config.Item("autoescape").GetValue<bool>() && Player.CountEnemiesInRange(300) >= 1)
                {
                    var objAiHero = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(x => x.IsAlly && x.CountEnemiesInRange(300) == 0 && x.HealthPercentage() > 45 && E.IsInRange(x));
                    if (objAiHero != null) {
                        var bestposition =
                            objAiHero.ServerPosition;
                        E.Cast(bestposition, usePacket);
                    }
           
                }
                if (Q.IsReady() && Player.Distance(target) <= Q.Range && Config.Item("UseQKs").GetValue<bool>())
                {
                    if (target.Health <= QDmg)
                    {
                        Orbwalker.SetAttack(false);
                        Q.Cast(target, usePacket);
                        Orbwalker.SetAttack(true);
                    }
                }

                if (E.IsReady() && Player.Distance(target) <= E.Range && Config.Item("UseEKs").GetValue<bool>())
                {
                    if (target.Health <= EDmg) 
                    {
                        Utility.DelayAction.Add(
                            Game.Ping + Config.Item("EDelay").GetValue<Slider>().Value, delegate
                            {
                                PredictionOutput pred = E.GetPrediction(target); 
                                if (target.IsValid && !target.IsDead)
                                    E.Cast(pred.CastPosition, usePacket);
                            });
                    }
                }

                if (W.IsReady() && Wnorm && Player.Distance(target) <= W.Range && Config.Item("UseWKs").GetValue<bool>())
                {
                    if (target.Health <= WDmg)
                    {
                        if (W.GetPrediction(target).Hitchance >= HitChance.Medium)
                        {
                            PredictionOutput pred = W.GetPrediction(target);
                            W.Cast(pred.CastPosition);
                        }
                    }
                    if (W.IsReady() && Wevolved && Player.Distance(target) <= W.Range &&
                        Config.Item("UseWKs").GetValue<bool>())
                    {
                        if (target.Health <= WDmg)
                        {
                            PredictionOutput pred = W.GetPrediction(target);
                            CastWE(target, pred.UnitPosition.To2D());
                        }

                        if (W.GetPrediction(target).Hitchance >= HitChance.Collision)
                            {
                                List<Obj_AI_Base> PCollision = W.GetPrediction(target).CollisionObjects;
                                foreach (Obj_AI_Base PredCollisionChar in
                                    PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 30))
                                {
                                    W.Cast(PredCollisionChar.Position, Config.Item("usePackets").GetValue<bool>());
                                }

                            }
                        }
                    }

                    // Mixed's EQ KS
                    if (Q.IsReady() && E.IsReady() && Player.Distance(target) <= E.Range + Q.Range 
                         && Config.Item("UseEQKs").GetValue<bool>())
                    {
                        if ((target.Health <= QDmg + EDmg))
                        {
                            Utility.DelayAction.Add(Game.Ping + 200, delegate
                            {
                                PredictionOutput pred = E.GetPrediction(target);
                                if (target.IsValid && !target.IsDead)
                                E.Cast(pred.CastPosition);
                            });
                         
                        }
                    }
                   
                    // MIXED EW KS
                    if (W.IsReady() && E.IsReady() && Wnorm && Player.Distance(target) <= W.Range + E.Range
                        && Config.Item("UseEWKs").GetValue<bool>())
                    {
                        if (target.Health <= WDmg)
                        {

                            Utility.DelayAction.Add(Game.Ping + 200, delegate
                            {
                                PredictionOutput pred = E.GetPrediction(target);
                                if (target.IsValid && !target.IsDead)
                                E.Cast(pred.CastPosition);
                            });
                        }
                    }
                  
           
                    if (TIA.IsReady() && Player.Distance(target) <= TIA.Range &&
                        Config.Item("UseTiaKs").GetValue<bool>())
                    {
                        if (target.Health <= tiamatdmg)
                        {
                            TIA.Cast();
                        }
                    }
                    if (HDR.IsReady() && Player.Distance(target) <= HDR.Range &&
                    Config.Item("UseTiaKs").GetValue<bool>())
                    {
                        if (target.Health <= hydradmg)
                        {
                            HDR.Cast();
                        }
                    }
                }
            
        }


        private static void CheckSpells()
        {

            //check for evolutions
            if (ObjectManager.Player.HasBuff("khazixqevo", true))
            {
                Q.Range = 375;
            } 
            if (ObjectManager.Player.HasBuff("khazixwevo", true))
            {
                Wevolved = true;
                Wnorm = false;
                W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            } 
            if (ObjectManager.Player.HasBuff("khazixeevo", true))
            {
                E.Range = 1000;
            } 

            if (!ObjectManager.Player.HasBuff("khazixwevo", true))
            {
                Wnorm = true;
                Wevolved = false;
            }

             
        }

        //Trees
        private static HitChance HarassHitChance()
        {
            var hitchance = Config.Item("AutoWHitchance").GetValue<StringList>();
            switch (hitchance.SList[hitchance.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }
        // Trees End

        private static void AutoHarrass()
        {
            if (!Config.Item("AutoHarrass").GetValue<bool>() || Player.IsRecalling())
            {
                return;
            }
            
            var usePacket = Config.Item("usePackets").GetValue<bool>();
            Obj_AI_Hero target = TargetSelector.GetTarget(950, TargetSelector.DamageType.Physical);
            var autoWI = Config.Item("AutoWI").GetValue<bool>();
            var autoWD = Config.Item("AutoWD").GetValue<bool>();
            var hitchance = HarassHitChance();
            if (target != null && W.IsReady() && W.GetPrediction(target).Hitchance >= hitchance)
            {
       
                if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("AutoHarrass").GetValue<bool>())
                {
                    PredictionOutput predw = W.GetPrediction(target);
                    if (predw.Hitchance == HarassHitChance())
                    {
                        W.Cast(predw.CastPosition, usePacket);

                    }
                }
                if (Wevolved && Player.Distance(target) <= W.Range && Config.Item("AutoHarrass").GetValue<bool>() && W.IsReady())
                    {
      
                            if (target.IsValidTarget(WE.Range * 2))
                            {
                           
                                PredictionOutput pred = W.GetPrediction(target);
                                if ((pred.Hitchance == HitChance.Immobile && autoWI) || (pred.Hitchance == HitChance.Dashing && autoWD) || pred.Hitchance >= hitchance)
                                {
        
                                    CastWE(target, pred.UnitPosition.To2D());
                                }
                            }
                    }
                }
            }
        

        public static List<Obj_AI_Hero> GetIsolatedTargets()
        {
           var validtargets = HeroList.Where(h => h.IsEnemy && h.Distance(Player) <= E.Range);
          //  var validtargets = ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy).ToList();
            var isolatedheroes = new List<Obj_AI_Hero>();
            foreach (var x in validtargets)
            {
                var isolatedtargets = ObjectManager.Get<Obj_AI_Base>().Where(xd => xd.IsEnemy && x.NetworkId != xd.NetworkId && x.ServerPosition.Distance(xd.ServerPosition) < 500).ToList(); 
                if (!isolatedtargets.Any())
                {
                    if (!x.IsDead && x.IsVisible)
                    {
                        isolatedheroes.Add(x);

                    }
                }
            }
            return isolatedheroes;
        }

        private static void Combo()
        {
            if (Player.IsDead)
            {
                return; 
            }
            var usePacket = Config.Item("usePackets").GetValue<bool>();
            var isolatedlist = GetIsolatedTargets();
            HitChance hitchance = HarassHitChance();
            Obj_AI_Hero target = new Obj_AI_Hero();

            if (isolatedlist != null && isolatedlist.Any())
            {
                var isolated = isolatedlist.OrderByDescending(
                    hero =>
                        Player.CalcDamage(hero, Damage.DamageType.Physical, 100)/(1 + hero.Health)*
                        TargetSelector.GetPriority(hero)).FirstOrDefault();

                target = isolated;
                isolatedlist.Clear();
            }
            else
            {
                target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            }
          
            if (target == null || !target.IsValid || !target.IsEnemy || target.IsDead || Player.Distance(target) > E.Range + 100)
            {
                target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            }
         
     
            if ((target != null))
            {
            
                // Normal abilities
                if (Player.Distance(target) <= Q.Range && Config.Item("UseQCombo").GetValue<bool>() &&
                    Q.IsReady())
                {
                    Orbwalker.SetAttack(false);
                    Q.Cast(target, usePacket);
                    Orbwalker.SetAttack(true);
                }
                if (Wnorm && Player.Distance(target) <= W.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && W.GetPrediction(target).Hitchance >= hitchance)
                {
                        PredictionOutput pred = W.GetPrediction(target);
                        W.Cast(pred.CastPosition, usePacket);
                }
        
                if (Player.Distance(target) <= E.Range && Config.Item("UseECombo").GetValue<bool>() &&
                    E.IsReady() && Player.Distance(target) > Q.Range)
                {
                        PredictionOutput pred = E.GetPrediction(target);
                        if (target.IsValid && !target.IsDead)
                        E.Cast(pred.CastPosition, usePacket);
                }

                // Use EQ AND EW Synergy
                if ((Player.Distance(target) <= E.Range + Q.Range && Player.Distance(target) > Q.Range && E.IsReady() &&
                    Config.Item("UseEGapclose").GetValue<bool>()) || (Player.Distance(target) <= E.Range + W.Range && Player.Distance(target) > Q.Range && E.IsReady() && W.IsReady() &&
                    Config.Item("UseEGapcloseW").GetValue<bool>()))
                {
                        PredictionOutput pred = E.GetPrediction(target);
                        if (target.IsValid && !target.IsDead)
                        E.Cast(pred.CastPosition, usePacket);
                        if (Config.Item("UseRGapcloseW").GetValue<bool>() && R.IsReady())
                            R.CastOnUnit(ObjectManager.Player);
                }


                // Ult Usage
                if (R.IsReady() && !Q.IsReady() && !W.IsReady() && !E.IsReady() &&
                    Config.Item("UseRCombo").GetValue<bool>())
                {
                    R.Cast();
                    if (Config.Item("Debugon").GetValue<bool>())
                    {
                        Game.PrintChat("9 - Basic Ult Cast");
                    }
                }
                // Evolved
              
                if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && W.GetPrediction(target).Hitchance >= hitchance)
                {
                        PredictionOutput pred = WE.GetPrediction(target);
                       // W.Cast(pred.CastPosition, usePacket); 
                        CastWE(target, pred.UnitPosition.To2D()); 
                }
                if (Wevolved && Player.Distance(target) <= WE.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && W.GetPrediction(target).Hitchance >= HitChance.Collision)
                {
                        List<Obj_AI_Base> PCollision = W.GetPrediction(target).CollisionObjects;
                        foreach (
                            Obj_AI_Base PredCollisionChar in
                                PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 30))
                        {
                            W.Cast(PredCollisionChar.Position, usePacket);
                        }
                }
              
                if (Player.Distance(target) <= E.Range && Player.Distance(target) > Q.Range &&
                    Config.Item("UseECombo").GetValue<bool>() && E.IsReady())
                {
                        PredictionOutput pred = E.GetPrediction(target);
                        if (target.IsValid && !target.IsDead)
                        E.Cast(pred.CastPosition, usePacket);
                }
     

                if (Config.Item("UseItems").GetValue<bool>())
                {
                    UseItems(target);
                }
            }
        }

        private static void UseItems(Obj_AI_Base target)
        {
            if (HDR.IsReady() && Player.Distance(target) <= HDR.Range)
            {
                HDR.Cast();
            }
            if (TIA.IsReady() && Player.Distance(target) <= TIA.Range)
            {
                TIA.Cast();
            }
            if (BKR.IsReady() && Player.Distance(target) <= BKR.Range)
            {
                BKR.Cast(target);
            }
            if (YOU.IsReady() && Player.Distance(target) <= YOU.Range)
            {
                YOU.Cast(target);
            }
            if (BWC.IsReady() && Player.Distance(target) <= BWC.Range)
            {
                BWC.Cast(target);
            }
        }

  

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("Debugon").GetValue<bool>())
            {
                var isolatedtargs = GetIsolatedTargets();
                foreach (var x in isolatedtargs)
                {
                    var heroposwts = Drawing.WorldToScreen(x.Position);
                    Drawing.DrawText(heroposwts.X, heroposwts.Y, Color.White, "Isolated");
      

                }
            }


            if (Config.Item("CircleLag").GetValue<bool>())
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
                }
            if (Config.Item("DrawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, W.Range, Color.Red);
            }


            if (Config.Item("DrawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Green);
            }

            }

            else
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                        Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
              
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                 
                        Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Red);
          
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    
                        Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Green);
                }
            }
        }
    }
}
