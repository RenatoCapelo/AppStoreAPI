namespace AppStoreAPI.Models
{
    public class ApplicationRating
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public UserToGet Author { get; set; }
        public string Comment { get; set; }
    }
}
