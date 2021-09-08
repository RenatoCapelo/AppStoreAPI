using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class UserToCreate
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        [MinLength(1)]
        public string name { get; set; }
        [Required]
        public DateTime dob { get; set; }
        [Required]
        public int idGender { get; set; }
        [Required]
        [MinLength(6)]
        public string password { get; set; }
    }
}
