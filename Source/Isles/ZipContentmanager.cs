#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//-----------------------------------------------------------------------------
//
//  Copyright (c) 2006, 2007 Microsoft Corporation.  All rights reserved.
//
//  Implements the CRC algorithm, which is used in zip files.  The zip format calls for
//  the zipfile to contain a CRC for the unencrypted byte stream of each file.
//
//  It is based on example source code published at
//    http://www.vbaccelerator.com/home/net/code/libraries/CRC32/Crc32_zip_CRC32_CRC32_cs.asp
//
//  This implementation adds a tweak of that code for use within zip creation.  While
//  computing the CRC we also compress the byte stream, in the same read loop. This
//  avoids the need to read through the uncompressed stream twice - once to computer CRC
//  and another time to compress.
//
//
//  Thu, 30 Mar 2006  13:58
//-----------------------------------------------------------------------------
//
//  ZipContentManager Modified From EasyZip (Copyright?)
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
#endregion


namespace Isles
{
    using Isles.Zip;


    #region ZipContentManager
    /// <summary>
    /// A content manager used to read files stored inside of a .zip file.
    /// </summary>
    public class ZipContentManager : ContentManager
    {
        ZipFile zipFile;
        
        /// <summary>
        /// Default to false
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// Creates a new ZipContentManager that loads files from the specified
        /// zip file.
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="zipFile">Zip file to extract assets from</param>
        public ZipContentManager(IServiceProvider serviceProvider, string zipFile)
            : this(serviceProvider, zipFile, false)
        {

        }

        /// <summary>
        /// Creates a new ZipContentManager that loads files from the specified
        /// zip file.
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="zipFile">Zip file to extract assets from</param>
        /// <param name="caseSensitive">Whether or not loading assets is case-sensitive</param>
        public ZipContentManager(IServiceProvider serviceProvider, string zipFile, bool caseSensitive)
            : base(serviceProvider)
        {
            try
            {
                this.IsCaseSensitive = caseSensitive;
                this.zipFile = ZipFile.Read(zipFile);
            }
            catch { this.zipFile = null; }
        }

        protected override Stream OpenStream(string assetName)
        {
            assetName = Path.Combine(RootDirectory, assetName);

            try
            {
                return base.OpenStream(assetName);
            }
            catch (Exception e) { e.ToString(); }

            if (zipFile != null)
            {
                // Check the zip file if no file is found directly
                assetName = assetName.Replace("\\", "/");

                string fullAssetName = assetName + ".xnb";

                if (!IsCaseSensitive)
                    fullAssetName = fullAssetName.ToLower();

                foreach (ZipEntry entry in zipFile)
                {
                    string entryName = (IsCaseSensitive) ? entry.FileName : entry.FileName.ToLower();

                    if (entryName.Equals(fullAssetName))
                        return entry.GetStream();
                }
            }

            throw new Exception("Failed to load asset: " + assetName);
        }
    }
    #endregion
}