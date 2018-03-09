using System.ComponentModel.DataAnnotations;

namespace VKAnalyzer.Models.VKModels.AffinityIndex
{
    public class AffinityIndexOptionsModel
    {
        [Required]
        [Display(Name = "Название анализа:")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Агентский кабинет:")]
        public string AccountId { get; set; }

        [Display(Name = "Клиент рекламного кабинета:")]
        public string ClientId { get; set; }

        public AffinityIndexOptionsAuditoryModel Auditory1 { get; set; }

        public AffinityIndexOptionsAuditoryModel Auditory2 { get; set; }
    }

    public class AffinityIndexOptionsAuditoryModel
    {
        [Required]
        [Display(Name = "Название аудитории")]
        public string Name { get; set; }

        [Display(Name = "Пол:")]
        public string Gender { get; set; }

        [Display(Name = "Возраст от:")]
        public string AgesFrom { get; set; }

        [Display(Name = "Возраст до:")]
        public string AgesUpTo { get; set; }

        [Display(Name = "Семейное положение:")]
        public string Status { get; set; }

        [Display(Name = "Страна:")]
        public string Country { get; set; }

        [Display(Name = "Города:")]
        public string Cities { get; set; }

        [Display(Name = "За исключением:")]
        public string ExcludeCities { get; set; }

        [Display(Name = "Университет:")]
        public string Univercity { get; set; }

        [Display(Name = "Сообщества по интересам:")]
        public string InterestGroupIds { get; set; }

        [Display(Name = "За исключением")]
        public string ExcludeInterestGroupIds { get; set; }

        [Display(Name = "Аудитории ретаргетинга:")]
        public string RetargetGroupIds { get; set; }

        [Display(Name = "За исключением:")]
        public string ExcludeRetargetGroupIds { get; set; }
    }
}