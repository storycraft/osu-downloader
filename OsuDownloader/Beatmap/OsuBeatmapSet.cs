using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsuDownloader.IO;

namespace OsuDownloader.Beatmap
{
    public class OsuBeatmapSet : IBeatmapSet
    {
        public string RankedName { get; }
        public string RankedNameUnicode { get; }

        public string Artist { get; }
        public string ArtistUnicode { get; }

        public List<string> Tags { get; }

        public string PackageType { get; }

        public int RankedID { get; }

        public Dictionary<int, IBeatmap> Beatmaps { get; }

        public OsuBeatmapSet(string rankedName, string rankedNameUnicode,
            string artist,string artistUnicode,
            List<string> tags,
            int rankedId,string packageType)
        {
            Beatmaps = new Dictionary<int, IBeatmap>();
            RankedName = rankedName;
            RankedID = rankedId;
            PackageType = packageType;
        }
    }
}
