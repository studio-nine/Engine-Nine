#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Nine;
using Microsoft.Xna.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nine.Navigation.Steering.Test
{
    [TestClass()]
    public class SteererTest
    {
        private GameTime ElapsedTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.102342f));

        [TestMethod()]
        public void SeekToPosition()
        {
            Steerer steerer = new Steerer();
            steerer.MaxSpeed = 10;
            steerer.Acceleration = 10;
            steerer.Behaviors.Add(new SeekBehavior() { Target = Vector2.One * 1000 });

            float[] targetSpeed = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 };

            for (int i = 0; i < targetSpeed.Length; i++)
            {
                steerer.Update(ElapsedTime);
                Assert.AreEqual<int>((int)targetSpeed[i], (int)Math.Round(steerer.Speed));
            }
        }

        [TestMethod()]
        public void SeekToPositionMaxAcceleration()
        {
            Steerer steerer = new Steerer();
            steerer.MaxSpeed = 10;
            steerer.Acceleration = float.MaxValue;
            steerer.Behaviors.Add(new SeekBehavior() { Target = Vector2.One * 1000 });

            float[] targetSpeed = new float[] { 10, 10, 10 };

            for (int i = 0; i < targetSpeed.Length; i++)
            {
                steerer.Update(ElapsedTime);
                Assert.AreEqual<int>((int)targetSpeed[i], (int)Math.Round(steerer.Speed));
            }
        }

        [TestMethod()]
        public void ArriveAtPosition()
        {
            Steerer steerer = new Steerer();
            steerer.MaxSpeed = 10;
            steerer.Acceleration = 20.02345f;
            steerer.Behaviors.Add(new ArriveBehavior() { Target = Vector2.One * 100 });

            float[] targetSpeed = new float[] { 2, 4, 6, 8, 10, 10, 10 };

            for (int i = 0; i < targetSpeed.Length; i++)
            {
                steerer.Update(ElapsedTime);
                Assert.AreEqual<int>((int)targetSpeed[i], (int)Math.Round(steerer.Speed));
            }

            bool hasStopped = false;
            bool hasFullyStopped = false;
            for (int i = 0; i < 2000; i++)
            {
                steerer.Update(ElapsedTime);

                if (hasStopped)
                {
                    Assert.IsTrue(steerer.Speed < steerer.MaxSpeed);
                    if (steerer.Speed <= 0)
                    {
                        hasFullyStopped = true;
                        Assert.IsTrue(Vector2.Distance(Vector2.One * 100, steerer.Position) < 1f);
                    }
                    if (hasFullyStopped)
                    {
                        Assert.IsTrue(steerer.Speed <= float.Epsilon);
                    }
                }
                if ((int)Math.Round(steerer.Speed) != (int)Math.Round(steerer.MaxSpeed))
                    hasStopped = true;
            }
            Assert.IsTrue(hasStopped);
            Assert.IsTrue(hasFullyStopped);
        }

        [TestMethod()]
        public void ArriveAtPositionMaxAcceleration()
        {
            Steerer steerer = new Steerer();
            steerer.MaxSpeed = 10;
            steerer.Acceleration = float.MaxValue;
            steerer.Behaviors.Add(new ArriveBehavior() { Target = Vector2.One * 100 });

            float[] targetSpeed = new float[] { 10, 10, 10 };

            for (int i = 0; i < targetSpeed.Length; i++)
            {
                steerer.Update(ElapsedTime);
                Assert.AreEqual<int>((int)targetSpeed[i], (int)Math.Round(steerer.Speed));
            }

            bool hasStopped = false;
            for (int i = 0; i < 2000; i++)
            {
                steerer.Update(ElapsedTime);

                if (hasStopped)
                {
                    Assert.AreEqual<int>(0, (int)Math.Round(steerer.Speed));
                    Assert.IsTrue(Vector2.Distance(Vector2.One * 100, steerer.Position) < 1f);
                }
                if ((int)Math.Round(steerer.Speed) != (int)Math.Round(steerer.MaxSpeed))
                    hasStopped = true;
            }
            Assert.IsTrue(hasStopped);
        }
        
        [TestMethod()]
        public void ArriveAtPositionNearby()
        {
            Steerer steerer = new Steerer();
            steerer.MaxSpeed = 10;
            steerer.Acceleration = 20.02345f;
            steerer.Behaviors.Add(new ArriveBehavior() { Target = Vector2.One * 5 });

            bool hasStopped = false;
            bool hasFullyStopped = false;
            float previousSpeed = float.MinValue;
            for (int i = 0; i < 200; i++)
            {
                steerer.Update(ElapsedTime);

                if (hasStopped)
                {
                    Assert.IsTrue(steerer.Speed < steerer.MaxSpeed);
                    if (steerer.Speed <= 0)
                    {
                        hasFullyStopped = true;
                        Assert.IsTrue(Vector2.Distance(Vector2.One * 5, steerer.Position) < 0.5f);
                    }
                    if (hasFullyStopped)
                    {
                        Assert.IsTrue(steerer.Speed <= float.Epsilon);
                    }
                }
                if (steerer.Speed < previousSpeed)
                    hasStopped = true;
                previousSpeed = steerer.Speed;
            }
            Assert.IsTrue(hasStopped);
            Assert.IsTrue(hasFullyStopped);
        }
    }
}
