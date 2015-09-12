#region imports
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion;


namespace SephSoraka
{

	#region Initiliazation

	internal static class Soraka
	{
		#region vars

		public static Obj_AI_Hero Player;
		public static Menu Config;
		private static SpellSlot IgniteSlot = SpellSlot.Summoner1;
		private static readonly string[] adcs = {
			"Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves",
			"Jinx", "Kalista", "KogMaw", "Lucian", "Miss Fortune", "Quinn", "Sivir", "Tristana", "Twitch", "Urgot", "Varus", "Vayne"
		};

		private static Items.Item mikaels = new Items.Item(3222);

		private static Obj_AI_Hero myADC
		{
			get
			{
				var name = Config.Item("adc").GetValue<StringList>().SelectedValue;
				var adc = HeroManager.Allies.FirstOrDefault(x => x.ChampionName == name);
				return adc;
			}
		}

		#endregion

		#region OnLoad

		public static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += SorakaMain;
		}

		private static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
		{
			{SpellSlot.Q, new Spell(SpellSlot.Q, 950f, TargetSelector.DamageType.Magical)},
			{SpellSlot.W, new Spell(SpellSlot.W, 550f)},
			{SpellSlot.E, new Spell(SpellSlot.E, 920f, TargetSelector.DamageType.Magical)},
			{SpellSlot.R, new Spell(SpellSlot.R) },
			{IgniteSlot, new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 550f)}
		};

		private static void InitializeSpells()
		{
			Spells[SpellSlot.Q].SetSkillshot(0.500f, 300f, 1750f, false, SkillshotType.SkillshotCircle);
			Spells[SpellSlot.E].SetSkillshot(0.500f, 250f, 1300f, false, SkillshotType.SkillshotCircle);
		}

		//private static bool done;
		/*
        private static void PrintSData()
        {
            if (done) return;
            foreach (var spell in Player.Spellbook.Spells)
            {
                var sdata = spell.SData;
                Game.PrintChat("spell " + spell.Slot + " speed " + sdata.MissileSpeed + " delay " + sdata.DelayCastOffsetPercent + " width " + sdata.LineWidth + " range " + sdata.CastRange);
            }
            done = true;
        }
        */

		private static void SorakaMain(EventArgs args)
		{
			Player = ObjectManager.Player;

			if (Player.CharData.BaseSkinName != "Soraka")
			{
				return;
			}

			Config = SorakaMenu.CreateMenu();

			Config.AddToMainMenu();

			InitializeSpells();

			AntiGapcloser.OnEnemyGapcloser += OnGapClose;
			Interrupter2.OnInterruptableTarget += OnInterruptableTarget;

			Obj_AI_Base.OnProcessSpellCast += DangerDetector;

			Orbwalking.BeforeAttack += BeforeAttack;

			Game.OnUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;

			Game.PrintChat("SephSoraka Loaded!");
		}

		#endregion

		#region DetectAdc

		private static List<Obj_AI_Hero> PotentialADCS = new List<Obj_AI_Hero>();

		internal static Obj_AI_Hero DetectADC()
		{
			Obj_AI_Hero theadc = null;
			var adc = HeroManager.Allies.Where(x => adcs.Contains(x.ChampionName) && (x.CheckSumm("summonerheal") || x.CheckSumm("summonerboost") || x.CheckSumm("summonerbarrier")) && (!x.CheckSumm("summonersmite") || !x.CheckSumm("summonerteleport")));
			var objAiHeroes = adc as List<Obj_AI_Hero> ?? adc.ToList();
			if (objAiHeroes.Count() == 1)
			{
				theadc = objAiHeroes.FirstOrDefault();
				Game.PrintChat("ADC detected as " + theadc.ChampionName + ".");
			}
			if (objAiHeroes.Count() > 1)
			{
				Game.PrintChat("Found multiple ADCS in game. Please select the correct one in the priorities menu.");
				PotentialADCS = objAiHeroes;
				theadc = null;
			}

			if (theadc == null)
			{
				Game.PrintChat("Could not detect ADC. Set ADC in the menu.");
				return null;
			}
			return theadc;
		}

		private static bool CheckSumm(this Obj_AI_Hero hero, string summoner)
		{
			return hero.GetSpellSlot(summoner) != SpellSlot.Unknown;
		}

		#endregion DetectAdc

		#endregion

		#region OnUpdate

		private static void OnUpdate(EventArgs args)
		{
			if (Player.IsDead || Player.IsRecalling())
			{
				if (Debug)
				{
					Game.PrintChat("Dead or recalling so won't cast");
				}
				return;
			}

			if (Misc.Active("Misc.AutoEStunned"))
			{
				AutoEStunned();
			}

			if (Misc.Active("ultifadcignited"))
			{
				AutoUltIgniteADC();
			}

			if (Misc.Active("Misc.UseMikael"))
			{
				UseMikaels();
			}

			Healing();

			Killsteal();

			var target = TargetSelector.GetTarget(
				Spells[SpellSlot.Q].Range, TargetSelector.DamageType.Magical);

			if (target != null && Misc.ActiveKeyBind("Keys.HarassT"))
			{
				Harass(target);
			}

			if (Debug && target == null && Misc.ActiveKeyBind("Keys.HarassT"))
			{
				Game.PrintChat("target is null");
			}

			switch (SorakaMenu.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if (target != null)
					{
						Combo(target);
					}
					else if (Debug)
					{
						Game.PrintChat("Target is null");
					}
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					WaveClear();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					MixedModeLogic(target, true);
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					MixedModeLogic(target, false);
					break;
			}
		}

		#endregion

		static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
		{
			if (Misc.Active("Farm.Disableauto") && args.Target.Type != GameObjectType.obj_AI_Hero)
			{
				var alliesinrange = HeroManager.Allies.Count(x => !x.IsMe && x.Distance(Player) <= FarmRange);
				if (alliesinrange > 0)
				{
					args.Process = false;
				}
			}

		}


		static void DangerDetector(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			var ally = args.Target as Obj_AI_Hero;
			if (sender.IsChampion() && sender.Team != Player.Team && ally != null && ally.IsAlly && (!Misc.Active("Misc.Nohealshop") || !ally.InShop()))
			{
				if (Misc.Active("Healing.UseW"))
				{
					if (Misc.Active("w" + ally.ChampionName) && !ally.IsMe && !ally.IsZombie &&
						ally.Distance(Player) <= Spells[SpellSlot.W].Range)
					{
						var damage = sender.GetSpellDamage(ally, args.SData.Name);
						var afterdmg = ((ally.Health - damage) / (ally.MaxHealth)) * 100f;
						if (ally.HealthPercent < Misc.GetSlider("wpct" + ally.ChampionName) ||
							Misc.Active("wcheckdmgafter") && afterdmg < Misc.GetSlider("wpct" + ally.ChampionName))
						{
							if (Misc.Active("wonlyadc") &&
								(ally == myADC || myADC.HealthPercent <= Misc.GetSlider("wpct" + myADC.ChampionName)))
							{
								Spells[SpellSlot.W].CastOnUnit(myADC);
							}
							else if (!Misc.Active("wonlyadc"))
							{
								Spells[SpellSlot.W].CastOnUnit(ally);
							}
						}
					}
				}
				if (Misc.Active("Healing.UseR") && Misc.Active("r" + ally.ChampionName) && !ally.IsZombie)
				{
					var damage = sender.GetSpellDamage(ally, args.SData.Name);
					var afterdmg = ((ally.Health - damage) / (ally.MaxHealth)) * 100f;
					if (ally.HealthPercent < Misc.GetSlider("rpct" + ally.ChampionName) || (Misc.Active("rcheckdmgafter") && afterdmg < Misc.GetSlider("rpct" + ally.ChampionName)))
					{
						var otherallies = HeroManager.Allies.Where(x => x != ally && Misc.Active("r" + x.ChampionName) && Misc.GetSlider("rpct" + x.ChampionName) > x.HealthPercent);

						var count = otherallies.Count() + 1;

						if (count >= Misc.GetSlider("minallies"))
						{
							if (Misc.Active("ultonlyadc") && (ally == myADC || myADC.HealthPercent <= Misc.GetSlider("rpct" + myADC.ChampionName)))
							{
								Spells[SpellSlot.R].Cast();
							}

							else if (!Misc.Active("ultonlyadc"))
							{
								Spells[SpellSlot.R].Cast();
							}
						}
					}
				}
			}
		}
		#region Combo


		static void Healing()
		{
			if (Misc.Active("Healing.UseW"))
			{
				UseW();
			}
			if (Misc.Active("Healing.UseR"))
			{
				UseR();
			}
		}


		private static void UseW()
		{
			if (Spells[SpellSlot.W].IsReady())
			{
				var alliesinneed =
					HeroManager.Allies.Where(
						hero => !hero.IsMe &&
								!hero.IsDead && (!Misc.Active("Misc.Nohealshop") || !hero.InShop()) && !hero.IsZombie && hero.Distance(Player) <= Spells[SpellSlot.W].Range &&
								Misc.Active("w" + hero.ChampionName) &&
								hero.HealthPercent <= Misc.GetSlider("wpct" + hero.ChampionName))
						.ToList();

				if (Misc.Active("wonlyadc") && alliesinneed.Contains(myADC) && Player.Distance(myADC) <= Spells[SpellSlot.W].Range)
				{
					Spells[SpellSlot.W].CastOnUnit(myADC);
				}

				else if (!Misc.Active("wonlyadc"))
				{
					Obj_AI_Hero allytoheal = null;

					allytoheal = SorakaMenu.PriorityType() == SorakaMenu.HealPriority.PriorityList ? alliesinneed.MaxOrDefault(h => h.GetSetPriority()) : alliesinneed.MinOrDefault(x => x.HealthPercent);

					if (allytoheal != null)
					{
						Spells[SpellSlot.W].CastOnUnit(allytoheal);
					}
				}
			}
		}

		private static void UseR()
		{
			if (Spells[SpellSlot.R].IsReady())
			{
				switch (GetUltMode())
				{
					case SorakaMenu.UltMode.Default:
						UltDefault();
						break;
					case SorakaMenu.UltMode.Advanced:
						UltAdvanced();
						break;
				}
			}
		}

		private static void UltDefault()
		{
			List<Obj_AI_Hero> alliesinneed = HeroManager.Allies.Where(
		  hero =>
			  !hero.IsDead && !hero.IsZombie && (!Misc.Active("Misc.Nohealshop") || !hero.InShop()) && hero.Distance(Player) <= Spells[SpellSlot.W].Range &&
			  hero.HealthPercent <= Misc.GetSlider("rpct" + hero.ChampionName))
		  .ToList();

			if (alliesinneed.Count >= Misc.GetSlider("minallies"))
			{
				if (Misc.Active("ultonlyadc") && alliesinneed.Contains(myADC))
				{
					Spells[SpellSlot.R].Cast();
				}

				else if (!Misc.Active("ultonlyadc"))
				{
					Spells[SpellSlot.R].Cast();
				}
			}
		}

		private static void UltAdvanced()
		{
			List<Obj_AI_Hero> alliesinneed = HeroManager.Allies.Where(
	  hero =>
		  !hero.IsDead && !hero.IsZombie && (!Misc.Active("Misc.Nohealshop") || !hero.InShop()) && hero.Distance(Player) <= Spells[SpellSlot.W].Range && Misc.Active("r" + hero.ChampionName) &&
		  hero.HealthPercent <= Misc.GetSlider("rpct" + hero.HealthPercent))
	  .ToList();

			var indanger = alliesinneed.Count(x => x.CountEnemiesInRange(600) > 0);
			if (alliesinneed.Count >= Misc.GetSlider("minallies") && indanger >= 1)
			{
				if (Misc.Active("ultonlyadc") && alliesinneed.Contains(myADC))
				{
					Spells[SpellSlot.R].Cast();
				}

				else if (!Misc.Active("ultonlyadc"))
				{
					Spells[SpellSlot.R].Cast();
				}
			}
		}

		private static void Combo(Obj_AI_Hero target)
		{
			if (Debug)
			{
				Game.PrintChat("Combo Called");
			}
			if (Spells[SpellSlot.Q].IsReady() && Misc.Active("Combo.UseQ"))
			{
				if (Debug) { Game.PrintChat("Q is ready"); }
				var pred = Spells[SpellSlot.Q].GetPrediction(target);
				if (Debug) { Game.PrintChat("Q pred result is " + pred.Hitchance + " " + pred.CastPosition); }
				if (pred.Hitchance >= Misc.GetHitChance("Hitchance.Q"))
				{
					if (Debug)
					{
						Game.PrintChat("Hitchance exceeds set value, casting");
					}
					Spells[SpellSlot.Q].Cast(pred.CastPosition);
				}
				else
				{
					if (Debug)
					{
						Game.PrintChat("Failed min hitchance");
					}
				}
			}

			if (Debug)
			{
				Game.PrintChat("Q is not ready or set to not use Q");
			}



			if (Spells[SpellSlot.E].IsReady() && Misc.Active("Combo.UseE"))
			{
				var pred = Spells[SpellSlot.E].GetPrediction(target);
				if (pred.Hitchance >= Misc.GetHitChance("Hitchance.E"))
				{
					Spells[SpellSlot.E].Cast(pred.CastPosition);
				}
			}
		}
		#endregion

		#region Waveclear

		private static void WaveClear()
		{
			var Minions =
				ObjectManager.Get<Obj_AI_Minion>()
					.Where(
						m =>
							m.IsValidTarget() &&
							(Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.R].Range));

			if (SpellSlot.Q.IsReady() && Misc.Active("Farm.UseQ"))
			{
				var qminions =
					Minions.Where(
						m =>
							Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range);
				MinionManager.FarmLocation QLocation =
					MinionManager.GetBestCircularFarmLocation(
						qminions.Select(m => m.ServerPosition.To2D()).ToList(), Spells[SpellSlot.Q].Width,
						Spells[SpellSlot.Q].Range);
				if (QLocation.MinionsHit >= 1)
				{
					Spells[SpellSlot.Q].Cast(QLocation.Position);
				}
			}
		}
		#endregion Waveclear


		#region MixedModeLogic

		static void MixedModeLogic(Obj_AI_Hero target, bool isMixed)
		{
			if (Misc.Active("Harass.InMixed"))
			{
				Harass(target);
			}
		}
		#endregion MixedModeLogic

		#region Harass

		static void Harass(Obj_AI_Hero target)
		{
			if (Spells[SpellSlot.Q].IsReady() && Misc.Active("Harass.UseQ") && Player.ManaPercent > Misc.GetSlider("Harass.Mana"))
			{
				var pred = Spells[SpellSlot.Q].GetPrediction(target, true);
				if (pred.Hitchance >= Misc.GetHitChance("Hitchance.Q"))
				{
					Spells[SpellSlot.Q].Cast(pred.CastPosition);
				}
			}
			if (Spells[SpellSlot.E].IsReady() && Misc.Active("Harass.UseE"))
			{
				var pred = Spells[SpellSlot.E].GetPrediction(target, true);
				if (pred.Hitchance >= Misc.GetHitChance("Hitchance.E"))
				{
					Spells[SpellSlot.E].Cast(pred.CastPosition);
				}
			}
		}
		#endregion

		#region KillSteal

		private static void Killsteal()
		{
			if (!Misc.Active("Killsteal"))
			{
				return;
			}

			var targets = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsInvulnerable & !x.IsZombie);

			if (Spells[SpellSlot.Q].IsReady() && Misc.Active("Killsteal.UseQ"))
			{
				Obj_AI_Hero qtarget =
					targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.Q].Range)
						.MinOrDefault(x => x.Health);
				if (qtarget != null)
				{
					var qdmg = Player.GetSpellDamage(qtarget, SpellSlot.Q);
					if (qtarget.Health < qdmg)
					{
						var pred = Spells[SpellSlot.Q].GetPrediction(qtarget);
						if (pred != null && pred.Hitchance >= Misc.GetHitChance("Hitchance.Q"))
						{
							Spells[SpellSlot.Q].Cast(pred.CastPosition);
							return;
						}
					}
				}

				if (Spells[SpellSlot.E].IsReady() && Misc.Active("Killsteal.UseE"))
				{
					Obj_AI_Hero etarget =
						targets.Where(x => x.Distance(Player.Position) < Spells[SpellSlot.E].Range)
							.MinOrDefault(x => x.Health);
					if (etarget != null)
					{
						var edmg = Player.GetSpellDamage(etarget, SpellSlot.E);
						if (etarget.Health < edmg)
						{
							Spells[SpellSlot.E].Cast(etarget);
						}
					}
				}

				if (Spells[IgniteSlot].IsReady() && Misc.Active("Killsteal.UseIgnite"))
				{
					var targ =
						HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget() &&
																Vector3.Distance(Player.ServerPosition, x.ServerPosition) <
																Spells[IgniteSlot].Range &&
																x.Health <
														  (Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite)));
					if (targ != null)
					{
						Spells[IgniteSlot].Cast(targ);
					}
				}
			}
		}

		#endregion KillSteal

		#region Mikaels

		static void UseMikaels()
		{
			if (mikaels.IsReady())
			{
				foreach (
					var ally in
						HeroManager.Allies.Where(
							x =>
								!x.IsMe && x.Distance(Player) <= 750 &&
								x.GetSetPriority() == Misc.GetSlider("Priorities.Mikaels")))
				{
					if (ally.HasBuffOfType(BuffType.Fear) || ally.HasBuffOfType(BuffType.Stun) ||
						ally.HasBuffOfType(BuffType.Taunt) ||
						(ally.HasBuffOfType(BuffType.Blind) && ally.NetworkId == myADC.NetworkId) ||
						ally.HasBuffOfType(BuffType.Polymorph) || ally.HasBuffOfType(BuffType.Snare))
					{
						mikaels.Cast(ally);
					}
				}
			}
		}
		#endregion Mikaels


		#region AutoUltIgniteADC

		private static void AutoUltIgniteADC()
		{
			var sethealth = Misc.GetSlider("adcignitedhealth");
			if (Spells[SpellSlot.R].IsReady() && myADC.HasBuff("summonerdot") && myADC.HealthPercent <= sethealth)
			{
				Spells[SpellSlot.R].Cast();
			}
		}
		#endregion AutoUltIgniteADC


		#region AutoEStunned

		private static void AutoEStunned()
		{
			var enemy = HeroManager.Enemies.Find(x => x.IsMovementImpaired());
			if (enemy != null)
			{
				var pred = Spells[SpellSlot.E].GetPrediction(enemy);
				if (pred.Hitchance == HitChance.Immobile)
				{
					Spells[SpellSlot.E].Cast(pred.CastPosition);
				}
			}
		}
		#endregion

		#region AntiGapcloser

		static void OnGapClose(ActiveGapcloser args)
		{
			if (Player.IsDead || Player.IsRecalling())
			{
				return;
			}
			var sender = args.Sender;

			if (sender.IsValidTarget())
			{
				if (Misc.Active("Interrupter.AG.UseQ") && Vector3.Distance(args.End, Player.ServerPosition) <= Spells[SpellSlot.Q].Range)
				{
					Spells[SpellSlot.Q].Cast(sender.ServerPosition);
				}
			}
			if (sender.IsValidTarget())
			{
				if (Misc.Active("Interrupter.AG.UseE") && Vector3.Distance(args.End, Player.ServerPosition) <= Spells[SpellSlot.E].Range)
				{
					Spells[SpellSlot.E].Cast(sender.ServerPosition);
				}
			}
		}
		#endregion


		#region Interrupter
		static void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
		{
			if (Player.IsDead || Player.IsRecalling())
			{
				return;
			}
			if (sender.IsValidTarget())
			{
				if (Misc.Active("Interrupter.UseQ") && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.Q].Range)
				{
					Spells[SpellSlot.Q].Cast(sender.Position);
				}

				if (Misc.Active("Interrupter.UseE") && Vector3.Distance(sender.ServerPosition, Player.ServerPosition) <= Spells[SpellSlot.E].Range)
				{
					Spells[SpellSlot.E].Cast(sender.Position);
				}
			}
		}
		#endregion


		#region Drawing

		static void OnDraw(EventArgs args)
		{
			if (Debug)
			{
				for (int i = 0; i < Player.Buffs.Count(); i++)
				{
					var buff = Player.Buffs[i];
					var wts = Drawing.WorldToScreen(Player.Position);
					var adjustedy = wts.Y + (i*15);
					Drawing.DrawText(wts.X, adjustedy, Color.Aqua, buff.Name + " " + buff.Type);
				}
			}
			if (Player.IsDead || Player.IsRecalling() || Misc.Active("Drawing.Disable"))
			{
				return;
			}

			var DrawQ = Config.Item("Drawing.DrawQ").GetValue<Circle>();
			var DrawW = Config.Item("Drawing.DrawW").GetValue<Circle>();
			var DrawE = Config.Item("Drawing.DrawE").GetValue<Circle>();
			if (DrawQ.Active)
			{
				Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.Q].Range, DrawQ.Color);
			}
			if (DrawW.Active)
			{
				Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.W].Range, DrawW.Color);
			}
			if (DrawE.Active)
			{
				Render.Circle.DrawCircle(Player.Position, Spells[SpellSlot.E].Range, DrawE.Color);
			}
			if (Misc.Active("Drawing.Drawfarm"))
			{
				Render.Circle.DrawCircle(Player.Position, FarmRange, Color.Red);
			}
		}
		#endregion

		private static int FarmRange
		{
			get { return Config.Item("Farm.Range").GetValue<Slider>().Value; }
		}

		internal static SorakaMenu.UltMode GetUltMode()
		{
			switch (Config.Item("ultmode").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					return SorakaMenu.UltMode.Default;
				case 1:
					return SorakaMenu.UltMode.Advanced;
				default:
					return SorakaMenu.UltMode.Default;
			}
		}

		internal static bool Debug
		{
			get { return Config.Item("Debug").GetValue<bool>(); }
		}

		internal static int GetSetPriority(this Obj_AI_Hero hero)
		{
			return Config.Item("p" + hero.ChampionName).GetValue<Slider>().Value;
		}

		static string[] p4 = {
				"Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
				"Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
				"Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
				"Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
				"Zed", "Ziggs"
			};

		static string[] p3 =
	   {
				"Akali", "Diana", "Ekko", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
				"Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
				"Zilean"
			};

		static string[] p2 =
			{
				"Aatrox", "Darius", "Elise", "Evelynn", "Galio", "Gangplank", "Gragas", "Irelia", "Jax",
				"Lee Sin", "Maokai", "Morgana", "Nocturne", "Pantheon", "Poppy", "Rengar", "Rumble", "Ryze", "Swain",
				"Trundle", "Tryndamere", "Udyr", "Urgot", "Vi", "XinZhao", "RekSai"
			};

		static string[] p1 =
			{
				"Alistar", "Amumu", "Bard", "Blitzcrank", "Braum", "Cho'Gath", "Dr. Mundo", "Garen", "Gnar",
				"Hecarim", "Janna", "Jarvan IV", "Leona", "Lulu", "Malphite", "Nami", "Nasus", "Nautilus", "Nunu",
				"Olaf", "Rammus", "Renekton", "Sejuani", "Shen", "Shyvana", "Singed", "Sion", "Skarner", "Sona",
				"Soraka", "Taric", "Thresh", "Volibear", "Warwick", "MonkeyKing", "Yorick", "Zac", "Zyra"
			};

		internal static int loadpriority(this Obj_AI_Hero hero)
		{
			if (adcs.Contains(hero.ChampionName))
			{
				return 5;
			}
			if (p4.Contains(hero.ChampionName))
			{
				return 4;
			}

			if (p3.Contains(hero.ChampionName))
			{
				return 3;
			}
			if (p2.Contains(hero.ChampionName))
			{
				return 2;
			}
			if (p1.Contains(hero.ChampionName))
			{
				return 1;
			}
			return 1;
		}

	}
}





