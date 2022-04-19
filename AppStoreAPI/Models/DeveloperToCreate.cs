using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class DeveloperToCreate
    {    
        [Required]
        public string devName { get; set; }

        [Required]
        [Phone]
        public string phoneNum { get; set; }

        [EmailAddress]
        public string secEmail { get; set; }
    }
}
