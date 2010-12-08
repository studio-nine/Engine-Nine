#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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

namespace Nine.Content.Pipeline.Importers
{
    [ContentImporter(".unknown", DisplayName = "Sequential Image List Importer - Engine Nine")]
    public class SequentialImageListImporter : ContentImporter<string[]>
    {
        public override string[] Import(string filename, ContentImporterContext context)
        {
            int i = 0;
            int num = 0;
            int digits = 0;

            string name = Path.GetFileNameWithoutExtension(filename);

            for (i = name.Length - 1; i >= 0; i--)
            {
                if (!(name[i] <= '9' && name[i] >= '0'))
                    break;

                digits++;
            }

            if (digits <= 0)
                return new string[] { filename };

            string baseName = name.Substring(0, i + 1);
            num = int.Parse(name.Substring(i + 1));
            num++;

            string ext = Path.GetExtension(filename);
            string dir = filename.Substring(0, filename.LastIndexOf(Path.DirectorySeparatorChar));                    
            string file = Path.Combine(dir, baseName + string.Format("{0:d" + digits + "}", (num++)) + ext);

            List<string> result = new List<string>();

            result.Add(filename);

            while (File.Exists(file))
            {
                result.Add(file);

                file = Path.Combine(dir, baseName + string.Format("{0:d" + digits + "}", (num++)) + ext);
            }

            return result.ToArray();
        }
    }
}
