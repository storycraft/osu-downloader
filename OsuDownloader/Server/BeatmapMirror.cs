using OsuDownloader.Beatmap;
using System.Collections.Generic;
using System.IO;

namespace OsuDownloader.Server
{
    interface BeatmapMirror
    {
        string MirrorSite { get; }
        int BeatmapSetCount { get; }
        Dictionary<int, OnlineBeatmapSet> CachedList { get; }
        bool HasBeatmapSet(IBeatmapSet map);
        BeatmapSetFile DowmloadBeatmap(OnlineBeatmapSet map,FileStream location,bool perferNoVid);
    }
}
