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
    public sealed class SpinnerTransition<T> : ITransition<T>
    {
        private Type type;
        private Vector2 value2;
        private Vector3 value3;
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

        public T Value
        {
            get
            {
                if (type == typeof(float))
                    return (T)(object)Angle;
                if (type == typeof(Vector2))
                    return (T)(object)value2;
                return (T)(object)value3;
            }
        }

        object ITransition.Value
        {
            get { return Value; }
        }

        public SpinnerTransition()
        {
            Speed = 1.0f;
            Radius = 1.0f;
            Axis = Vector3.UnitZ;
            Duration = TimeSpan.FromSeconds(1);
            type = typeof(T);

            if (type != typeof(float) && type != typeof(Vector2) && type != typeof(Vector3))
                throw new InvalidCastException("Spinner Transition only support float, Vector2 and Vector3.");
        }


        public void Update(GameTime time)
        {
            Angle += (float)(Speed * Math.PI * 2 * time.ElapsedGameTime.TotalMilliseconds / Duration.TotalMilliseconds);

            if (Angle > MathHelper.Pi * 2)
                Angle -= MathHelper.Pi * 2;
            if (Angle < MathHelper.Pi * 2)
                Angle += MathHelper.Pi * 2;

            if (type == typeof(Vector2) || type == typeof(Vector3))
            {
                value2.X = Radius * (float)Math.Cos(Angle) + Position.X;
                value2.Y = Radius * (float)Math.Sin(Angle) + Position.Y;
            }

            if (type == typeof(Vector3))
            {
                value3.X = value2.X - Position.X;
                value3.Y = value2.Y - Position.Y;
                value3.Z = 0;

                value3 = Vector3.Transform(value3, rotation) + Position;
            }
        }
    }
}