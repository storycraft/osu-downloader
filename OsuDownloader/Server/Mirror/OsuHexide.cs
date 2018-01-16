using System;
using OsuDownloader.Beatmap;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using OsuDownloader.Exceptions.Server;
using Newtonsoft.Json;

namespace OsuDownloader.Server.Mirror
{
    public class OsuHexide : BeatmapMirror
    {
        private readonly string URL = "https://osu.hexide.com";

        private readonly string DB_PATH = "beatmaps";

        private readonly string FILE_PATH = "download";
        private readonly string FILE_NOVID_PATH = "download/novid";

        public Dictionary<int, IBeatmapSet> BeatmapSets { get; }

        public int BeatmapSetCount { get => BeatmapSets.Count; }

        public string MirrorSite { get => URL; }

        public OsuHexide()
        {
            BeatmapSets = new Dictionary<int, IBeatmapSet>();

            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create($"{URL}/{DB_PATH}");
            request.UserAgent = Program.USER_AGENT;

            using (Stream stream = request.GetResponse().GetResponseStream())
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    using (JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        reader.SupportMultipleContent = true;

                        var serializer = new JsonSerializer();

                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                RawBeatmapSet rawSet = serializer.Deserialize<RawBeatmapSet>(reader);

                                OnlineBeatmapSet set = new OnlineBeatmapSet(rawSet.Title, rawSet.Type, rawSet.Ranked_id);
                                BeatmapSets[set.RankedID] = set;
                            }
                        }
                    }
                }
            }
        }

        public BeatmapSetFile DowmloadBeatmap(IBeatmapSet map, FileStream fileStream, bool perferNoVid)
        {
            if (!HasBeatmapSet(map.RankedID))
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

        public bool HasBeatmapSet(int id)
        {
            return BeatmapSets.ContainsKey(id);
        }
    }

    class RawBeatmapSet
    {
        public int Id { get; set; }
        public int Ranked_id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string Date { get; set; }
    }
}
