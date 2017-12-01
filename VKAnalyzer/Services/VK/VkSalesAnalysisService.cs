using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Models.VKModels.JsonModels;

namespace VKAnalyzer.Services.VK
{
    public class VkSalesAnalysisService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private VkAdsRequestService VkAdsRequestService { get; set; }
        private VkRequestService VkRequestService { get; set; }

        public VkSalesAnalysisService()
        {
            VkAdsRequestService = new VkAdsRequestService();
            VkRequestService = new VkRequestService();
        }

        

        private void CleanupRetargets(string accountId, string clientId, string accessToken)
        {
            var retargetsXml = VkAdsRequestService.Request(CreateCleanupRetargetGroupUrl(accountId, clientId, accessToken));

            var ids = retargetsXml.Descendants("id")
                .Where(s => s.Value.Contains("EvilMarketingServiceForPost"))
                .Select(s => s.Value)
                .ToList();

            DeleteTargetGroup(accountId, clientId, ids, accessToken);
        }

        public List<VkAnalyseSalesResultModel> CreateRetargets(List<CohortAnalysisModel> posts, string accountId, string clientId, string excludeTargetGroupdId, string accessToken)
        {
            //CleanupRetargets(accountId, clientId, accessToken);

            var results = new List<VkAnalyseSalesResultModel>();
            for (var i = 0; i < posts.Count(); i = i + 30)
            {
                var items = posts.Skip(i).Take(30).ToList();
                var campaignIds = new List<string>();
                var adIds = new List<string>();
                var retargetGroupsIds = new List<string>();

                foreach (var item in items)
                {
                    try
                    {
                        var retargetXml = VkAdsRequestService.Request(CreateRetargetGroupUrl(accountId, clientId, item.PostId, accessToken));

                        if (retargetXml.Descendants("id").FirstOrDefault() == null)
                        {
                            continue;
                        }
                        var retrgetId = retargetXml.Descendants("id").FirstOrDefault().Value;
                        retargetGroupsIds.Add(retrgetId);

                        //Если при создании группы ретаргетинга в ней меньше 100 человек, мы добавляем совершенно левых, чтобы удовлетворять требуемым условиям
                        if (item.LikedIds.Count() < 100)
                        {
                            var needToAdd = 150 - item.LikedIds.Count();
                            item.LikedIds.AddRange(GetRandomUsers(needToAdd, accessToken));
                        }

                        var contacts = item.LikedIds.Aggregate(string.Empty, (current, id) => current + string.Format("{0},", id));

                        //Добавляем пользователей в группу ретаргета
                        //VkAdsRequestService.UpdateRetargetGroup(accountId, clientId, retrgetId, contacts, accessToken);
                        VkAdsRequestService.Request(CreateImportRetargetContactsUrl(accountId, clientId, retrgetId, contacts, accessToken));

                        //Создаём рекламную кампанию
                        var campaignXml = VkAdsRequestService.Request(CreateCampaignUrl(accountId, clientId, string.Format("EvilMarkettingServiceForPost_Campaign_{0}", item.PostId), accessToken));

                        if (campaignXml.Descendants("id").FirstOrDefault() == null)
                        {
                            continue;
                        }
                        var campaignId = campaignXml.Descendants("id").FirstOrDefault().Value;
                        campaignIds.Add(campaignId);

                        //Создаём рекламное объявление
                        var adsXml = VkAdsRequestService.Request(CreateAdUrl(accountId, campaignId, retrgetId, string.Format("EM_Ad_{0}", item.PostId), accessToken));

                        if (adsXml.Descendants("id").FirstOrDefault() == null)
                        {
                            continue;
                        }
                        var adsId = adsXml.Descendants("id").FirstOrDefault().Value;

                        //Получаем охват текущей рекламной кампании
                        var adsInfo = VkAdsRequestService.Request(CreateGetAdsTargetingUrl(accountId, clientId, string.Format("[{0}]", adsId), accessToken));
                        
                        var fCount = adsInfo.Descendants("count").FirstOrDefault().Value;
                        adIds.Add(adsId);

                        //Добавляем в исключение выбранную пользователем группу ретаргета
                        var updatedInfo = VkAdsRequestService.Request(UpdateAd(accountId, adsId, excludeTargetGroupdId, accessToken));

                        var result = new VkAnalyseSalesResultModel
                        {
                            PostId = item.PostId,
                            Date = item.PostDate
                        };

                        if (updatedInfo.Descendants("error_code").Any())
                        {
                            //Плохая группа ретаргета и невозможно провести обновление
                            result.Result = "<100";
                        }
                        else
                        {
                            var updatedAdsInfo = VkAdsRequestService.Request(CreateGetAdsTargetingUrl(accountId, clientId, string.Format("[{0}]", adsId), accessToken));

                            if (updatedAdsInfo.Descendants("count").FirstOrDefault() == null)
                            {
                                continue;
                            }
                            var uCount = updatedAdsInfo.Descendants("count").FirstOrDefault().Value;
                            result.Result = (Convert.ToInt32(fCount) - Convert.ToInt32(uCount)).ToString();
                        }

                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }

                DeleteAds(accountId, adIds, accessToken);
                DeleteCampaigns(accountId, campaignIds, accessToken);
                DeleteTargetGroup(accountId, clientId, retargetGroupsIds, accessToken);
            }

            return results;
        }



        private string UpdateAd(string accountId, string adId, string excludeTargetGroupId, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                ad_id = adId,
                retargeting_groups_not = Convert.ToInt32(excludeTargetGroupId),
            });

