namespace Samples
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Serialization;

    public interface ISample
    {
        Scene CreateScene(GraphicsDevice graphics, ContentLoader content);
    }
    
    public class SampleGame : Microsoft.Xna.Framework.Game
    {
        Input input;
        ContentLoader loader;

        Scene currentScene;
        Scene[] scenes;
        ISample[] samples;
        int nextScene;

        public SampleGame()
        {
            var graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            Window.AllowUserResizing = true;
        }
        
        protected override void LoadContent()
        {
            loader = new ContentLoader(Services);
            loader.SearchDirectories.Add("../Content");
#if WINDOWS
            loader.Resolvers.Add(new FileSystemResolver());
            loader.Resolvers.Add(new PackageResolver());
#endif

            Package.Build(
                "../Content.nine",
                "../Content.n");

            
            Components.Add(new FrameRate(GraphicsDevice, loader.Load<SpriteFont>("Fonts/Consolas")));
            Components.Add(new InputComponent(Window.Handle));  

            // Find all test games
            samples = (
                from type in Assembly.GetExecutingAssembly().GetTypes().OrderBy(x => x.Name)
                where type.IsClass && typeof(ISample).IsAssignableFrom(type)
                select (ISample)Activator.CreateInstance(type)).ToArray();

            InitializeInput();
            LoadNextScene();

            base.LoadContent();
        }

        private void LoadNextScene()
        {
            if (scenes == null)
                scenes = new Scene[samples.Length];
            if (scenes[nextScene] == null)
                scenes[nextScene] = samples[nextScene].CreateScene(GraphicsDevice, loader);
            currentScene = scenes[nextScene];

            Window.Title = samples[nextScene].GetType().Name.Replace("_", " ");
            nextScene = (nextScene + 1) % samples.Length;
        }

        private void InitializeInput()
        {
            // Create an event based input handler.
            // Note that you have to explicitly keep a strong reference to the Input instance.
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
        }


        protected override void Update(GameTime gameTime)
        {
#if XBOX
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                Exit();
#endif
            currentScene.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            currentScene.Draw(GraphicsDevice, (float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                currentScene.DrawDiagnostics(GraphicsDevice, (float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Draw(gameTime);
        }

#if WINDOWS || XBOX
        static void Main(string[] args)
        {
            using (SampleGame game = new SampleGame())
            {
                game.Run();
            }
        }
#endif
    }
}
