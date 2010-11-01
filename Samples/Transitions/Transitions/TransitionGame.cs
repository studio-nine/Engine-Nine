#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Graphics;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
#endif
using Nine.Animations;
#endregion

namespace Transitions
{
    /// <summary>
    /// Demonstrates how to use transitions to create various smoothed tweening animations.
    /// </summary>
    public class TransitionGame : Microsoft.Xna.Framework.Game
    {
#if WINDOWS_PHONE
        private const int TargetFrameRate = 30;
        private const int BackBufferWidth = 800;
        private const int BackBufferHeight = 480;
#elif XBOX
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 720;
#else
        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 900;
        private const int BackBufferHeight = 600;
#endif

        SpriteBatch spriteBatch;
        SpriteFont font;
        
#if !WINDOWS_PHONE
        ScrollEffect background;
#endif

        List<Button> buttons;        

        TweenAnimation<Color> startAnimation;
        Animation menuAnimation;
        Animation imageAnimation;
        
        Texture2D image;

        // These properties need to be public since
        // TransitonManager works with only public properties and fields.
        public float ImageScale { get; set; }
        public float ImageRoation { get; set; }
        public Color ImageColor { get; set; }


        public TransitionGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a input component to be used by buttons
            Input input = new Input();

            // Load sprite font
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Consolas");
            image = Content.Load<Texture2D>("glacier");


            // Set initial property for images
            ImageColor = Color.White * 0;
            ImageRoation = 0;
            ImageScale = 1;

            
#if !WINDOWS_PHONE
            // Create a scrolling background.
            //
            // Most effects from Nine.Graphics can be used with GraphicsExtensions.DrawSprite
            // method to draw 2D textures with a custom shader.
            background = new ScrollEffect(GraphicsDevice);
            background.Texture = Content.Load<Texture2D>("checker");
            background.TextureScale = Vector2.One * 0.2f;
            background.Direction = -MathHelper.PiOver4;
#endif
            
            // Create a color tweener to adjust the color of "Press any key to continue...".
            //
            // There are two ways to use transitions. This one demenstrates how to use TweenAnimation<T>
            // class directly by creating an instance and call its Update method every frame.
            //
            // TweenAnimation<T> supports most Xna data structures right out of the box, like
            // Vector2/3/4, Matrix, Quaternion, Rectangle, Point, Color, int, float, double.
            // For custom type, you need to provide your own lerp function through the constructor.
            startAnimation = new TweenAnimation<Color>()
            {
                Duration = TimeSpan.FromSeconds(0.5f),
                From = Color.White,     // Tween from white
                To = Color.Black,       // to black
                Curve = new SinCurve(), // Use sin wave
                Repeat = float.MaxValue,// Loop forever
                AutoReverse = true,     // Create a back and forth yoyo effect
            };
            startAnimation.Play();

            
            
            // Another way to use transitions is through the managed TransitionManager.
            // TranisitonManager are more powerful then Tweener<T> in that you can set a delay
            // to a transition, or queue a transition.
            // TransitionManager can be worked either through reflection or callback delegation.
            //transitions = new TransitionManager();
            
            
            // Create some buttons
            buttons = new List<Button>();

            Type[] types = new Type[]
            {
                typeof(LinearCurve), typeof(ExponentialCurve), typeof(ElasticCurve), typeof(BounceCurve), typeof(SinCurve), typeof(SmoothCurve)
            };

            foreach (Type type in types)
            {
                Button button = new Button(input) 
                {
                    X = 900, Text = type.Name, Tag = type, 
                    Bounds = MeasureBounds(type.Name, font) 
                };
                /*
                button.MouseOver += new EventHandler<MouseEventArgs>(button_MouseOver);
                button.MouseOut += new EventHandler<MouseEventArgs>(button_MouseOut);
                button.Click += new EventHandler<MouseEventArgs>(button_Click);
                */
                buttons.Add(button);
            }


            // Transite in our menus
            //TransiteIn(TimeSpan.Zero);
        }

        /// <summary>
        /// Helper method to compute a rectange bounds from text.
        /// </summary>
        private Rectangle MeasureBounds(string text, SpriteFont font)
        {
            Vector2 size = font.MeasureString(text);

            return new Rectangle(0, 0, (int)size.X, (int)size.Y);
        }

