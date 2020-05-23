using Autofac;

namespace Poc.Rabbitmq.Balancer.WindowsService.Bootstrapping.Fluent
{
    public interface IBuildWindowsService
    {
        ConsumerWindowsService Build();
        IContainer BuildDebug();
    }
}
