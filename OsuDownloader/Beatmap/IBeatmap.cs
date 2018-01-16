using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Beatmap
{
    public interface IBeatmap
    {
        int Id { get; }

        string RankedNameUnicode { get; }

        string Artist { get; }
        string ArtistUnicode { get; }

        List<string> Tags { get; }

        int SetId { get; }
        string RankedName { get; }
        string Creator { get; }
        string DiffcultyName { get; }
        string AudioFileName { get; }
    }
}
