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
        int SetId { get; }
        string Title { get; }
        string Creator { get; }
        string DiffcultyName { get; }
        string AudioFileName { get; }
    }
}
