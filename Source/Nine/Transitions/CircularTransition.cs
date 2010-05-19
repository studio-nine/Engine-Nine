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


namespace Isles.Transitions
{
    public sealed class CircularTransition : ITransition<float>, ITransition<Vector2>, ITransition<Vector3>
    {
        private Vector3 axis;
        private Quaternion rotation;

        public float Radius { get; set; }
        public float Angle { get; set; }
        public float Speed { get; set; }
        public Vector3 Position { get; set; }
        
        public TimeSpan Duration { get; set; }

        public Vector3 Axis
        {
            get { return axis; }
            set
            {
                axis = Vector3.Normalize(value);

                Vector3 p = Vector3.Cross(Vector3.UnitZ, axis);
                float q = (float)Math.Acos(Vector3.Dot(Vector3.UnitZ, axis));

                rotation = Quaternion.CreateFromAxisAngle(p, q);
            }
        }


        public CircularTransition()
        {
            Speed = 1.0f;
            Radius = 1.0f;
            Axis = Vector3.UnitZ;
            Duration = TimeSpan.FromSeconds(1);
        }


        float ITransition<float>.Update(GameTime time)
        {
            Angle += (float)(Speed * Math.PI * 2 * time.ElapsedGameTime.TotalMilliseconds / Duration.TotalMilliseconds);

            if (Angle > MathHelper.Pi * 2)
                Angle -= MathHelper.Pi * 2;
            if (Angle < MathHelper.Pi * 2)
                Angle += MathHelper.Pi * 2;

            return Angle;
        }


        Vector2 ITransition<Vector2>.Update(GameTime time)
        {
            Vector2 v;
            
            (this as ITransition<float>).Update(time);

            v.X = Radius * (float)Math.Cos(Angle) + Position.X;
            v.Y = Radius * (float)Math.Sin(Angle) + Position.Y;

            return v;
        }


        Vector3 ITransition<Vector3>.Update(GameTime time)
        {
            Vector3 result;
            Vector2 v = (this as ITransition<Vector2>).Update(time);

            result.X = v.X - Position.X;
            result.Y = v.Y - Position.Y;
            result.Z = 0;

            return Vector3.Transform(result, rotation) + Position;
        }
    }
}