using System;
using System.Collections.Generic;
using System.Linq;

namespace VKAnalyzer.Services.VK
{
    public class VkBaseService
    {


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public IEnumerable<string> ConvertstringToList(string input)
        {
            var result = input.Split(new[] { "\r\n", ";" }, StringSplitOptions.RemoveEmptyEntries);
            return input.Replace("\r\n", "").Split(';').ToList();
        }
    }
}