        /*
        void button_MouseOver(object sender, MouseEventArgs e)
        {
            // Turn button color to yellow
            //
            // The second null parameter indicates that TransitionManager will be using
            // the current value of the specfied property as the begin value of the transition.
            transitions.Start<Color, Linear>(0.4f, null, Color.Yellow, sender, "Color");
        }

        void button_MouseOut(object sender, MouseEventArgs e)
        {
            // Turn button color to white.
            transitions.Start<Color, Linear>(0.4f, null, Color.White, sender, "Color");
        }

        void button_Click(object sender, MouseEventArgs e)
        {
            int index = 0;

            // Find which button is pressed
            for (index = 0; index < buttons.Count; index++)
                if (buttons[index] == sender)
                    break;

            // Transite menus
            TransiteOut(index);
            TransiteIn(TimeSpan.FromSeconds(1.2f));

            float duration = 0.25f;

            // Fade image
            transitions.Start<Color, Linear>(duration, null, Color.White * 0, this, "ImageColor");
            transitions.Queue<Color, Linear>(duration, null, Color.White, this, "ImageColor");
            
            // Scale image
            transitions.Start<float, Exponential>(duration, null, 0.6f, this, "ImageScale");

            if ((Type)buttons[index].Tag == typeof(Linear))
            {
                transitions.Queue<float, Linear>(duration, null, 1.0f, this, "ImageScale");
            }
            else if ((Type)buttons[index].Tag == typeof(Exponential))
            {
                transitions.Queue<float, Exponential>(duration, null, 1.0f, this, "ImageScale");
            }
            else if ((Type)buttons[index].Tag == typeof(Elastic))
            {
                transitions.Queue<float, Elastic>(duration, null, 1.0f, this, "ImageScale");
            }
            else if ((Type)buttons[index].Tag == typeof(Bounce))
            {
                transitions.Queue<float, Bounce>(duration, null, 1.0f, this, "ImageScale");
            }
            else if ((Type)buttons[index].Tag == typeof(Sin))
            {
                transitions.Queue<float, Sin>(duration, null, 1.0f, this, "ImageScale");
            }
            else if ((Type)buttons[index].Tag == typeof(Smooth))
            {
                transitions.Queue<float, Smooth>(duration, null, 1.0f, this, "ImageScale");
            }
        }

        /// <summary>
        /// Shows all buttons like what they did with Red Alert 2.
        /// </summary>
        private void TransiteIn(TimeSpan delay)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Y = 220 + i * 24;

                transitions.Start<float, Exponential>(TimeSpan.FromSeconds(i * 0.1f) + delay,
                                                      TimeSpan.FromSeconds(0.4f),
                                                      900, 780,
                                                      LoopStyle.None, Easing.In,
                                                      buttons[i], "X");
            }
        }

        /// <summary>
        /// Hides all buttons like what they did with Red Alert 2.
        /// </summary>
        private void TransiteOut(int triggerIndex)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                transitions.Start<float, Exponential>(TimeSpan.FromSeconds(Math.Abs(i - triggerIndex) * 0.1f),
                                                      TimeSpan.FromSeconds(0.4f),
                                                      null, 900,
                                                      LoopStyle.None, Easing.In,
                                                      buttons[i], "X");
            }
        }
        */

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);


            // Initialize render state
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            

            // Update transitions
            //transitions.Update(gameTime);
            startAnimation.Update(gameTime);

#if !WINDOWS_PHONE
            // Update background scroll
            background.Update(gameTime);

            // Draw background
            background.SetViewport(GraphicsDevice.Viewport.Bounds);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, background);
            spriteBatch.Draw(background.Texture, GraphicsDevice.Viewport.Bounds, null, Color.White);
            spriteBatch.End();
#endif
            spriteBatch.Begin();
            
            spriteBatch.DrawString(font, "Press any key to continue..." , Vector2.Zero, startAnimation.Value);

            spriteBatch.Draw(image, new Vector2(450, 300), null, ImageColor, ImageRoation, new Vector2(400, 300), ImageScale * 0.6f, SpriteEffects.None, 0);
            
            // Draw buttons
            foreach (Button button in buttons)
            {
                spriteBatch.DrawString(font, button.Text, new Vector2(button.X, button.Y), button.Color);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
