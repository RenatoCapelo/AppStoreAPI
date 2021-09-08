using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class UserToGet
    {
        public Guid guid { get; set; }
        public string email { get; set; }
        public Guid photo_guid { get; set; }
        public string name { get; set; }
        public DateTime dob { get; set; }
        public UserGender Gender { get; set; }
    }
}
