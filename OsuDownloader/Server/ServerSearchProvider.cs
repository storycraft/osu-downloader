using OsuDownloader.Beatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Server
{
    public interface ServerSearchProvider
    {
        BeatmapMirror Provider { get; set; }

        string Title { get; set; }

        string Artist { get; set; }

        string Tags { get; set; }

        int Approved { get; set; }

        List<IBeatmapSet> GetResult();
    }
}
