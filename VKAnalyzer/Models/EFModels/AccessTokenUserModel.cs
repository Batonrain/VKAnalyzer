using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKAnalyzer.Models.EFModels
{
    [Table("UserAccessTokens")]
    public class UserAccessToken
    {
        [Key]
        public string VkUserId { get; set; }

        public string AccessToken { get; set; }
    }
}