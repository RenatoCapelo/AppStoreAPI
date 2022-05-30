using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppStoreAPI.Models
{
    public class AppInfo
    {
        public Guid devGuid { get; set; }
        public Guid appGuid { get; set; }
        public string packageName { get; set; }
        public string versionName { get; set; }
        public string label { get; set; }
        public string versionCode { get; set; }
        public string minSdkVersion { get; set; }
        public string targetSdkVersion { get; set; }
        public List<string> Permissions { get; set; }
    }
}
