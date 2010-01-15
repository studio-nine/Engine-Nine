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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Isles.Transitions;
#endregion


namespace Isles.Samples
{
    [SampleClass]
    public class TransitionGame : BasicModelViewerGame
    {
        int index = 0;
        List<ITransition<Vector2>> transitions = new List<ITransition<Vector2>>();

        public TransitionGame()
        {
            Camera.Input.MouseDown += new EventHandler<MouseEventArgs>(Input_LeftButtonDown);
        }

        void Input_LeftButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                index = (index + 1) % transitions.Count;

                if (transitions[index] is IAnimation)
                    (transitions[index] as IAnimation).Play();

                Window.Title = "Transition Type: " + transitions[index].GetType().Name;
            }
        }

        protected override void LoadContent()
        {
            LinearTransition<Vector2> linear = new LinearTransition<Vector2>();

            linear.Start = new Vector2(100, 100);
            linear.End = new Vector2(100, 400);
            linear.Effect = TransitionEffect.Loop;
            linear.Complete += new EventHandler(linear_Complete);

            transitions.Add(linear);

            SinTransition<Vector2> sin = new SinTransition<Vector2>();

            sin.Start = new Vector2(100, 100);
            sin.End = new Vector2(100, 400);
            sin.Effect = TransitionEffect.Yoyo;

            transitions.Add(sin);

            ExponentialTransition<Vector2> exp = new ExponentialTransition<Vector2>();

            exp.Power = 32;
            exp.Delay = TimeSpan.FromSeconds(1);
            exp.Start = new Vector2(100, 100);
            exp.End = new Vector2(100, 400);
            exp.Effect = TransitionEffect.Pulse;

            transitions.Add(exp);

            CurveTransition<Vector2> curve = new CurveTransition<Vector2>();

            curve.Curve = Content.Load<Curve>("Curve");
            curve.Start = new Vector2(100, 100);
            curve.End = new Vector2(100, 400);
            curve.Effect = TransitionEffect.None;

            transitions.Add(curve);


            CircularTransition circular = new CircularTransition();

            circular.Radius = 100;
            circular.Position = new Vector3(200, 200, 0);

            transitions.Add(circular);


            base.LoadContent();
        }

        void linear_Complete(object sender, EventArgs e)
        {
            Random random = new Random();

            FrameRate.Color = new Color((float)random.NextDouble(), 
                                        (float)random.NextDouble(), 
                                        (float)random.NextDouble());
        }

        protected override void Update(GameTime gameTime)
        {
            FrameRate.Position = transitions[index].Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    
        [SampleMethod]
        public static void Test()
        {
            using (TransitionGame game = new TransitionGame())
            {
                game.Run();
            }
        }
    }
}
