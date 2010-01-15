#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Components
{
    public class Timer : GameComponent
    {
        private float time;
        private float interval;
        private bool runOnlyOnce;

        public float Interval 
        {
            get { return interval; }
            set { if (value <= 0) throw new ArgumentException(); interval = value; }
        }

        public event EventHandler Tick;

        public Timer(Game game) : this(game, 1000) { }

        public Timer(Game game, float interval) : this(game, interval, false, null) { }
        
        public Timer(Game game, float interval, bool runOnlyOnce, EventHandler handler)
            : base(game)
        {
            this.Interval = interval;
            this.runOnlyOnce = runOnlyOnce;

            if (handler != null)
                Tick += handler;
        }

        public override void Update(GameTime gameTime)
        {
            time += (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            while (time > Interval)
            {
                if (Tick != null)
                    Tick(this, EventArgs.Empty);

                if (runOnlyOnce)
                {
                    Game.Components.Remove(this);
                    break;
                }

                time -= Interval;
            }

            base.Update(gameTime);
        }
    }
}