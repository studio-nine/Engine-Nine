namespace Nine.Samples
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Serialization;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Microsoft.Xna.Framework.Content;

    public abstract class Sample
    {
        public virtual string Title { get { return GetType().Name; } }
        public abstract Scene CreateScene(GraphicsDevice graphics, ContentManager content);
    }
    
    public class SampleGame : Microsoft.Xna.Framework.Game
    {
        Nine.Input input;
        ContentManager content;

        int nextScene;
        Scene currentScene;
        List<Scene> scenes = new List<Scene>();
        List<Sample> samples = new List<Sample>();

        public SampleGame()
        {
            var graphics = new GraphicsDeviceManager(this);

            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            Window.AllowUserResizing = true;
            Window.Position = new Point(0, 0);
        }
        
        protected override void LoadContent()
        {
            content = new ContentManager(Services, "./Content");

            Components.Add(new FrameRate(GraphicsDevice, content.Load<SpriteFont>("Fonts/Consolas")));
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

            samples = new List<Sample> {
                //new PixelPerfectTest(),
                new SpriteTest(),
                new DynamicPrimitiveTest(),
                //new PrimitiveStressTest(),
            };
        }

        private void LoadNextScene()
        {
            if (nextScene <= scenes.Count)
            {
                scenes.Add(currentScene = samples[nextScene].CreateScene(GraphicsDevice, content));

                // TODO: rework on this design
                //currentScene.Add(new Camera2D(GraphicsDevice) { InputEnabled = true });
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
            input = new Nine.Input();
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
            input.KeyDown += (sender, e) =>
            {
                if (e.Key == Keys.F1)
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
            GraphicsDevice.Clear(new Color(95, 120, 157));

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
