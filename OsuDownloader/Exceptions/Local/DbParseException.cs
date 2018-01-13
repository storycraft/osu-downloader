using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.Exceptions.Local
{
    class DbParseException : Exception
    {
        public DbParseException(string message) : base(message) { }
        public DbParseException(string message, System.Exception inner) : base(message, inner) { }

        protected DbParseException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }
}
