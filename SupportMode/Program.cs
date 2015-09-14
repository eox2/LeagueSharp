using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SupportMode
{
	class Program
	{
		static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += OnLoad;
		}

		static void OnLoad(EventArgs args)
		{
			SupportMode supportmode = new SupportMode();
		}

	}
}
