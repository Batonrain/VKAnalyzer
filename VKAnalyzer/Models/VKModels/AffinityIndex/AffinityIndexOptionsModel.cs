using System.ComponentModel.DataAnnotations;

namespace VKAnalyzer.Models.VKModels.AffinityIndex
{
    public class AffinityIndexOptionsModel
    {
        [Required]
        [Display(Name = "Название анализа")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Агентский кабинет")]
        public string AccountId { get; set; }

        [Required]
        [Display(Name = "Клиент рекламного кабинета")]
        public string ClientId { get; set; }

        public AffinityIndexOptionsAuditoryModel Auditory1 { get; set; }

        public AffinityIndexOptionsAuditoryModel Auditory2 { get; set; }
    }

    public class AffinityIndexOptionsAuditoryModel
    {
        [Required]
        [Display(Name = "Название аудитории")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Пол")]
        public string Gender { get; set; }

        [Display(Name = "Город")]
        public string Cities { get; set; }

        [Required]
        [Display(Name = "Возраст от")]
        public int AgesFrom { get; set; }

        [Required]
        [Display(Name = "Возраст до")]
        public int AgesUpTo { get; set; }

        [Display(Name = "Сообщества по интересам")]
        public string InterestGroupIds { get; set; }

        [Display(Name = "За исключением")]
        public string ExcludeInterestGroupIds { get; set; }
    }
}