using System.Collections.Generic;
using VKAnalyzer.Models.VKModels.AffinityIndex;

namespace VKAnalyzer.Services.Interfaces
{
    public interface IAffinityIndexService
    {
        void Start(IEnumerable<AffinityIndexOptionsAuditoryModel> audiencesUnderAnalysis, IEnumerable<AffinityIndexOptionsAuditoryModel> comparativeAudience, string accountId, string clientId, string accessToken, string userId);
    }
}
