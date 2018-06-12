using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VKAnalyzer.Models.VKModels.JsonModels;

namespace VKAnalyzer.Services.VK.Common
{
    public class VkWallRequestService
    {
        public string Request(string requestString, bool sleepLong = false)
        {
            while (true)
            {
                using (var wc = new WebClient())
                {
                    var result = wc.DownloadData(requestString);
                    var json = Encoding.UTF8.GetString(result);

                    if (!json.Contains("error_code"))
                    {
                        return json;
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<List<Error>>(JObject.Parse(json)["response"].ToString()).FirstOrDefault();
                    }

                }
            }
        }
    }
}