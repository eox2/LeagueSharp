#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Mediator.cs is part of SFXUtility.
 
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

    public class Mediator
    {
        #region Fields

        private readonly MessageToActionsMap _messageToCallbacksMap;

        #endregion

        #region Constructor

        public Mediator()
        {
            _messageToCallbacksMap = new MessageToActionsMap();
        }

        #endregion

        #region Methods

        public void NotifyColleagues(object from, object message)
        {
            List<Action<object>> actions = _messageToCallbacksMap.GetActions(from);

            if (actions != null)
                actions.ForEach(action => action(message));
        }

        public void Register(object from, Action<object> callback)
        {
            _messageToCallbacksMap.AddAction(from, callback);
        }

        #endregion
    }
}