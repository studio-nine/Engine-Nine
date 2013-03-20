namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Compression;

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
    #region ZipArchiveEntry
    [System.Diagnostics.DebuggerDisplay("{FullName}")]
    internal class ZipArchiveEntry
    {
        const int ZipEntrySignature = 0x04034b50;
        const int ZipEntryDataDescriptorSignature = 0x08074b50;
        
        private long basePosition;

        public ZipArchive Archive { get; private set; }
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public long CompressedLength { get; set; }
        public Int32 UncompressedLength { get; private set; }
        public DateTime LastWriteTime { get; private set; }

        private ZipArchiveEntry(ZipArchive archive)
        {
            this.Archive = archive;
        }

        public static ZipArchiveEntry Read(ZipArchive archive, Stream s)
        {
            var entry = new ZipArchiveEntry(archive);
            return entry.ReadHeader(s) ? entry : null;
        }

        private bool ReadHeader(Stream s)
        {
            int signature = Shared.ReadSignature(s);

            // return null if this is not a local file header signature
            if (SignatureIsNotValid(signature))
            {
                s.Seek(-4, System.IO.SeekOrigin.Current);
                return false;
            }

            byte[] block = new byte[26];
            int n = s.Read(block, 0, block.Length);
            if (n != block.Length) return false;

            int i = 0;
            var _VersionNeeded = (short)(block[i++] + block[i++] * 256);
            var _BitField = (short)(block[i++] + block[i++] * 256);
            var _CompressionMethod = (short)(block[i++] + block[i++] * 256);
            var _LastModDateTime = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;

            // the PKZIP spec says that if bit 3 is set (0x0008), then the CRC, Compressed size, and uncompressed size
            // come directly after the file data.  The only way to find it is to scan the zip archive for the signature of 
            // the Data Descriptor, and presume that that signature does not appear in the (compressed) data of the compressed file.  

            if ((_BitField & 0x0008) != 0x0008)
            {
                var _Crc32 = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                CompressedLength = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                UncompressedLength = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
            }
            else
            {
                // the CRC, compressed size, and uncompressed size are stored later in the stream.
                // here, we advance the pointer.
                i += 12;
            }

            Int16 filenameLength = (short)(block[i++] + block[i++] * 256);
            Int16 extraFieldLength = (short)(block[i++] + block[i++] * 256);

            block = new byte[filenameLength];
            n = s.Read(block, 0, block.Length);
            FullName = Extensions.CleanPath(Shared.StringFromBuffer(block, 0, block.Length));
            Name = Path.GetFileName(FullName);

            s.Seek(extraFieldLength, SeekOrigin.Current);

            // transform the time data into something usable
            LastWriteTime = Shared.PackedToDateTime(_LastModDateTime);

            // actually get the compressed size and CRC if necessary
            if ((_BitField & 0x0008) == 0x0008)
            {
                long posn = s.Position;
                long SizeOfDataRead = Shared.FindSignature(s, ZipEntryDataDescriptorSignature);
                if (SizeOfDataRead == -1) return false;

                // read 3x 4-byte fields (CRC, Compressed Size, Uncompressed Size)
                block = new byte[12];
                n = s.Read(block, 0, block.Length);
                if (n != 12) return false;
                i = 0;
                var _Crc32 = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                CompressedLength = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;
                UncompressedLength = block[i++] + block[i++] * 256 + block[i++] * 256 * 256 + block[i++] * 256 * 256 * 256;

                if (SizeOfDataRead != CompressedLength)
                    throw new Exception("Data format error (bit 3 is set)");

                // seek back to previous position, to read file data
                s.Seek(posn, SeekOrigin.Begin);
            }

            basePosition = s.Position;

            s.Seek(CompressedLength, SeekOrigin.Current);

            // finally, seek past the (already read) Data descriptor if necessary
            if ((_BitField & 0x0008) == 0x0008)
            {
                s.Seek(16, SeekOrigin.Current);
            }
            return true;
        }

        private static bool SignatureIsNotValid(int signature)
        {
            return (signature != ZipEntrySignature);
        }
        
        public Stream Open()
        {
            var result = new OffsetStream(Archive.stream, basePosition, CompressedLength);

            if (CompressedLength == UncompressedLength)
                return result;

            return new DeflateStream(result, CompressionMode.Decompress);
        }
    }
    #endregion

    #region ZipArchive
    internal class ZipArchive : IDisposable
    {
        internal Stream stream;
        private bool isDisposed;
        private List<ZipArchiveEntry> entries;
        private Dictionary<string, ZipArchiveEntry> entriesDictionary;

        public ReadOnlyCollection<ZipArchiveEntry> Entries { get; private set; }

        public ZipArchive(Stream stream)
        {
            this.stream = stream;
            this.entries = new List<ZipArchiveEntry>();
            this.Entries = new ReadOnlyCollection<ZipArchiveEntry>(entries);

            ZipArchiveEntry e;
            while ((e = ZipArchiveEntry.Read(this, stream)) != null)
            {
                // Don't add directory entries
                if (!string.IsNullOrEmpty(e.Name))
                    entries.Add(e);
            }
        }

        public ZipArchiveEntry GetEntry(string filename)
        {
            if (entriesDictionary == null)
            {
                entriesDictionary = new Dictionary<string, ZipArchiveEntry>(entries.Count, StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < entries.Count; i++)
                    entriesDictionary.Add(entries[i].FullName, entries[i]);
            }

            ZipArchiveEntry result;
            if (entriesDictionary.TryGetValue(filename, out result))
                return result;
            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (!this.isDisposed)
            {
                if (disposeManagedResources)
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                    }
                }
                this.isDisposed = true;
            }
        }
    }
    #endregion

    #region CRC32
    /// <summary>
    /// Calculates a 32bit Cyclic Redundancy Checksum (CRC) using the
    /// same polynomial used by Zip.
    /// </summary>
    class CRC32
    {
        private UInt32[] crc32Table;
        private const int BUFFER_SIZE = 8192;

        private Int32 _TotalBytesRead = 0;
        public Int32 TotalBytesRead
        {
            get
            {
                return _TotalBytesRead;
            }
        }

        /// <summary>
        /// Returns the CRC32 for the specified stream.
        /// </summary>
        /// <param name="input">The stream over which to calculate the CRC32</param>
        /// <returns>the CRC32 calculation</returns>
        public UInt32 GetCrc32(System.IO.Stream input)
        {
            return GetCrc32AndCopy(input, null);
        }

        /// <summary>
        /// Returns the CRC32 for the specified stream, and writes the input into the output stream.
        /// </summary>
        /// <param name="input">The stream over which to calculate the CRC32</param>
        /// <param name="output">The stream into which to deflate the input</param>
        /// <returns>the CRC32 calculation</returns>
        public UInt32 GetCrc32AndCopy(System.IO.Stream input, System.IO.Stream output)
        {
            unchecked
            {
                UInt32 crc32Result;
                crc32Result = 0xFFFFFFFF;
                byte[] buffer = new byte[BUFFER_SIZE];
                int readSize = BUFFER_SIZE;

                _TotalBytesRead = 0;
                int count = input.Read(buffer, 0, readSize);
                if (output != null) output.Write(buffer, 0, count);
                _TotalBytesRead += count;
                while (count > 0)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        crc32Result = ((crc32Result) >> 8) ^ crc32Table[(buffer[i]) ^ ((crc32Result) & 0x000000FF)];
                    }
                    count = input.Read(buffer, 0, readSize);
                    if (output != null) output.Write(buffer, 0, count);
                    _TotalBytesRead += count;

                }

                return ~crc32Result;
            }
        }


        /// <summary>
        /// Construct an instance of the CRC32 class, pre-initialising the table
        /// for speed of lookup.
        /// </summary>
        public CRC32()
        {
            unchecked
            {
                // This is the official polynomial used by CRC32 in PKZip.
                // Often the polynomial is shown reversed as 0x04C11DB7.
                UInt32 dwPolynomial = 0xEDB88320;
                UInt32 i, j;

                crc32Table = new UInt32[256];

                UInt32 dwCrc;
                for (i = 0; i < 256; ++i)
                {
                    dwCrc = i;
                    for (j = 8; j > 0; j--)
                    {
                        if ((dwCrc & 1) == 1)
                        {
                            dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                        }
                        else
                        {
                            dwCrc >>= 1;
                        }
                    }
                    crc32Table[i] = dwCrc;
                }
            }
        }
    }
    #endregion

    #region Shared
    class Shared
    {
        protected internal static string StringFromBuffer(byte[] buf, int start, int maxlength)
        {
            int i;
            char[] c = new char[maxlength];
            for (i = 0; (i < maxlength) && (i < buf.Length) && (buf[i] != 0); ++i)
            {
                c[i] = (char)buf[i]; // System.BitConverter.ToChar(buf, start+i*2);
            }
            string s = new System.String(c, 0, i);
            return s;
        }

        protected internal static int ReadSignature(System.IO.Stream s)
        {
            int n = 0;
            byte[] sig = new byte[4];
            n = s.Read(sig, 0, sig.Length);
            if (n != sig.Length) throw new Exception("Could not read signature - no data!");
            int signature = (((sig[3] * 256 + sig[2]) * 256) + sig[1]) * 256 + sig[0];
            return signature;
        }

        protected internal static long FindSignature(System.IO.Stream s, int SignatureToFind)
        {
            long startingPosition = s.Position;

            int BATCH_SIZE = 1024;
            byte[] targetBytes = new byte[4];
            targetBytes[0] = (byte)(SignatureToFind >> 24);
            targetBytes[1] = (byte)((SignatureToFind & 0x00FF0000) >> 16);
            targetBytes[2] = (byte)((SignatureToFind & 0x0000FF00) >> 8);
            targetBytes[3] = (byte)(SignatureToFind & 0x000000FF);
            byte[] batch = new byte[BATCH_SIZE];
            int n = 0;
            bool success = false;
            do
            {
                n = s.Read(batch, 0, batch.Length);
                if (n != 0)
                {
                    for (int i = 0; i < n; ++i)
                    {
                        if (batch[i] == targetBytes[3])
                        {
                            s.Seek(i - n, System.IO.SeekOrigin.Current);
                            int sig = ReadSignature(s);
                            success = (sig == SignatureToFind);
                            if (!success) s.Seek(-3, System.IO.SeekOrigin.Current);
                            break; // out of for loop
                        }
                    }
                }
                else break;
                if (success) break;
            } while (true);
            if (!success)
            {
                s.Seek(startingPosition, System.IO.SeekOrigin.Begin);
                return -1;  // or throw?
            }

            // subtract 4 for the signature.
            long bytesRead = (s.Position - startingPosition) - 4;
            // number of bytes read, should be the same as compressed size of file            
            return bytesRead;
        }
        protected internal static DateTime PackedToDateTime(Int32 packedDateTime)
        {
            Int16 packedTime = (Int16)(packedDateTime & 0x0000ffff);
            Int16 packedDate = (Int16)((packedDateTime & 0xffff0000) >> 16);

            int year = 1980 + ((packedDate & 0xFE00) >> 9);
            int month = (packedDate & 0x01E0) >> 5;
            int day = packedDate & 0x001F;


            int hour = (packedTime & 0xF800) >> 11;
            int minute = (packedTime & 0x07E0) >> 5;
            int second = packedTime & 0x001F;

            DateTime d = System.DateTime.Now;
            try { d = new System.DateTime(year, month, day, hour, minute, second, 0); }
            catch
            {
                Console.Write("\nInvalid date/time?:\nyear: {0} ", year);
                Console.Write("month: {0} ", month);
                Console.WriteLine("day: {0} ", day);
                Console.WriteLine("HH:MM:SS= {0}:{1}:{2}", hour, minute, second);
            }

            return d;
        }


        protected internal static Int32 DateTimeToPacked(DateTime time)
        {
            UInt16 packedDate = (UInt16)((time.Day & 0x0000001F) | ((time.Month << 5) & 0x000001E0) | (((time.Year - 1980) << 9) & 0x0000FE00));
            UInt16 packedTime = (UInt16)((time.Second & 0x0000001F) | ((time.Minute << 5) & 0x000007E0) | ((time.Hour << 11) & 0x0000F800));
            return (Int32)(((UInt32)(packedDate << 16)) | packedTime);
        }
    }
    #endregion

    #region OffsetStream
    class OffsetStream : Stream
    {
        private long length;
        private long position;
        private long basePosition;
        private Stream stream;

        public OffsetStream(Stream s, long basePosition, long length)
        {
            this.stream = s;
            this.basePosition = basePosition;
            this.length = length;
        }

        public override long Position
        {
            get { return position; }
            set { position = value; stream.Position = basePosition + value; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                return stream.Seek(basePosition + offset, SeekOrigin.Begin) - basePosition;
            if (origin == SeekOrigin.Current)
                return stream.Seek(basePosition + position + offset, SeekOrigin.Begin) - basePosition;
            return stream.Seek(basePosition + length + offset, SeekOrigin.Begin) - basePosition;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (stream.Position != basePosition + position)
                stream.Seek(basePosition + position, SeekOrigin.Begin);
            return stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Length
        {
            get { return length; }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
    #endregion
}