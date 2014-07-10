using System.Configuration;

namespace Knigoskop.Site.Code.Configuration
{
    public class CalibreSettings : ConfigurationSection
    {
        [ConfigurationProperty("path")]
        public string Path
        {
            get { return (string) this["path"]; }
            set { this["path"] = value; }
        }
    }
}