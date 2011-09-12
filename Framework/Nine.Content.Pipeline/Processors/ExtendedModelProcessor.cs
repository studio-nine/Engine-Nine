#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
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
    using Nine.Content.Pipeline.Graphics;

    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName = "Model - Engine Nine")]
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
        /// Gets or sets a value indicating whether vertex color channel will be generated.
        /// </summary>
        [DefaultValue(false)]
        [DisplayName("Generate Vertex Color")]
        public bool GenerateVertexColor { get; set; }

        /// <summary>
        /// Gets or sets the depth of the collision tree.
        /// </summary>
        [DefaultValue(3)]
        [DisplayName("Collision Tree Depth")]
        public int CollisionTreeDepth { get; set; }

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
        private List<BoundingBox> partBound = new List<BoundingBox>();

        /// <summary>
        /// Creates a new instance of ExtendedModelProcessor.
        /// </summary>
        public ExtendedModelProcessor()
        {
            GenerateCollisionData = true;
            GenerateAnimationData = true;
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
            ValidateMesh(input, context, null);

            ModelTag tag = new ModelTag();

            if (GenerateAnimationData)
            {
                ModelAnimationProcessor animationProcessor = new ModelAnimationProcessor();

                animationProcessor.RotationX = RotationX;
                animationProcessor.RotationY = RotationY;
                animationProcessor.RotationZ = RotationZ;
                animationProcessor.Scale = Scale;

                tag.Animations = animationProcessor.Process(input, context);
                tag.Skeleton = animationProcessor.Skeleton;
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
            if (tag.Skeleton != null)
                FlattenTransforms(input);
            
            // Format textures before processing model.
            FormatAttachedTextures();

            // Find model mesh part bounds.
            FindModelMeshPartBounds(input);

            GenerateVertexColorChannel(input);

            ModelContent model = base.Process(input, context);

            ProcessModelMeshParts(model);

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
            ExternalReference<TextureContent> textureContent;
            Dictionary<TextureUsage, ContentReference<TextureContent>> textureDictionary = new Dictionary<TextureUsage, ContentReference<TextureContent>>();

            if (material.Textures.TryGetValue("Texture", out textureContent))
            {
                string filename = textureContent.Filename;
                foreach (string name in attachedTextureNames.Keys)
                {
                    TextureUsage usage = TextureUsage.None;
                    if (!Enum.TryParse(name, out usage))
                    {
                        throw new InvalidContentException("Unknown texture usage: " + name);
                    }

                    string path = filename.Substring(0, filename.LastIndexOf(Path.GetFileName(filename)));
                    string textureFilename = attachedTextureNames[name].Replace("*", Path.GetFileNameWithoutExtension(filename));
                    textureFilename = Path.Combine(path, textureFilename);

                    if (File.Exists(textureFilename))
                    {
                        string processor = null;
                        attachedTextureProcessors.TryGetValue(name, out processor);

                        ExternalReference<TextureContent> texture = new ExternalReference<TextureContent>(textureFilename);
                        texture = context.BuildAsset<TextureContent, TextureContent>(texture, processor);
                        textureDictionary.Add(usage, new ContentReference<TextureContent>(texture.Filename));
                    }
                }
            }

            material.OpaqueData.Add("AttachedTextures", textureDictionary);
            return base.ConvertMaterial(material, context);
        }

        private void FindModelMeshPartBounds(NodeContent input)
        {

        }

        private void ProcessModelMeshParts(ModelContent model)
        {
            int i = 0;
            foreach (ModelMeshContent mesh in model.Meshes)
            {
                foreach (ModelMeshPartContent part in mesh.MeshParts)
                {
                    part.Tag = new ModelMeshPartTagContent() 
                    {
                        Textures = part.Material.OpaqueData["AttachedTextures"] as Dictionary<TextureUsage, ContentReference<TextureContent>>,
                        BoundingBox = new BoundingBox(),
                    };
                }
            }
        }

        private void GenerateVertexColorChannel(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;
            
            if (mesh != null)
            {
                foreach (var geometry in mesh.Geometry)
                {
                    if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Color(0)))
                    {
                        geometry.Vertices.Channels.Add<Color>(VertexChannelNames.Color(0), 
                            Enumerable.Range(0, geometry.Vertices.VertexCount).Select(i => Color.White));
                    }
                }
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in node.Children)
                GenerateVertexColorChannel(child);
        }
    }
}
