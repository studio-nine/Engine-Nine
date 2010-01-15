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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Movements
{
    public abstract class Movement : IMovement
    {
        private Vector3 target;
        private ISurface surface;

        public float Speed { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Heading { get; set; }

        public Vector3 Up { get { return Vector3.UnitZ; } }
        public Vector3 Forward { get { return Vector3.UnitX; } }

        
        public virtual Matrix Transform
        {
            get 
            {
                Vector3 left = Vector3.Cross(Up, Heading);
                Vector3 forward = Vector3.Cross(left, Up);

                Vector3 axis = Vector3.Cross(Forward, forward);
                float angle = (float)Math.Acos(Vector3.Dot(Forward, forward));

                return Matrix.CreateFromAxisAngle(axis, angle) * Matrix.CreateTranslation(Position) ;
            }
        }


        public Vector3 Target
        {
            get { return target; }

            set
            {
                Vector3 old = target;
                target = value;
                targetChangedSinceLastReach = true;

                target = TargetChanged(old);
            }
        }

        public ISurface Surface
        {
            get { return surface; }

            set
            {
                ISurface old = surface;
                surface = value;
                SurfaceChanged(old);
            }
        }


        private bool targetChangedSinceLastReach = false;

        public event EventHandler TargetReached;
        public event EventHandler TargetFailed;


        public Movement()
        {
            Speed = 1.0f;
        }


        protected virtual Vector3 TargetChanged(Vector3 oldTarget) { return Target; }
        protected virtual void SurfaceChanged(ISurface oldSuface) { }

        protected abstract void UpdatePosition(GameTime time);


        public void Update(GameTime time)
        {
            Vector2 v;

            v.X = Position.X - target.X;
            v.Y = Position.Y - target.Y;

            float min = (float)(Speed * time.ElapsedGameTime.TotalSeconds);

            if (v.LengthSquared() < min * min)
            {
                targetChangedSinceLastReach = false;

                if (TargetReached != null)
                    TargetReached(this, EventArgs.Empty);
            }
            else
            {
                UpdatePosition(time);
            }
        }
    }
}