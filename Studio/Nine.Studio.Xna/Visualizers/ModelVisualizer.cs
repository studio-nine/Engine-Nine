#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel.Composition;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Nine.Components;
using Nine.Graphics;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Visualizers
{
    [Export(typeof(IDocumentVisualizer))]
    public class ModelVisualizer : GameVisualizer<ModelContent, Model>
    {
        ModelBatch modelBatch;
        ModelViewerCamera camera;
        BasicEffect basicEffect;
        SpriteBatch spriteBatch;
        BoundingSphere modelBounds;

        public bool ShowWireframe { get; set; }

        protected override void LoadContent()
        {
            Components.Add(new InputComponent());

            modelBatch = new ModelBatch(GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            camera = new ModelViewerCamera(GraphicsDevice);
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.EnableDefaultLighting();
        }

        protected override Model CreateDrawable(GraphicsDevice graphics, ModelContent content)
        {
            var model = base.CreateDrawable(graphics, content);
            modelBounds = MeasureModel(model);
            return model;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            Matrix world = Matrix.CreateTranslation(-modelBounds.Center) *
                           Matrix.CreateScale(8.0f / modelBounds.Radius);

            modelBatch.Begin(camera.View, camera.Projection);
            modelBatch.Draw(Drawable, world, null);
            modelBatch.End();
        }
        
        /// <summary>
        /// Whenever a new model is selected, we examine it to see how big
        /// it is and where it is centered. This lets us automatically zoom
        /// the display, so we can correctly handle models of any scale.
        /// </summary>
        BoundingSphere MeasureModel(Model model)
        {
            // Look up the absolute bone transforms for this model.
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Compute an (approximate) model center position by
            // averaging the center of each mesh bounding sphere.
            Vector3 modelCenter = Vector3.Zero;

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere meshBounds = mesh.BoundingSphere;
                Matrix transform = boneTransforms[mesh.ParentBone.Index];
                Vector3 meshCenter = Vector3.Transform(meshBounds.Center, transform);

                modelCenter += meshCenter;
            }

            modelCenter /= model.Meshes.Count;

            // Now we know the center point, we can compute the model radius
            // by examining the radius of each mesh bounding sphere.
            float modelRadius = 0;

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere meshBounds = mesh.BoundingSphere;
                Matrix transform = boneTransforms[mesh.ParentBone.Index];
                Vector3 meshCenter = Vector3.Transform(meshBounds.Center, transform);

                float transformScale = transform.Forward.Length();

                float meshRadius = (meshCenter - modelCenter).Length() +
                                   (meshBounds.Radius * transformScale);

                modelRadius = Math.Max(modelRadius, meshRadius);
            }

            return new BoundingSphere(modelCenter, modelRadius);
        }
    }
}
