using System;

namespace VKAnalyzer.Models.VKModels.AffinityIndex
{
    public class AffinityIndexResultsViewModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfCollection { get; set; }
        public string AnalyseType { get; set; }
    }
}