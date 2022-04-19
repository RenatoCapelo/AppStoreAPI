namespace AppStoreAPI.Models
{
    public class ApplicationRatings_dbo
    {
        public int id { get; set; }
        public int idUser { get; set; }
        public int idApplication { get; set; }
        public double rating { get; set; }
        public string comment { get; set; }
    }
}
