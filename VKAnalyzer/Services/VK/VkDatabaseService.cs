using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VKAnalyzer.Models.VKModels.JsonModels;
using VKAnalyzer.Services.Interfaces;

namespace VKAnalyzer.Services.VK
{
    public class VkDatabaseService : IVkDatabaseService
    {
        private const int SleepTime = 5000;

        public string GetCities()
        {
            throw new NotImplementedException();
        }

        public string GetUniversities()
        {
            throw new NotImplementedException();
        }

        public string GetInterestsCategories(string accessToken)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("https://api.vk.com/api.php?oauth=1&method=ads.getSuggestions&section=interest_categories&lang=ru&access_token={0}", accessToken);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);
                var parsed = JObject.Parse(json);

                return parsed["response"].ToString();
            }
        }
    }
}