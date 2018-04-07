using System.Collections.Generic;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace VKAnalyzer.Services.VK.Common
{
    public class VkUrlService
    {
        private string BaseUrl { get; set; }

        public VkUrlService()
        {
            BaseUrl = string.Format("{0}&{1}", WebConfigurationManager.AppSettings["VkApiBaseUrl"],
                WebConfigurationManager.AppSettings["VkApiActualVersion"]);
        }

        public string CreateCampaignUrl(string accountId, string clientId, string name, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                client_id = clientId,
                type = "normal",
                name = name
            });

            return string.Format("{0}&method=ads.createCampaigns&access_token={1}&account_id={2}&data={3}",
                                  BaseUrl, accessToken, accountId, string.Format("[{0}]", json));
        }

        public string CreateAdUrl(string accountId, int campaignId, string accessToken, string name, string sex, int ageFrom, int ageUpTo, string status,
                                  string groups, string excludedGroups,string country, string cities, string excludedCities, string interestCategories,
                                  string retargetGroups, string excludedRetargetGroups)
        {
            if (ageFrom < 14)
            {
                ageFrom = 14;
            }
            if (ageUpTo < 14 || ageUpTo > 80)
            {
                ageUpTo = 80;
            }
            if (status == null || status == "0")
            {
                status = string.Empty;
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
                age_to = ageUpTo ,
                statuses = status,
                country = country ?? string.Empty,
                cities = cities ?? string.Empty,
                cities_not = excludedCities ?? string.Empty,
                groups = groups ?? string.Empty,
                groups_not = excludedGroups ?? string.Empty,
                interest_categories = interestCategories,
                retargeting_groups = retargetGroups ?? string.Empty,
                retargeting_groups_not = excludedRetargetGroups ?? string.Empty
            });
            
            return string.Format(
                    "{0}&method=ads.createAds&access_token={1}&account_id={2}&data={3}", BaseUrl, accessToken, accountId, string.Format("[{0}]", json));
        }

        public string CreateAdUrl(string accountId, string campaignId, string targetGroupId, string name, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                campaign_id = campaignId,
                ad_format = 1,
                cost_type = 1,
                cpm = 6.00,
                link_url = "www.mysite.com",
                retargeting_groups = string.IsNullOrEmpty(targetGroupId) ? string.Empty : targetGroupId,
                title = name,
                photo = @"size:s|server:841422|photo_data:eyJrIjp7InZvbHVtZV9pZCI6Ijg0MTQyMjg3NiIsImxvY2FsX2lkIjoiMzgyMGYiLCJzZWNyZXQiOiJGaHJwelBySmxHcyIsIndpZHRoIjoxNDUsImhlaWdodCI6ODV9LCJsIjp7InZvbHVtZV9pZCI6Ijg0MTQyMjAzOCIsImxvY2FsX2lkIjoiM2Q0NjkiLCJzZWNyZXQiOiJKeEgzVHFQWDRMTSIsIndpZHRoIjoyOTAsImhlaWdodCI6MTcwfX0=|width:145|height:85|kid:5658351b3aac2f8717d72354bff2d437|hash:e80d823db405786d52607ee9c3ecad7a",
                description = name
            });

            return string.Format(
                    "{0}&method=ads.createAds&access_token={1}&account_id={2}&data={3}", BaseUrl, accessToken, accountId, string.Format("[{0}]", json));
        }

        public string UpdateAd(string accountId, string adId, string excludeTargetGroupId, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                ad_id = adId,
                retargeting_groups_not = excludeTargetGroupId,
            });

            return string.Format(
                "{0}&method=ads.updateAds&access_token={1}&account_id={2}&data={3}", BaseUrl,
                accessToken, accountId, string.Format("[{0}]", json));
        }

        public string CreateGetAdsTargetingUrl(string accountId, string clientId, string adsIds, string accessToken)
        {
            var client = string.IsNullOrEmpty(clientId) ? string.Empty : string.Format("&client_id={0}", clientId);

            return string.Format(
                "{0}&method=ads.getAdsTargeting&access_token={1}&account_id={2}{3}&ad_ids={4}",
                BaseUrl, accessToken, accountId, client, adsIds);
        }

        public string CreateRetargetGroupUrl(string accountId, string clientId, string accessToken, string postId)
        {
            var client = string.IsNullOrEmpty(clientId) ? string.Empty : string.Format("&client_id={0}", clientId);

            return string.Format("{0}&method=ads.createTargetGroup&account_id={1}{2}&name={3}&access_token={4}", BaseUrl,
                                  accountId, client, string.Format("EM_SalesAnalyse_RG_{0}", postId), accessToken);
        }

        public string CreateImportRetargetContactsUrl(string accountId, string clientId, string targetGroupId, string contacts, string accessToken)
        {
            var client = string.IsNullOrEmpty(clientId) ? string.Empty : string.Format("&client_id={0}", clientId);

            return string.Format("{0}&method=ads.importTargetContacts&access_token={1}&account_id={2}{3}&target_group_id={4}&contacts={5}", BaseUrl,
                                   accessToken, accountId, client, targetGroupId, contacts);
        }

        public string CreateDeleteRetargetGroupUrl(string accountId, string clientId, string targetGroup, string accessToken)
        {
            var client = string.IsNullOrEmpty(clientId) ? string.Empty : string.Format("&client_id={0}", clientId);

            return string.Format(
                    "{0}&method=ads.deleteTargetGroup&access_token={1}&account_id={2}{3}&target_group_id={4}", BaseUrl,
                    accessToken, accountId, client, targetGroup);
        }

        public string CreateDeleteAdsUrl(string accountId, IEnumerable<int> ids, string accessToken)
        {
            var jsonArray = JsonConvert.SerializeObject(ids);

            return string.Format("{0}&method=ads.deleteAds&access_token={1}&account_id={2}&ids={3}",
                                  BaseUrl, accessToken, accountId, jsonArray);
        }
    }
}