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
        Input input;

        List<Button> buttons;

        AnimationPlayer animations = new AnimationPlayer();
        
        Texture2D image;
        Texture2D background;

        // These properties need to be public since
        // TransitonManager works with only public properties and fields.
        public float ImageScale { get; set; }
        public float ImageRoation { get; set; }
        public Color ImageColor { get; set; }
        public Color TextColor { get; set; }


        public TransitionGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Components.Add(new FrameRate(this, "Consolas"));
            Components.Add(new InputComponent(Window.Handle));
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a input component to be used by buttons
            input = new Input();

            // Load sprite font
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Consolas");
            image = Content.Load<Texture2D>("glacier");
            background = Content.Load<Texture2D>("checker");


            // Set initial property for images
            ImageColor = Color.White * 0;
            ImageRoation = 0;
            ImageScale = 1;
            
            // Create a color tweener to adjust the color of "Press any key to continue...".
            //
            // TweenAnimation<T> supports most Xna data structures right out of the box, like
            // Vector2/3/4, Matrix, Quaternion, Rectangle, Point, Color, int, float, double.
            // For custom type, you need to provide your own lerp function through the constructor.
            animations["Start"].Play(new TweenAnimation<Color>()
            {
                Target = this,
                TargetProperty = "TextColor",
                Duration = TimeSpan.FromSeconds(0.5f),
                From = Color.White,     // Tween from white
                To = Color.Black,       // to black
                Curve = Curves.Sin,     // Use sin wave
                Repeat = float.MaxValue,// Loop forever
                AutoReverse = true,     // Create a back and forth yoyo effect
            });

            
            // Create some buttons
            buttons = new List<Button>();

            ICurve[] curves = new ICurve[]
            {
                Curves.Linear,
                Curves.Sin,
                Curves.Exponential,
                Curves.Elastic,
                Curves.Bounce,
                Curves.Smooth,
            };

            foreach (ICurve curve in curves)
            {
                Button button = new Button(input) 
                {
                    X = 900, Text = curve.GetType().Name, Tag = curve,
                    Bounds = MeasureBounds(curve.GetType().Name, font) 
                };

                button.MouseOver += new EventHandler<MouseEventArgs>(button_MouseOver);
                button.MouseOut += new EventHandler<MouseEventArgs>(button_MouseOut);
                button.Click += new EventHandler<MouseEventArgs>(button_Click);
                buttons.Add(button);
            }


            // Transite in our menus
            TransiteIn(TimeSpan.Zero);
        }

        /// <summary>
        /// Helper method to compute a rectange bounds from text.
        /// </summary>
        private Rectangle MeasureBounds(string text, SpriteFont font)
        {
            Vector2 size = font.MeasureString(text);

            return new Rectangle(0, 0, (int)size.X, (int)size.Y);
        }

        void button_MouseOver(object sender, MouseEventArgs e)
        {
            // Turn button color to yellow
            animations[sender].Play(new TweenAnimation<Color>()
            {
                Target = sender,
                TargetProperty = "Color",
                Duration = TimeSpan.FromSeconds(0.5f),
                To = Color.Yellow,
            });
        }

        void button_MouseOut(object sender, MouseEventArgs e)
        {
            // Turn button color to white.
            animations[sender].Play(new TweenAnimation<Color>()
            {
                Target = sender,
                TargetProperty = "Color",
                Duration = TimeSpan.FromSeconds(0.5f),
                To = Color.White,
            });
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

            TimeSpan duration = TimeSpan.FromSeconds(0.25f);

            // Fade image
            animations["ImageColor"].Play(new SequentialAnimation(
                new TweenAnimation<Color>()
                {
                    Duration = duration,
                    To = Color.White * 0,
                    Target = this,
                    TargetProperty = "ImageColor",
                },
                new TweenAnimation<Color>()
                {
                    Duration = duration,
                    To = Color.White,
                    Target = this,
                    TargetProperty = "ImageColor",
                }));

            animations["ImageScale"].Play(new SequentialAnimation(
                new TweenAnimation<float>()
                {
                    Curve = Curves.Exponential,
                    Duration = duration,
                    To = 0.6f,
                    Target = this,
                    TargetProperty = "ImageScale",
                },
                new TweenAnimation<float>()
                {
                    Curve = (ICurve)buttons[index].Tag,
                    Duration = duration,
                    To = 1f,
                    Target = this,
                    TargetProperty = "ImageScale",
                }));
        }

        /// <summary>
        /// Shows all buttons like what they did with Red Alert 2.
        /// </summary>
        private IAnimation TransiteIn(TimeSpan delay)
        {
            LayeredAnimation layeredAnimation = new LayeredAnimation();

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Y = 220 + i * 24;

                layeredAnimation.Animations.Add(new SequentialAnimation(
                        new DelayAnimation(TimeSpan.FromSeconds(i * 0.1f) + delay),
                        new TweenAnimation<float>()
                        {
                            Curve = Curves.Exponential,
                            Duration = TimeSpan.FromSeconds(0.4f),
                            To = 780,
                            Target = buttons[i],
                            TargetProperty = "X",
                        }));
            }

            animations["Menu"].Play(layeredAnimation);

            return layeredAnimation;
        }

        /// <summary>
        /// Hides all buttons like what they did with Red Alert 2.
        /// </summary>
        private void TransiteOut(int triggerIndex)
        {
            LayeredAnimation layeredAnimation = new LayeredAnimation();

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Y = 220 + i * 24;

                layeredAnimation.Animations.Add(new SequentialAnimation(
                        new DelayAnimation(TimeSpan.FromSeconds(Math.Abs(i - triggerIndex) * 0.1f)),
                        new TweenAnimation<float>()
                        {
                            Curve = Curves.Exponential,
                            Duration = TimeSpan.FromSeconds(0.4f),
                            To = 900,
                            Target = buttons[i],
                            TargetProperty = "X",
                        }));
            }

            animations["Menu"].Play(new SequentialAnimation(layeredAnimation, TransiteIn(TimeSpan.FromSeconds(1.2f))));
        }

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
            animations.Update(gameTime);


            spriteBatch.Begin();
            spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, null, Color.White);            
            spriteBatch.DrawString(font, "Press any key to continue..." , new Vector2(0, GraphicsDevice.Viewport.Height - 20), TextColor);
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
