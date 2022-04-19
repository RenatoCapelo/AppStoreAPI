using System;

namespace AppStoreAPI.Models
{
    public class AppToGet
    {
        public Guid applicationGuid { get; set; }
        public DeveloperToGet developer { get; set; }
        public ApplicationCategory applicationCategory { get; set; }
        public string packageName { get; set; }
        public string title { get; set; }
        public int minSdkVersion { get; set; }
        public double applicationSize { get; set; }
        public int versionCode { get; set; }
        public string versionName { get; set; }
        public double ratingAverage { get; set; }
    }
}
