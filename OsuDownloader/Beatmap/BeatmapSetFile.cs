using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Beatmap
{
    public class BeatmapSetFile : IDisposable
    {
        public IBeatmapSet BeatmapSet { get; private set; }
        public Stream Stream { get; private set; }

        public BeatmapSetFile(IBeatmapSet set, Stream stream)
        {
            BeatmapSet = set;
            Stream = stream;
        }

        public void Dispose()
        {
            Stream.Dispose();

            BeatmapSet = null;
            Stream = null;
        }
    }
}
