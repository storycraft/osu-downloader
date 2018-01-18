using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuDownloader.IO
{
    public static class JsonUtil
    {
        public static List<Result> ParseLargeJson<Result>(Stream stream)
        {
            List<Result> list = new List<Result>();

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
                            Result res = serializer.Deserialize<Result>(reader);

                            list.Add(res);
                        }
                    }
                }
            }

            return list;
        }
    }
}
