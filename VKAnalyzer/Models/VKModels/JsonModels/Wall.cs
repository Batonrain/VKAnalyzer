using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace VKAnalyzer.Models.VKModels.JsonModels
{
    public class Post
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("from_id")]
        public int FromId { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("marked_as_ads")]
        public bool MarkedAsAds { get; set; }

        [JsonProperty("post_type")]
        public string PostType { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("can_pin")]
        public string CanPin { get; set; }
    }

    public class Attachement
    {
    }


}