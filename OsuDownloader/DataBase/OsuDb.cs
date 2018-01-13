using OsuDownloader.Beatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.DataBase
{
    public class OsuDb : BeatmapDb
    {
        public int Version { get; }

        public int FolderCount { get; }

        public bool AccountLocked { get; }
        public DateTime AccountUnlock { get; }

        public string PlayerName { get; }

        public Dictionary<int, IBeatmapSet> BeatmapSets { get; }

        public int BeatmapCount { get => BeatmapSets.Count; }

        public OsuDb(int version,int folderCount,bool AccountLocked,DateTime AccountUnlock,
            string PlayerName,Dictionary<int, IBeatmapSet> beatmapSets)
        {
            BeatmapSets = beatmapSets;
            Version = version;
        }

        public bool HasBeatmap(int id)
        {
            return BeatmapSets.ContainsKey(id);
        }
    }
}
