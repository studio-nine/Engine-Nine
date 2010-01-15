/*
 * Copyright 2006 Sony Computer Entertainment Inc.
 * 
 * Licensed under the SCEA Shared Source License, Version 1.0 (the "License"); you may not use this 
 * file except in compliance with the License. You may obtain a copy of the License at:
 * http://research.scea.com/scea_shared_source_license.html
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License 
 * is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
 * implied. See the License for the specific language governing permissions and limitations under the 
 * License.
 */

#region Using Statements
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content.Pipeline; // ContentImporter
using Microsoft.Xna.Framework.Content.Pipeline.Processors; // ModelProcessor
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler; // ContentTypeWriter
using Microsoft.Xna.Framework.Content.Pipeline.Graphics; // MeshBuilder



using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;  // serialization


#endregion

namespace Isles.Pipeline.Importers
{
    internal class Processor
    {
        /// <summary>
        /// Helper function, returns the p[] value for the given index
        /// </summary>
        /// <param name="input">The "<input>" element we need the index of.</param>
        /// <param name="primitive">The "<primitive>" element the "<input>" is from.</param>
        /// <param name="index"> The index for which we need the p[] value.</param>
        static public int GetPValue(Document.Input input, Document.Primitive primitive, int index)
        {
            int stride = primitive.stride;
            int offset = input.offset;
            return primitive.p[index * stride + offset];
        }
        /// <summary>
        /// Helper function, returns the value of a (float) source for the given index
        /// </summary>
        /// <param name="input">The "<input>" element we need the index of.</param>
        /// <param name="channel">The name of the "<param>" we are looking for.</param>
        /// <param name="index"> The index for which we need the value.</param>
        static public float GetSourceValue(Document.Input input, int channel, int index)
        {
            Document.Source src = (Document.Source)input.source;
            Document.Array<float> farray = (Document.Array<float>)(src.array);
            int idx = src.accessor.parameters[channel].index;
            int stride = src.accessor.stride;
            int offset = src.accessor.offset;
            return farray[offset + index * stride + idx];
        }
        /// <summary>
        /// Helper function, returns the "<input>" that has the POSITION semantic
        /// </summary>
        /// <param name="mesh">The "<mesh>" element we need the POSITION from.</param>
        static public Document.Input GetPositionInput(Document.Mesh mesh)
        {
                int i;
                for (i = 0; i < mesh.vertices.inputs.Count; i++)
                {
                    if (mesh.vertices.inputs[i].semantic == "POSITION")
                        return mesh.vertices.inputs[i];
                }
                throw new Exception("No POSITION in vertex input");
        }
        /// <summary>
        /// A context passed into all the COLLADA Processor functions. 
        /// Contains the current document, the materialTable and binding information.
        /// </summary>
        public class Context
        {
            public Document doc;
            public Dictionary<string, MaterialContent> materialTable;
            public Dictionary<string, string> materialBinding;
            public Dictionary<uint, uint> textureCoordinateBinding;
            public Dictionary<string, Dictionary<string, uint>> textureBindings;
        }
        /// <summary>
        /// Resolve the material binding  for the given "<instance_naterial>".
        /// <param name="instance">The "<instance_material>" element we need to resolve the binding for.</param>
        /// <param name="context"> The current context for the COLLADA Processor</param>
        /// </summary>
        static public void resolveBinding(
            Document.InstanceWithMaterialBind instance,
            Context context)
        {
            foreach (DictionaryEntry de in (IDictionary)(instance.bindMaterial.instanceMaterials))
            {
                // material binding
                context.materialBinding[((Document.InstanceMaterial)de.Value).symbol] = (string)de.Key;
                if (((Document.InstanceMaterial)de.Value).bindVertexInputs != null) // textureset binding 
                    foreach (Document.InstanceMaterial.BindVertexInput bindVertexInput in ((Document.InstanceMaterial)de.Value).bindVertexInputs)
                    {
                        if (context.textureBindings[(string)de.Key].ContainsKey(bindVertexInput.semantic))
                        {
                            uint tmp = context.textureBindings[(string)de.Key][bindVertexInput.semantic];
                            uint tmp2 = bindVertexInput.inputSet;
                            context.textureCoordinateBinding[tmp2] = tmp;
                        }
                    }
                else if (((Document.InstanceMaterial)de.Value).binds != null)
                    foreach (Document.InstanceMaterial.Bind bind in ((Document.InstanceMaterial)de.Value).binds)
                    {
                        uint tmp = context.textureBindings[(string)de.Key][bind.semantic];
                        // assuming only one texture coordinate set
                        context.textureCoordinateBinding[0] = tmp;
                    }
            }
        }
        /// <summary>
        /// Create a BasicMaterialContent from a "<material>".
        /// <param name="material">The "<material>" element to be converted.</param>
        /// <param name="context"> The current context for the COLLADA Processor</param>
        /// </summary>
        static public MaterialContent ReadMaterial(
            Document.Material material,
            Context context)
        {
            uint textureChannel = 0;
            Dictionary<string, uint> textureBinding = context.textureBindings[material.id];
            BasicMaterialContent materialContent = new BasicMaterialContent();
            Document.Effect effect = (Document.Effect)context.doc.dic[material.instanceEffect.Fragment];

            if (effect == null) throw new Exception("cannot find effect#" + material.instanceEffect.Fragment);
            // search common profile with correct asset....
            Document.ProfileCOMMON profile;
            foreach (Document.IProfile tmpProfile in effect.profiles)
            {
                if (tmpProfile is Document.ProfileCOMMON)
                {
                    profile = (Document.ProfileCOMMON)tmpProfile;
                    goto Found;
                }
            }
            throw new Exception("Could not find profile_COMMON in effect" + effect.ToString());
        Found:
            // read params
            Dictionary<string, string> samplerBind = new Dictionary<string, string>();
            Dictionary<string, string> imageBind = new Dictionary<string, string>();

            // Read Technique
            Document.SimpleShader shader = ((Document.ProfileCOMMON)profile).technique.shader;

            // BasicShader only accept texture for the diffuse channel
            if (shader.diffuse is Document.Texture)
            {
                string sampler = ((Document.Texture)shader.diffuse).texture;
                string surface = ((Document.Sampler2D)profile.newParams[sampler].param).source;
                string image = ((Document.Surface)profile.newParams[surface].param).initFrom;
                // now find image
                string imagePath = ((Document.Image)context.doc.dic[image]).init_from.Uri.LocalPath;
                // here associate 1 texture binding per texture in material
                textureBinding[((Document.Texture)shader.diffuse).texcoord] = textureChannel++;
                materialContent.Texture = new ExternalReference<TextureContent>(imagePath);
            }
            else if (shader.diffuse is Document.Color)
            {
                Document.Color color = (Document.Color)shader.diffuse;
                // TODO: manage color[3] in transparency
                materialContent.DiffuseColor = new Vector3(color[0], color[1], color[2]);
            }
            if (shader.ambient is Document.Texture)
            {
                // Basic Material does not accept texture on ambient channel
                /*
                string sampler = ((Document.Texture)shader.ambient).texture;
                string surface = ((Document.Sampler2D)profile.newParams[sampler].param).source;
                string image = ((Document.Surface)profile.newParams[surface].param).initFrom;
                // now find image
                string imagePath = ((Document.Image)doc.dic[image]).init_from.Uri.LocalPath;
                // here associate 1 texture binding per texture in material
                textureBinding[((Document.Texture)shader.ambient).texcoord] = textureChannel++;
                materialContent.Texture = new ExternalReference<TextureContent>(imagePath);
                */

            }
            else if (shader.ambient is Document.Color)
            {
                // XNA BasicMaterial has no ambient 
            }
            if (shader.emission is Document.Texture)
            {
                // XNA BasicMaterial does not accept texture for emmision
                /*
                string sampler = ((Document.Texture)shader.emission).texture;
                string surface = ((Document.Sampler2D)profile.newParams[sampler].param).source;
                string image = ((Document.Surface)profile.newParams[surface].param).initFrom;
                // now find image
                string imagePath = ((Document.Image)doc.dic[image]).init_from.Uri.LocalPath;
                // here associate 1 texture binding per texture in material
                textureBinding[((Document.Texture)shader.emission).texcoord] = textureChannel++;
                materialContent.Texture = new ExternalReference<TextureContent>(imagePath);
                 */
            }
            else if (shader.emission is Document.Color)
            {
                Document.Color color = (Document.Color)shader.emission;
                materialContent.EmissiveColor = new Vector3(color[0], color[1], color[2]);
            }
            if (shader.specular is Document.Texture)
            {
                // XNA BasicMaterial does not accept texture for specular
                /*
                string sampler = ((Document.Texture)shader.specular).texture;
                string surface = ((Document.Sampler2D)profile.newParams[sampler].param).source;
                string image = ((Document.Surface)profile.newParams[surface].param).initFrom;
                // now find image
                string imagePath = ((Document.Image)doc.dic[image]).init_from.Uri.LocalPath;
                // here associate 1 texture binding per texture in material
                textureBinding[((Document.Texture)shader.specular).texcoord] = textureChannel++;
                materialContent.Texture = new ExternalReference<TextureContent>(imagePath);
                 */
            }
            else if (shader.specular is Document.Color)
            {
                Document.Color color = (Document.Color)shader.specular;
                materialContent.SpecularColor = new Vector3(color[0], color[1], color[2]);
                if (shader.shininess is Document.Float)
                    materialContent.SpecularPower = ((Document.Float)shader.shininess).theFloat;
            }
            if (shader.transparency is Document.Texture)
            {
                // XNA Basic Shader does not accept a texture for the transparency channel
                /*
                string sampler = ((Document.Texture)shader.transparency).texture;
                string surface = ((Document.Sampler2D)profile.newParams[sampler].param).source;
                string image = ((Document.Surface)profile.newParams[surface].param).initFrom;
                // now find image
                string imagePath = ((Document.Image)doc.dic[image]).init_from.Uri.LocalPath;
                // here associate 1 texture binding per texture in material
                textureBinding[((Document.Texture)shader.transparency).texcoord] = textureChannel++;
                materialContent.Texture = new ExternalReference<TextureContent>(imagePath);
                 */
            }
            else if (shader.transparency is Document.Float)
            {
                materialContent.Alpha = ((Document.Float)shader.transparency).theFloat;
            }

            return materialContent;
        }
        /// <summary>
        /// Parse the COLLADA node graph and create appropriate NodeContent, including transforms.
        /// <param name="node">The "<node>" element to be converted.</param>
        /// <param name="context"> The current context for the COLLADA Processor</param>
        /// </summary>
        static public NodeContent ReadNode(
            Document.Node node,
            Context context)
        {
            NodeContent content = null;
            
            if (node.instances != null)
                foreach (Document.Instance instance in node.instances)
                {
                    if (instance is Document.InstanceGeometry)
                    {
                        // resolve bindings
                        // MaterialBinding contails the material_id bind to each symbol in the <mesh>
                        context.materialBinding = new Dictionary<string, string>();
                        // TextureCoordinateBinding contains XNA mesh channel number for a given texcoord set#
                        context.textureCoordinateBinding = new Dictionary<uint, uint>();
                        resolveBinding((Document.InstanceWithMaterialBind)instance, context);

                        Document.Geometry geo = (Document.Geometry)context.doc.dic[instance.url.Fragment];
                        content = ReadGeometry(geo, context);
                        content.Name = node.name;

                    } else if (instance is Document.InstanceCamera)
                    {
                        // TODO: camera
                        content = new NodeContent();
                        content.Name = node.name;
                    } else if (instance is Document.InstanceLight)
                    {
                        // TODO: light
                        content = new NodeContent();
                        content.Name = node.name;
                    } else if (instance is Document.InstanceController)
                    {

                        Document.ISkinOrMorph skinOrMorph = ((Document.Controller)context.doc.dic[((Document.InstanceController)instance).url.Fragment]).controller;
                        if (skinOrMorph is Document.Skin)
                        {
                            // XNA has no support for skining ?
                            // get the pose model, and convert it to a static mesh for now

                            // resolve bindings
                            // MaterialBinding contails the material_id bind to each symbol in the <mesh>
                            context.materialBinding = new Dictionary<string, string>();
                            // TextureCoordinateBinding contains XNA mesh channel number for a given texcoord set#
                            context.textureCoordinateBinding = new Dictionary<uint, uint>();
                            resolveBinding((Document.InstanceWithMaterialBind)instance, context);

                            Document.Geometry geo = ((Document.Geometry)context.doc.dic[((Document.Skin)skinOrMorph).source.Fragment]);
                            content = ReadGeometry(geo, context);
                            content.Name = node.name;
                        } else if (skinOrMorph is Document.Morph)
                        {
                            // TODO: morph
                            content = new NodeContent();
                            content.Name = node.name;

                        } else
                            throw new Exception("Unknowned type of controler:" + skinOrMorph.GetType().ToString());
                    }
                    else if (instance is Document.InstanceNode)
                    {
                        Document.Node instanceNode = ((Document.Node)context.doc.dic[instance.url.Fragment]);
                        content = ReadNode(instanceNode, context);
                        content.Name = node.name;

                    }
                    else
                        throw new Exception("Unkowned type of INode in scene :" + instance.GetType().ToString());
                }


            if (content == null) content = new NodeContent();
            
            // read transforms
            content.Transform = Matrix.Identity;
            
            if (node.transforms != null)
                foreach (Document.TransformNode transform in node.transforms)
                {
                    
                    if (transform is Document.Translate)
                        content.Transform = Matrix.CreateTranslation(transform[0], transform[1], transform[2]) * content.Transform;
                    else if (transform is Document.Rotate) 
                        content.Transform = Matrix.CreateFromAxisAngle(new Vector3(transform[0], transform[1], transform[2]), MathHelper.ToRadians(transform[3])) * content.Transform;
                    else if (transform is Document.Lookat)
                        content.Transform = Matrix.CreateLookAt(new Vector3(transform[0], transform[1], transform[2]), new Vector3(transform[3], transform[4], transform[5]), new Vector3(transform[6], transform[7], transform[8])) * content.Transform;
                    else if (transform is Document.Matrix)
                        content.Transform = new Matrix(transform[0], transform[1], transform[2], transform[3],
                                                        transform[4], transform[5], transform[6], transform[7],
                                                        transform[8], transform[9], transform[10], transform[11],
                                                        transform[12], transform[13], transform[14], transform[15]) * content.Transform;
                    else if (transform is Document.Scale)
                        content.Transform = Matrix.CreateScale(transform[0], transform[1], transform[2]) * content.Transform;
                    else if (transform is Document.Skew)
                    {
                        // Convert Skew to a matrix
                        float angle = MathHelper.ToRadians(transform[0]);
                        Vector3 a = new Vector3(transform[1], transform[2], transform[3]);
                        Vector3 b = new Vector3(transform[4], transform[5], transform[6]);
                        Vector3 n2 = Vector3.Normalize(b);
                        Vector3 a1 = n2*Vector3.Dot(a,n2);
                        Vector3 a2 = a-a1;
                        Vector3 n1 = Vector3.Normalize(a2);
                        float an1=Vector3.Dot(a,n1);
                        float an2=Vector3.Dot(a,n2);
                        double rx=an1*Math.Cos(angle) - an2*Math.Sin(angle);
                        double ry=an1*Math.Sin(angle) + an2*Math.Cos(angle);
                        float alpha = 0.0f;
                        Matrix m = Matrix.Identity;

                        if (rx <= 0.0) throw new Exception("Skew: angle too large");
                        if (an1!=0) alpha=(float)(ry/rx-an2/an1);

                        m.M11 = n1.X * n2.X * alpha + 1.0f;
                        m.M12 = n1.Y * n2.X * alpha;
                        m.M13 = n1.Z * n2.X * alpha;

                        m.M21 = n1.X * n2.Y * alpha;
                        m.M22 = n1.Y * n2.Y * alpha + 1.0f;
                        m.M23 = n1.Z * n2.Y * alpha;

                        m.M31 = n1.X * n2.Z * alpha;
                        m.M32 = n1.Y * n2.Z * alpha;
                        m.M33 = n1.Z * n2.Z * alpha + 1.0f;

                        content.Transform = m * content.Transform;

                    }
                }
                     
            
            if (node.children != null)
                foreach (Document.Node child in node.children)
                {
                    content.Children.Add(ReadNode(child, context));
                }

            return content;

        }
        /// <summary>
        /// Convert a TRIANGLE based "<geometry>" in a MeshContent
        /// <param name="geo">The "<geometry>" element to be converted.</param>
        /// <param name="context"> The current context for the COLLADA Processor</param>
        /// </summary>
        static public MeshContent ReadGeometry(Document.Geometry geo, Context context)
        {
            Document.Input vertexInput = null;
            Document.Input positionInput = null;
            List<int> normals = new List<int>();
            List<Document.Input> normalInputs = new List<Document.Input>();
            int normalCount = 0;
            List<int> textures = new List<int>();
            List<Document.Input> textureInputs = new List<Document.Input>();
            int textureCount = 0;
            List<int> colors = new List<int>();
            List<Document.Input> colorInputs = new List<Document.Input>();
            int colorCount = 0;
            List<int> tangents = new List<int>();
            List<Document.Input> tangentInputs = new List<Document.Input>();
            int tangentCount = 0;
            List<int> binormals = new List<int>();
            List<Document.Input> binormalInputs = new List<Document.Input>();
            int binormalCount = 0;
            List<int> userData = new List<int>();
            List<Document.Input> userDataInputs = new List<Document.Input>();
            int userDataCount = 0;

            int i, j, k;


            // Note: XNA meshbuilder does not allow for creating meshes that have different channel information
            // so all the primitives in the COLLADA mesh must have the same number of channel for the following to work

            MeshBuilder builder = MeshBuilder.StartMesh(geo.id);
            positionInput = GetPositionInput(geo.mesh);
            builder.SwapWindingOrder = true;
            int positionCount = ((Document.Source)positionInput.source).accessor.count;
            for (i = 0; i < positionCount; i++)
            {
                builder.CreatePosition(GetSourceValue(positionInput, 0, i), GetSourceValue(positionInput, 1, i), GetSourceValue(positionInput, 2, i));
            }
            string usage;
            bool vertexChannelsCreated = false;
            foreach (Document.Primitive prim in geo.mesh.primitives)
            {
                Document.Primitive primitive = prim;
                // reset counters for input arrays
                normalCount = 0;
                textureCount = 0;
                colorCount = 0;
                tangentCount = 0;
                binormalCount = 0;
                userDataCount = 0;

                // set primitive material from already resolved binding
                if (context.materialTable != null)
                    builder.SetMaterial(context.materialTable[context.materialBinding[primitive.material]]);

                // Note: convert all geometry to triangle before calling this routine !

                if (primitive is Document.Triangle )
                {
                    foreach (Document.Input input in primitive.Inputs)
                    {
                        switch (input.semantic)
                        {
                            case "VERTEX":
                                {
                                    vertexInput = input;
                                    // Check for other inputs per vertex
                                    foreach (Document.Input indirectInput in geo.mesh.vertices.inputs)
                                    {
                                        switch (indirectInput.semantic)
                                        {
                                            case "POSITION":
                                                positionInput.offset = input.offset;
                                                break;
                                            case "NORMAL":
                                                if (vertexChannelsCreated == false)
                                                {
                                                    usage = VertexChannelNames.EncodeName("Normal", normalCount);
                                                    normals.Add(builder.CreateVertexChannel<Vector3>(usage));
                                                }
                                                indirectInput.offset = input.offset;
                                                normalInputs.Add(indirectInput);
                                                normalCount++;
                                                break;
                                            case "TEXCOORD":
                                                if (context.textureCoordinateBinding != null)
                                                {
                                                    uint inputSet = 0;
                                                    if (indirectInput.set >= 0) inputSet = (uint)indirectInput.set;
                                                    // if no binding ca be found, then this text coord is not used.
                                                    // XBA BuildMesh will complain if texcoords are there, but no texture is associated
                                                    if (context.textureCoordinateBinding.ContainsKey(inputSet))
                                                    {
                                                        uint usageSet = context.textureCoordinateBinding[inputSet];
                                                        if (vertexChannelsCreated == false)
                                                        {
                                                            usage = VertexChannelNames.EncodeName("TextureCoordinate", (int)usageSet);
                                                            textures.Add(builder.CreateVertexChannel<Vector2>(usage));
                                                        }
                                                        indirectInput.offset = input.offset;
                                                        textureInputs.Add(indirectInput);
                                                        textureCount++;
                                                    }
                                                }
                                                break;
                                            case "COLOR":
                                                if (vertexChannelsCreated == false)
                                                {
                                                    usage = VertexChannelNames.EncodeName("Color", colorCount);
                                                    colors.Add(builder.CreateVertexChannel<Vector3>(usage));
                                                }
                                                indirectInput.offset = input.offset;
                                                colorInputs.Add(indirectInput);
                                                colorCount++;
                                                // set material for enabling vertex color 
                                                ((BasicMaterialContent)context.materialTable[context.materialBinding[primitive.material]]).VertexColorEnabled = true;
                                                break;
                                            case "TANGENT":
                                                if (vertexChannelsCreated == false)
                                                {
                                                    usage = VertexChannelNames.EncodeName("Tangent", tangentCount);
                                                    tangents.Add(builder.CreateVertexChannel<Vector3>(usage));
                                                }
                                                indirectInput.offset = input.offset;
                                                tangentInputs.Add(indirectInput);
                                                tangentCount++;
                                                break;
                                            case "BINORMAL":
                                                if (vertexChannelsCreated == false)
                                                {
                                                    usage = VertexChannelNames.EncodeName("Binormal", binormalCount);
                                                    binormals.Add(builder.CreateVertexChannel<Vector3>(usage));
                                                }
                                                indirectInput.offset = input.offset;
                                                binormalInputs.Add(indirectInput);
                                                binormalCount++;
                                                break;
                                            case "UV":
                                                // TODO: do something with the user data. XNA is likely not to like "UV"
                                                if (vertexChannelsCreated == false)
                                                {
                                                    usage = VertexChannelNames.EncodeName("UV", userDataCount);
                                                    userData.Add(builder.CreateVertexChannel<Vector2>(usage));
                                                }
                                                indirectInput.offset = input.offset;
                                                userDataInputs.Add(indirectInput);
                                                userDataCount++;
                                                break;
                                            default:
                                                throw new Exception("Primitive Channel Vertices Semantic " + indirectInput.semantic + " Not supported");
                                        }
                                    }
                                }
                                break;
                            case "NORMAL":
                                if (vertexChannelsCreated == false)
                                {
                                    usage = VertexChannelNames.EncodeName("Normal", normalCount);
                                    normals.Add(builder.CreateVertexChannel<Vector3>(usage));
                                }
                                normalInputs.Add(input);
                                normalCount++;
                                break;
                            case "TEXCOORD":
                                if (context.textureCoordinateBinding != null)
                                {
                                    uint inputSet = 0;
                                    if (input.set >= 0) inputSet = (uint)input.set;
                                    // if no binding ca be found, then this text coord is not used.
                                    // XBA BuildMesh will complain if texcoords are there, but no texture is associated
                                    if (context.textureCoordinateBinding.ContainsKey(inputSet))
                                    {
                                        uint usageSet = context.textureCoordinateBinding[inputSet];
                                        if (vertexChannelsCreated == false)
                                        {
                                            usage = VertexChannelNames.EncodeName("TextureCoordinate", (int)usageSet);
                                            textures.Add(builder.CreateVertexChannel<Vector2>(usage));
                                        }
                                        textureInputs.Add(input);
                                        textureCount++;
                                    }
                                }
                                break;
                            case "COLOR":
                                if (vertexChannelsCreated == false)
                                {
                                    usage = VertexChannelNames.EncodeName("Color", colorCount);
                                    colors.Add(builder.CreateVertexChannel<Vector3>(usage));
                                }
                                colorInputs.Add(input);
                                colorCount++;
                                // set material for enabling vertex color 
                                ((BasicMaterialContent)context.materialTable[context.materialBinding[primitive.material]]).VertexColorEnabled = true;
                                break;
                            case "TANGENT":
                                if (vertexChannelsCreated == false)
                                {
                                    usage = VertexChannelNames.EncodeName("Tangent", tangentCount);
                                    tangents.Add(builder.CreateVertexChannel<Vector3>(usage));
                                }
                                tangentInputs.Add(input);
                                tangentCount++;
                                break;
                            case "BINORMAL":
                                if (vertexChannelsCreated == false)
                                {
                                    usage = VertexChannelNames.EncodeName("Binormal", binormalCount);
                                    binormals.Add(builder.CreateVertexChannel<Vector3>(usage));
                                }
                                binormalInputs.Add(input);
                                binormalCount++;
                                break;
                            case "UV":
                                // where does user data go ?
                                if (vertexChannelsCreated == false)
                                {
                                    usage = VertexChannelNames.EncodeName("UV", userDataCount);
                                    userData.Add(builder.CreateVertexChannel<Vector2>(usage));
                                }
                                userDataInputs.Add(input);
                                userDataCount++;
                                break;
                            default:
                                throw new Exception("Primitive Channel Semantic " + input.semantic + " Not supported");
                        }
                    }
                    vertexChannelsCreated = true;
                    for (i = 0; i < primitive.count * 3; i++)
                    {
                        for (k = 0; k < normalCount; k++)
                        {
                            j = GetPValue(normalInputs[k], primitive, i);
                            Vector3 normal = new Vector3(
                                GetSourceValue(normalInputs[k], 0, j),
                                GetSourceValue(normalInputs[k], 1, j),
                                GetSourceValue(normalInputs[k], 2, j));
                            builder.SetVertexChannelData(normals[k], normal);
                        }
                        for (k = 0; k < textureCount; k++)
                        {
                            // TODO: support for 3D texture coordinates 
                            // Reversed texture 'T' coordinate for Direct X.

                            j = GetPValue(textureInputs[k], primitive, i);
                            Vector2 textureCoordinate = new Vector2(
                                     GetSourceValue(textureInputs[k], 0, j),
                                1.0f - GetSourceValue(textureInputs[k], 1, j));
                            builder.SetVertexChannelData(textures[k], textureCoordinate);
                        }
                        for (k = 0; k < colorCount; k++)
                        {
                            j = GetPValue(colorInputs[k], primitive, i);
                            Vector3 color = new Vector3(
                                GetSourceValue(colorInputs[k], 0, j),
                                GetSourceValue(colorInputs[k], 1, j),
                                GetSourceValue(colorInputs[k], 2, j));
                            builder.SetVertexChannelData(colors[k], color);
                        }
                        for (k = 0; k < binormalCount; k++)
                        {
                            j = GetPValue(binormalInputs[k], primitive, i);
                            Vector3 binormal = new Vector3(
                                GetSourceValue(binormalInputs[k], 0, j),
                                GetSourceValue(binormalInputs[k], 1, j),
                                GetSourceValue(binormalInputs[k], 2, j));
                            builder.SetVertexChannelData(binormals[k], binormal);
                        }
                        for (k = 0; k < userDataCount; k++)
                        {
                            j = GetPValue(userDataInputs[k], primitive, i);
                            Vector2 generic = new Vector2(
                                GetSourceValue(userDataInputs[k], 0, j),
                                GetSourceValue(userDataInputs[k], 1, j));
                            builder.SetVertexChannelData(userData[k], generic);
                        }
                        for (k = 0; k < tangentCount; k++)
                        {
                            j = GetPValue(tangentInputs[k], primitive, i);
                            Vector3 tangent = new Vector3(
                                GetSourceValue(tangentInputs[k], 0, j),
                                GetSourceValue(tangentInputs[k], 1, j),
                                GetSourceValue(tangentInputs[k], 2, j));
                            builder.SetVertexChannelData(tangents[k], tangent);
                        }
                        j = GetPValue(positionInput, primitive, i);
                        builder.AddTriangleVertex(j);
                    }
                }
                else 
                    throw new Exception(primitive.GetType().ToString() + " is not implemented yet in COLLADA/Processor.ReadGeometry");

            }
            return builder.FinishMesh();
        }
        /// <summary>
        /// Convert the "<visual_scene>" of a COLLADA document into a ModeContent.
        /// Includes geometry, material and hierarchy
        /// <param name="doc">The COLLADA document to be converted.</param>
        /// </summary>
        static public NodeContent Convert(Document doc)
        {

            string urlFrag = doc.instanceVisualScene.url.Fragment;
            Document.VisualScene scene = (Document.VisualScene)doc.dic[urlFrag];
            // Maybe we should load all the scenes from the scene library instead of only the instanced one ?
            if (scene == null) throw new Exception("NO VISUAL SCENE IN DOCUMENT");

            // Read materials
            Context context = new Context(); ;
            context.doc = doc;
            context.materialTable = new Dictionary<String, MaterialContent>();
            context.textureBindings = new Dictionary<string, Dictionary<string, uint>>();

            foreach (Document.Material material in doc.materials)
            {
                // textureBindings contains the XNA mesh texture channel number for each texture binding target
                context.textureBindings[material.id] = new Dictionary<string, uint>();
                // materialTable contains the XNA material for each COLLADA material
                context.materialTable[material.id] = ReadMaterial(material, context);
            }

            // recursive call to load scene
            NodeContent content = new NodeContent();
            content.Name = scene.name;

            foreach (Document.Node node in scene.nodes)
            {
                content.Children.Add(ReadNode(node, context));
            }
            return content;
        }
    }


    /// <summary>
    /// The COLLADA Importer hook for the XNA content pipeline
    /// Load the COLLADA document, then call the necessary condioners on it
    /// </summary>
    [ContentImporter(".dae", DisplayName = "COLLADA Importer - Isles")]
    public class COLLADAImporter : ContentImporter<NodeContent>
    {
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            Document doc = new Document(filename);
            Conditioner.ConvexTriangulator(doc);
            NodeContent input = Processor.Convert(doc);
            return input;
        }
    }
}


