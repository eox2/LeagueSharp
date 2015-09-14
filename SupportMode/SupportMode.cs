using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SupportMode
{
	class SupportMode
	{
		private Menu menu;
		private Obj_AI_Hero Player;

		public SupportMode()
		{
			Player = ObjectManager.Player;
			menu = new Menu("Support Mode", "SupportMode", true);
			menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true)).Permashow(true, "Support Mode");
			menu.AddItem(new MenuItem("range", "Distance from allies").SetValue(new Slider(1400, 700, 2000)));
			menu.AddItem(new MenuItem("drawenabled", "Enable Drawing").SetValue(new Circle(true, System.Drawing.Color.Red)));
			menu.AddToMainMenu();

			Game.PrintChat("Support Mode Loaded " + "Status: " + enabled);

			Orbwalking.BeforeAttack += BeforeAttack;
			Drawing.OnDraw += OnDraw;

		}

		void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
		{
			if (enabled)
			{
				if (args.Target.Type != GameObjectType.obj_AI_Hero)
				{
					var alliesinrange = HeroManager.Allies.Count(x => !x.IsMe && x.Distance(Player) <= range);
					if (alliesinrange > 0)
					{
						args.Process = false;
					}
				}
            }
		}

		void OnDraw(EventArgs args)
		{
			if (drawenabled.Active)
			{
				Render.Circle.DrawCircle(Player.Position, range, drawenabled.Color);
			}
		}

		bool enabled
		{
			get { return menu.Item("enabled").GetValue<bool>(); }
		}

		Circle drawenabled
		{
			get { return menu.Item("drawenabled").GetValue<Circle>(); }
		}

		float range
		{
			get { return menu.Item("range").GetValue<Slider>().Value; }
		}
	}
}
