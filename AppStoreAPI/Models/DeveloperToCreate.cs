using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class DeveloperToCreate
    {
        //To be replaced with OAuth
        [Required]
        public Guid userGuid { get; set; }
        [Required]
        public string userPassword { get; set; }
        [Required]
        public string developerName { get; set; }
        [Required]
        [Phone]
        public string phoneNum { get; set; }
        [EmailAddress]
        public string secondEmail { get; set; }
    }
}
