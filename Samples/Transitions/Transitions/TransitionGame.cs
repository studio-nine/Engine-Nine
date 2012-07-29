#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;

#if !WINDOWS_PHONE

#endif
using Nine.Animations;
using System.ComponentModel;
using Nine.Components;
#endregion

namespace Transitions
{
    [Category("Animation")]
    [DisplayName("Tween Animations")]
    [Description("This sample demenstrates how to create various smoothed tweening animations.")]
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
        private const int BackBufferWidth = 128;
        private const int BackBufferHeight = 128;
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
        public float ImageScale;
        public float ImageRoation;
        public Color ImageColor;
        public Color TextColor;


        public TransitionGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
            
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

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
                    X = GraphicsDevice.Viewport.Width, Text = curve.GetType().Name, Tag = curve,
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
            animations["ImageColor"].Play(new AnimationSequence(
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

            animations["ImageScale"].Play(new AnimationSequence(
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
            AnimationGroup layeredAnimation = new AnimationGroup();

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Y = 220 + i * 24;

                layeredAnimation.Animations.Add(new AnimationSequence(
                        new DelayAnimation(TimeSpan.FromSeconds(i * 0.1f) + delay),
                        new TweenAnimation<float>()
                        {
                            Curve = Curves.Exponential,
                            Duration = TimeSpan.FromSeconds(0.4f),
                            By = -120,
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
            AnimationGroup layeredAnimation = new AnimationGroup();

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Y = 220 + i * 24;

                layeredAnimation.Animations.Add(new AnimationSequence(
                        new DelayAnimation(TimeSpan.FromSeconds(Math.Abs(i - triggerIndex) * 0.1f)),
                        new TweenAnimation<float>()
                        {
                            Curve = Curves.Exponential,
                            Duration = TimeSpan.FromSeconds(0.4f),
                            By = 120,
                            Target = buttons[i],
                            TargetProperty = "X",
                        }));
            }

            animations["Menu"].Play(new AnimationSequence(layeredAnimation, TransiteIn(TimeSpan.FromSeconds(1.2f))));
        }
        Random r = new Random();
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Test for slow unstable frame rates.
            //TargetElapsedTime = TimeSpan.FromSeconds(0.1395374587 + r.NextDouble() * 0.1);

            // Initialize render state
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            

            // Update transitions
            animations.Update(gameTime.ElapsedGameTime);


            spriteBatch.Begin(0, null, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, null, Color.White);            
            spriteBatch.DrawString(font, "Press any key to continue..." , new Vector2(0, GraphicsDevice.Viewport.Height - 20), TextColor);
            spriteBatch.Draw(image, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), null, ImageColor, ImageRoation, new Vector2(400, 300), ImageScale * 0.6f, SpriteEffects.None, 0);
            
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
