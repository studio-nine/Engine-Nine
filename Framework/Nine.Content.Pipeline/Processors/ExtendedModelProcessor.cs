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
    [ContentProcessor(DisplayName = "Model Processor - Engine Nine")]
    public class ExtendedModelProcessor : ModelProcessor
    {
        const string DefaultTextures = "NormalMap|*_n.dds|BumpMap|*_b.dds|Diffuse|*_d.dds|Specular|*_s.dds";
        const string DefaultProcessors = "NormalMap|NormalTextureProcessor";

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
        /// Gets or sets whether animations tracks outside the skeleton will be discarded.
        /// </summary>
        [DefaultValue(true)]
        [DisplayName("Clamp Animation to Skeleton")]
        public bool ClampAnimationToSkeleton { get; set; }

        /// <summary>
        /// Gets or sets the textures attached to the model.
        /// </summary>
        [DefaultValue(DefaultTextures)]
        [DisplayName("Textures")]
        public string Textures { get; set; }

        /// <summary>
        /// Gets or sets the processors for each texture attached to the model.
        /// </summary>
        [DefaultValue(DefaultProcessors)]
        [DisplayName("Texture Processors")]
        public string TextureProcessors { get; set; }

        private Dictionary<string, string> attachedTextureNames = new Dictionary<string, string>();
        private Dictionary<string, string> attachedTextureProcessors = new Dictionary<string, string>();
        private List<Dictionary<string, ExternalReference<TextureContent>>> attachedTextures = new List<Dictionary<string, ExternalReference<TextureContent>>>();

        /// <summary>
        /// Creates a new instance of ExtendedModelProcessor.
        /// </summary>
        public ExtendedModelProcessor()
        {
            GenerateCollisionData = true;
            GenerateAnimationData = true;
            ClampAnimationToSkeleton = true;
            CollisionTreeDepth = 3;
            Textures = DefaultTextures;
            TextureProcessors = DefaultProcessors;
        }

        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            ValidateMesh(input, context, null);

            ModelTag tag = new ModelTag();

            if (GenerateAnimationData)
            {
                ModelAnimationProcessor animationProcessor = new ModelAnimationProcessor();
                ModelSkinningProcessor skinningProcessor = new ModelSkinningProcessor();

                tag.Skinning = skinningProcessor.Process(input, context);
                tag.Animations = animationProcessor.Process(input, context);

                ClampAnimation(tag);
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

            FormatAttachedTextures();

            ModelContent model = base.Process(input, context);

            ProcessAttachedTextures(model);

            if (GenerateAnimationData || GenerateCollisionData)
                model.Tag = tag;

            return model;
        }
        
        private void ClampAnimation(ModelTag tag)
        {
            if (ClampAnimationToSkeleton && tag.Skinning != null)
            {
                foreach (var animation in tag.Animations.Values)
                {
                    for (int i = 0; i < tag.Skinning.SkeletonIndex; i++)
                    {
                        if (i < animation.Transforms.Length)
                            animation.Transforms[i] = null;
                    }
                }
            }
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

        private void FormatAttachedTextures()
        {
            string[] formats = Textures.Split('|');

            for (int i = 0; i < formats.Length; i += 2)
            {
                if (i + 1 < formats.Length && !string.IsNullOrEmpty(formats[i]) && !string.IsNullOrEmpty(formats[i + 1]))
                    attachedTextureNames.Add(formats[i].Trim(), formats[i + 1].Trim());
            }

            formats = TextureProcessors.Split('|');

            for (int i = 0; i < formats.Length; i += 2)
            {
                if (i + 1 < formats.Length && !string.IsNullOrEmpty(formats[i]) && !string.IsNullOrEmpty(formats[i + 1]))
                    attachedTextureProcessors.Add(formats[i].Trim(), formats[i + 1].Trim());
            }
        }

        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            string filename = material.Textures["Texture"].Filename;

            Dictionary<string, ExternalReference<TextureContent>> textureDictionary = new Dictionary<string, ExternalReference<TextureContent>>();

            foreach (string name in attachedTextureNames.Keys)
            {
                string path = filename.Substring(0, filename.LastIndexOf(Path.GetFileName(filename)));
                string textureFilename = attachedTextureNames[name].Replace("*", Path.GetFileNameWithoutExtension(filename));
                textureFilename = Path.Combine(path, textureFilename);

                if (File.Exists(textureFilename))
                {
                    string processor = null;
                    attachedTextureProcessors.TryGetValue(name, out processor);

                    ExternalReference<TextureContent> texture = new ExternalReference<TextureContent>(textureFilename);
                    texture = context.BuildAsset<TextureContent, TextureContent>(texture, processor);
                    textureDictionary.Add(name, texture);
                }
            }

            attachedTextures.Add(textureDictionary.Count > 0 ? textureDictionary : null);
            return base.ConvertMaterial(material, context);
        }

        private void ProcessAttachedTextures(ModelContent model)
        {
            int i = 0;
            foreach (ModelMeshContent mesh in model.Meshes)
            {
                foreach (ModelMeshPartContent part in mesh.MeshParts)
                {
                    part.Tag = new ModelMeshPartTagContent() { Textures = attachedTextures[i++] };
                }
            }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.ModelMeshPartTag, Nine")]
    public class ModelMeshPartTagContent
    {
        /// <summary>
        /// Gets the additional textures (E.g. normalmap) attached to the ModelMeshPart.
        /// </summary>
        [ContentSerializer()]
        public Dictionary<string, ExternalReference<TextureContent>> Textures { get; internal set; }
    }
}