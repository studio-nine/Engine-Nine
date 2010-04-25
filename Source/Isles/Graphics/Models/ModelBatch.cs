#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Isles.Graphics.Effects;
#endregion


namespace Isles.Graphics.Models
{
    public class ModelBatch
    {
        private SkinnedEffect basicSkinnedEffect;


        public ModelBatch(GraphicsDevice graphics)
        {
            basicSkinnedEffect = new SkinnedEffect(graphics);
        }

        public void Begin() { }
        public void End() { }


        public void Draw(Model model, Matrix world, Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = false;

                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                }

                mesh.Draw();
            }
        }


        public void Draw(Model model, ModelMesh mesh, Matrix world, Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.LightingEnabled = false;

                effect.View = view;
                effect.Projection = projection;
                effect.World = transforms[mesh.ParentBone.Index] * world;
            }

            mesh.Draw();
        }


        public void Draw(Model model, Matrix[] bones, Matrix view, Matrix projection)
        {
            GraphicsDevice graphics = model.Meshes[0].Effects[0].GraphicsDevice;

            foreach (ModelMesh mesh in model.Meshes)
            {
                graphics.Indices = mesh.IndexBuffer;

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    graphics.VertexDeclaration = part.VertexDeclaration;
                    graphics.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);

                    basicSkinnedEffect.Texture = (part.Effect as BasicEffect).Texture;
                    basicSkinnedEffect.Bones = bones;
                    basicSkinnedEffect.View = view;
                    basicSkinnedEffect.Projection = projection;

                    basicSkinnedEffect.Begin();
                    basicSkinnedEffect.CurrentTechnique.Passes[0].Begin();

                    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);

                    basicSkinnedEffect.CurrentTechnique.Passes[0].End();
                    basicSkinnedEffect.End();
                }
            }
        }


        public void Draw(Model model, ModelMesh mesh, Matrix[] bones, Matrix view, Matrix projection)
        {
            GraphicsDevice graphics = mesh.Effects[0].GraphicsDevice;

            graphics.Indices = mesh.IndexBuffer;


            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                graphics.VertexDeclaration = part.VertexDeclaration;
                graphics.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);

                basicSkinnedEffect.Texture = (part.Effect as BasicEffect).Texture;
                basicSkinnedEffect.Bones = bones;
                basicSkinnedEffect.View = view;
                basicSkinnedEffect.Projection = projection;

                basicSkinnedEffect.Begin();
                basicSkinnedEffect.CurrentTechnique.Passes[0].Begin();

                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.BaseVertex, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);

                basicSkinnedEffect.CurrentTechnique.Passes[0].End();
                basicSkinnedEffect.End();
            }
        }
    }
}
