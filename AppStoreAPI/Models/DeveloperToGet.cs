using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class DeveloperToGet
    {
        public Guid devGuid { get; set; }
        public Guid userGuid { get; set; }
        public string devName { get; set; }
        public string secEmail { get; set; }
    }
}
