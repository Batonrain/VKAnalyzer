using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using VKAnalyzer.BusinessLogic.CohortAnalyser.Models;
using VKAnalyzer.Models.VKModels;
using VKAnalyzer.Models.VKModels.JsonModels;
using VKAnalyzer.Services.VK.Common;

namespace VKAnalyzer.Services.VK.CohortAndSale
{
    public class VkSalesAnalysisService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private VkAdsRequestService VkAdsRequestService { get; set; }
        private VkRequestService VkRequestService { get; set; }
        private VkUrlService VkUrlService { get; set; }

        public VkSalesAnalysisService()
        {
            VkAdsRequestService = new VkAdsRequestService();
            VkRequestService = new VkRequestService();
            VkUrlService = new VkUrlService();
        }

        public IEnumerable<VkAnalyseSalesResultModel> CreateRetargets(List<CohortAnalysisModel> posts, string accountId, string clientId, string excludeTargetGroupdId, string accessToken)
        {
            var results = new List<VkAnalyseSalesResultModel>();

            // Создание группы ретаргета
            var retargetJson = VkAdsRequestService.RequestJs(VkUrlService.CreateRetargetGroupUrl(accountId, clientId, accessToken));
            var retargetGroup = JsonConvert.DeserializeObject<AdsRetargetGroup>(retargetJson);

            for (var i = 0; i < posts.Count(); i = i + 30)
            {
                var items = posts.Skip(i).Take(30).ToList();
                var campaignIds = new List<string>();
                var adIds = new List<string>();

                foreach (var item in items)
                {
                    try
                    {
                        //Если при создании группы ретаргетинга в ней меньше 100 человек, мы добавляем совершенно левых, чтобы удовлетворять требуемым условиям
                        if (item.LikedIds.Count() < 100)
                        {
                            var needToAdd = 150 - item.LikedIds.Count();
                            item.LikedIds.AddRange(GetRandomUsers(needToAdd, accessToken));
                        }

                        var contacts = item.LikedIds.Aggregate(string.Empty, (current, id) => current + string.Format("{0},", id));

                        //Добавляем пользователей в группу ретаргета
                        var updatedGroup = VkAdsRequestService.RequestJs(VkUrlService.CreateImportRetargetContactsUrl(accountId, clientId, retargetGroup.Id, contacts, accessToken));

                        //Создаём рекламную кампанию
                        var campaignJson = VkAdsRequestService.RequestJs(VkUrlService.CreateCampaignUrl(accountId, clientId, string.Format("EvilMarkettingServiceForPost_Campaign_{0}", item.PostId), accessToken));
                        var campaign = JsonConvert.DeserializeObject<List<VkCampaignSuccess>>(JObject.Parse(campaignJson)["data"].ToString());

                        if (campaign != null && campaign.FirstOrDefault().ErrorCode != 0 && string.IsNullOrEmpty(campaign.FirstOrDefault().ErrorDesc))
                        {
                            continue;
                        }
                        var campaignId = campaign.FirstOrDefault().Id;

                        //Создаём рекламное объявление
                        var adsXml = VkAdsRequestService.Request(CreateAdUrl(accountId, campaignId.ToString(), retargetGroup.Id, string.Format("EM_Ad_{0}", item.PostId), accessToken));

                        if (adsXml.Descendants("id").FirstOrDefault() == null)
                        {
                            continue;
                        }
                        var adsId = adsXml.Descendants("id").FirstOrDefault().Value;

                        //Получаем охват текущей рекламной кампании
                        var adsInfo = VkAdsRequestService.Request(CreateGetAdsTargetingUrl(accountId, clientId, string.Format("[{0}]", adsId), accessToken));

                        if (adsInfo.Descendants("count").FirstOrDefault() == null)
                        {
                            continue;
                        }
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
                            result.Result = (Math.Abs(Convert.ToInt32(fCount) - Convert.ToInt32(uCount))).ToString();
                        }

                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error during foreach in CreateRetargets:");
                        _logger.Error(ex);
                        _logger.Error("Inner Exception:");
                        _logger.Error(ex.InnerException);
                    }
                }

                //DeleteAds(accountId, adIds, accessToken);
                //DeleteCampaigns(accountId, campaignIds, accessToken);
            }

            return results;
        }

        public string GetAccounts(string accessToken)
        {
            return VkAdsRequestService.GetAccounts(accessToken);
        }

        public string GetClients(string accountId, string accessToken)
        {
            return VkAdsRequestService.GetClients(accountId, accessToken);
        }

        private string UpdateAd(string accountId, string adId, string excludeTargetGroupId, string accessToken)
        {
            var json = JsonConvert.SerializeObject(new
            {
                ad_id = adId,
                retargeting_groups_not = Convert.ToInt32(excludeTargetGroupId),
            });

            return string.Format(
                "https://api.vk.com/api.php?oauth=1&method=ads.updateAds.xml&access_token={0}&account_id={1}&data={2}",
                accessToken, accountId, string.Format("[{0}]", json));
        }

        private void DeleteCampaigns(string accountId, IEnumerable<string> campaignIds, string accessToken)
        {
            VkAdsRequestService.DeleteCampaigns(accountId, GenerateJsonArray(campaignIds), accessToken);
        }

        private void DeleteAds(string accountId, IEnumerable<string> adsId, string accessToken)
        {
            VkAdsRequestService.DeleteAds(accountId, GenerateJsonArray(adsId), accessToken);
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
            var client = string.IsNullOrEmpty(clientId) ? string.Empty : string.Format("&client_id={0}", clientId);

            return string.Format(
                "https://api.vk.com/api.php?oauth=1&method=ads.getAdsTargeting.xml&access_token={0}&account_id={1}{2}&ad_ids={3}",
                accessToken, accountId, client, adsIds);
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