using OsuDownloader.Beatmap;
using OsuDownloader.DataBase;
using OsuDownloader.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsuDownloader
{
    public class BeatmapSearcher
    {
        private Regex TitleRegex { get; set; }

        public string TitleKeyword { set => TitleRegex = new Regex(value); }

        public BeatmapDb Provider { get; }

        public BeatmapSearcher(BeatmapDb provider)
        {
            Provider = provider;
        }

        public List<IBeatmapSet> Search()
        {
            List<IBeatmapSet> list = new List<IBeatmapSet>();

            Parallel.ForEach(Provider.BeatmapSets.Values,(IBeatmapSet set) =>
            {
                if (TitleRegex.IsMatch(set.RankedName))
                    list.Add(set);
            });

            return list;
        }
    }
}
