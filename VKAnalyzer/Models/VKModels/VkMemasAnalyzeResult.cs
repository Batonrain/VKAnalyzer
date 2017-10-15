using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKAnalyzer.Models.VKModels
{
    [Table("VkMemasAnalyzeResults")]
    public class VkMemasAnalyzeResult
    {
        [Key]
        public Int64 Id { get; set; }

        public string Name { get; set; }

        public string UserId { get; set; }

        public DateTime CollectionDate { get; set; }

        public byte[] Result { get; set; }
    }

}