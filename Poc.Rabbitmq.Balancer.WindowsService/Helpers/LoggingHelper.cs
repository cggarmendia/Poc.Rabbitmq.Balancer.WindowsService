using System;
using System.Configuration;

namespace Poc.Rabbitmq.Balancer.WindowsService.Helpers
{
    public static class LoggingHelper
    {
        public static void TraceInformation(Func<string> func)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["DebugMode"]))
            {
                var message = func.Invoke();
                if (!string.IsNullOrEmpty(message))
                {
                    System.Diagnostics.Trace.TraceInformation(message);
                }
            }
        }
    }
}
