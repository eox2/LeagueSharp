using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace SephKhazix
{

    internal class Program
    {
        private const string ChampionName = "Khazix";
        private static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, WE;
        private const float Wangle = 22 * (float)Math.PI / 180;
        private static Menu Config;
        private static Items.Item HDR, TIA, BKR, BWC, YOU;
        private static SpellSlot IgniteSlot;
        private static List<Obj_AI_Hero> HeroList;
        private static bool EvolvedQ, EvolvedW, EvolvedE, EvolvedR;
        private static List<Vector3> EnemyTurretPositions = new List<Vector3>();
        private static Vector3 NexusPosition;
        private static Vector3 Jumppoint1 = new Vector3();
        private static Vector3 Jumppoint2 = new Vector3();
        private static bool Jumping = false;

        private static Obj_AI_Hero Player;
        private static bool Wnorm =  true, Wevolved, Eevolved;
  


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName)
            {
                return;
            }

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
            Config.SubMenu("Ks").AddItem(new MenuItem("Edelay", "E Delay (ms)").SetValue(new Slider(0, 0, 300)));
            Config.SubMenu("Ks").AddItem(new MenuItem("autoescape", "Use E to get out when low")).SetValue(false);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);

            Config.AddSubMenu(new Menu("Double Jumping", "DoubleJump"));
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("djumpenabled", "Enabled")).SetValue(true);
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("JEDelay", "Delay between jumps").SetValue(new Slider(250, 250, 500)));
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("jumpmode", "Jump Mode").SetValue(new StringList(new[] { "Default (jumps towards your nexus)", "Custom - Settings below" }, 0)));
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("save", "Save Double Jump Abilities")).SetValue(true);
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("noauto", "Wait for Q instead of autos")).SetValue(false);
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("jcursor", "Jump to Cursor (true) or false for script logic")).SetValue(true);
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("secondjump", "Do second Jump")).SetValue(true);
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("jcursor2", "Second Jump to Cursor (true) or false for script logic")).SetValue(true);
            Config.SubMenu(("DoubleJump")).AddItem(new MenuItem("jumpdrawings", "Enable Jump Drawinsg")).SetValue(true);


            //Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);


            //Debug
            Config.AddSubMenu(new Menu("Debug", "Debug"));
            Config.SubMenu("Debug").AddItem(new MenuItem("Debugon", "Enable Debugging").SetValue(false));
            Config.AddToMainMenu();

            //Get Turrets

            foreach (var t in ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy))
            {
                EnemyTurretPositions.Add(t.ServerPosition);
            }
            NexusPosition = ObjectManager.Get<Obj_HQ>().Where(o => o.IsAlly).FirstOrDefault().Position;
            Game.OnUpdate += OnGameUpdate;
            Game.OnUpdate += CheckSpells;
            Game.OnUpdate += DoubleJump;
            Spellbook.OnCastSpell += SpellCast;
            Orbwalking.BeforeAttack += BeforeAttack;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("<font color='#1d87f2'>SephKhazix has been Loaded. Version 1.9. If anything is not functioning as it used to, disable Double Jumping.</font>");
            HeroList = HeroManager.AllHeroes;
        }

        private static int GetJumpMode()
        {
            return Config.Item("jumpmode").GetValue<StringList>().SelectedIndex;
        }

        private static void CastWE(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0)
        {
            var usePacket = Config.Item("usePackets").GetValue<bool>();
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            Vector2 startPoint = ObjectManager.Player.ServerPosition.To2D();
            Vector2 originalDirection = W.Range * (unitPosition - startPoint).Normalized();

            foreach (Obj_AI_Hero enemy in HeroList)
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    PredictionOutput pos = W.GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int)enemy.BoundingRadius);
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
            if (Player.IsDead)
            {
                return;
            }

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
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady() && !Jumping)
                {
                    Orbwalker.SetAttack(false);
                    Q.Cast(target, usePacket);
                    Orbwalker.SetAttack(true);
                }

                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
                    Wnorm)
                {
                    W.Cast(target, usePacket);
                }
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady() &&
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
                if (Q.IsReady() && minion.IsValidTarget() && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= Q.Range &&
                    Config.Item("UseQFarm").GetValue<bool>())
                {
                    Orbwalker.SetAttack(false);
                    Q.Cast(minion);
                    Orbwalker.SetAttack(true);
                }
                if (W.IsReady() && minion.IsValidTarget() && Wnorm && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= W.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);

                    W.Cast(pred.Position);
                }
                if (E.IsReady() && minion.IsValidTarget() && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= E.Range &&
                    Config.Item("UseEFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestCircularFarmLocation(pos, 300, 600);
                    E.Cast(pred.Position);
                }
                //Evolved

                if (W.IsReady() && minion.IsValidTarget() && Wevolved && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= WE.Range &&
                    Config.Item("UseWFarm").GetValue<bool>() && (pos.Any()))
                {
                    MinionManager.FarmLocation pred = MinionManager.GetBestLineFarmLocation(pos, 70, 1025);

                    W.Cast(pred.Position);
                }
                if (Config.Item("UseItems").GetValue<bool>())
                {
                    if (HDR.IsReady() && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= HDR.Range)
                    {
                        HDR.Cast();
                        // Items.UseItem(3077, ObjectManager.Player);
                    }
                    if (TIA.IsReady() && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= TIA.Range)
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
                                minion, (int)(Vector3.Distance(Player.ServerPosition, minion.ServerPosition) * 1000 / 1400)) <
                            0.75 * Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    if (Vector3.Distance(minion.ServerPosition, ObjectManager.Player.ServerPosition) >
                        Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) <= Q.Range)
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


                    if (Vector2.Distance(Player.ServerPosition.To2D(), farmLocation.Position) <= W.Range)
                        W.Cast(farmLocation.Position);
                }
                if (Wevolved && !Wnorm)
                {

                    if (Vector2.Distance(Player.ServerPosition.To2D(), farmLocation.Position) <= WE.Range)
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

                if (Vector2.Distance(Player.ServerPosition.To2D(), farmLocation.Position) <= W.Range)
                    E.Cast(farmLocation.Position);
            }


            if (Config.Item("UseItemsFarm").GetValue<bool>())
            {
                MinionManager.FarmLocation farmLocation =
                    MinionManager.GetBestCircularFarmLocation(
                        MinionManager.GetMinions(Player.Position, HDR.Range)
                            .Select(minion => minion.ServerPosition.To2D())
                            .ToList(), HDR.Range, HDR.Range);

                if (HDR.IsReady() && Vector2.Distance(Player.ServerPosition.To2D(), farmLocation.Position) <= HDR.Range && farmLocation.MinionsHit >= 2)
                {
                    Items.UseItem(3074, ObjectManager.Player);
                }
                if (TIA.IsReady() && Vector2.Distance(Player.ServerPosition.To2D(), farmLocation.Position) <= TIA.Range && farmLocation.MinionsHit >= 2)
                {
                    Items.UseItem(3077, ObjectManager.Player);
                }
            }
        }

        //Detuks
        public static bool targetisisolated(Obj_AI_Base target)
        {
            var enes = ObjectManager.Get<Obj_AI_Base>()
                .Where(her => her.IsEnemy && her.NetworkId != target.NetworkId && Vector3.Distance(target.ServerPosition, her.ServerPosition) < 500 && !her.IsMe)
                .ToArray();
            return !enes.Any();
        }
        //Detuks

        /*
        private static bool targetisisolated(Obj_AI_Hero Target)
        {
            return !ObjectManager.Get<Obj_AI_Base>().Any(x => x.Distance(Target) < 500 && !x.IsAlly && !x.IsMe);
        }
        */
        private static double GetQDamage(Obj_AI_Hero target)
        {
                if (Q.Range == 325)
                {
                    return 0.984 * Player.GetSpellDamage(target, SpellSlot.Q, targetisisolated(target) ? 1 : 0);
                }
                if (Q.Range > 325)
                {
                    var isolated = targetisisolated(target);
                    if (isolated)
                    {
                        return 0.984 * Player.GetSpellDamage(target, SpellSlot.Q, 3);
                    }
                        return Player.GetSpellDamage(target, SpellSlot.Q, 0);
                }
            return 0;
        }

        public static bool ishealthy()
        {
            return Player.HealthPercent > 20;
        }


        public static bool PointUnderEnemyTurret(Vector3 Point)
        {
            var EnemyTurrets =
                EnemyTurretPositions.Where(t => Vector3.Distance(t, Point) <= 900f);
            return EnemyTurrets.Any();
        }

        private static void DoubleJump(EventArgs args)
        {
            if (!E.IsReady() || !Eevolved || !Config.Item("djumpenabled").GetValue<bool>() || Player.IsDead)
            {
                return;
            }

            var Targets = HeroList.Where(x => x.IsValidTarget() && !x.IsInvulnerable && !x.IsZombie);

            if (Q.IsReady())
            {
                var CheckQKillable = Targets.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Q.Range - 50 && GetQDamage(x) > x.Health).FirstOrDefault();

                if (CheckQKillable != null)
                {
                    Jumping = true;
                    Jumppoint1 = GetJumpPoint(CheckQKillable);
                    E.Cast(Jumppoint1);
                    Q.Cast(CheckQKillable);
                    var oldpos = Player.ServerPosition;
                    Utility.DelayAction.Add(Config.Item("JEDelay").GetValue<Slider>().Value + Game.Ping, () =>
                    {
                        if (E.IsReady())
                        {
                            Jumppoint2 = GetJumpPoint(CheckQKillable, false);
                            E.Cast(Jumppoint2);
                        }
                        Jumping = false;
                    });
                    return;
                }
            }
        }

        static Vector3 GetJumpPoint(Obj_AI_Hero Qtarget, bool firstjump = true)
        {
            if (PointUnderEnemyTurret(Player.ServerPosition))
            {
                return Player.ServerPosition.Extend(NexusPosition, E.Range);
            }

            if (GetJumpMode() == 0)
            {
                return Player.ServerPosition.Extend(NexusPosition, E.Range);
            }

            if (firstjump && Config.Item("jcursor").GetValue<bool>())
            {
                return Game.CursorPos;
            }

            if (!firstjump && Config.Item("jcursor2").GetValue<bool>())
            {
                return Game.CursorPos;
            }

            Vector3 Position = new Vector3();
            var jumptarget = ishealthy()
                  ? HeroList
                      .Where(
                          x =>
                              x.IsValidTarget() && !x.IsZombie && x != Qtarget &&
                              Vector3.Distance(Player.ServerPosition, x.ServerPosition) < E.Range)
                      .FirstOrDefault()
                  :
              HeroList
                  .Where(
                      x =>
                          x.IsAlly && !x.IsZombie && !x.IsDead && !x.IsMe &&
                          Vector3.Distance(Player.ServerPosition, x.ServerPosition) < E.Range)
                  .FirstOrDefault();

            if (jumptarget != null)
            {
                Position = jumptarget.ServerPosition;
            }
            if (jumptarget == null)
            {
                //Position = Player.ServerPosition.Extend(TurretPositions.MinOrDefault(h => Player.Distance(h)), E.Range);
                return Player.ServerPosition.Extend(NexusPosition, E.Range);
            }
            return Position;
        }

        static void SpellCast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!Eevolved)
            {
                return;
            }
            if (args.Slot.Equals(SpellSlot.Q) && args.Target is Obj_AI_Hero && Config.Item("djumpenabled").GetValue<bool>() && Config.Item("save").GetValue<bool>())
            {
                var target = args.Target as Obj_AI_Hero;
                var qdmg = GetQDamage(target);
                var dmg = (Player.GetAutoAttackDamage(target) * 2) + qdmg;
                if (target.Health < dmg && target.Health > qdmg)
                { //save some unnecessary q's if target is killable with 2 autos + Q instead of Q as Q is important for double jumping
                   // Game.PrintChat("cancelled Q");
                    args.Process = false;
                }
            }

            /*
            if (args.Slot.Equals(SpellSlot.E) && sender.IsCastingSpell && sender.ActiveSpellSlot == SpellSlot.Q) // if E is cast and another spell is being casted dont process 
            {
                args.Process = false;
            }
             * */
            
        }

        static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.Type == GameObjectType.obj_AI_Hero && Config.Item("djumpenabled").GetValue<bool>() && Config.Item("noauto").GetValue<bool>())
            {
                if (args.Target.Health < GetQDamage((Obj_AI_Hero) args.Target) &&
                    Player.ManaPercent > 15)
                {
                    args.Process = false;
                }
            }
        }

        private static void KillSteal()
        {
            Obj_AI_Hero target = HeroList
                .Where(x => x.IsValidTarget() && x.Distance(Player.Position) < 1000f && !x.IsZombie)
                .MinOrDefault(x => x.Health);
            
            var usePacket = Config.Item("usePackets").GetValue<bool>();

            if (target != null)
            {
                if (Config.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    double igniteDmg = Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                    if (igniteDmg > target.Health)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                        return;
                    }
                }

                if (Config.Item("autoescape").GetValue<bool>() && !ishealthy())
                {
                    var ally =
                        HeroList.Where(h => h.HealthPercent > 50 && h.CountEnemiesInRange(400) == 0 && !PointUnderEnemyTurret(h.ServerPosition)).FirstOrDefault();
                    if (ally != null && ally.IsValid)
                    {
                        E.Cast(ally.ServerPosition);
                        return;
                    }
                    var objAiturret = EnemyTurretPositions.Where(x => Vector3.Distance(Player.ServerPosition, x) <= 900f);
                    if (objAiturret.Any() || Player.CountEnemiesInRange(500) >= 1)
                    {
                        var bestposition = Player.ServerPosition.Extend(NexusPosition, E.Range);
                        E.Cast(bestposition, usePacket);
                        return;
                    }
                }

                if (Config.Item("UseQKs").GetValue<bool>()&& Q.IsReady() &&
                    Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range)
                {
                    double QDmg = GetQDamage(target);
                    if (!Jumping && target.Health <= QDmg)
                    {
                        Q.Cast(target, usePacket);
                        return;
                    }
                }

                if (Config.Item("UseEKs").GetValue<bool>() && E.IsReady() &&
                    Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range && Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Q.Range)
                {
                    double EDmg = Player.GetSpellDamage(target, SpellSlot.E);
                    if (!Jumping && target.Health < EDmg)
                    {
                        Utility.DelayAction.Add(
                            Game.Ping + Config.Item("EDelay").GetValue<Slider>().Value, delegate
                            {
                                PredictionOutput pred = E.GetPrediction(target);
                                if (target.IsValid && !target.IsDead)
                                {
                                    E.Cast(pred.CastPosition, usePacket);
                                    return;
                                }
                            });
                    }
                }

                if (W.IsReady() && Wnorm && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range &&
                    Config.Item("UseWKs").GetValue<bool>())
                {
                    double WDmg = Player.GetSpellDamage(target, SpellSlot.W);
                    if (target.Health <= WDmg)
                    {
                        var pred = W.GetPrediction(target);
                        if (pred.Hitchance >= HitChance.Medium)
                        {
                            W.Cast(pred.CastPosition);
                            return;
                        }
                    }
                }

                if (W.IsReady() && Wevolved &&
                        Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range &&
                        Config.Item("UseWKs").GetValue<bool>())
                    {
                        double WDmg = Player.GetSpellDamage(target, SpellSlot.W);
                        PredictionOutput pred = W.GetPrediction(target);
                        if (target.Health <= WDmg && pred.Hitchance > HitChance.Medium)
                        {
                            CastWE(target, pred.UnitPosition.To2D());
                            return;
                        }

                        if (pred.Hitchance >= HitChance.Collision)
                        {
                            List<Obj_AI_Base> PCollision = pred.CollisionObjects;
                            var x =
                                PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 30)
                                    .FirstOrDefault();
                            if (x != null)
                            {
                                W.Cast(x.Position, Config.Item("usePackets").GetValue<bool>());
                                return;
                            }
                        }
                    }


                    // Mixed's EQ KS
                    if (Q.IsReady() && E.IsReady() &&
                        Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range + Q.Range
                        && Config.Item("UseEQKs").GetValue<bool>())
                    {
                        double QDmg = GetQDamage(target);
                        double EDmg = Player.GetSpellDamage(target, SpellSlot.E);
                        if ((target.Health <= QDmg + EDmg))
                        {
                            Utility.DelayAction.Add(Config.Item("EDelay").GetValue<Slider>().Value, delegate
                            {
                                PredictionOutput pred = E.GetPrediction(target);
                                if (target.IsValidTarget() && !target.IsZombie)
                                {
                                    E.Cast(pred.CastPosition);
                                    return;
                                }
                            });

                        }
                    }

                    // MIXED EW KS
                    if (W.IsReady() && E.IsReady() && Wnorm &&
                        Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range + E.Range
                        && Config.Item("UseEWKs").GetValue<bool>())
                    {
                        double WDmg = Player.GetSpellDamage(target, SpellSlot.W);
                        if (target.Health <= WDmg)
                        {

                            Utility.DelayAction.Add(Config.Item("EDelay").GetValue<Slider>().Value, delegate
                            {
                                PredictionOutput pred = E.GetPrediction(target);
                                if (target.IsValid && !target.IsDead)
                                {
                                    E.Cast(pred.CastPosition);
                                    return;
                                }
                            });
                        }
                    }


                    if (TIA.IsReady() &&
                        Vector2.Distance(Player.ServerPosition.To2D(), target.ServerPosition.To2D()) <= TIA.Range &&
                        Config.Item("UseTiaKs").GetValue<bool>())
                    {
                        double tiamatdmg = Player.GetItemDamage(target, Damage.DamageItems.Tiamat);
                        if (target.Health <= tiamatdmg)
                        {
                            TIA.Cast();
                            return;
                        }
                    }
                    if (HDR.IsReady() &&
                        Vector2.Distance(Player.ServerPosition.To2D(), target.ServerPosition.To2D()) <= HDR.Range &&
                        Config.Item("UseTiaKs").GetValue<bool>())
                    {
                        double hydradmg = Player.GetItemDamage(target, Damage.DamageItems.Hydra);
                        if (target.Health <= hydradmg)
                        {
                            HDR.Cast();
                            return;
                        }
                    }
                }
            }
        


        private static void CheckSpells(EventArgs args)
        {
            //check for evolutions
            if (ObjectManager.Player.HasBuff("khazixqevo", true) && !EvolvedQ)
            {
                Q.Range = 375;
                EvolvedQ = true;
            }
            if (ObjectManager.Player.HasBuff("khazixwevo", true) && !Wevolved)
            {
                Wevolved = true;
                Wnorm = false;
                W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            }
            
            if (ObjectManager.Player.HasBuff("khazixeevo", true) && !Eevolved)
            {
                E.Range = 1000;
                Eevolved = true;
            }
        }
        
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

                if (Wnorm && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range && Config.Item("AutoHarrass").GetValue<bool>())
                {
                    PredictionOutput predw = W.GetPrediction(target);
                    if (predw.Hitchance == HarassHitChance())
                    {
                        W.Cast(predw.CastPosition, usePacket);

                    }
                }
                if (Wevolved && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range && Config.Item("AutoHarrass").GetValue<bool>() && W.IsReady())
                {
                    if (target.IsValidTarget(WE.Range + 200))
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
            var validtargets = HeroList.Where(h => h.IsValidTarget(E.Range));
            var isolatedheroes = new List<Obj_AI_Hero>();
            foreach (var x in validtargets)
            {
                var minions = ObjectManager.Get<Obj_AI_Base>().Where(xd => xd.IsEnemy && x.NetworkId != xd.NetworkId && x.ServerPosition.Distance(xd.ServerPosition) < 500 && (xd.Type == GameObjectType.obj_AI_Hero || xd.Type == GameObjectType.obj_AI_Minion || xd.Type == GameObjectType.obj_AI_Turret));
                if (!minions.Any())
                {
                    if (!x.IsDead)
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
                var isolated = isolatedlist.MaxOrDefault(h => TargetSelector.GetPriority(h));
                target = isolated;
            }

            else
            {
                target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            }

            if (target == null && target.IsValidTarget(E.Range + 100) && !target.IsZombie)
            {
                target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            }

            if ((target != null))
            {

                // Normal abilities
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range && Config.Item("UseQCombo").GetValue<bool>() &&
                    Q.IsReady() && !Jumping)
                {
                    Orbwalker.SetAttack(false);
                    Q.Cast(target, usePacket);
                    Orbwalker.SetAttack(true);
                }
                if (Wnorm && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady() && W.GetPrediction(target).Hitchance >= hitchance)
                {
                    PredictionOutput pred = W.GetPrediction(target);
                    W.Cast(pred.CastPosition, usePacket);
                }

                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range && Config.Item("UseECombo").GetValue<bool>() &&
                    E.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Q.Range + (0.7 * Player.MoveSpeed))
                {
                    PredictionOutput pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead)
                    {
                        E.Cast(pred.CastPosition, usePacket);
                    }
                }

                // Use EQ AND EW Synergy
                if ((Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range + Q.Range + (0.7 * Player.MoveSpeed) && Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Q.Range && E.IsReady() &&
                    Config.Item("UseEGapclose").GetValue<bool>()) || (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range + W.Range && Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Q.Range && E.IsReady() && W.IsReady() &&
                    Config.Item("UseEGapcloseW").GetValue<bool>()))
                {
                    PredictionOutput pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead)
                    {
                        E.Cast(pred.CastPosition, usePacket);
                    }
                    if (Config.Item("UseRGapcloseW").GetValue<bool>() && R.IsReady())
                    {
                        R.CastOnUnit(ObjectManager.Player);
                    }
                }


                // Ult Usage
                if (R.IsReady() && !Q.IsReady() && !W.IsReady() && !E.IsReady() &&
                    Config.Item("UseRCombo").GetValue<bool>())
                {
                    R.Cast();
                }
                // Evolved

                if (Wevolved && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= WE.Range && Config.Item("UseWCombo").GetValue<bool>() &&
                    W.IsReady())
                {
                    PredictionOutput pred = WE.GetPrediction(target);
                    if (pred.Hitchance >= hitchance)
                    {
                        CastWE(target, pred.UnitPosition.To2D());
                    }
                    if (pred.Hitchance >= HitChance.Collision)
                    {
                        List<Obj_AI_Base> PCollision = pred.CollisionObjects;
                        var x = PCollision.Where(PredCollisionChar => PredCollisionChar.Distance(target) <= 30).FirstOrDefault();
                        if (x != null)
                        {
                            W.Cast(x.Position, usePacket);
                        }
                    }
                }

                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range + (0.7 * Player.MoveSpeed) && Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Q.Range &&
                    Config.Item("UseECombo").GetValue<bool>() && E.IsReady())
                {
                    PredictionOutput pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead)
                    {
                        E.Cast(pred.CastPosition, usePacket);
                    }
                }

                if (Config.Item("UseItems").GetValue<bool>())
                {
                    UseItems(target);
                }
            }
        }

        private static void UseItems(Obj_AI_Base target)
        {
            var PlayerServerPosition = Player.ServerPosition.To2D();
            var targetServerPosition = target.ServerPosition.To2D();

            if (HDR.IsReady() && Vector2.Distance(PlayerServerPosition, targetServerPosition) <= HDR.Range)
            {
                HDR.Cast();
            }
            if (TIA.IsReady() && Vector2.Distance(PlayerServerPosition, targetServerPosition) <= TIA.Range)
            {
                TIA.Cast();
            }
            if (BKR.IsReady() && Vector2.Distance(PlayerServerPosition, targetServerPosition) <= BKR.Range)
            {
                BKR.Cast(target);
            }
            if (YOU.IsReady() && Vector2.Distance(PlayerServerPosition, targetServerPosition) <= YOU.Range)
            {
                YOU.Cast(target);
            }
            if (BWC.IsReady() && Vector2.Distance(PlayerServerPosition, targetServerPosition) <= BWC.Range)
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

            if (Config.Item("jumpdrawings").GetValue<bool>() && Jumping)
            {
                var PlayerPosition = Drawing.WorldToScreen(Player.Position);
                var Jump1 = Drawing.WorldToScreen(Jumppoint1).To3D();
                var Jump2 = Drawing.WorldToScreen(Jumppoint2).To3D();
                Render.Circle.DrawCircle(Jump1, 250, Color.White);
                Render.Circle.DrawCircle(Jump2, 250, Color.White);
                Drawing.DrawLine(PlayerPosition.X, PlayerPosition.Y, Jump1.X, Jump1.Y, 10, Color.DarkCyan);
                Drawing.DrawLine(Jump1.X, Jump1.Y, Jump2.X, Jump2.Y, 10, Color.DarkCyan);
            }
            if (Config.Item("DrawQ").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Config.Item("DrawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Red);
            }

            if (Config.Item("DrawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.Green);
            }

        }
    }
}

