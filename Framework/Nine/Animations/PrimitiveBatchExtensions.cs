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
        public static void DrawSkeleton(this PrimitiveBatch primitiveBatch, IBoneHierarchy skeleton, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                float d = DrawSkeleton(primitiveBatch, skeleton, 0, Matrix.Identity, color);
            }
            primitiveBatch.EndPrimitive();
        }

        private static float DrawSkeleton(this PrimitiveBatch primitiveBatch, IBoneHierarchy skeleton, int bone, Matrix parentTransform, Color color)
        {
            float distance = 0;
            Matrix start = parentTransform;
            Matrix end = skeleton.GetBoneTransform(bone) * parentTransform;

            if (Vector3.Subtract(end.Translation, start.Translation).LengthSquared() > 0)
            {
                primitiveBatch.AddVertex(start.Translation, color);
                primitiveBatch.AddVertex(end.Translation, color);

                distance = Vector3.Subtract(end.Translation, start.Translation).Length();
            }

            foreach (int child in skeleton.GetChildBones(bone))
            {
                distance += DrawSkeleton(primitiveBatch, skeleton, child, end, color);
            }
            return distance;
        }
    }
}
