using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreAPI.Models
{
    [Table("Appstore.dbo.Application")]
    public class Application_DBO
    {
        [Key]
        public int id { get; set; }
        public int idDeveloper { get; set; }
        public int idAppCategory { get; set; }
        public string packageName { get; set; }
        public string title { get; set; }
        public int minsdkversion { get; set; }
        public double applicationSize { get; set; }
        public Guid applicationGuid { get; set; }
        public int versionCode { get; set; }
        public string versionName { get; set; }
        public string descripiton { get; set; }
    }
}
