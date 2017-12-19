using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VKAnalyzer.Models.VKModels
{
    public class ListOfResults
    {
        public List<AnalyseResultsViewModel> CohortAnalyseResults { get; set; }
        public List<AnalyseResultsViewModel> MemasAnalyzeResults { get; set; }
        public List<AnalyseResultsViewModel> CohortSalesAnalyseResults { get; set; }
    }
}