﻿using System;
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
        public bool hasIcon { get; set; }
        public string iconFileName { get; set; }
        public string minSdkVersion { get; set; }
        public string targetSdkVersion { get; set; }
        public List<string> Permissions { get; set; }
        public bool supportAnyDensity { get; set; }
        public bool supportLargeScreens { get; set; }
        public bool supportNormalScreens { get; set; }
        public bool supportSmallScreens { get; set; }
        public string hostedAt { get; set; }
    }
}