#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName = "Model Processor - Nine")]
    public class ExtendedModelProcessor : ModelProcessor
    {
        /// <summary>
        /// Gets or sets a value indicating whether animation data will be generated.
        /// </summary>
        [DefaultValue(true)]
        [DisplayName("Generate Animation Data")]
        public bool GenerateAnimationData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether collision data will be generated.
        /// </summary>
        [DefaultValue(true)]
        [DisplayName("Generate Collision Data")]
        public bool GenerateCollisionData { get; set; }

        /// <summary>
        /// Gets or sets the depth of the collision tree.
        /// </summary>
        [DefaultValue(3)]
        [DisplayName("Collision Tree Depth")]
        public int CollisionTreeDepth { get; set; }

        /// <summary>
        /// Creates a new instance of ExtendedModelProcessor.
        /// </summary>
        public ExtendedModelProcessor()
        {
            GenerateCollisionData = true;
            GenerateAnimationData = true;
            CollisionTreeDepth = 3;
        }

        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ValidateMesh(input, context, null);

            ModelTag tag = new ModelTag();

            if (GenerateAnimationData)
            {
                ModelAnimationProcessor animationProcessor = new ModelAnimationProcessor();
                ModelSkinningProcessor skinningProcessor = new ModelSkinningProcessor();

                tag.Skinning = skinningProcessor.Process(input, context);
                tag.Animations = animationProcessor.Process(input, context);
            }

            if (GenerateCollisionData)
            {
                ModelCollisionProcessor collisionProcessor = new ModelCollisionProcessor();

                collisionProcessor.RotationX = RotationX;
                collisionProcessor.RotationY = RotationY;
                collisionProcessor.RotationZ = RotationZ;
                collisionProcessor.Scale = Scale;
                collisionProcessor.CollisionTreeDepth = CollisionTreeDepth;

                tag.Collision = collisionProcessor.Process(input, context);
            }

            // Only skinned models need to be baked ???
            if (tag.Skinning != null)
                FlattenTransforms(input);

            ModelContent model = base.Process(input, context);

            if (GenerateAnimationData || GenerateCollisionData)
                model.Tag = tag;

            return model;
        }

        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context, string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. ExtendedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the bones, because that is special.
                if (child is BoneContent)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child);
            }
        }
    }
}
