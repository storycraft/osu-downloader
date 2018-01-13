using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Beatmap
{
    public class OsuBeatmap : IBeatmap
    {
        public string Title { get; }

        public int Id { get; }
        public int SetId { get; }

        public string DiffcultyName { get; }

        public string AudioFileName { get; }

        public string Creator { get; }

        public OsuBeatmap(string title,string creator, string diffcultyName,string audioFileName,int id,int setId)
        {
            Title = title;
            Creator = creator;
            DiffcultyName = diffcultyName;
            AudioFileName = audioFileName;
            Id = id;
            SetId = id;
        }
    }
}
