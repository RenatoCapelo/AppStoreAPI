using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class DeveloperToGetOnAuth
    {
        public Guid devGuid { get; set; }
        public string devName { get; set; }
        public string secEmail { get; set; }
    }
}
