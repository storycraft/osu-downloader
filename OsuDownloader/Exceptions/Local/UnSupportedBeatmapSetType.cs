using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.OsuDownloader.Exceptions.Local
{
    public class UnSupportedBeatmapSetType : Exception
    {
        public UnSupportedBeatmapSetType(string message) : base(message) { }
        public UnSupportedBeatmapSetType(string message, System.Exception inner) : base(message, inner) { }

        protected UnSupportedBeatmapSetType(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
