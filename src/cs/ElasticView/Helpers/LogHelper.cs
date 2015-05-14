using log4net;

namespace ElasticView.Helpers
{
    /// <summary>
    /// Logging helper.
    /// </summary>
    public static class LogHelper
    {
        private static ILog log = null;
        public static ILog Log
        {
            get
            {
                if (log == null)
                {
                    log4net.Config.XmlConfigurator.Configure();
                    log = LogManager.GetLogger("Common");
                }
                return log;
            }
        }
    }
}
