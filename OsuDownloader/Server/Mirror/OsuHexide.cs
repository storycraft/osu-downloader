using System;
using OsuDownloader.Beatmap;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using OsuDownloader.Exceptions.Server;

namespace OsuDownloader.Server.Mirror
{
    class OsuHexide : BeatmapMirror
    {
        private readonly string URL = "https://osu.hexide.com";

        private readonly string DB_PATH = "beatmaps";

        private readonly string FILE_PATH = "download";
        private readonly string FILE_NOVID_PATH = "download/novid";

        public Dictionary<int, OnlineBeatmapSet> CachedList { get; }

        public int BeatmapSetCount { get => CachedList.Count; }

        public string MirrorSite { get => URL; }

        public OsuHexide()
        {
            CachedList = new Dictionary<int, OnlineBeatmapSet>();

            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create($"{URL}/{DB_PATH}");
            request.UserAgent = Program.USER_AGENT;

            using (Stream stream = request.GetResponse().GetResponseStream())
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);

                    byte[] data = memStream.ToArray();
                    JArray rawArray = JArray.Parse(new string(Encoding.UTF8.GetChars(data)));

                    for (int i = rawArray.Count - 1; i >= 0; i--)
                    {
                        JToken rawMapSet = rawArray[i];
                        int rankedId = rawMapSet["ranked_id"].Value<int>();
                        string rankedName = rawMapSet["title"].Value<string>();

                        string packageType = rawMapSet["type"].Value<string>();

                        OnlineBeatmapSet set = new OnlineBeatmapSet(rankedName, packageType, rankedId);
                        CachedList[set.RankedID] = set;
                    }

                    GC.SuppressFinalize(rawArray);
                    //to prevent memory leak
                }
            }
        }

        public BeatmapSetFile DowmloadBeatmap(OnlineBeatmapSet map, FileStream fileStream, bool perferNoVid)
        {
            if (!HasBeatmapSet(map))
                throw new BeatmapNotFoundException($"Beatmap set ${map.RankedID} ${map.RankedName} not found");

            string fileName = map.RankedID + " " + map.RankedName;

            HttpWebRequest request;
            if (perferNoVid)
                request = (HttpWebRequest)HttpWebRequest.Create($"{URL}/{DB_PATH}/{map.RankedID}/{FILE_NOVID_PATH}/{fileName}");
            else
                request = (HttpWebRequest)HttpWebRequest.Create($"{URL}/{DB_PATH}/{map.RankedID}/{FILE_PATH}/{fileName}");
            request.UserAgent = Program.USER_AGENT;

            Stream stream = request.GetResponse().GetResponseStream();

            stream.CopyTo(fileStream);

            return new BeatmapSetFile(map, fileStream);
        }

        public bool HasBeatmapSet(IBeatmapSet map)
        {
            return CachedList.ContainsKey(map.RankedID);
        }
    }
}
