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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nine.StateMachines
{
    public abstract class State : IState
    {
        private class EmptyState : State
        {
        }

        public static readonly State Empty = new EmptyState();

        protected virtual void BeginState()
        {
        }

        protected virtual void Update(GameTime gameTime)
        {
        }

        protected virtual void EndState()
        {
        }

        #region IState Members

        void IState.BeginState()
        {
            BeginState();
        }

        void IState.Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Update(gameTime);
        }

        void IState.EndState()
        {
            EndState();
        }

        #endregion
    }
}
