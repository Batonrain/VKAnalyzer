using System.Collections.Generic;

namespace VKAnalyzer.Models.VKModels
{
    public class CohortAnalysisResultModel
    {
        public List<string>[,] ResultMatrix { get; set; }

        public int TableLength { get; set; }

        public string GroupId { get; set; }
    }
}