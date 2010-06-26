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

namespace Nine.StateMachines
{
    public interface IState
    {
        void BeginState();
        void Update(GameTime gameTime);
        void EndState();
    }
}
