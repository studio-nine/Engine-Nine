namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;
    using Nine.Graphics.Materials;
    
    #region IModelSource
    /// <summary>
    /// Defines an interface for 3d models made up of triangle meshes
    /// </summary>
    public interface IModelSource
    {
        /// <summary>
        /// Gets the subset count.
        /// </summary>
        int MeshCount { get; }

        /// <summary>
        /// Gets the bounding box of this instance.
        /// </summary>
        BoundingBox BoundingBox { get; }

        /// <summary>
        /// Gets the vertex buffer for the given subset.
        /// </summary>
        void GetVertexBuffer(int mesh, out VertexBuffer vertexBuffer, out int vertexOffset, out int numVertices);

        /// <summary>
        /// Gets the index buffer for the given subset.
        /// </summary>
        void GetIndexBuffer(int mesh, out IndexBuffer indexBuffer, out int startIndex, out int primitiveCount);
    }
    #endregion

    #region ModelSource
    /// <summary>
    /// Represents a 3d model resource made up of triangle meshes.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    public class ModelSource : IModelSource, IDisposable
    {
        [Nine.Serialization.BinarySerializable]
        internal ModelMeshSource[] MeshMeshes;

        /// <summary>
        /// Gets the subset count.
        /// </summary>     
        public int MeshCount { get { return MeshMeshes != null ? MeshMeshes.Length : 0; } }

        /// <summary>
        /// Gets the bounding box of this instance.
        /// </summary>
        public BoundingBox BoundingBox { get; internal set; }

        /// <summary>
        /// Gets the default skeleton of this <see cref="ModelSource"/>.
        /// </summary>
        public Skeleton Skeleton { get; private set; }

        /// <summary>
        /// Gets a collection of default animations that is bundled to this <see cref="ModelSource"/>.
        /// </summary>
        public ReadOnlyCollection<BoneAnimation> Animations { get; private set; }

        /// <summary>
        /// Gets the vertex buffer for the given subset.
        /// </summary>
        public void GetVertexBuffer(int mesh, out VertexBuffer vertexBuffer, out int vertexOffset, out int numVertices)
        {
            var modelMesh = MeshMeshes[mesh];
            vertexBuffer = modelMesh.VertexBuffer;
            vertexOffset = modelMesh.VertexOffset;
            numVertices = modelMesh.NumVertices;
        }

        /// <summary>
        /// Gets the index buffer for the given subset.
        /// </summary>
        public void GetIndexBuffer(int mesh, out IndexBuffer indexBuffer, out int startIndex, out int primitiveCount)
        {
            var modelMesh = MeshMeshes[mesh];
            indexBuffer = modelMesh.IndexBuffer;
            startIndex = modelMesh.StartIndex;
            primitiveCount = modelMesh.PrimitiveCount;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (MeshMeshes != null)
                    foreach (var mesh in MeshMeshes)
                        mesh.Dispose();
            }
        }

        /// <summary>
        /// Gets the model source.
        /// </summary>
        public static ModelSource GetSource(Model model)
        {
            return null;
            //return model.Source as ModelSource;
        }

        /// <summary>
        /// Sets the model source.
        /// </summary>
        public static void SetSource(Model model, ModelSource source)
        {
            //model.Source = source;
            model.Skeleton = source != null ? source.Skeleton : null;
            model.Animations.Clear();
            if (source != null)
            {
                foreach (var animation in source.Animations)
                    model.Animations.Add(animation.Name, animation);
            }
        }
    }
    #endregion

    #region ModelMeshSource
    /// <summary>
    /// Represent each model mesh, used by <see cref="ModelSource"/>.
    /// This class is intentionally set as internal.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    sealed class ModelMeshSource : IDisposable, ISupportInitialize
    {
        public int VertexOffset;
        public int NumVertices;
        public int PrimitiveCount;
        public int StartIndex;

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

        void ISupportInitialize.BeginInit() { }
        void ISupportInitialize.EndInit()
        {
            
        }

        public void Dispose()
        {
            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }

            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }
        }
    }
    #endregion
}