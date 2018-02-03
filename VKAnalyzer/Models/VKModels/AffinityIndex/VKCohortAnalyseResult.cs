using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKAnalyzer.Models.VKModels.AffinityIndex
{
    [Table("VkAffinityIndexResults")]
    public class VkAffinityIndexResults
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