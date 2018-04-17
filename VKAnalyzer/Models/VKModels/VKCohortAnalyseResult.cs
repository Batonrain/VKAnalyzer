using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKAnalyzer.Models.VKModels
{
    [Table("VkCohortAnalyseResults")]
    public class VkCohortAnalyseResult
    {
        [Key]
        public Int64 Id { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public DateTime CollectionDate { get; set; }

        public string GroupId { get; set; }

        public byte[] Result { get; set; }
    }


    [Table("VkCohortSalesAnalyseResults")]
    public class VkCohortSalesAnalyseResults
    {
        [Key]
        public Int64 Id { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public DateTime CollectionDate { get; set; }

        public string GroupId { get; set; }

        public byte[] Result { get; set; }
    }

    [Table("VkCohortSalesAnalyseWithListResults")]
    public class VkCohortSalesAnalyseWithListResults
    {
        [Key]
        public Int64 Id { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public DateTime CollectionDate { get; set; }

        public string GroupId { get; set; }

        public byte[] Result { get; set; }
    }
}