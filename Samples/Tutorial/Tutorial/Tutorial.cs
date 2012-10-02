namespace Tutorial
{
    using System;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Physics;

    public class TutorialGame : Microsoft.Xna.Framework.Game
    {
        private Scene scene;
        private string[] tutorials;
        private int nextTutorial;
        private Input input;
        
        public TutorialGame()
        {
#if !SILVERLIGHT
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;

            Window.AllowUserResizing = true;
#endif
            Content = new ContentLoader(Services);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Add a frame rate counter component.
            // FrameRate is not an Xna game component, it is achieved using an extension method
            // defined in Nine.Components. So you have to include that namespace first.
            //Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));

            // An input component is required for framework to handle input events.
            // You can optionally pass in the handle to the current window, so the input event
            // is driven by windows messages on Windows.
            // This can avoid the event missing problem for Xna pulled input model when frame rate is low.
            Components.Add(new InputComponent(Window.Handle));

            Components.Add(new ScreenshotCapturer(GraphicsDevice));

            // Loads the names of tutorial files
            tutorials = Content.Load<string[]>("Tutorials/Tutorials");

            // Shows the next scene
            LoadNextScene();
            
            // Create an event based input handler.
            // Note that you have to explictly keep a strong reference to the Input instance.
            input = new Input();
#if XBOX
            input.ButtonDown += (sender, e) =>
            {
                if (e.Button == Buttons.A)
                    LoadNextScene();
            };
#elif WINDOWS_PHONE
            input.EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.DoubleTap;
            input.GestureSampled += (sender, e) =>
            {
                if (e.GestureType == Microsoft.Xna.Framework.Input.Touch.GestureType.DoubleTap)
                    LoadNextScene();
            };
#else
            input.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    LoadNextScene();
            };       
#endif
            base.LoadContent();
        }

        /// <summary>
        /// Loads the next scene.
        /// </summary>
        private void LoadNextScene()
        {
            scene = Content.Load<Scene>(tutorials[nextTutorial]);

            // Gets the drawing context to adjust drawing settings.
            var drawingContext = scene.GetDrawingContext(GraphicsDevice);
            drawingContext.TextureFilter = TextureFilter.Anisotropic;
            
            Window.Title = tutorials[nextTutorial];

            nextTutorial = (nextTutorial + 1) % tutorials.Length;
        }

        /// <summary>
        /// This is called when the game should update itself.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
#if XBOX
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                Exit();
#endif
            scene.UpdatePhysicsAsync(gameTime.ElapsedGameTime);
            scene.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            scene.Draw(GraphicsDevice, gameTime.ElapsedGameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                scene.DrawDiagnostics(GraphicsDevice, gameTime.ElapsedGameTime);

            base.Draw(gameTime);
        }

#if WINDOWS || XBOX
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TutorialGame game = new TutorialGame())
            {
                game.Run();
            }
        }
#endif
    }

#if SILVERLIGHT
    public class App : System.Windows.Application
    {
        public App()
        {
            Startup += (sender, e) =>
            {
                RootVisual = new TutorialGame();
            };
        }
    }
#endif
}
