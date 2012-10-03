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
    using BEPUphysics.CollisionShapes;
    using BEPUphysics.MathExtensions;

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
            set { if (source != value) { source = value; RebuildCollider(); } }
        }
        private Microsoft.Xna.Framework.Graphics.Model source;

        /// <summary>
        /// Gets or sets the name of the collision mesh.
        /// </summary>
        public string CollisionMesh
        {
            get { return collisionMesh; }
            set { if (collisionMesh != value) { collisionMesh = value; RebuildCollider(); } }
        }
        private string collisionMesh;
        
        private bool isStatic = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelCollider"/> class.
        /// </summary>
        public ModelCollider() { }

        protected override void OnAttached(RigidBody body)
        {
            isStatic = false;
            RebuildCollider();
        }
        
        private void RebuildCollider()
        {
            if (Nine.Content.ContentProperties.IsContentBuild)
            {
                // Fake an entity when we are compiling the model collider.
                NotifyColliderChanged(new Sphere(Vector3.Zero, 1));
                return;
            }

            if (source == null)
            {
                NotifyColliderChanged(null);
                return;
            }

            Vector3[] vertices;
            int[] indices;
            GetVerticesAndIndicesFromModel(source, collisionMesh, out vertices, out indices);

            if (isStatic)
            {
                NotifyColliderChanged(new StaticMesh(vertices, indices));
                return;
            }

            // Entities in Bepu is centered, so need to adjust the graphical transform accordingly.
            var mesh = new MobileMesh(vertices, indices, AffineTransform.Identity, MobileMeshSolidity.Counterclockwise);            
            Offset = -mesh.Position;
            mesh.Position = Vector3.Zero;
            
            NotifyColliderChanged(mesh);
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
