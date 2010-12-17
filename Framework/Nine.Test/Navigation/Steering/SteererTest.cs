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
        private GameTime ElapsedTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1));

        [TestMethod()]
        public void SeekToPosition()
        {
            Steerer steerer = new Steerer();
            steerer.MaxSpeed = 10;
            steerer.Acceleration = 10;
            steerer.Behaviors.Add(new SeekBehavior() { Target = Vector2.UnitX * 1000 });

            for (int i = 0; i < 11; i++)
            {
                steerer.Update(ElapsedTime);
                if (i == 8)
                {
                    Assert.IsTrue(steerer.Speed < steerer.MaxSpeed);
                }
                if (i == 10)
                {
                    Assert.IsTrue(steerer.Speed == steerer.MaxSpeed);
                    Assert.IsTrue(steerer.Position.Length() > 5);
                }
            }
        }

        [TestMethod()]
        public void ArriveAtPosition()
        {
            Steerer steerer = new Steerer();
            steerer.MaxSpeed = 10;
            steerer.Acceleration = 10;
            steerer.Behaviors.Add(new ArriveBehavior() { Target = Vector2.UnitX * 15 });

            for (int i = 0; i < 40; i++)
            {
                steerer.Update(ElapsedTime);
                if (i == 8)
                {
                    Assert.IsTrue(steerer.Speed < steerer.MaxSpeed);
                }
                if (i == 10)
                {
                    Assert.IsTrue(steerer.Speed == steerer.MaxSpeed);
                    Assert.IsTrue(steerer.Position.Length() > 5);
                }
                if (steerer.Position.Length() > 11)
                {
                    Assert.IsTrue(steerer.Speed < steerer.MaxSpeed);
                }
            }

            Assert.AreEqual<float>(0, steerer.Speed);
        }
    }
}
