using Newtonsoft.Json;
using OsuDownloader.Beatmap;
using OsuDownloader.IO;
using OsuDownloader.Server;
using OsuDownloader.Server.Mirror.Hexide;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsuDownloader.OsuDownloader.Server.Mirror.Hexide
{
    public class OsuHexideSearcher : ServerSearchProvider
    {
        private const string SEARCH_PATH = "search";
        private readonly string FULL_SEARCH_PATH = OsuHexide.URL + "/" + SEARCH_PATH;

        public BeatmapMirror Provider { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Tags { get; set; }
        public int Approved { get; set; }


        public OsuHexideSearcher(OsuHexide provider)
        {
            Provider = provider;
            Approved = 0;
        }

        private string FilterArgs
        {
            get => "maps.ranked_id";
        }

        private bool ApprovedLoved
        {
            get => (Approved & 0x10000) != 0;
            set
            {
                if (value)
                    Approved |= 0x10000;
                else
                    Approved &= 0x01111;
            }
        }

        private bool ApprovedRanked
        {
            get => (Approved & 0x01000) != 0;
            set
            {
                if (value)
                    Approved |= 0x01000;
                else
                    Approved &= 0x10111;
            }
        }

        private bool ApprovedApproved
        {
            get => (Approved & 0x00100) != 0;
            set
            {
                if (value)
                    Approved |= 0x00100;
                else
                    Approved &= 0x11011;
            }
        }

        private bool ApprovedQulified
        {
            get => (Approved & 0x00010) != 0;
            set
            {
                if (value)
                    Approved |= 0x00010;
                else
                    Approved &= 0x11101;
            }
        }

        private bool ApprovedPending
        {
            get => (Approved & 0x00001) != 0;
            set
            {
                if (value)
                    Approved |= 0x00001;
                else
                    Approved &= 0x11110;
            }
        }

        private bool ApprovedMultiple
        {
            get => (ApprovedLoved ? 1 : 0) +
                   (ApprovedApproved ? 1 : 0) +
                   (ApprovedRanked ? 1 : 0) +
                   (ApprovedQulified ? 1 : 0) +
                   (ApprovedPending ? 1 : 0) > 1;
        }

        private bool ApprovedAll
        {
            get => (Approved & 0x11111) == 0x11111;
        }

        private string TitleArgs
        {
            get
            {
                return "maps.title.like." + Title;
            }
        }

        private string ArtistArgs
        {
            get
            {
                return "metadata.m_artist.like." + Artist;
            }
        }

        private string TagsArgs
        {
            get
            {
                return "metadata.m_tags.like." + Tags;
            }
        }

        private string ApprovedArgs
        {
            get
            {
                char number;

                if (ApprovedLoved)
                    number = '4';
                else if (ApprovedRanked)
                    number = '1';
                else if (ApprovedApproved)
                    number = '2';
                else if (ApprovedQulified)
                    number = '3';
                else
                    number = '0';

                return "metadata.a_approved.eq." + number;
            }
        }

        private string SearchArgs {
            get => FilterArgs + "/" +
                (Title != null ? TitleArgs + "/" : "") +
                (Artist != null ? ArtistArgs + "/" : "") +
                (Tags != null ? TagsArgs + "/" : "") +
                (!ApprovedAll ? ApprovedArgs : "");
        }
        
        public List<IBeatmapSet> GetResult()
        {
            if (Title != null || Artist != null || Tags != null || !ApprovedAll)
            {
                List<IBeatmapSet> list = new List<IBeatmapSet>();
                if (ApprovedMultiple && !ApprovedAll)
                {
                    if (ApprovedLoved)
                        list.AddRange(new OsuHexideSearcher((OsuHexide)Provider) { Title = Title, Artist = Artist, Tags = Tags, ApprovedLoved = true }.GetResult());
                    if (ApprovedRanked)
                        list.AddRange(new OsuHexideSearcher((OsuHexide)Provider) { Title = Title, Artist = Artist, Tags = Tags, ApprovedRanked = true }.GetResult());
                    if (ApprovedApproved)
                        list.AddRange(new OsuHexideSearcher((OsuHexide)Provider) { Title = Title, Artist = Artist, Tags = Tags, ApprovedApproved = true }.GetResult());
                    if (ApprovedQulified)
                        list.AddRange(new OsuHexideSearcher((OsuHexide)Provider) { Title = Title, Artist = Artist, Tags = Tags, ApprovedQulified = true }.GetResult());
                    if (ApprovedPending)
                        list.AddRange(new OsuHexideSearcher((OsuHexide)Provider) { Title = Title, Artist = Artist, Tags = Tags, ApprovedPending = true }.GetResult());

                    return list.Distinct().ToList();
                }

                WebRequest req = WebRequest.CreateHttp(FULL_SEARCH_PATH + "/" + SearchArgs);

                try
                {
                    using (Stream stream = req.GetResponse().GetResponseStream())
                    {
                        foreach (IdObject rawSet in JsonUtil.ParseLargeJson<IdObject>(stream))
                        {
                            if (Provider.BeatmapSets.ContainsKey(rawSet.Ranked_id))
                                list.Add(Provider.BeatmapSets[rawSet.Ranked_id]);
                        }
                    }
                } catch (WebException e)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                        return list;
                    else
                        throw e;
                }
                return list;
            }
            else
                return new List<IBeatmapSet>(Provider.BeatmapSets.Values);
        }
    }

    struct IdObject
    {
        public int Ranked_id;
    }
}