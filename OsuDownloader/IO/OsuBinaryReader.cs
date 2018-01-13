using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.IO
{
    public class OsuBinaryReader : BinaryReader
    {
        public bool CanRead { get => base.BaseStream.CanRead; }
        public bool CanSeek { get => base.BaseStream.CanRead; }
        public bool CanWrite { get => base.BaseStream.CanRead; }

        public long Length { get => base.BaseStream.Length; }

        public long Position { get => base.BaseStream.Position; set => base.BaseStream.Position = value; }

        public OsuBinaryReader(Stream stream) : base(stream) { }
        public OsuBinaryReader(Stream stream, Encoding encoding) : base(stream, encoding) { }

        public byte[] ReadBytes()
        {
            int length = ReadInt32();
            return length > 0
                ? base.ReadBytes(length)
                : new byte[0];
        }

        public override string ReadString()
        {
            byte b = ReadByte();
            if (b == 0x0B)
            {
                return base.ReadString();
            }
            else if (b == 0x00)
                return null;
            else
                throw new Exception($"Type byte must 0x00 or 0x11, but readed 0x{b:X2} at {Position})");
        }

        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary(ReadInt64());
        }

        public Dictionary<int, double> ReadIntDoublePair()
        {
            int length = ReadInt32();
            Dictionary<int, double> mods = new Dictionary<int, double>();
            for (int i = 0; i < length; i++)
            {
                ReadByte();
                int key = ReadInt32();
                ReadByte();
                double value = ReadDouble();
                mods[key] = value;
            }
            return mods;
        }

        public List<TimingPoint> ReadTimingPointList()
        {
            List<TimingPoint> list = new List<TimingPoint>();
            int length = ReadInt32();
            for (int i = 0; i < length; i++) list.Add(TimingPoint.ReadFromReader(this));
            return list;
        }
    }
}