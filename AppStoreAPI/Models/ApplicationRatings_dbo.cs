using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreAPI.Models
{
    [Table("ApplicationRating")]
    public class ApplicationRatings_dbo
    {
        [Key]
        public int id { get; set; }
        public int idUser { get; set; }
        public int idApplication { get; set; }
        public int rating { get; set; }
        public string comment { get; set; }
    }
}
