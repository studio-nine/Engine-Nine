#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Content
{
    #region QuadTree<T>
    public class QuadTreeWriter<T> : ContentTypeWriter<QuadTree<T>>
    {
        protected override void Write(ContentWriter output, QuadTree<T> value)
        {
            output.Write(value.Count);     
            output.Write(value.Depth);
            output.Write(value.MaxDepth);       
            output.WriteObject<BoundingRectangle>(value.Bounds);
            output.WriteRawObject<QuadTreeNode<T>>(value.Root, new QuadTreeNodeWriter<T>());
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(QuadTree<T>).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(QuadTreeReader<T>).AssemblyQualifiedName;
        }
    }

    public class QuadTreeNodeWriter<T> : ContentTypeWriter<QuadTreeNode<T>>
    {
        protected override void Write(ContentWriter output, QuadTreeNode<T> value)
        {
            output.Write(value.HasChildren);
            output.Write(value.Depth);
            output.WriteObject<BoundingRectangle>(value.Bounds);
            output.WriteObject(value.Values);
            output.WriteObject(value.Tag);

            if (value.HasChildren)
                output.WriteObject(value.ChildNodes);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(QuadTreeNode<T>).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(QuadTreeReader<T>).AssemblyQualifiedName;
        }
    }
    #endregion

    #region Octree<T>
    public class OctreeWriter<T> : ContentTypeWriter<Octree<T>>
    {
        protected override void Write(ContentWriter output, Octree<T> value)
        {
            output.Write(value.Count);
            output.Write(value.Depth);
            output.Write(value.MaxDepth);
            output.WriteObject<BoundingBox>(value.Bounds);
            output.WriteRawObject<OctreeNode<T>>(value.Root, new OctreeNodeWriter<T>());
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Octree<T>).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(OctreeReader<T>).AssemblyQualifiedName;
        }
    }

    public class OctreeNodeWriter<T> : ContentTypeWriter<OctreeNode<T>>
    {
        protected override void Write(ContentWriter output, OctreeNode<T> value)
        {
            output.Write(value.HasChildren);
            output.Write(value.Depth);
            output.WriteObject<BoundingBox>(value.Bounds);
            output.WriteObject(value.Values);
            output.WriteObject(value.Tag);

            if (value.HasChildren)
                output.WriteObject(value.ChildNodes);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(OctreeNode<T>).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(OctreeReader<T>).AssemblyQualifiedName;
        }
    }
    #endregion
}
