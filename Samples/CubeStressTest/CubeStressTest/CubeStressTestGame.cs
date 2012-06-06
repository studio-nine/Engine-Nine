#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Components;
using Nine.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;
#endregion

namespace CubeStressTest
{
    [Category("Graphics")]
    [DisplayName("Cube Stress Test")]
    [Description("This sample demenstrates drawing a large amount of cubes.")]
    public class CubeStressTestGame : Microsoft.Xna.Framework.Game
    {
        Scene scene;
        Scene scene2;
        Scene scene3;
        Microsoft.Xna.Framework.Graphics.Model cube;
        FrameRate frameRate;
        BasicEffect basicEffect;
        BasicMaterial basicMaterial;
        Texture2D texture;
        PrimitiveGroup primitiveGroup;

        public CubeStressTestGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
            graphics.EnablePerfHudProfiling();

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
        }

        int size = 20;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(frameRate = new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")) { UpdateFrequency = TimeSpan.FromSeconds(0.2) });
            Components.Add(new InputComponent(Window.Handle));

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            
            cube = Content.Load<Microsoft.Xna.Framework.Graphics.Model>("cube");
            texture = Content.Load<Texture2D>("fire");

            scene = new Scene(GraphicsDevice);
            //scene = new Scene(GraphicsDevice, null, new PassThroughSceneManager());
            //scene = new Scene(GraphicsDevice, null, new BruteForceSceneManager());
            scene.Settings.BackgroundColor = Color.Gray;
            scene.Camera = new FreeCamera(GraphicsDevice) { Position = new Vector3(20, -50, 20) };
            //scene.Add(new AmbientLight(GraphicsDevice));
            scene.Add(new DirectionalLight(GraphicsDevice) { DiffuseColor = Color.Red.ToVector3(), Direction = Vector3.UnitX });
            scene.Add(new DirectionalLight(GraphicsDevice) { DiffuseColor = Color.Green.ToVector3(), Direction = Vector3.UnitY });
            scene.Add(new DirectionalLight(GraphicsDevice) { DiffuseColor = Color.Blue.ToVector3(), Direction = Vector3.UnitZ });

            scene2 = new Scene(GraphicsDevice);
            scene2.Settings.BackgroundColor = Color.Gray;
            scene2.Camera = scene.Camera;
            scene2.Add(new DirectionalLight(GraphicsDevice) { DiffuseColor = Color.Red.ToVector3(), Direction = Vector3.UnitX });
            scene2.Add(new DirectionalLight(GraphicsDevice) { DiffuseColor = Color.Green.ToVector3(), Direction = Vector3.UnitY });
            scene2.Add(new DirectionalLight(GraphicsDevice) { DiffuseColor = Color.Blue.ToVector3(), Direction = Vector3.UnitZ });

            scene3 = new Scene(GraphicsDevice);
            scene3.Settings.BackgroundColor = Color.Gray;
            scene3.Camera = scene.Camera;
            scene3.Add(primitiveGroup = new PrimitiveGroup(GraphicsDevice));

            primitiveGroup.Material = basicMaterial = new BasicMaterial(GraphicsDevice);

            int step = 2;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    for (int z = 0; z < size; z++)
                    {
                        scene.Add(new Nine.Graphics.ObjectModel.Model(cube)
                        {
                            Material = new BasicMaterial(GraphicsDevice) { Texture = texture, LightingEnabled = true },
                            Transform = Matrix.CreateTranslation(x * step, y * step, z * step)
                        });

                        scene2.Add(new Nine.Graphics.Primitives.Box(GraphicsDevice) 
                        {
                            Material = new BasicMaterial(GraphicsDevice) { Texture = texture, LightingEnabled = true },
                            Transform = Matrix.CreateTranslation(x * step, y * step, z * step),
                        });
                        
                        primitiveGroup.AddSolidBox(new Vector3(x * step, y * step, z * step), Vector3.One, null, Color.White);
                    }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                GraphicsDevice.Clear(Color.Gray);
                
            
                int step = 2;

                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        for (int z = 0; z < size; z++)
                        {
                            GraphicsDevice.BlendState = BlendState.Opaque;

                            basicEffect.View = scene.Camera.View;
                            basicEffect.Projection = scene.Camera.Projection;
                            basicEffect.World = Matrix.CreateTranslation(x * step, y * step, z * step);

                            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                            {
                                basicEffect.SpecularPower = 10;
                                basicEffect.SpecularColor = Vector3.One * 0.2f;
                                basicEffect.DiffuseColor = Vector3.One * 0.2f;
                                basicEffect.EmissiveColor = Vector3.One * 0.2f;
                                basicEffect.Texture = texture;
                                basicEffect.TextureEnabled = true;
                                basicEffect.EnableDefaultLighting();
                            }

                            basicEffect.CurrentTechnique.Passes[0].Apply();
                            
                            foreach (var mesh in cube.Meshes)
                                foreach (var part in mesh.MeshParts)
                                {
                                    GraphicsDevice.Indices = part.IndexBuffer;
                                    GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                                }
                        }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D2))
            {
                scene2.Draw(gameTime.ElapsedGameTime);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D3))
            {
                GraphicsDevice.Clear(Color.Gray);

                int step = 2;

                primitiveGroup.Clear();
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        for (int z = 0; z < size; z++)
                        {
                            primitiveGroup.AddSolidBox(new Vector3(x * step, y * step, z * step), Vector3.One, null, Color.White);
                        }

                scene3.Draw(gameTime.ElapsedGameTime);
            }
            else
            {
                scene.Draw(gameTime.ElapsedGameTime);
                if (scene.Context.CurrentFrame >= 200)
                    ;// Exit();
            }

            base.Draw(gameTime);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
