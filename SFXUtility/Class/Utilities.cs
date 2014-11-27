#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Utilities.cs is part of SFXUtility.
 
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using Color = System.Drawing.Color;

    #endregion

    public static class Utilities
    {
        #region Methods

        public static void AddItems(this Menu menu, List<MenuItem> menuItems)
        {
            foreach (var menuItem in menuItems)
            {
                menu.AddItem(menuItem);
            }
        }

        public static void DrawCross(Vector2 pos, float size, float thickness, Color color)
        {
            Drawing.DrawLine(pos.X - size, pos.Y - size, pos.X + size, pos.Y + size, thickness, color);
            Drawing.DrawLine(pos.X + size, pos.Y - size, pos.X - size, pos.Y + size, thickness, color);
        }

        public static void DrawTextCentered(Vector2 pos, Color color, string content)
        {
            var rec = Drawing.GetTextExtent(content);
            Drawing.DrawText(pos.X - rec.Width/2f, pos.Y - rec.Height/2f, color, content);
        }

        public static string GetMd5(string content)
        {
            return
                BitConverter.ToString(
                    ((HashAlgorithm) CryptoConfig.CreateFromName("MD5")).ComputeHash(new UTF8Encoding().GetBytes(content)))
                    .Replace("-", string.Empty)
                    .ToLower();
        }

        public static Obj_AI_Minion GetNearestMinionByNames(Vector3 pos, string[] names)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        minion =>
                            minion.IsValid &&
                            names.Any(
                                name => String.Equals(minion.SkinName, name, StringComparison.CurrentCultureIgnoreCase)));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }

        public static bool HasItem(this Obj_AI_Hero hero, int id)
        {
            return hero.IsValid && Items.HasItem(id, hero);
        }

        public static float HealthPercentage(this Obj_AI_Hero hero)
        {
            return hero.Health*100/hero.MaxHealth;
        }

        public static string HexConverter(SharpDX.Color color)
        {
            return string.Format("#{0}", color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"));
        }

        public static bool IsBetween(int start, int end, int value)
        {
            return start >= end && value >= start && value <= end;
        }

        public static bool IsBuffActive(this Obj_AI_Hero hero, string buffName)
        {
            return hero.Buffs.Any(buff => buff.IsActive && buff.Name == buffName);
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open))
                {
                }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }

        public static bool IsOnScreen(this Obj_AI_Base obj)
        {
            Vector2 screen = Drawing.WorldToScreen(obj.Position);
            return !(screen.X < 0) && !(screen.X > Drawing.Width) && !(screen.Y < 0) && !(screen.Y > Drawing.Height);
        }

        public static bool IsOnScreen(this Obj_AI_Base obj, float radius)
        {
            Vector2 screen = Drawing.WorldToScreen(obj.Position);
            return !(screen.X + radius < 0) && !(screen.X - radius > Drawing.Width) && !(screen.Y + radius < 0) &&
                   !(screen.Y - radius > Drawing.Height);
        }

        public static bool IsOnScreen(Vector2 start, Vector2 end)
        {
            if (start.X > 0 && start.X < Drawing.Width && start.Y > 0 && start.Y < Drawing.Height && end.X > 0 &&
                end.X < Drawing.Width && end.Y > 0 && end.Y < Drawing.Height)
            {
                return true;
            }

            return new List<Geometry.IntersectionResult>
            {
                new Vector2(0, 0).Intersection(new Vector2(0, Drawing.Width), start, end),
                new Vector2(0, Drawing.Width).Intersection(new Vector2(Drawing.Height, Drawing.Width), start, end),
                new Vector2(Drawing.Height, Drawing.Width).Intersection(new Vector2(Drawing.Height, 0), start, end),
                new Vector2(Drawing.Height, 0).Intersection(new Vector2(0, 0), start, end)
            }.Any(intersection => intersection.Intersects);
        }

        public static float ManaPercentage(this Obj_AI_Hero hero)
        {
            return hero.Mana*100/hero.MaxMana;
        }

        #endregion
    }
}