using OsuDownloader.Beatmap;
using OsuDownloader.DataBase;
using System.Collections.Generic;
using System.IO;

namespace OsuDownloader.Server
{
    public interface BeatmapMirror : BeatmapDb
    {
        string MirrorSite { get; }
        int BeatmapSetCount { get; }

        BeatmapSetFile DowmloadBeatmap(IBeatmapSet map,FileStream location,bool perferNoVid);
    }
}
