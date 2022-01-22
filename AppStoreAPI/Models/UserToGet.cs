using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class UserToGet
    {
        public Guid Guid { get; set; }
        public string Email { get; set; }
        public Guid PhotoGuid { get; set; }
        public string Name { get; set; }
        public DateTime Dob { get; set; }
        public UserGender Gender { get; set; }
        public DeveloperToGetOnAuth Developer { get; set; }
        public string Role { get; set; }
    }
}
