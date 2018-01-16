using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Beatmap
{
    public class OsuBeatmap : IBeatmap
    {
        public string RankedName { get; }
        public string RankedNameUnicode { get; }

        public string Artist { get; }
        public string ArtistUnicode { get; }

        public List<string> Tags { get; }

        public int Id { get; }
        public int SetId { get; }

        public string DiffcultyName { get; }

        public string AudioFileName { get; }

        public string Creator { get; }

        public OsuBeatmap(string rankedName,string rankedNameUnicode,
            string artist,string artistUnicode,
            List<string> tags,
            string creator, string diffcultyName,string audioFileName,int id,int setId)
        {
            RankedName = rankedName;
            RankedNameUnicode = RankedNameUnicode;

            Artist = artist;
            ArtistUnicode = artistUnicode;

            Tags = tags;

            Creator = creator;
            DiffcultyName = diffcultyName;
            AudioFileName = audioFileName;
            Id = id;
            SetId = id;
        }
    }
}
