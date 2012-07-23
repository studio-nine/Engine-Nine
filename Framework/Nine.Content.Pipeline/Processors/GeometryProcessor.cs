namespace Nine.Content.Pipeline.Processors
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.Graphics;

    /// <summary>
    /// A custom processor to convert a mesh into a geometry.
    /// </summary>
    [ContentProcessor(DisplayName = "Geometry - Engine Nine")]
    public class GeometryProcessor : ContentProcessor<NodeContent, Geometry>
    {
        [DefaultValue(false)]
        public bool GenerateNormals { get; set; }

        [DefaultValue(false)]
        public bool GenerateTextureCoordinates { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationX { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationY { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationZ { get; set; }

        [DefaultValue(1f)]
        public virtual float Scale { get; set; }


        public GeometryProcessor()
        {
            Scale = 1.0f;
        }

        public override Geometry Process(NodeContent input, ContentProcessorContext context)
        {
            List<Vector3> positions = new List<Vector3>();
            List<ushort> indices = new List<ushort>();

            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();

            Matrix transform = Matrix.CreateScale(Scale) * 
                               Matrix.CreateRotationX(MathHelper.ToRadians(RotationX)) *
                               Matrix.CreateRotationY(MathHelper.ToRadians(RotationY)) *
                               Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ));

            ProcessNode(transform, input, positions, indices);

            return new Geometry(positions, indices);
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

                        foreach (int index in geometry.Indices)
                            indices.Add((ushort)(currentVertex + index));
                    }
                }

                foreach (NodeContent child in input.Children)
                    ProcessNode(transform, child, positions, indices);
            }
        }
    }
}
