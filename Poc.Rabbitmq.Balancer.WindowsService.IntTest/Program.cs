using Vueling.Configuration.Library;
using Vueling.Messaging.RabbitMqEndpoint.Contracts.ServiceLibrary.Consumers.Events;

namespace Poc.Rabbitmq.Balancer.WindowsService.IntTest
{
    static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args")]
        static void Main(string[] args)
        {
            VuelingEnvironment.InitializeCurrentForApplication("Poc.Rabbitmq.Balancer.WindowsService");
            EndpointConfiguration.SetGetRabbitMqConnections();

            var container = MessageConsumerBuilder.RegisterMessageHandlers(
                    typeof(BookingBalancerHandler))
                .RegisterCustomisations()
                .BuildDebug();

            var handleEvent = EndpointResolver.Container.Resolve<IHandleEvent<BookingBalancerRequest>>() as BookingBalancerHandler;
            handleEvent.Handle(
                new BookingBalancerRequest()
                {
                    RecordLocator = "PJV74B",
                    Email = "skysalesauto@gmail.com",                    
                    Amount = 99.96M,
                    ProcessType = "CancelCreditShell",
                    RefundCurrencyCode = "EUR",
                    RefundType = "Percentage"
                }
            );
        }
    }
}
