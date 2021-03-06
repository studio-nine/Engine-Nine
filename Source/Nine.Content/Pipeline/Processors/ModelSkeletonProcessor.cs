namespace Nine.Content.Pipeline.Processors
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.Graphics;

    /// <summary>
    /// Custom processor that extract skinning info from a skinned model.
    /// </summary>
    /// <remarks>
    /// This processor is used by ExtendedModelProcessor,
    /// There is no need to expose it to the xna build.
    ///</remarks>
    [DesignTimeVisible(false)]
    [ContentProcessor(DisplayName="Model Skeleton - Engine Nine")]
    class ModelSkeletonProcessor : ContentProcessor<NodeContent, ModelSkeletonData>
    {
        [DefaultValue(0f)]
        public virtual float RotationX { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationY { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationZ { get; set; }

        [DefaultValue(1f)]
        public virtual float Scale { get; set; }

        public ModelSkeletonProcessor()
        {
            Scale = 1;
        }

        public override ModelSkeletonData Process(NodeContent input, ContentProcessorContext context)
        {
            // Check if the input model is a skinned model.
            if (!IsSkinned(input))
                return null;

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                return null;


            // Find the index of the first bone content.
            int skeletonIndex = 0;

            if (!FindSkeletonIndex(input, skeleton, ref skeletonIndex))
                return null;
            

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            List<Matrix> inverseBindPose = new List<Matrix>();

            foreach (BoneContent bone in bones)
            {
                inverseBindPose.Add(Matrix.Invert(Transform(bone.AbsoluteTransform)));
            }
            
            // Store our custom animation data in the Tag property of the model.
            return new ModelSkeletonData(inverseBindPose, skeletonIndex);
        }

        /// <summary>
        /// Finds the index of the first node content that is a bone content.
        /// </summary>
        private bool FindSkeletonIndex(NodeContent input, BoneContent skeleton, ref int n)
        {
            if (input == skeleton)
                return true;

            foreach (NodeContent child in input.Children)
            {
                n++;
                if (FindSkeletonIndex(child, skeleton, ref n))
                {
                    return true;
                }
            }

            return false;
        }
                
        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether a mesh is a skinned mesh
        /// </summary>
        static bool IsSkinned(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null && MeshHasSkinning(mesh))
                return true;

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                if (IsSkinned(child))
                    return true;

            return false;
        }

        private Matrix Transform(Matrix matrix)
        {
            return matrix * Matrix.CreateScale(Scale)
                          * Matrix.CreateRotationX(MathHelper.ToRadians(RotationX))
                          * Matrix.CreateRotationY(MathHelper.ToRadians(RotationY))
                          * Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ));
        }
    }
}
