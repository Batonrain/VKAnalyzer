using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKAnalyzer.Models.EFModels
{
    [Table("Groups")]
    public class Group
    {
        [Key]
        public Int64 Id { get; set; }

        public string Link { get; set; }
    }
}