namespace Nine.Serialization.Processors
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.Graphics;

    /// <summary>
    /// A custom processor to extract model collision data.
    /// </summary>
    public class ModelCollisionProcessor : ContentProcessor<NodeContent, ModelCollision>
    {
        [DefaultValue(3)]
        public int CollisionTreeDepth { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationX { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationY { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationZ { get; set; }

        [DefaultValue(1f)]
        public virtual float Scale { get; set; }

        public ModelCollisionProcessor()
        {
            CollisionTreeDepth = 3;
        }

        public override ModelCollision Process(NodeContent input, ContentProcessorContext context)
        {
            List<Vector3> positions = new List<Vector3>();
            List<ushort> indices = new List<ushort>();

            Matrix transform = Matrix.CreateRotationX(MathHelper.ToRadians(RotationX)) *
                               Matrix.CreateRotationY(MathHelper.ToRadians(RotationY)) *
                               Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ)) *
                               Matrix.CreateScale(Scale);

            ProcessNode(transform, input, positions, indices);

            ModelCollision collision = new ModelCollision();

            collision.CollisionTree = new Octree<bool>(BoundingBox.CreateFromPoints(positions), CollisionTreeDepth);

            for (int i = 0; i < indices.Count; i += 3)
            {
                collision.CollisionTree.ExpandAll((o) =>
                {
                    bool contains = o.bounds.Contains(new Triangle(
                                positions[indices[i]],
                                positions[indices[i + 1]],
                                positions[indices[i + 2]])) != ContainmentType.Disjoint;
                    
                    if (contains)
                        o.value = true;

                    return contains;
                });
            }

            return collision;
        }

        private static void ProcessNode(Matrix transform, NodeContent input, List<Vector3> positions, List<ushort> indices)
        {
            if (input != null)
            {
                // Find mesh content
                MeshContent mesh = input as MeshContent;

                if (mesh != null)
                {
                    foreach (GeometryContent geometry in mesh.Geometry)
                    {
                        int currentVertex = positions.Count;

                        foreach (Vector3 position in geometry.Vertices.Positions)
                            positions.Add(Vector3.Transform(position, mesh.AbsoluteTransform * transform));

                        foreach (int index in indices)
                            indices.Add((ushort)(currentVertex + index));
                    }
                }

                foreach (NodeContent child in input.Children)
                    ProcessNode(transform, child, positions, indices);
            }
        }
    }
}
