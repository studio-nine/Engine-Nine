#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
using Nine.Animations;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Views
{
    public class ModelView : IUpdateable, IDrawableView
    {
        [ContentSerializer(Optional = true)]
        public bool Visible { get; set; }

        [ContentSerializer(Optional = true)]
        public Matrix Transform { get; set; }

        [ContentSerializer(Optional = true)]
        public Model Model { get; set; }

        [ContentSerializer(Optional = true)]
        public Effect Effect { get; set; }

        public BoneAnimation Animation { get; private set; }

        public ModelView()
        {
            Visible = true;
            Transform = Matrix.Identity;
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (Animation != null)
                Animation.Update(elapsedTime);
        }

        public void Draw(GraphicsContext context)
        {
            Draw(context, Effect);
        }

        public void Draw(GraphicsContext context, Effect effect)
        {
            if (Model == null || !Visible)
                return;

            if (Animation != null)
            {
                if (Model.IsSkinned())
                {
                    context.ModelBatch.DrawSkinned(Model, Transform, Animation.GetBoneTransforms(), Effect);
                }
                else
                {
                    if (boneTransforms == null || boneTransforms.Length < Model.Bones.Count)
                    {
                        boneTransforms = new Matrix[Model.Bones.Count];
                    }

                    Animation.CopyAbsoluteBoneTransformsTo(boneTransforms);

                    foreach (var mesh in Model.Meshes)
                    {
                        context.ModelBatch.Draw(Model, mesh, boneTransforms[mesh.ParentBone.Index] * Transform, Effect);
                    }
                }
            }
            else
            {
                context.ModelBatch.Draw(Model, Transform, Effect);
            }
        }
        static Matrix[] boneTransforms;    
    }
}