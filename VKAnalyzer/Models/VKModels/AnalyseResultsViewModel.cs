﻿using System;

namespace VKAnalyzer.Models.VKModels
{
    public class AnalyseResultsViewModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string GroupId { get; set; }
        public DateTime DateOfCollection { get; set; }
        public string AnalyseType { get; set; }
        public Types? Type { get; set; }
    }

    public enum Types
    {
        CohortWithRetarget = 1,
        CohortWithList = 2
    }
}