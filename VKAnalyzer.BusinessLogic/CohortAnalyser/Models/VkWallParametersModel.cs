using System;

namespace VKAnalyzer.BusinessLogic.CohortAnalyser.Models
{
    public class VkWallParametersModel
    {
        public string GroupId { get; set; }

        public int InnerStep { get; set; }

        public int OuterStep { get; set; }

        public int CyclesCount { get; set; }
    }
}
