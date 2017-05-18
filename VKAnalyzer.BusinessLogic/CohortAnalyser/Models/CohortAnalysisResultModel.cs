using System;
using System.Collections.Generic;

namespace VKAnalyzer.Models.VKModels
{
    public class CohortAnalysisResultModel
    {
        public List<string>[,] ResultMatrix { get; set; }

        public int TableLength { get; set; }

        public string GroupId { get; set; }

        public List<string> Dates { get; set; }

        public List<int> TotalVertical { get; set; }

        public List<int> TotalHorizontal { get; set; }

        public List<string> TotalNews { get; set; }
        public List<string> TotalOld { get; set; }
    }
}