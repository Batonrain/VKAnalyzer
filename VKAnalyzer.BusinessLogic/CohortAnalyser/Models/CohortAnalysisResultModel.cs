using System;
using System.Collections.Generic;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser.Models
{
    [Serializable]
    public class CohortAnalysisResultModel
    {
        public List<string>[,] AbsoluteValues { get; set; }

        public string[,] RelativeValues { get; set; }

        public List<string>[,] AbsoluteValuesWithShift { get; set; }

        public string[,] RelativeValuesWithShift { get; set; }

        public string[,] MediumValuesWithShift { get; set; }

        public int TableLength { get; set; }

        public string GroupId { get; set; }

        public List<string> Dates { get; set; }

        public List<int> TotalVertical { get; set; }

        public List<int> TotalHorizontal { get; set; }

        public List<string> TotalNews { get; set; }
        public List<string> TotalOld { get; set; }

        public CohortAnalysisResultModel()
        {
            Dates = new List<string>();
        }
    }
}