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
            Values = new List<SalesActivitiesRetargetPostResult>();
        }

        public string Date { get; set; }
        public List<SalesActivitiesRetargetPostResult> Values { get; set; }
    }

    [Serializable]
    public class SalesActivitiesRetargetPostResult
    {
        public string PostId { get; set; }
        public string Result { get; set; }
    }
}
