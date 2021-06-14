using System.Collections.Generic;
using Newtonsoft.Json;

namespace brainKiller.API
{
    internal class wikisearchhelper
    {
        public class Response
        {
            [JsonProperty("batchcomplete")] public string Batchcomplete { get; set; }

            [JsonProperty("query")] public Query Query { get; set; }
        }

        public class Query
        {
            [JsonProperty("normalized")] public List<Normalized> Normalized { get; set; }

            [JsonProperty("pages")] public Dictionary<string, Page> Pages { get; set; }
        }

        public class Normalized
        {
            [JsonProperty("from")] public string From { get; set; }

            [JsonProperty("to")] public string To { get; set; }
        }

        public class Page
        {
            [JsonProperty("ns")] public long Ns { get; set; }

            [JsonProperty("title")] public string Title { get; set; }

            [JsonProperty("missing")] public string Missing { get; set; }

            [JsonProperty("extract")] public string Extract { get; set; }

            [JsonProperty("thumbnail")] public Thumbnail Thumbnail { get; set; }
        }

        public class Thumbnail
        {
            [JsonProperty("source")] public string source { get; set; }
        }

        public class WRoot
        {
            [JsonProperty("batchcomplete")] public string Batchcomplete;

            [JsonProperty("query")] public Query Query;
        }
    }
}