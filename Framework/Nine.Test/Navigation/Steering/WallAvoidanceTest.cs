#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace Nine.Navigation.Steering.Test
{/*
    [TestClass()]
    public class WallAvoidanceTest
    {
        private static TimeSpan ElapsedTime = TimeSpan.FromSeconds(0.102342f);

        [TestMethod]
        public void MoveAcrossAWall()
        {
            Move(60, 60, 10, -Vector2.UnitX * 1000, Vector2.UnitX * 1000, new Vector2[] 
            {
                new Vector2(0, -100), new Vector2(0, 100)
            });
        }

        [TestMethod]
        public void MoveAcrossAWallMaxAcceleration()
        {
            Move(float.MaxValue, 60, 10, -Vector2.UnitX * 1000, Vector2.UnitX * 1000, new Vector2[] 
            {
                new Vector2(0, -100), new Vector2(0, 100)
            });
        }

        [TestMethod]
        public void MoveCloseToAWall()
        {
            Move(60, 60, 10, -Vector2.UnitX * 1000, Vector2.UnitX * 99, new Vector2[] 
            {
                new Vector2(0, -100), new Vector2(0, 100)
            });
        }

        [TestMethod]
        public void MoveToAWall()
        {
            Move(60, 60, 10, -Vector2.UnitX * 1000, Vector2.UnitX * 100, new Vector2[] 
            {
                new Vector2(0, -100), new Vector2(0, 100)
            });
        }

        static int Size = 64;

        public void Move(float acceleration, float maxSpeed, float boundingRadius, Vector2 from, Vector2 to, IList<Vector2> walls)
        {
            BoundingBox box = BoundingBox.CreateFromPoints(walls.Select(v => new Vector3(v, 0)));
            box.Max += Vector3.One * Size;
            box.Min -= Vector3.One * Size;

            GridSceneManager<LineSegment> wallQuery = new GridSceneManager<LineSegment>(box, 1, 1);
            for (int i = 0; i < walls.Count - 1; i += 2)
                wallQuery.Add(new LineSegment(walls[i], walls[i + 1]), box);

            Steerable steerer = new Steerable();
            steerer.BlendMode = SteeringBehaviorBlendMode.Solo;
            steerer.MaxSpeed = maxSpeed;
            steerer.Acceleration = acceleration;
            steerer.BoundingRadius = boundingRadius;
            steerer.Position = from;
            steerer.Target = to;
            steerer.Behaviors.Add(new WallAvoidanceBehavior() { Walls = wallQuery });
            steerer.Behaviors.Add(new ArriveBehavior());

            int frames = 10000;
            bool hasFullyStopped = false;
            for (int i = 0; i < frames; i++)
            {
                steerer.Update(ElapsedTime);
                if (steerer.Speed <= 0 && steerer.Force.LengthSquared() <= 0)
                {
                    hasFullyStopped = true;
                    break;
                }
            }

            Assert.IsTrue(hasFullyStopped, string.Format("The entity didn't stop after {0} seconds", frames * ElapsedTime.TotalSeconds));
            Assert.IsTrue((steerer.Position - to).Length() <= steerer.BoundingRadius, "The entity isn't close enough to the expected target. ");
        }
    }*/
}
