using System;
using System.ComponentModel.DataAnnotations;

namespace VKAnalyzer.Models.VKModels
{
    public class VkCohortAnalyseOfSalesModel
    {
        [Required]
        [Display(Name = "Название анализа")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Идентификатор группы")]
        public string GroupId { get; set; }

        //[Required]
        //[Display(Name = "Учитывать сторонних пользователей")]
        //public bool ExcludeUsers { get; set; }

        [Required]
        [Display(Name = "Шаг анализа")]
        public int Step { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'.'MM'.'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата начала")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'.'MM'.'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата окончания")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Список покупателей")]
        public string ListOfBuyers { get; set; }
        
    }
}