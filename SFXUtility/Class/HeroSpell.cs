#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 HeroSpell.cs is part of SFXUtility.
 
 SFXUtility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 SFXUtility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with SFXUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace SFXUtility.Class
{
    #region

    using System;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;

    #endregion

    public class HeroSpell
    {
        #region Fields

        public bool Available = false;
        public string HeroName = string.Empty;
        public float Range = 0;
        public SpellSlot Slot = SpellSlot.Unknown;

        #endregion

        #region Constructors

        public HeroSpell(string heroName, SpellSlot slot, float range)
        {
            HeroName = heroName;
            Slot = slot;
            Range = range;
            Available = String.Equals(ObjectManager.Player.BaseSkinName, HeroName,
                StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion

        #region Methods

        public double CalculateDamage()
        {
            switch (HeroName)
            {
                case "Nunu":
                    return 250 + 150*ObjectManager.Player.Spellbook.GetSpell(Slot).Level;
                case "Chogath":
                    return 1000 + 0.7*(ObjectManager.Player.BaseAbilityDamage + ObjectManager.Player.FlatMagicDamageMod);
                case "Olaf":
                    return 25 + 45*ObjectManager.Player.Spellbook.GetSpell(Slot).Level +
                           0.4*(ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod);
                default:
                    return 0;
            }
        }

        public bool CanUseSpell()
        {
            return Available && !ObjectManager.Player.IsDead && !ObjectManager.Player.IsStunned &&
                   Slot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(Slot) == SpellState.Ready;
        }

        public bool CanUseSpell(Obj_AI_Minion target)
        {
            return CanUseSpell() && target != null && target.IsValid && !target.IsInvulnerable && !target.IsAlly &&
                   IsInRange(target);
        }

        public bool CastSpell(Obj_AI_Minion target, bool packet = false)
        {
            if (!CanUseSpell(target))
                return false;
            if (packet)
            {
                Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(target.NetworkId, Slot)).Send();
            }
            else
            {
                ObjectManager.Player.Spellbook.CastSpell(Slot, target);
            }
            return true;
        }

        public bool IsInRange(Obj_AI_Minion target)
        {
            return target != null && target.IsValid &&
                   Vector3.Distance(ObjectManager.Player.Position, target.Position) <= Range + target.BoundingRadius;
        }

        #endregion
    }
}