using OsuDownloader.Beatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.DataBase
{
    public interface BeatmapDb
    {
        Dictionary<int, IBeatmapSet> BeatmapSets { get; }
        bool HasBeatmap(int id);

        
    }
}
