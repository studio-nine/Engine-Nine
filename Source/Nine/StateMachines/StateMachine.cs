#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//
//  Author  : Mahdi Khodadadi Fard.
//  Date    : 2010-06-26
//
//=============================================================================
#endregion

using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Nine.StateMachines
{
    /// <summary>
    /// Represents a Finite State Machine (FSM) which is useful for controlling different states for game entities.
    /// </summary>
    public sealed class StateMachine
    {
        IState currentState = State.Empty;

        /// <summary>
        /// Gets the current state of this <seealso cref="Nine.StateMachines.StateMachine"/> instance.
        /// </summary>
        public IState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        public void GoToState(IState value)
        {
            if (currentState != null)
            {
                currentState.EndState();
            }

            currentState = value;

            if (currentState == null)
            {
                currentState = State.Empty;
            }

            if (currentState != null)
            {
                currentState.BeginState();
            }
        }

        public StateMachine()
        {
        }

        public void Update(GameTime gameTime)
        {
            if (currentState != null)
            {
                currentState.Update(gameTime);
            }
        }
    }
}
