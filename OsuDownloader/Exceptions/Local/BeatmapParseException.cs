using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Exceptions.Local
{
    class BeatmapParseException : Exception
    {
        public BeatmapParseException(string message) : base(message) { }
        public BeatmapParseException(string message, System.Exception inner) : base(message, inner) { }

        protected BeatmapParseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
