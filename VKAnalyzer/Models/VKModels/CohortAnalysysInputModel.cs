using System;
using System.ComponentModel.DataAnnotations;

namespace VKAnalyzer.Models.VKModels
{
    public class CohortAnalysysInputModel
    {
        [Required]
        [Display(Name = "Идентификатор группы")]
        public string GroupId { get; set; }

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
    }
}