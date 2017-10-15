using System;
using System.Collections.Generic;
using VKAnalyzer.Services.VK;

namespace VKAnalyzer.Models.VKModels.Memas
{
    [Serializable]
    public class MemasAnalyzeResultModel
    {
        public List<MemasPost> TopByLikes { get; set; }
        public List<MemasPost> TopByComments { get; set; }
        public List<MemasPost> TopByReposts { get; set; }
        public List<MemasPost> TopByViews { get; set; }
    }
}