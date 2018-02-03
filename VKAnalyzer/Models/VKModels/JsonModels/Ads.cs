﻿namespace VKAnalyzer.Models.VKModels.JsonModels
{
    public class AdsAccount
    {
        public string account_id { get; set; }
        public string account_type { get; set; }
        public int account_status { get; set; }
        public string account_name { get; set; }
        public string access_role { get; set; }
    }

    public class AdsClient
    {
        public string id { get; set; }
        public string name { get; set; }
        public int day_limits { get; set; }
        public string all_limits { get; set; }
    }

    public class AdsRetargetGroup
    {
        public string id { get; set; }
        public string name { get; set; }
        public string last_updated { get; set; }
        public string is_audience { get; set; }
        public string audience_count { get; set; }
        public string lifetime { get; set; }
        public string file_source { get; set; }
        public string api_source { get; set; }
        public string lookalike_source { get; set; }
        public string pixel { get; set; }
        public string domain { get; set; }
    }

    public class VkInterestCategory
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class VkCampaignSuccess
    {
        public int id { get; set; }

        public Error Error { get; set; }
    }

    public class VkAdSuccess
    {
        public int id { get; set; }

        public Error Error { get; set; }
    }

    public class VkAdTargetInfo
    {
        public int id { get; set; }
        public string campaign_id { get; set; }
        public int age_from { get; set; }
        public int age_to { get; set; }
        public string groups { get; set; }
        public int count { get; set; }

        public Error Error { get; set; }
    }

    public class Error
    {
        public int error_code { get; set; }
        public string error_msg { get; set; }
    }
}