using System;
using System.Collections.Generic;
using System.Text;

namespace TeamsConfiguration
{
    public class CreationResult
    {
        public string GroupId { get; set; }
        public List<string> ProjectChannelLibTabsLibNames { get; set; }
        public string ProjectSiteUrl { get; set; }
        public string ProjectChannelId { get; set; }
        public string FullProjectTitle { get; set; }
        public string ProjectDescription { get; set; }
    }
}
