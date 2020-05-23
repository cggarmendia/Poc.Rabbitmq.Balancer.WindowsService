namespace Poc.Rabbitmq.Balancer.WindowsService.Configuration
{
    public interface IConfiguration
    {
        short PerConsumerConcurrency { get; }
    }
}
