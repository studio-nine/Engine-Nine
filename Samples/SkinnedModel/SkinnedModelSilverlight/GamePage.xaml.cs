using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using Nine.Animations;

namespace SkinnedModelSilverlight
{
    public partial class GamePage : PhoneApplicationPage
    {
        ContentManager contentManager;
        GameTimer timer;
        SpriteBatch spriteBatch;

        Model model;
        ModelBatch modelBatch;
        ModelSkeleton skeleton;
        BoneAnimation animation;
        ModelViewerCamera camera;
        InputComponent inputComponent;
        Matrix world = Matrix.CreateTranslation(0, -60, 0) * Matrix.CreateScale(0.1f);

        public GamePage()
        {
            InitializeComponent();

            // Get the content manager from the application
            contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            GraphicsDevice graphicsDevice = SharedGraphicsDeviceManager.Current.GraphicsDevice;

            // Manually create and update the input component.
            inputComponent = new InputComponent(new SilverlightInputSource(this));

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);

            // Create a model viewer camera to help us visualize the scene
            camera = new ModelViewerCamera(graphicsDevice);

            // Model batch makes drawing models easier
            modelBatch = new ModelBatch(graphicsDevice);

            // Load our model assert.
            // If the model is processed by our ExtendedModelProcesser,
            // we will try to add model animation and skinning data.
            model = contentManager.Load<Model>("peon");
            
            skeleton = new ModelSkeleton(model);

            animation = new BoneAnimation(skeleton, model.GetAnimation(0));
            animation.Play();

            // Convert the effect used by the model to SkinnedEffect to
            // support skeleton animation.
            SkinnedEffect skinned = new SkinnedEffect(graphicsDevice);
            skinned.EnableDefaultLighting();

            // ModelBatch will use this skinned effect to draw the model.
            model.ConvertEffectTo(skinned);

            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            inputComponent.Update(e.ElapsedTime);
            animation.Update(e.ElapsedTime);
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.CornflowerBlue);

            modelBatch.Begin(ModelSortMode.Immediate, camera.View, camera.Projection, null, null, null, RasterizerState.CullNone);
            {
                modelBatch.DrawSkinned(model, world, skeleton.GetSkinTransforms(), null);
            }
            modelBatch.End();
        }
    }
}