using System.Collections.Generic;
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

        [JsonProperty("can_pin")]
        public CommentInfo Comments { get; set; }

        [JsonProperty("can_pin")]
        public LikeInfo Likes { get; set; }

        [JsonProperty("can_pin")]
        public RepostInfo Reposts { get; set; }

        [JsonProperty("can_pin")]
        public ViewInfo Views { get; set; }
    }

    public class CommentInfo
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("groups_can_post")]
        public bool GroupCanPost { get; set; }

        [JsonProperty("can_post")]
        public int CanPost { get; set; }
    }

    public class LikeInfo
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("user_likes")]
        public bool UserLikes { get; set; }

        [JsonProperty("can_like")]
        public int CanLike { get; set; }

        [JsonProperty("can_publish")]
        public int CanPublish { get; set; }
    }

    public class RepostInfo
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("user_reposted")]
        public int UserReposted { get; set; }
    }

    public class ViewInfo
    {
        [JsonProperty("count")]
        public int Count { get; set; }

    }

    public class Repost
    {
        [JsonProperty("items")]
        public List<RepostItem> Items { get; set; }

        [JsonProperty("profiles")]
        public List<RepostItem> Profiles { get; set; }
    }

    public class RepostItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        [JsonProperty("from_id")]
        public int FromId { get; set; }

        [JsonProperty("created_by ")]
        public int CreatedBy { get; set; }

        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Profile
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("sex ")]
        public int Sex { get; set; }

        [JsonProperty("screen_name")]
        public int ScreenName { get; set; }

        [JsonProperty("photo_50")]
        public string PhotoSmall { get; set; }

        [JsonProperty("photo_100")]
        public string PhotoBig { get; set; }

        [JsonProperty("online")]
        public int Online { get; set; }
    }

    public class Like
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}