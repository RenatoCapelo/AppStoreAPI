using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class GenderToCreate
    {
        [Required]
        public string description { get; set; }
    }
}
