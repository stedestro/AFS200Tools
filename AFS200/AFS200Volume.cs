using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AFS200Tools.AFS200
{
    public class AFS200Volume
    {

        public struct AFS200VolumeHeader
        {
            public uint fileNumber { get; }
            public uint filesIndex { get; }

            public AFS200VolumeHeader(uint fileNumber, uint filesIndex)
            {
                this.fileNumber = fileNumber;
                this.filesIndex = filesIndex;
            }
        }

        public struct AFS200FileEntry
        {
            public ushort entryType { get; }
            public uint fileIndex { get; }
            public uint fileSize { get; }
            public uint fileSizeClone { get; }
            public string filename { get; }

            public AFS200FileEntry(byte[] bytearray)
            {
                entryType = BytesSwapping.SwapUint16(BitConverter.ToUInt16(bytearray, 2));
                fileIndex = BytesSwapping.SwapUint32(BitConverter.ToUInt32(bytearray, 4));
                fileSize = BytesSwapping.SwapUint32(BitConverter.ToUInt32(bytearray, 8));
                fileSizeClone = BytesSwapping.SwapUint32(BitConverter.ToUInt32(bytearray, 12));
                StringBuilder fn = new StringBuilder("");
                for (int i = 16; i < 36; i++)
                    if (bytearray[i] != 0x00)
                        fn.Append(Convert.ToChar(bytearray[i]));
                    else
                        break;
                filename = fn.ToString();
            }
        }

        private static readonly byte[] MAGIC = { 0x41, 0x46, 0x53, 0x5f, 0x56, 0x4f, 0x4c, 0x5f, 0x32, 0x30, 0x30, 0x0 };
        private static readonly ushort MAGICFILE = 0x4958;
        private static readonly int ENTRYSIZE = 36;

        private string filename;
        private AFS200VolumeHeader header;

        public AFS200Volume(string filename)
        {
            if (File.Exists(filename))
            {
                this.filename = filename;
                if (!CheckValidity())
                    throw new Exception("Not a valid AFS Volume 200 file");
                header = new AFS200VolumeHeader(GetFileNumber(), GetFiletableIndex());
            }
            else
                throw new Exception("File doesn't exists");
        }

        public bool CheckValidity()
        {
            using (BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                byte[] buffer = new byte[12];
                buffer = br.ReadBytes(12);
                if (buffer.SequenceEqual(MAGIC))
                    return true;
                else
                    return false;
            }
        }

        private uint GetFileNumber()
        {
            using (BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                br.BaseStream.Position = 0x0c;
                uint nb = br.ReadUInt32();
                return BytesSwapping.SwapUint32(nb);
            }
        }

        private uint GetFiletableIndex()
        {
            using (BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                br.BaseStream.Position = 0x10;
                uint nb = br.ReadUInt32();
                return BytesSwapping.SwapUint32(nb);
            }
        }

        public AFS200FileEntry GetFileEntry(int idx)
        {
            byte[] buffer = new byte[ENTRYSIZE];
            using (BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                br.BaseStream.Position = header.filesIndex + (ENTRYSIZE * idx);
                buffer = br.ReadBytes(ENTRYSIZE);
            }
            ushort magic = BytesSwapping.SwapUint16(BitConverter.ToUInt16(buffer, 0));
            if (magic != MAGICFILE)
                throw new Exception("Not a valid file entry. Index : " + idx.ToString() + " Position : " + (header.filesIndex + (ENTRYSIZE * idx)).ToString());
            AFS200FileEntry entry = new AFS200FileEntry(buffer);
            return entry;
        }

        public List<AFS200FileEntry> GetFileList()
        {
            List<AFS200FileEntry> filelist = new List<AFS200FileEntry>();

            for (int i = 0; i < header.fileNumber; i++)
                filelist.Add(GetFileEntry(i));
            return filelist;
        }

        public AFS200VolumeHeader GetHeader()
        {
            return header;
        }

        public void ExtractFileAtIndex(int index, string path)
        {
            AFS200FileEntry entry = GetFileEntry(index);
            if (entry.entryType == 2)
                throw new Exception("Extract file : entry is not a file but a directory");
            byte[] buffer = new byte[entry.fileSize];
            using (BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                br.BaseStream.Position = entry.fileIndex;
                buffer = br.ReadBytes((int)entry.fileSize);
            }
            using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                bw.Write(buffer);
            }
        }
    }

    public class BytesSwapping
    {
        public static uint SwapUint32(uint nb)
        {
            byte[] bytes = BitConverter.GetBytes(nb);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static ushort SwapUint16(ushort nb)
        {
            byte[] bytes = BitConverter.GetBytes(nb);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }
    }
}
