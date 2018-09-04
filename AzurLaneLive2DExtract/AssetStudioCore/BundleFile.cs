using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lz4;

namespace AssetStudioCore
{
    public class StreamFile
    {
        public string fileName;
        public Stream stream;
    }

    public class BlockInfo
    {
        public uint compressedSize;
        public uint uncompressedSize;
        public short flag;
    }

    public class BundleFile
    {
        private string path;
        public string versionPlayer;
        public string versionEngine;
        public List<StreamFile> fileList = new List<StreamFile>();

        public BundleFile(string path, EndianBinaryReader bundleReader)
        {
            this.path = path;
            var signature = bundleReader.ReadStringToNull();
            switch (signature)
            {
                case "UnityFS":
                    {
                        var format = bundleReader.ReadInt32();
                        versionPlayer = bundleReader.ReadStringToNull();
                        versionEngine = bundleReader.ReadStringToNull();
                        if (format == 6)
                        {
                            ReadFormat6(bundleReader);
                        }
                        break;
                    }
            }
        }

        private void ReadFormat6(EndianBinaryReader bundleReader, bool padding = false)
        {
            var bundleSize = bundleReader.ReadInt64();
            int compressedSize = bundleReader.ReadInt32();
            int uncompressedSize = bundleReader.ReadInt32();
            int flag = bundleReader.ReadInt32();
            if (padding)
                bundleReader.ReadByte();
            byte[] blocksInfoBytes;
            if ((flag & 0x80) != 0)//at end of file
            {
                var position = bundleReader.Position;
                bundleReader.Position = bundleReader.BaseStream.Length - compressedSize;
                blocksInfoBytes = bundleReader.ReadBytes(compressedSize);
                bundleReader.Position = position;
            }
            else
            {
                blocksInfoBytes = bundleReader.ReadBytes(compressedSize);
            }
            MemoryStream blocksInfoStream;
            switch (flag & 0x3F)
            {
                default://None
                    {
                        blocksInfoStream = new MemoryStream(blocksInfoBytes);
                        break;
                    }
                case 1://LZMA
                    {
                        blocksInfoStream = SevenZipHelper.StreamDecompress(new MemoryStream(blocksInfoBytes));
                        break;
                    }
                case 2://LZ4
                case 3://LZ4HC
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (var decoder = new Lz4DecoderStream(new MemoryStream(blocksInfoBytes)))
                        {
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                        }
                        blocksInfoStream = new MemoryStream(uncompressedBytes);
                        break;
                    }
                    //case 4:LZHAM?
            }
            using (var blocksInfoReader = new EndianBinaryReader(blocksInfoStream))
            {
                blocksInfoReader.Position = 0x10;
                int blockcount = blocksInfoReader.ReadInt32();
                var blockInfos = new BlockInfo[blockcount];
                for (int i = 0; i < blockcount; i++)
                {
                    blockInfos[i] = new BlockInfo
                    {
                        uncompressedSize = blocksInfoReader.ReadUInt32(),
                        compressedSize = blocksInfoReader.ReadUInt32(),
                        flag = blocksInfoReader.ReadInt16()
                    };
                }
                Stream dataStream;
                var uncompressedSizeSum = blockInfos.Sum(x => x.uncompressedSize);
                if (uncompressedSizeSum > int.MaxValue)
                {
                    /*var memoryMappedFile = MemoryMappedFile.CreateNew(Path.GetFileName(path), uncompressedSizeSum);
                    assetsDataStream = memoryMappedFile.CreateViewStream();*/
                    dataStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                }
                else
                {
                    dataStream = new MemoryStream();
                }
                foreach (var blockInfo in blockInfos)
                {
                    switch (blockInfo.flag & 0x3F)
                    {
                        default://None
                            {
                                bundleReader.BaseStream.CopyTo(dataStream, blockInfo.compressedSize);
                                break;
                            }
                        case 1://LZMA
                            {
                                SevenZipHelper.StreamDecompress(bundleReader.BaseStream, dataStream, blockInfo.compressedSize, blockInfo.uncompressedSize);
                                break;
                            }
                        case 2://LZ4
                        case 3://LZ4HC
                            {
                                var lz4Stream = new Lz4DecoderStream(bundleReader.BaseStream, blockInfo.compressedSize);
                                lz4Stream.CopyTo(dataStream, blockInfo.uncompressedSize);
                                break;
                            }
                            //case 4:LZHAM?
                    }
                }
                dataStream.Position = 0;
                using (dataStream)
                {
                    var entryinfo_count = blocksInfoReader.ReadInt32();
                    for (int i = 0; i < entryinfo_count; i++)
                    {
                        var file = new StreamFile();
                        var entryinfo_offset = blocksInfoReader.ReadInt64();
                        var entryinfo_size = blocksInfoReader.ReadInt64();
                        flag = blocksInfoReader.ReadInt32();
                        file.fileName = Path.GetFileName(blocksInfoReader.ReadStringToNull());
                        if (entryinfo_size > int.MaxValue)
                        {
                            /*var memoryMappedFile = MemoryMappedFile.CreateNew(file.fileName, entryinfo_size);
                            file.stream = memoryMappedFile.CreateViewStream();*/
                            var extractPath = path + "_unpacked\\";
                            Directory.CreateDirectory(extractPath);
                            file.stream = File.Create(extractPath + file.fileName);
                        }
                        else
                        {
                            file.stream = new MemoryStream();
                        }
                        dataStream.Position = entryinfo_offset;
                        dataStream.CopyTo(file.stream, entryinfo_size);
                        file.stream.Position = 0;
                        fileList.Add(file);
                    }
                }
            }
        }
    }
}
