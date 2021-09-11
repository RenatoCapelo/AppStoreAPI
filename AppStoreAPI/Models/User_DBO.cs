using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class User_DBO
    {
        public int id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public DateTime dob { get; set; }
        public int idGender { get; set; }
        public string password { get; set; }
        public Guid guid { get; set; }
        public Guid photoguid { get; set; }
    }
}
