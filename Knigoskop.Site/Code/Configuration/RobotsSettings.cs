using System.Configuration;

namespace Knigoskop.Site.Code.Configuration
{
    public enum CrawlerAccess
    {
        Private,
        Public
    }

    public class RobotsSettings : ConfigurationSection
    {
        [ConfigurationProperty("robotsConfigFile")]
        public string RobotsConfigFile
        {
            get { return (string) this["robotsConfigFile"]; }
            set { this["robotsConfigFile"] = value; }
        }

        [ConfigurationProperty("crawlerAccess", DefaultValue = CrawlerAccess.Private, IsRequired = true)]
        public CrawlerAccess CrawlerAccess
        {
            get { return (CrawlerAccess) this["crawlerAccess"]; }
            set { this["crawlerAccess"] = value; }
        }
    }
}