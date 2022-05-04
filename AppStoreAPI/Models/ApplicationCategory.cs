using Dapper.Contrib.Extensions;

namespace AppStoreAPI.Models
{
    [Table("dbo.ApplicationCategory")]
    public class ApplicationCategory
    {
        public int id { get; set; }
        public string name { get; set; }
        public ApplicationMasterCategory masterCategory { get; set; }
    }
}
