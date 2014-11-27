#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 MessageToActionsMap.cs is part of SFXUtility.
 
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

    #endregion

    internal class MessageToActionsMap
    {
        #region Fields

        private readonly Dictionary<object, List<WeakAction>> _map;

        #endregion

        #region Constructor

        internal MessageToActionsMap()
        {
            _map = new Dictionary<object, List<WeakAction>>();
        }

        #endregion

        #region Methods

        internal void AddAction(object message, Action<object> callback)
        {
            if (!_map.ContainsKey(message))
                _map[message] = new List<WeakAction>();

            _map[message].Add(new WeakAction(callback));
        }

        internal List<Action<object>> GetActions(object message)
        {
            if (!_map.ContainsKey(message))
                return null;

            List<WeakAction> weakActions = _map[message];
            var actions = new List<Action<object>>();
            for (int i = weakActions.Count - 1; i > -1; --i)
            {
                WeakAction weakAction = weakActions[i];
                if (!weakAction.IsAlive)
                    weakActions.RemoveAt(i);
                else
                    actions.Add(weakAction.CreateAction());
            }

            RemoveMessageIfNecessary(weakActions, message);

            return actions;
        }

        private void RemoveMessageIfNecessary(List<WeakAction> weakActions, object message)
        {
            if (weakActions.Count == 0)
                _map.Remove(message);
        }

        #endregion
    }
}