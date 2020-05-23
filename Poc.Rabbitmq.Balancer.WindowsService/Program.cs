using System.Diagnostics;
using Poc.Rabbitmq.Balancer.WindowsService.Bootstrapping;
using Poc.Rabbitmq.Balancer.WindowsService.CommandHandlers;

namespace Poc.Rabbitmq.Balancer.WindowsService
{
    static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
        static void Main(string[] args)
        {

            var service = MessageConsumerBuilder.RegisterMessageHandlers(                                                
                        typeof(BookingBalancerHandler))
                    .RegisterCustomisations()
                    .Build();
            Trace.TraceInformation("Service created");

#if (!DEBUG)
            Trace.TraceInformation("ReleaseMode");
            System.ServiceProcess.ServiceBase.Run(service);
#else
            Trace.TraceInformation("DebugMode");
            service.Start();
#endif
        }


    }
}
