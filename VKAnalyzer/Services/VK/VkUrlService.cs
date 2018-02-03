using System;
using Newtonsoft.Json;

namespace VKAnalyzer.Services.VK
{
    public class VkUrlService
    {
        public string CreateCampaignUrl(string accountId, string clientId, string name, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                client_id = clientId,
                type = "normal",
                name = name
            });

            return string.Format("https://api.vk.com/api.php?oauth=1&method=ads.createCampaigns&access_token={0}&account_id={1}&data={2}",
                                  accessToken, accountId, string.Format("[{0}]", json));
        }

        public string CreateAdUrl(string accountId, int campaignId, string accessToken, string name, string sex, int ageFrom, int ageUpTo,
                                   string groups, string excludedGroups, string country = "1", string cities = "2", string interestCategories = "")
        {
            if (ageFrom < 14)
            {
                ageFrom = 14;
            }
            if (ageUpTo < 14)
            {
                ageUpTo = 0;
            }

            var json = JsonConvert.SerializeObject(new
            {
                campaign_id = campaignId,
                ad_format = 1,
                cost_type = 1,
                cpm = 6.00,
                link_url = "www.mysite.com",
                title = name,
                photo = @"size:s|server:841422|photo_data:eyJrIjp7InZvbHVtZV9pZCI6Ijg0MTQyMjg3NiIsImxvY2FsX2lkIjoiMzgyMGYiLCJzZWNyZXQiOiJGaHJwelBySmxHcyIsIndpZHRoIjoxNDUsImhlaWdodCI6ODV9LCJsIjp7InZvbHVtZV9pZCI6Ijg0MTQyMjAzOCIsImxvY2FsX2lkIjoiM2Q0NjkiLCJzZWNyZXQiOiJKeEgzVHFQWDRMTSIsIndpZHRoIjoyOTAsImhlaWdodCI6MTcwfX0=|width:145|height:85|kid:5658351b3aac2f8717d72354bff2d437|hash:e80d823db405786d52607ee9c3ecad7a",
                description = name,
                sex = sex,
                age_from = ageFrom,
                age_to = ageUpTo,
                //country = country,
                //cities = cities,
                groups = groups ?? string.Empty,
                groups_not = excludedGroups ?? string.Empty,
                interest_categories = interestCategories
            });

            return string.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.createAds&access_token={0}&account_id={1}&data={2}", accessToken, accountId, string.Format("[{0}]", json));
        }

        public string CreateGetAdsTargetingUrl(string accountId, string clientId, string adsIds, string accessToken)
        {
            var client = string.IsNullOrEmpty(clientId) ? string.Empty : string.Format("&client_id={0}", clientId);

            return string.Format(
                "https://api.vk.com/api.php?oauth=1&method=ads.getAdsTargeting&access_token={0}&account_id={1}{2}&ad_ids={3}",
                accessToken, accountId, client, adsIds);
        }
    }
}