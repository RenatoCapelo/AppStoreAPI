using System;
using System.ComponentModel.DataAnnotations;

namespace AppStoreAPI.Models
{
    public class ApplicationDetailsToUpdate
    {
        [Required]
        public Guid applicationGuid { get; set; }
        [Required]
        public string title { get; set; }

        [Required]
        public string description { get; set; }

        [Required]
        public int idAppCategory { get; set; }
    }
}
