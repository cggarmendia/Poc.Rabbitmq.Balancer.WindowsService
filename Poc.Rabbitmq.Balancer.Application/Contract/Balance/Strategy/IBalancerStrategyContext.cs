using Poc.Rabbitmq.Balancer.Application.Dto.Balance;

namespace Poc.Rabbitmq.Balancer.Application.Contract.Balance.Strategy
{
    public interface IBalancerStrategyContext
    {
        void SetStrategy(string processType);
        decimal DoBalance(BalanceRequestDto parameters);
    }
}