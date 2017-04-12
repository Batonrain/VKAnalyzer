using System;
using System.Collections.Generic;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser.Models
{
    public class CohortAnalysisModel
    {
        public string PostId { get; set; }

        public List<string> LikedIds { get; set; }

        public DateTime PostDate { get; set; }
    }
}