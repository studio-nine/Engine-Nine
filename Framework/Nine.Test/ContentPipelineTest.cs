#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace Nine
{
    [TestClass]
    public class ContentPipelineTest : GraphicsTest
    {
        internal ContentBuilder ContentBuilder { get; private set; }
        public ContentManager Content { get { return Game.Content; } }

        [TestInitialize()]
        public override void Initialize()
        {
            base.Initialize();

            ContentBuilder = new ContentBuilder();
            Content.RootDirectory = ContentBuilder.OutputDirectory;
        }

        [TestCleanup()]
        public override void Cleanup()
        {
            base.Cleanup();

            ContentBuilder.Clear();
            ContentBuilder.Dispose();
        }

        protected IDictionary<string, object> Parameters(params object[] parameters)
        {
            var dict = new Dictionary<string, object>();
            for (int i = 0; i < parameters.Length; i += 2)
            {
                dict.Add(parameters[i].ToString(), parameters[i + 1]);
            }
            return dict;
        }

        protected string BuildObjectUsingDefaultContentProcessor(object input)
        {
            return BuildObject(input, "DefaultContentProcessor", null);
        }

        protected string BuildObject(object input, string processor, IDictionary<string, object> processorParameters)
        {
            var filename = Guid.NewGuid().ToString("B");
            using (var writer = XmlWriter.Create(filename + ".xml"))
            {
                IntermediateSerializer.Serialize(writer, input, null);
            }
            ContentBuilder.Add(filename + ".xml", null, null, processor, processorParameters);
            return filename;
        }

        protected void BuildModel(string filename, IDictionary<string, object> processorParameters = null)
        {
            ContentBuilder.Add(filename, null, null, "ExtendedModelProcessor", processorParameters);
        }

        protected void BuildFont(string filename)
        {
            ContentBuilder.Add(filename, null, null, "FontDescriptionProcessor", null);
        }

        protected void BuildTexture(string filename, IDictionary<string, object> processorParameters = null)
        {
            ContentBuilder.Add(filename, null, null, "TextureProcessor", processorParameters);
        }

        protected string BuildTexture(Color color, IDictionary<string, object> processorParameters = null)
        {
            PixelBitmapContent<Color> bitmap = new PixelBitmapContent<Color>(1, 1);
            bitmap.SetPixel(0, 0, color);
            return BuildTexture(bitmap, processorParameters);
        }

        protected string BuildTexture(BitmapContent bitmap, IDictionary<string, object> processorParameters = null)
        {
            TextureContent texture = new Texture2DContent();
            texture.Faces[0] = new MipmapChain(bitmap);
            return BuildObject(texture, "TextureProcessor", processorParameters);
        }

        protected void BuildNormalMap(string filename)
        {
            ContentBuilder.Add(filename, null, null, "NormalTextureProcessor", null);
        }

        protected void RunTheBuild()
        {
            ContentBuilder.Build();
        }
    }
}
