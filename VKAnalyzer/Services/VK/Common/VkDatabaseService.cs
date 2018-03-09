using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VKAnalyzer.Models.VKModels.JsonModels;

namespace VKAnalyzer.Services.VK
{
    public class VkDatabaseService
    {
        private const int SleepTime = 5000;
        private string BaseUrl { get; set; }

        public VkDatabaseService()
        {
            BaseUrl = string.Format("{0}&{1}", WebConfigurationManager.AppSettings["VkApiBaseUrl"],
                WebConfigurationManager.AppSettings["VkApiActualVersion"]);
        }

        public string GetCities(string accessToken, int country = 1)
        {
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("{0}&method=ads.getSuggestions&section=cities&country=1&lang=ru&access_token={1}", BaseUrl, accessToken);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);

                return json;
            }
        }

        public string GetUniversities(string accessToken, int country = 1, int city = 1)
        {
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("{0}&method=database.getUniversities&section=interest_categories&lang=ru&access_token={1}", BaseUrl, accessToken);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                var parsed = JObject.Parse(json);

                return parsed["response"].ToString();
            }
        }

        public string GetTargetGroups(string accountId, string clientId, string accessToken)
        {
            var client = string.IsNullOrEmpty(clientId) ? string.Empty : string.Format("&client_id={0}", clientId);
            var json = string.Empty;

            using (var wc = new WebClient())
            {
                var requestString = string.Format(
                    "{0}&method=ads.getTargetGroups&access_token={1}&account_id={2}{3}", BaseUrl,
                    accessToken, accountId, client);

                var data = wc.DownloadData(requestString);
                json = Encoding.UTF8.GetString(data);
            }

            var targetGroupsToDeserialize = GetJsonFromResponse(json);
            var targetGroupsDeserialized = JsonConvert.DeserializeObject<List<AdsRetargetGroup>>(targetGroupsToDeserialize);
            var correctTargetGroups =
                targetGroupsDeserialized.Where(g => !g.Name.Contains("EvilMarketing"));

            var result = JsonConvert.SerializeObject(new
            {
                response = correctTargetGroups
            });
            return result;
        }

        public string GetInterestsCategories(string accessToken)
        {
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("{0}&method=ads.getSuggestions&section=interest_categories&lang=ru&access_token={1}", BaseUrl, accessToken);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                var parsed = JObject.Parse(json);

                return parsed["response"].ToString();
            }
        }

        private string GetJsonFromResponse(string json)
        {
            var parsed = JObject.Parse(json);
            return parsed["response"].ToString();
        }
    }
}