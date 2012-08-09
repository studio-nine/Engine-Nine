namespace Nine.Physics.Colliders
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.DataStructures;
    using BEPUphysics.Collidables;

    /// <summary>
    /// Represents a collider based on triangle meshes.
    /// </summary>
    public class ModelCollider : Collider
    {
        /// <summary>
        /// Gets or sets the source model.
        /// </summary>
        public Microsoft.Xna.Framework.Graphics.Model Source
        {
            get { return source; }
            set { if (source != value) { source = value; NotifyColliderChanged(); } }
        }
        private Microsoft.Xna.Framework.Graphics.Model source;

        /// <summary>
        /// Gets or sets the name of the collision mesh.
        /// </summary>
        public string CollisionMesh
        {
            get { return collisionMesh; }
            set { if (collisionMesh != value) { collisionMesh = value; NotifyColliderChanged(); } }
        }
        private string collisionMesh;

        protected override Entity CreateCollidable()
        {
            Vector3[] vertices;
            int[] indices;
            GetVerticesAndIndicesFromModel(source, collisionMesh, out vertices, out indices);
            //return new StaticMesh(vertices, indices);
            return null;
        }

        private static void GetVerticesAndIndicesFromModel(Microsoft.Xna.Framework.Graphics.Model collisionModel, string collisionMesh, out Vector3[] vertices, out int[] indices)
        {
            if (string.IsNullOrEmpty(collisionMesh) || !collisionModel.Meshes.Any(mesh => mesh.Name == collisionMesh))
            {
                TriangleMesh.GetVerticesAndIndicesFromModel(collisionModel, out vertices, out indices);
                return;
            }

            var verticesList = new List<Vector3>();
            var indicesList = new List<int>();
            var transforms = new Matrix[collisionModel.Bones.Count];
            collisionModel.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix transform;
            foreach (Microsoft.Xna.Framework.Graphics.ModelMesh mesh in collisionModel.Meshes)
            {
                if (mesh.Name == collisionMesh)
                {
                    if (mesh.ParentBone != null)
                        transform = transforms[mesh.ParentBone.Index];
                    else
                        transform = Matrix.Identity;
                    TriangleMesh.AddMesh(mesh, transform, verticesList, indicesList);
                }
            }

            vertices = verticesList.ToArray();
            indices = indicesList.ToArray();
        }
    }
}
