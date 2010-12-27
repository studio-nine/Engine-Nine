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
using Nine.Navigation;
using Nine.Animations;
#endregion

namespace NavigationSample
{
    /// <summary>
    /// Demonstrates how to create a terrain based on a heightmap.
    /// </summary>
    public class NavigationGame : Microsoft.Xna.Framework.Game
    {
        TopDownEditorCamera camera;

        Input input;
        Model model;
        ModelBatch modelBatch;
        PrimitiveBatch primitiveBatch;
        DrawableSurface terrain;
        BasicEffect basicEffect;

        GridObjectManager<Navigator> objectManager;
        List<Navigator> movingEntities;

        Point beginSelect;
        BoundingFrustum pickFrustum;
        List<Navigator> selectedEntities = new List<Navigator>();

        public NavigationGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

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
            // Create a top down perspective editor camera to help us visualize the scene
            camera = new TopDownEditorCamera(GraphicsDevice);

            // Create a terrain based on the terrain geometry loaded from file
            terrain = new DrawableSurface(GraphicsDevice, Content.Load<Heightmap>("MountainHeightmap"), 8);

            primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            modelBatch = new ModelBatch(GraphicsDevice);
            model = Content.Load<Model>("Peon");

            // Initialize terrain effects
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = Content.Load<Texture2D>("checker");
            basicEffect.EnableDefaultLighting();
            basicEffect.SpecularColor = Vector3.Zero;

            // Initialize navigators
            BoundingRectangle bounds = new BoundingRectangle(terrain.BoundingBox);
            //objectManager = new QuadTreeObjectManager<Navigator>(bounds, 8);
            objectManager = new GridObjectManager<Navigator>(bounds, 16, 16);
            movingEntities = new List<Navigator>();

            for (int i = 0; i < 1; i++)
            {
                Navigator navigator = new Navigator();
                navigator.Ground = terrain;
                navigator.Tag = new AnimationPlayer();
                navigator.Started += (o, e) => { ((AnimationPlayer)navigator.Tag).Play(new BoneAnimation(model, model.GetAnimation("Idle"))); };
                navigator.Stopped += (o, e) => { ((AnimationPlayer)navigator.Tag).Play(new BoneAnimation(model, model.GetAnimation("Run"))); };

                ((AnimationPlayer)navigator.Tag).Play(new BoneAnimation(model, model.GetAnimation("Idle")));
                movingEntities.Add(navigator);
            }

            // Initialize inputs
            input = new Input();
            input.MouseDown += new EventHandler<MouseEventArgs>(input_MouseDown);
            input.MouseUp += new EventHandler<MouseEventArgs>(input_MouseUp);
        }

        void input_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                beginSelect = e.Position;
                selectedEntities.Clear();
            }
        }

        void input_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (beginSelect == e.Position)
                {
                    // Select single object from ray
                    Ray pickRay = GraphicsDevice.Viewport.CreatePickRay(e.X, e.Y, camera.View, camera.Projection);

                    Navigator selected = objectManager.FindFirst(pickRay);

                    if (selected != null)
                        selectedEntities.Add(selected);
                }
                else
                {
                    // Select multiple objects from frustum
                    pickFrustum = GraphicsDevice.Viewport.CreatePickFrustum(beginSelect, e.Position, camera.View, camera.Projection);

                    foreach (Navigator selected in objectManager.Find(pickFrustum))
                    {
                        selectedEntities.Add(selected);
                    }
                }
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            objectManager.Clear();

            foreach (Navigator navigator in movingEntities)
            {
                navigator.Update(gameTime);
                objectManager.Add(navigator, navigator.Position, navigator.BoundingRadius);

                ((AnimationPlayer)navigator.Tag).Update(gameTime);
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Initialize render state
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the terrain
            BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (DrawableSurfacePatch patch in terrain.Patches)
            {
                // Cull patches that are outside the view frustum
                if (frustum.Contains(patch.BoundingBox) != ContainmentType.Disjoint)
                {
                    // Setup matrices
                    basicEffect.World = patch.Transform;
                    basicEffect.View = camera.View;
                    basicEffect.Projection = camera.Projection;

                    // Draw each path with the specified effect
                    patch.Draw(basicEffect);
                }
            }

            Matrix world = Matrix.CreateScale(0.01f) * Matrix.CreateRotationX(MathHelper.PiOver2);

            // Draw models
            modelBatch.Begin(camera.View, camera.Projection);
            {
                foreach (Navigator navigator in movingEntities)
                {
                    BoneAnimation animation = (BoneAnimation)(((AnimationPlayer)navigator.Tag).Current);

                    modelBatch.DrawSkinned(model, world * navigator.Transform, animation.GetBoneTransforms(), null);
                }
            }
            modelBatch.End();

            primitiveBatch.Begin(camera.View, camera.Projection);
            {
                primitiveBatch.DrawBox(BoundingBox.CreateFromSphere(new BoundingSphere(movingEntities[0].Position, movingEntities[0].BoundingRadius)), null, Color.YellowGreen);
                if (pickFrustum != null)
                    primitiveBatch.DrawSolidFrustum(pickFrustum, null, Color.YellowGreen * 0.3f);
            }
            primitiveBatch.End();
            base.Draw(gameTime);
        }
    }
}
