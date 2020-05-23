using System;
using System.Diagnostics;
using System.ServiceProcess;
using Poc.Rabbitmq.Balancer.WindowsService.Configuration;
using Poc.Rabbitmq.Core.Message.Domain.BookingBalancer;

namespace Poc.Rabbitmq.Balancer.WindowsService
{
    public partial class ConsumerWindowsService : ServiceBase
    {
        private readonly IConfiguration _configuration;

        public ConsumerWindowsService(IConfiguration configuration)
        {
            _configuration = configuration;

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                Endpoint.InitializeAsConsumer(_endpointManager)
                    .HandleEvent<BookingBalancerRequest>()                                                            
                    .WithCompetingConsumers(3, _configuration.PerConsumerConcurrency)
                    .Start();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not start windows service: " + ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                Endpoint.Stop(TimeSpan.FromSeconds(60));
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not stop windows service: " + ex);
            }
        }

  
        public void Start()
        {
            OnStart(null);

            Console.WriteLine("Started. Press any key to shutdown the consumer(s)");
            Console.Read();
            Console.WriteLine("Shutting down, this may take a few seconds...");
            Endpoint.Stop(TimeSpan.FromSeconds(60));
        }
    }
}
