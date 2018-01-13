using OsuDownloader.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Net.Http;
using OsuDownloader.Beatmap;
using OsuDownloader.Exceptions.Local;

namespace OsuDownloader.DataBase
{
    public static class OsuDbReader
    {
        public static OsuDb ParseFromStream(FileStream dbStream)
        {
            try
            {
                using (OsuBinaryReader reader = new OsuBinaryReader(dbStream))
                {
                    Dictionary<int, IBeatmapSet> beatmapSets = new Dictionary<int, IBeatmapSet>();
                    int ver = reader.ReadInt32();

                    bool old = ver < 20140609;

                    int folderCount = reader.ReadInt32();

                    bool locked = reader.ReadBoolean();
                    DateTime unlockDate = reader.ReadDateTime();

                    string playerName = reader.ReadString();

                    int beatmapCount = reader.ReadInt32();

                    for (int i = 0; i < beatmapCount; i++)
                    {
                        IBeatmap info = BeatmapParser.ParseFromReader(reader, old);
                        IBeatmapSet set;

                        if (beatmapSets.ContainsKey(info.SetId))
                            set = beatmapSets[info.SetId];
                        else
                        {
                            set = new OsuBeatmapSet(info.Title, info.SetId, "binary");
                            beatmapSets[set.RankedID] = set;
                        }

                        set.Beatmaps[info.Id] = info;
                    }
                    reader.ReadByte();//last Point
                    return new OsuDb(ver, folderCount, locked, unlockDate, playerName, beatmapSets);
                }
            } catch (Exception e)
            {
                throw new BeatmapParseException($"Failed to parse Db {dbStream.Name}",e);
            }
        }

        private static class BeatmapParser
        {
            public static IBeatmap ParseFromReader(OsuBinaryReader reader,bool old)
            {
                int totalSize = reader.ReadInt32();

                string artistName = reader.ReadString();
                string artistNameUnicode = reader.ReadString();

                string title = reader.ReadString();
                string titleUnicode = reader.ReadString();

                string creator = reader.ReadString();

                string diffcultyName = reader.ReadString();

                string audioFileName = reader.ReadString();

                string md5Hash = reader.ReadString();

                string osuFileName = reader.ReadString();

                byte rankedStatus = reader.ReadByte();

                short hitCircles = reader.ReadInt16();
                short sliders = reader.ReadInt16();
                short spinners = reader.ReadInt16();

                long lastModificationWT = reader.ReadInt64();

                float ar,cs,hp,od;
                if (old)
                {
                    ar = reader.ReadByte();
                    cs = reader.ReadByte();
                    hp = reader.ReadByte();
                    od = reader.ReadByte();
                }
                else
                {
                    ar = reader.ReadSingle();
                    cs = reader.ReadSingle();
                    hp = reader.ReadSingle();
                    od = reader.ReadSingle();
                }

                double sliderVelocity = reader.ReadDouble();

                Dictionary<int, double> osuStarRate, taikoStarRate, ctbStarRate, maniaStarRate;
                if (!old)
                {
                    osuStarRate = reader.ReadIntDoublePair();
                    taikoStarRate = reader.ReadIntDoublePair();
                    ctbStarRate = reader.ReadIntDoublePair();
                    maniaStarRate = reader.ReadIntDoublePair();
                }

                int drainTime = reader.ReadInt32();
                int totalTime = reader.ReadInt32();
                int audioPreviewTime = reader.ReadInt32();

                List<TimingPoint> timingPoints = reader.ReadTimingPointList();

                int beatmapId = reader.ReadInt32();
                int beatmapSetId = reader.ReadInt32();
                int threadId = reader.ReadInt32();

                byte topOsuGrade = reader.ReadByte();
                byte topTaikoGrade = reader.ReadByte();
                byte topCtbGrade = reader.ReadByte();
                byte topManiaGrade = reader.ReadByte();

                short beatmapOffset = reader.ReadInt16();

                float stackLeniency = reader.ReadSingle();

                byte gamePlayerMode = reader.ReadByte();
                string songSource = reader.ReadString();
                string songTag = reader.ReadString();

                short onlineOffset = reader.ReadInt16();
                string songFont = reader.ReadString();
                bool isUnplayed = reader.ReadBoolean();

                long lastPlayer = reader.ReadInt64();
                bool isOsz2 = reader.ReadBoolean();

                string folderName = reader.ReadString();
                long lastCheck = reader.ReadInt64();

                bool ignoreBeatmapSound = reader.ReadBoolean();
                bool ignoreBeatmapSkin = reader.ReadBoolean();
                bool disableStoryboard = reader.ReadBoolean();
                bool disableVideo = reader.ReadBoolean();
                bool visualOverride = reader.ReadBoolean();

                short? unknown1;
                if (old)
                    unknown1 = reader.ReadInt16();

                int lastModification = reader.ReadInt32();

                byte maniaScrollSpeed = reader.ReadByte();

                return new OsuBeatmap(title,creator,diffcultyName,audioFileName,beatmapId,beatmapSetId);
            }
        }
    }
}
