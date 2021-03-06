namespace Nine.Samples
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Serialization;
    using Nine.Physics;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public abstract class Sample
    {
        public virtual string Title { get { return GetType().Name; } }
        public abstract Scene CreateScene(GraphicsDevice graphics, ContentLoader content);
    }
    
    public class SampleGame : Microsoft.Xna.Framework.Game
    {
        Input input;
        ContentLoader loader;

        int nextScene;
        Scene currentScene;
        List<Scene> scenes = new List<Scene>();
        List<Sample> samples = new List<Sample>();

        public SampleGame()
        {
            //Package.BuildDirectory(@"D:\Git\nine\Content\", @"D:\Git\nine\Content.n", (file, err) => { System.Diagnostics.Trace.TraceError(file + "\n" + err.ToString()); });
            //Package.BuildFile(@"D:\Git\nine\Content\Scenes\01. Hello World.xaml", @"D:\01.xnb");

            var graphics = new GraphicsDeviceManager(this);

            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            Window.AllowUserResizing = true;
            System.Diagnostics.Trace.WriteLine(typeof(Fog).AssemblyQualifiedName.ToString());
            System.Diagnostics.Trace.WriteLine(typeof(SkyBox).AssemblyQualifiedName.ToString());
        }
        
        protected override void LoadContent()
        {
            loader = new ContentLoader(Services);
            loader.SearchDirectories.Add("../Content");
                        
#if WINDOWS
            loader.Resolvers.Add(new FileSystemResolver());
#endif

            Components.Add(new FrameRate(GraphicsDevice, loader.Load<SpriteFont>("Fonts/Consolas.spritefont")));
            Components.Add(new InputComponent(Window.Handle));  

            InitializeSamples();
            InitializeInput();
            LoadNextScene();

            base.LoadContent();
        }

        private void InitializeSamples()
        {
            samples.AddRange(
                from type in Assembly.GetExecutingAssembly().GetTypes().OrderBy(x => x.Name)
                where type.IsSubclassOf(typeof(Sample)) && type != typeof(Tutorial)
                select (Sample)Activator.CreateInstance(type));

            samples = new List<Sample> { new UITest() };
            //samples = new List<Sample> { new Tutorial("Scenes/03. Materials.xaml") };
        }

        private void LoadNextScene()
        {
            if (nextScene <= scenes.Count)
            {
                scenes.Add(currentScene = samples[nextScene].CreateScene(GraphicsDevice, loader));

                // TODO: rework on this design
                currentScene.Add(new FreeCamera(GraphicsDevice, new Vector3(0, 10, 40)) { InputEnabled = true });
                Nine.Graphics.SceneExtensions.SetDrawingContext(currentScene, new DrawingContext3D(GraphicsDevice, currentScene));
            }

            Window.Title = samples[nextScene].Title;
            nextScene = (nextScene + 1) % samples.Count;
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
            //currentScene.UpdatePhysicsAsync((float)gameTime.ElapsedGameTime.TotalSeconds);
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
