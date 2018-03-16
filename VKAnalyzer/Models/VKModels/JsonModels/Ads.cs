using System.Collections.Generic;
using Newtonsoft.Json;

namespace VKAnalyzer.Models.VKModels.JsonModels
{
    public class AdsAccount
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("account_type")]
        public string AccountType { get; set; }

        [JsonProperty("account_status")]
        public int AccountStatus { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("access_role")]
        public string AccessRole { get; set; }
    }

    public class AdsClient
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("day_limits")]
        public int DayLimits { get; set; }

        [JsonProperty("all_limits")]
        public string AllLimits { get; set; }
    }

    public class AdsRetargetGroup : Error
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; }

        [JsonProperty("is_audience")]
        public string IsAudience { get; set; }

        [JsonProperty("audience_count")]
        public string AudienceCount { get; set; }

        [JsonProperty("lifetime")]
        public string Lifetime { get; set; }

        [JsonProperty("file_source")]
        public string FileSource { get; set; }

        [JsonProperty("api_source")]
        public string ApiSource { get; set; }

        [JsonProperty("lookalike_source")]
        public string LookalikeSource { get; set; }

        [JsonProperty("pixel")]
        public string Pixel { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
    }

    public class VkAllInterestCategory
    {
        public List<VkInterestCategory> v1 { get; set; }

        public List<VkInterestCategory> v2 { get; set; }
    }

    public class VkInterestCategory
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class VkInterestSubcategory
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class VkCampaignSuccess
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_desc")]
        public string ErrorDesc { get; set; }
    }

    public class VkTargetGroupSuccess
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_desc")]
        public string ErrorDesc { get; set; }
    }

    public class VkAdSuccess
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_desc")]
        public string ErrorDesc { get; set; }
    }

    public class VkAdTargetInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("campaign_id")]
        public string CampaignId { get; set; }

        [JsonProperty("age_from")]
        public int AgeFrom { get; set; }

        [JsonProperty("age_to")]
        public int AgeTo { get; set; }

        [JsonProperty("groups")]
        public string Groups { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_desc")]
        public string ErrorDesc { get; set; }
    }

    public class VkAdTargeting
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("campaign_id")]
        public string CampaignId { get; set; }

        [JsonProperty("country")]
        public int Country { get; set; }

        [JsonProperty("cities")]
        public int Cities { get; set; }

        [JsonProperty("cities_not")]
        public string CitiesNot { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("statuses")]
        public int Statuses { get; set; }

        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_desc")]
        public string ErrorDesc { get; set; }
    }

    public class Error
    {
        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_desc")]
        public string ErrorDesc { get; set; }
    }
}