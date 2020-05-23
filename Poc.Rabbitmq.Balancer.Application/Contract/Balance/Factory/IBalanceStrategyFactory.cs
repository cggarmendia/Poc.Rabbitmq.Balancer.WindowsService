using Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy;

namespace Poc.Rabbitmq.Balancer.Application.Contract.Balance.Factory
{
    public interface IBalanceStrategyFactory
    {
        T GetBalanceStrategy<T>(params object[] objects) where T : class, IBalanceStrategy;
    }
}