using System;
using System.Collections.Generic;
using System.Text;

namespace TaboolaRasa.Configurations
{
    public enum BuildConfiguration
    {
        Debug,
        Snapshot,
        Staging,
        Production,
    }

    public class SharedConfig
    {
        protected static SharedConfig LocalInstance;
        public static string EmailTemplatePath;
        private BuildConfiguration _buildConfiguration;
        public BuildConfiguration BuildConfiguration
        {
            get
            {
                return _buildConfiguration;
            }
        }
        public string SiteUrl = "https://localhost:44377";
        public string AdminEmailAddress = "someone@email.com";
        public string EmailOverrideAddress = null;
        public string FromEmailAddress = "test@example.co";
        public string FromEmailName = "Test";


        private SharedConfig()
        {

#if DEVELOPMENT
            _buildConfiguration = BuildConfiguration.Snapshot;
#elif STAGING
            _buildConfiguration = BuildConfiguration.Staging;
#elif PRODUCTION
            _buildConfiguration = BuildConfiguration.Production;
#else
            _buildConfiguration = BuildConfiguration.Debug;
#endif            

            switch (_buildConfiguration)
            {
                case BuildConfiguration.Debug:
                    break;
                case BuildConfiguration.Snapshot:
                    break;
                case BuildConfiguration.Staging:
                    break;
                case BuildConfiguration.Production:
                    break;
            }
        }

        public static SharedConfig Instance
        {
            get { return LocalInstance ?? (LocalInstance = new SharedConfig()); }
        }
    }
}