            return String.Format(
                "https://api.vk.com/api.php?oauth=1&method=ads.updateAds.xml&access_token={0}&account_id={1}&data={2}",
                accessToken, accountId, string.Format("[{0}]", json));
        }

        private void DeleteTargetGroup(string accountId, string clientId, IEnumerable<string> retrgetIds, string accessToken)
        {
            foreach (var retargetId in retrgetIds)
            {
                VkAdsRequestService.DeleteTargetGroup(accountId, clientId, retargetId, accessToken);
            }
        }

        private void DeleteCampaigns(string accountId, IEnumerable<string> campaignIds, string accessToken)
        {
            VkAdsRequestService.DeleteCampaigns(accountId, GenerateJsonArray(campaignIds), accessToken);
        }

        private void DeleteAds(string accountId, IEnumerable<string> adsId, string accessToken)
        {
            VkAdsRequestService.DeleteAds(accountId, GenerateJsonArray(adsId), accessToken);
        }

        private string CreateRetargetGroupUrl(string accountId, string clientId, string name, string accessToken)
        {
            return string.Format("https://api.vk.com/api.php?oauth=1&method=ads.createTargetGroup.xml&account_id={0}&client_id={1}&name={2}&access_token={3}",
                                  accountId, clientId, string.Format("EvilMarketingServiceForPost_{0}", name), accessToken);
        }

        private string CreateCleanupRetargetGroupUrl(string accountId, string clientId, string accessToken)
        {
            return string.Format("https://api.vk.com/api.php?oauth=1&method=ads.getTargetGroups.xml&account_id={0}&client_id={1}&access_token={2}",
                                  accountId, clientId, accessToken);
        }

        private string CreateImportRetargetContactsUrl(string accountId, string clientId, string targetGroupId, string contacts, string accessToken)
        {
            return string.Format( "https://api.vk.com/api.php?oauth=1&method=ads.importTargetContacts.xml&access_token={0}&account_id={1}&client_id={2}&target_group_id={3}&contacts={4}",
                                   accessToken, accountId, clientId, targetGroupId, contacts);
        }

        private string CreateCampaignUrl(string accountId, string clientId, string name, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                client_id = clientId,
                type = "normal",
                name = name
            });

            return string.Format("https://api.vk.com/api.php?oauth=1&method=ads.createCampaigns.xml&access_token={0}&account_id={1}&data={2}",
                                  accessToken, accountId, json);
        }

        private string CreateAdUrl(string accountId, string campaignId, string targetGroupId, string name, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                campaign_id = campaignId,
                ad_format = 1,
                cost_type = 1,
                cpm = 6.00,
                link_url = "www.mysite.com",
                retargeting_groups = Convert.ToInt32(targetGroupId),
                title = name,
                photo = @"size:s|server:841422|photo_data:eyJrIjp7InZvbHVtZV9pZCI6Ijg0MTQyMjg3NiIsImxvY2FsX2lkIjoiMzgyMGYiLCJzZWNyZXQiOiJGaHJwelBySmxHcyIsIndpZHRoIjoxNDUsImhlaWdodCI6ODV9LCJsIjp7InZvbHVtZV9pZCI6Ijg0MTQyMjAzOCIsImxvY2FsX2lkIjoiM2Q0NjkiLCJzZWNyZXQiOiJKeEgzVHFQWDRMTSIsIndpZHRoIjoyOTAsImhlaWdodCI6MTcwfX0=|width:145|height:85|kid:5658351b3aac2f8717d72354bff2d437|hash:e80d823db405786d52607ee9c3ecad7a",
                description = name
            });

            return string.Format(
                    "https://api.vk.com/api.php?oauth=1&method=ads.createAds.xml&access_token={0}&account_id={1}&data={2}", accessToken, accountId, string.Format("[{0}]", json));
        }

        private string CreateGetAdsTargetingUrl(string accountId, string clientId, string adsIds, string accessToken)
        {
            return string.Format(
                "https://api.vk.com/api.php?oauth=1&method=ads.getAdsTargeting.xml&access_token={0}&account_id={1}&client_id={2}&ad_ids={3}",
                accessToken, accountId, clientId, adsIds);
        }

        public IEnumerable<AdsRetargetGroup> GetTargetsGroups(string accessToken)
        {
            var result = new List<AdsRetargetGroup>();

            var accountsJson = VkAdsRequestService.GetAccounts(accessToken);

            var accountsToDeserialize = GetJsonFromResponse(accountsJson);

            var accsDeserialized = JsonConvert.DeserializeObject<List<AdsAccount>>(accountsToDeserialize);

            foreach (var adsAccounts in accsDeserialized)
            {
                var clientsJson = VkAdsRequestService.GetClients(adsAccounts.account_id, accessToken);
                var clientsToDeserialize = GetJsonFromResponse(clientsJson);
                var clntsDeserialized = JsonConvert.DeserializeObject<List<AdsClient>>(clientsToDeserialize);

                foreach (var adsClients in clntsDeserialized)
                {
                    var targetGroupsJson = VkAdsRequestService.GetTargetsGroups(adsAccounts.account_id, adsClients.id,
                        accessToken);
                    var targetGroupsToDeserialize = GetJsonFromResponse(targetGroupsJson);
                    var targetGroupsDeserialized = JsonConvert.DeserializeObject<List<AdsRetargetGroup>>(targetGroupsToDeserialize);
                    var correctTargetGroups =
                        targetGroupsDeserialized.Where(g => !g.name.Contains("EvilMarketingServiceForPost"));
                    result.AddRange(correctTargetGroups);
                }
            }
            return result;
        }

        private string GenerateJsonArray(IEnumerable<string> values)
        {
            var array = new JArray();
            foreach (var value in values)
            {
                array.Add(new JValue(value));
            }
            return array.ToString();
        }

        private string GetJsonFromResponse(string json)
        {
            var parsed = JObject.Parse(json);
            return parsed["response"].ToString();
        }

        private IEnumerable<string> GetRandomUsers(int count, string accessToken)
        {
            var users = VkRequestService.GetRandomUsers("Колян", count, accessToken);
            return users.Descendants("uid").Select(x => x.Value).ToList();
        }
    }
}