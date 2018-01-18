using OsuDownloader.Beatmap;
using System;
using System.Collections.Generic;

namespace OsuDownloader.Server
{
    public class OnlineBeatmapSet : IBeatmapSet
    {
        public string RankedName { get; }
        public int RankedID { get; }

        public string PackageType { get; }

        public Dictionary<int, Beatmap.IBeatmap> Beatmaps => throw new NotImplementedException();

        public string RankedNameUnicode => throw new NotImplementedException();

        public string Artist => throw new NotImplementedException();

        public string ArtistUnicode => throw new NotImplementedException();

        public List<string> Tags => throw new NotImplementedException();

        public OnlineBeatmapSet(string rankedName,string packageType,int rankedId)
        {
            RankedName = rankedName;
            PackageType = packageType;
            RankedID = rankedId;
        }

        public override bool Equals(Object obj)
        {
            return RankedID == ((IBeatmapSet)obj).RankedID;
        }
    }
}
