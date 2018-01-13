using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.IO
{
    public class OsuBinaryWriter : BinaryWriter
    {
        public bool CanRead { get => base.BaseStream.CanRead; }
        public bool CanSeek { get => base.BaseStream.CanRead; }
        public bool CanWrite { get => base.BaseStream.CanRead; }

        public long Length { get => base.BaseStream.Length; }

        public long Position { get => base.BaseStream.Position; set => base.BaseStream.Position = value; }

        public OsuBinaryWriter(Stream stream) : base(stream) { }
        public OsuBinaryWriter(Stream stream, Encoding encoding) : base(stream, encoding) { }

        public void WriteBytes(byte[] bytes)
        {
            int length = bytes.Length;
            base.Write(length);
            base.Write(bytes);
        }

        public void WriteString(string str)
        {
            if (str != null)
            {
                base.Write(0x0b);
                base.Write(str);
            }
            else
                base.Write(0x00);
        }

        public void WriteDateTime(DateTime time)
        {
            base.Write(time.ToBinary());
        }

        public void WriteIntDoublePair(Dictionary<int, double> mods)
        {
            base.Write(mods.Count);
            foreach (int i in mods.Keys)
            {
                base.Write(0x08);
                base.Write(i);
                base.Write(0x0d);
                base.Write(mods[i]);
            }
        }

        public void WriteTimingPointList(List<TimingPoint> list)
        {
            base.Write(list.Count);
            foreach (TimingPoint point in list)
                point.WriteToWriter(this);
        }
    }
}
