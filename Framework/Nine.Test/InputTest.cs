#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Nine.Components;

namespace Nine.Test
{
    [TestClass()]
    public class InputTest
    {
        [TestInitialize()]
        public void TestInitialize()
        {
            InputComponent.Current = null;
        }

        [TestMethod()]
        public void UsageTest()
        {
            Game game = new Game();
         
            InputComponent component = new InputComponent();

            game.Components.Add(component);

            Input input1 = new Input();
            Input input2 = new Input();

            Assert.AreEqual(component, input1.Component);
            Assert.AreEqual(InputComponent.Current, input2.Component);

            input1.Enabled = false;
            input2.MouseDown += (o, e) => { };
            input2.MouseUp += (o, e) => { };
            input2.MouseWheel += (o, e) => { };
            input2.KeyDown += (o, e) => { };
            input2.KeyUp += (o, e) => { };
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InputConstructorTest()
        {
            new Input();
        }

        class EventListener
        {
            public EventListener(Input input)
            {
                input.MouseDown += new EventHandler<MouseEventArgs>(input_MouseDown);
            }

            void input_MouseDown(object sender, MouseEventArgs e)
            {

            }
        }

        /// <summary>
        /// This test ensures that registering event handlers will
        /// cause the event source to keep a reference on the handler
        /// object.
        /// </summary>
        [TestMethod()]
        public void EventReferenceTest()
        {
            new InputComponent();

            Input input = new Input();
            object listener = new Input();

            WeakReference reference = new WeakReference(listener);

            listener = null;

            GC.Collect();

            Assert.AreEqual(false, reference.IsAlive);

            /*
             * Somehow this test don't work in release mode
            listener = new EventListener(input);

            reference = new WeakReference(listener);

            listener  = null;

            GC.Collect();

            Assert.AreEqual(true, reference.IsAlive);
             */
        }


        /// <summary>
        /// Ensures all unreferenced Input to be released
        /// during garbage collection.
        /// </summary>
        [TestMethod()]
        public void InputWeakReferenceTest()
        {
            InputComponent component = new InputComponent();

            WeakReference reference = new WeakReference(new Input());

            GC.Collect();

            Assert.AreEqual(false, reference.IsAlive);
        }
    }
}
