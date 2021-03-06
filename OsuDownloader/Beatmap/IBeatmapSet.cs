﻿using OsuDownloader.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Beatmap
{
    public interface IBeatmapSet
    {
        string RankedName { get; }
        string RankedNameUnicode { get; }

        string Artist { get; }
        string ArtistUnicode { get; }

        List<string> Tags { get; }

        string PackageType { get; }

        int RankedID { get; }

        Dictionary<int,IBeatmap> Beatmaps { get; }
    }
}
