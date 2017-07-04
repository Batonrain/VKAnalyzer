using System;

namespace VKAnalyzer.Models.VKModels
{
    public class AnalyseResultsViewModel
    {
        public Int64 Id { get; set; }
        public string GroupId { get; set; }
        public string DateOfCollection { get; set; }
        public string AnalyseType { get; set; }
    }
}