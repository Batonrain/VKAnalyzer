using System;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace VKAnalyzer.Services.VK
{
    public class VkDatabaseService
    {
        private const int SleepTime = 5000;

        public string GetCities(string accessToken, int country = 1)
        {
            Thread.Sleep(SleepTime);
            using (var wc = new WebClient())
            {
                var requestUrl = String.Format("https://api.vk.com/api.php?oauth=1&method=ads.getSuggestions&section=cities&country=1&lang=ru&access_token={0}", accessToken);
                var result = wc.DownloadData(requestUrl);
                var json = Encoding.UTF8.GetString(result);

                return json;
            }
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