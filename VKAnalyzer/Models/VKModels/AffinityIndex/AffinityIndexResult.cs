using System;
using System.Collections.Generic;

namespace VKAnalyzer.Models.VKModels.AffinityIndex
{
    [Serializable]
    public class AffinityIndexResult
    {
        public string ErrorMessage { get; set; }

        public string Audience { get; set; }

        public string ComparativeAudience { get; set; }

        public List<AffinityIndexCounter> Results { get; set; }

        public DateTime DateOfCollection { get; set; }

        public AffinityIndexResult()
        {
            Results = new List<AffinityIndexCounter>();
        }
    }

    [Serializable]
    public class AffinityIndexCounter
    {
        public int CategoryId { get; set; }

        public string Category { get; set; }

        public decimal Audience1Result { get; set; }

        public decimal Audience2Result { get; set; }

        public decimal Index { get; set; }
    }
}