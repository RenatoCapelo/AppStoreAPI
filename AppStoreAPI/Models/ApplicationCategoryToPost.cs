using System.ComponentModel.DataAnnotations;

namespace AppStoreAPI.Models
{
    public class ApplicationCategoryToPost
    {
        [Required]
        public int masterCategoryID { get; set; }
        [Required]
        public string name { get; set; }
    }
}
