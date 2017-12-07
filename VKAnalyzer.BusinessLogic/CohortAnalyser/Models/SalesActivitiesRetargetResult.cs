using System;
using System.Collections.Generic;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser.Models
{
    [Serializable]
    public class SalesActivitiesRetargetResult
    {
        public SalesActivitiesRetargetResult()
        {
            Results = new List<SalesActivitiesRetargetStepData>();
        }

        public string GroupId { get; set; }

        public List<SalesActivitiesRetargetStepData> Results { get; set; }
    }

    [Serializable]
    public class SalesActivitiesRetargetStepData
    {
        public SalesActivitiesRetargetStepData()
        {
            Values = new List<string>();
        }

        public string Date { get; set; }
        public List<string> Values { get; set; }
    }
}
