using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Exceptions.Server
{
    class BeatmapNotFoundException : Exception
    {
        public BeatmapNotFoundException(string message) : base(message) { }
        public BeatmapNotFoundException(string message, System.Exception inner) : base(message, inner) { }

        protected BeatmapNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
