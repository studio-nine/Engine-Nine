#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Animations
{
    using Nine.Graphics;

    /// <summary>
    /// Contains extension method for <c>PrimitiveBatch</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PrimitiveBatchExtensions
    {
        public static void DrawSkeleton(this PrimitiveBatch primitiveBatch, BoneAnimation animation, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                DrawSkeleton(primitiveBatch, animation, animation.Model.Root, Matrix.Identity, world, color);
            }
            primitiveBatch.EndPrimitive();
        }

        private static void DrawSkeleton(this PrimitiveBatch primitiveBatch, BoneAnimation animation, ModelBone node, Matrix parentTransform, Matrix? world, Color color)
        {
            Matrix start = parentTransform;
            Matrix end =  animation.GetBoneTransform(node.Index) * parentTransform;
            
            if (node.Parent != null)
            {
                if (Vector3.Subtract(end.Translation, start.Translation).LengthSquared() > 0)
                {
                    primitiveBatch.AddVertex(start.Translation, color);
                    primitiveBatch.AddVertex(end.Translation, color);
                }
            }

            foreach (ModelBone child in node.Children)
            {
                DrawSkeleton(primitiveBatch, animation, child, end, world, color);
            }
        }
    }
}
