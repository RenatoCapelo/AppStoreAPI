using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class ApplicationToPost
    {
        [Required]
        public IFormFile apk { get; set; }

        [Required]
        public string title { get; set; }

        [Required]
        public string description { get; set; }

        [Required]
        public int idAppCategory { get; set; }

    }
}
