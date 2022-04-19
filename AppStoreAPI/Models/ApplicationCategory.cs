namespace AppStoreAPI.Models
{
    public class ApplicationCategory
    {
        public int id { get; set; }
        public string name { get; set; }
        public ApplicationMasterCategory masterCategory { get; set; }
    }
}
