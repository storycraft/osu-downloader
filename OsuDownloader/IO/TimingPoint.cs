using System.IO;

namespace OsuDownloader.IO
{
    public class TimingPoint
    {
        public double Time, MsPerQuarter;
        public bool TimingChange;

        public void WriteToWriter(BinaryWriter writer)
        {
            writer.Write(MsPerQuarter);
            writer.Write(Time);
            writer.Write(TimingChange);
        }

        public static TimingPoint ReadFromReader(BinaryReader r)
        {
            TimingPoint t = new TimingPoint
            {
                MsPerQuarter = r.ReadDouble(),
                Time = r.ReadDouble(),
                TimingChange = r.ReadByte() != 0
            };
            return t;
        }
    }
}
