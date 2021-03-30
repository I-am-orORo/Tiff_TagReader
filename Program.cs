using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Tiff_TagReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"E:\SampleData\Tiff_Sample\erdas_spnad83.tif";
            ReadTiffTag(path);
        }

        public static void ReadTiffTag(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                var tagReader = new TagReader(fs);

                bool isBigTiff = false;
                bool byteOrder = false; //isBigEndian : Need Reverse                

                var byteOrder_1 = tagReader.ReadByte(false);
                var byteOrder_2 = tagReader.ReadByte(false);
                var versionNo_1 = tagReader.ReadByte(false);
                var versionNo_2 = tagReader.ReadByte(false);

                #region ByteOrder / Tiff or BigTiff
                if (byteOrder_1 == 'I' || byteOrder_2 == 'I')
                {
                    byteOrder = false;

                    if (versionNo_1 == 42)
                        isBigTiff = false;
                    else if (versionNo_1 == 43)
                        isBigTiff = true;
                    else
                        return;
                }

                else if (byteOrder_1 == 'M' || byteOrder_2 == 'M')
                {
                    byteOrder = true;

                    if (versionNo_2 == 42)
                        isBigTiff = false;
                    else if (versionNo_2 == 43)
                        isBigTiff = true;
                    else
                        return;
                }
                else
                    return;

                #endregion

                #region Define Tag Info (Position / Count / Size)

                ulong startOffset = 0;
                ulong tagCount = 0;

                var tagSize = isBigTiff ? 20 : 12;
                var tagOffset = isBigTiff ? 8 : 2;
                var numberSize = isBigTiff ? 8 : 4;
                var dataSize = isBigTiff ? 8 : 4;

                if (isBigTiff) // BigTiff
                {
                    tagReader.Seek(8);
                    startOffset = tagReader.ReadLong64(byteOrder);
                    tagReader.Seek(startOffset);
                    startOffset = tagReader.ReadLong64(byteOrder);
                }
                else // Tiff
                {
                    tagReader.Seek(4);
                    startOffset = tagReader.ReadLong(byteOrder);
                    tagReader.Seek(startOffset);
                    tagCount = tagReader.ReadShort(byteOrder);
                }
                #endregion

                for (int tagIndex = 0; Convert.ToUInt64(tagIndex) < tagCount; tagIndex++)
                {
                    tagReader.Seek(startOffset + Convert.ToUInt64(tagOffset + tagIndex * tagSize));

                    var tagID = tagReader.ReadShort(byteOrder);
                    var DataType = tagReader.ReadShort(byteOrder);
                    ulong count = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);


                    //! BaseLine Tiff Tag
                    #region 254 : NewSubfileType / LONG(4)
                    if (tagID == 254)
                    {
                        ulong newSubfileType = tagReader.ReadLong(byteOrder);
                    }
                    #endregion
                    #region 255 : SubfileType / SHORT(3) 
                    else if (tagID == 255)
                    {
                        ushort subfileType = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 256 : ImageWidth / SHORT(3) or LONG(4)
                    else if (tagID == 256)
                    {
                        ulong imageWidth = DataType == 3 ? tagReader.ReadShort(byteOrder) : tagReader.ReadLong(byteOrder);
                    }
                    #endregion
                    #region 256 : ImageLength / SHORT(3) or LONG(4)
                    else if (tagID == 257)
                    {
                        ulong imageLength = DataType == 3 ? tagReader.ReadShort(byteOrder) : tagReader.ReadLong(byteOrder);
                    }
                    #endregion
                    #region 258 : BitsPerSample / SHORT(3) / count = N
                    else if (tagID == 258)
                    {
                        ushort[] bitsPerSample = new ushort[count];
                        if (2 * count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        for (ulong i = 0; i < count; i++)
                            bitsPerSample[i] = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 259 : Compression / SHORT(3)
                    else if (tagID == 259)
                    {
                        ushort compression = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 262 : PhotometricInterpretation / SHORT(3)
                    else if (tagID == 262)
                    {
                        ushort photometricInterpretation = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 263 : Threshholding / SHORT(3)
                    else if (tagID == 263)
                    {
                        ushort threshholding = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 264 : CellWidth / SHORT(3)
                    else if (tagID == 264)
                    {
                        ushort cellWidth = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 265 : CellLength / SHORT(3)
                    else if (tagID == 265)
                    {
                        ushort cellWidth = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 266 : FillOrder / SHORT(3)
                    else if (tagID == 266)
                    {
                        ushort fillOrder = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 270 : ImageDescription / ASCII(2)
                    else if (tagID == 270)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        string imageDescription = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion
                    #region 271 : Make / ASCII(2)
                    else if (tagID == 271)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        string make = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion
                    #region 272 : Model / ASCII(2)
                    else if (tagID == 272)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        string imageDescription = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion
                    #region 273 : StripOffsets / SHORT(3) or LONG(4) / count = N
                    else if (tagID == 273)
                    {
                        ulong[] stripOffsets = new ulong[count];
                        if (DataType == 3) // Short
                        {
                            if (2 * count > Convert.ToUInt64(dataSize))
                            {
                                ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                                tagReader.Seek(address);
                            }

                            for (ulong i = 0; i < count; i++)
                                stripOffsets[i] = tagReader.ReadShort(byteOrder);
                        }
                        else // Long
                        {
                            if (4 * count > Convert.ToUInt64(dataSize))
                            {
                                ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                                tagReader.Seek(address);
                            }

                            for (ulong i = 0; i < count; i++)
                                stripOffsets[i] = tagReader.ReadLong(byteOrder);
                        }

                    }
                    #endregion
                    #region 274  : Orientation / SHORT(3)
                    else if (tagID == 274)
                    {
                        ushort orientation = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 277  : SamplesPerPixel / SHORT(3)
                    else if (tagID == 277)
                    {
                        ushort samplesPerPixel = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 278 : RowsPerStrip / SHORT(3) or LONG(4)
                    else if (tagID == 278)
                    {
                        ulong rowsPerStrip = DataType == 3 ? tagReader.ReadShort(byteOrder) : tagReader.ReadLong(byteOrder);
                    }
                    #endregion
                    #region 279 : StripByteCounts / SHORT(3) or LONG(4)
                    else if (tagID == 278)
                    {
                        ulong stripByteCounts = DataType == 3 ? tagReader.ReadShort(byteOrder) : tagReader.ReadLong(byteOrder);
                    }
                    #endregion
                    #region 280  : MinSampleValue / SHORT(3)
                    else if (tagID == 280)
                    {
                        ushort minSampleValue = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 281  : MaxSampleValue / SHORT(3)
                    else if (tagID == 281)
                    {
                        ushort maxSampleValue = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 282  : XResolution / RATIONAL(5)
                    else if (tagID == 282)
                    {
                        double xResolution = tagReader.ReadRational(byteOrder);
                    }
                    #endregion
                    #region 283  : YResolution / RATIONAL(5)
                    else if (tagID == 283)
                    {
                        double yResolution = tagReader.ReadRational(byteOrder);
                    }
                    #endregion
                    #region 284  : PlanarConfiguration / SHORT(3)
                    else if (tagID == 284)
                    {
                        ushort planarConfiguration = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 288 : FreeOffsets / LONG(4) / count = N
                    else if (tagID == 258)
                    {
                        ulong[] freeOffsets = new ulong[count];
                        if (4 * count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        for (ulong i = 0; i < count; i++)
                            freeOffsets[i] = tagReader.ReadLong(byteOrder);
                    }
                    #endregion
                    #region 289 : FreeByteCounts / LONG(4) / count = N
                    else if (tagID == 258)
                    {
                        ulong[] freeByteCounts = new ulong[count];
                        if (4 * count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        for (ulong i = 0; i < count; i++)
                            freeByteCounts[i] = tagReader.ReadLong(byteOrder);
                    }
                    #endregion
                    #region 290   : GrayResponseUnit / SHORT(3)
                    else if (tagID == 290)
                    {
                        ushort grayResponseUnit = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 291  : GrayResponseCurve / SHORT(3) / count = N
                    else if (tagID == 291)
                    {
                        ushort[] grayResponseCurve = new ushort[count];
                        if (2 * count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        for (ulong i = 0; i < count; i++)
                            grayResponseCurve[i] = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 296   : ResolutionUnit / SHORT(3)
                    else if (tagID == 296)
                    {
                        ushort resolutionUnit = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 305 : Software / ASCII(2)
                    else if (tagID == 305)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }
                        string software = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion
                    #region 306 : DateTime / ASCII(2) / count = 20
                    else if (tagID == 306)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }
                        string dateTime = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion
                    #region 315 : Artist / ASCII(2)
                    else if (tagID == 315)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }
                        string artist = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion
                    #region 316 : HostComputer / ASCII(2)
                    else if (tagID == 316)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }
                        string hostComputer = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion
                    #region 320  : ColorMap / SHORT(3) / count = 3 * (2**BitsPerSample)
                    else if (tagID == 320)
                    {
                        ushort[] colorMap = new ushort[count];
                        if (2 * count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        for (ulong i = 0; i < count; i++)
                            colorMap[i] = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 338   : ExtraSamples / SHORT(3) / count = N
                    else if (tagID == 338)
                    {
                        ushort[] extraSamples = new ushort[count];
                        if (2 * count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }

                        for (ulong i = 0; i < count; i++)
                            extraSamples[i] = tagReader.ReadShort(byteOrder);
                    }
                    #endregion
                    #region 33432  : Copyright / ASCII(2)
                    else if (tagID == 33432)
                    {
                        if (count > Convert.ToUInt64(dataSize))
                        {
                            ulong address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                            tagReader.Seek(address);
                        }
                        string copyright = tagReader.ReadAscii(Convert.ToInt32(count));
                    }
                    #endregion


                    //! Extension Tiff Tag


                    //! Private Tiff Tag

                }
                return;
            }
        }
    }


    class TagReader : BinaryReader
    {
        public TagReader(System.IO.Stream stream) : base(stream) { }

        public bool Seek(object offset)
        {
            ulong pos = 0;
            try
            {
                pos = Convert.ToUInt64(offset);
            }
            catch
            {
                //MessageBox.Show("_Seek Convert.ToUInt64(offset) Error");
                return false;
            }

            if (pos > Int64.MaxValue)
            {
                base.BaseStream.Seek((Int64)(Int64.MaxValue), SeekOrigin.Begin);
                base.BaseStream.Seek((Int64)(pos - Int64.MaxValue), SeekOrigin.Current);
            }
            else
            {
                base.BaseStream.Seek((Int64)pos, SeekOrigin.Begin);
            }

            return true;
        }

        // 1 BYTE
        public byte ReadByte(bool byteOrder)
        {
            var data = base.ReadByte();
            return data;
        }

        // 2 ASCII
        public string ReadAscii(int count, bool byteOrder = false)
        {
            if (count == 0)
                return string.Empty;

            var data = base.ReadBytes(count);
            if (byteOrder)
                Array.Reverse(data);

            return Encoding.ASCII.GetString(data, 0, count);
            // return BitConverter.ToString(data, 0);
        }

        // 3 Unsigned Short
        public ushort ReadShort(bool byteOrder)
        {
            var data = base.ReadBytes(2);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        // 4 Unsigned LONG
        public UInt32 ReadLong(bool byteOrder)
        {
            var data = base.ReadBytes(4);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        // 5 Unsigned Rational
        public double ReadRational(bool byteOrder)
        {
            var data1 = base.ReadBytes(4);
            var data2 = base.ReadBytes(4);
            if (byteOrder)
            {
                Array.Reverse(data1);
                Array.Reverse(data2);
            }

            return Convert.ToDouble(BitConverter.ToUInt32(data1, 0))
                / Convert.ToDouble(BitConverter.ToUInt32(data2, 0));
        }

        // 6 Signed BYTE
        public sbyte ReadSByte(bool byteOrder)
        {
            var data = base.ReadByte();
            return Convert.ToSByte(data);
        }

        // 7 UNDEFINED
        public byte[] ReadUndefined(int count, bool byteOrder)
        {
            var data = base.ReadBytes(count);
            if (byteOrder)
                Array.Reverse(data);
            return data;
        }

        // 8 Signed SHORT
        public short ReadSShort(bool byteOrder)
        {
            var data = base.ReadBytes(2);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        // 9 Signed LONG
        public Int32 ReadSLong(bool byteOrder)
        {
            var data = base.ReadBytes(4);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        // 10 Signed Rational
        public double ReadSRational(bool byteOrder)
        {
            var data1 = base.ReadBytes(4);
            var data2 = base.ReadBytes(4);
            if (byteOrder)
            {
                Array.Reverse(data1);
                Array.Reverse(data2);
            }

            return Convert.ToDouble(BitConverter.ToInt32(data1, 0))
                / Convert.ToDouble(BitConverter.ToInt32(data2, 0));
        }

        // 11 Single Float
        public float ReadFloat(bool byteOrder)
        {
            var data = base.ReadBytes(4);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }

        // 12 Double Float
        public double ReadDouble(bool byteOrder)
        {
            var data = base.ReadBytes(4);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }

        // ** Unsigned unsigned 64 Long
        public UInt64 ReadLong64(bool byteOrder)
        {
            var data = base.ReadBytes(8);
            if (byteOrder)
                Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

    }
}
