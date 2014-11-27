#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 WeakAction.cs is part of SFXUtility.
 
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
    using System.Reflection;

    #endregion

    internal class WeakAction : WeakReference
    {
        #region Fields

        private readonly MethodInfo _method;

        #endregion

        #region Constructors

        internal WeakAction(Action<object> action)
            : base(action.Target)
        {
            _method = action.Method;
        }

        #endregion

        #region Methods

        internal Action<object> CreateAction()
        {
            if (!base.IsAlive)
                return null;

            try
            {
                return Delegate.CreateDelegate(
                    typeof (Action<object>),
                    base.Target,
                    _method.Name)
                    as Action<object>;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}